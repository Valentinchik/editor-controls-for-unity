namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    // Loopback server for the Unity package com.valentinchik.editor-controls. The plugin is the server so that it
    // survives Unity's domain reloads; each open editor connects as a client.
    // Which editor a key acts on: the one in front if it runs the package; none if the editor in front does not (the key
    // falls back to a keystroke, which reaches that window); the one focused last while Unity is in the background.
    // Wire format: one JSON object per line. Address and token are published in an endpoint file only this user can read.
    internal static class UnityBridge
    {
        public const Int32 Protocol = 1;
        private const Int32 PreferredPort = 47190;
        private static readonly TimeSpan ReloadGrace = TimeSpan.FromSeconds(60);

        private static readonly Object Sync = new();
        private static readonly List<EditorSession> Sessions = new();
        private static TcpListener _listener;
        private static CancellationTokenSource _cancellation;
        private static String _token;
        private static DateTime _reloadingUntilUtc = DateTime.MinValue;
        private static Boolean _reloadingForPlayMode;
        private static Boolean _reloadingWasFocused;
        private static Timer _reloadTimer;
        private static Func<Boolean> _unityInFront = () => false;
        private static Boolean _lastUnityInFront;

        // Raised on connect, disconnect and every state change of any editor; may come from a socket thread.
        public static event Action Changed;

        public static EditorView View
        {
            get
            {
                lock (Sync)
                {
                    var target = Target();
                    if (target?.State != null)
                    {
                        return new EditorView(target.State, target.Reloading, target.ReloadingForPlayMode);
                    }

                    // Mid domain reload the editor has no session; it counts as the target if it was the one in front.
                    var reloading = DateTime.UtcNow < _reloadingUntilUtc && (_reloadingWasFocused || !UnityInFront());
                    return reloading ? new EditorView(null, true, _reloadingForPlayMode) : EditorView.Disconnected;
                }
            }
        }

        public static EditorCatalog ActiveCatalog
        {
            get
            {
                lock (Sync)
                {
                    return Target()?.Catalog ?? EditorCatalog.Empty;
                }
            }
        }

        public static String ActiveProject
        {
            get
            {
                lock (Sync)
                {
                    return Target()?.Project;
                }
            }
        }

        // The process id of the editor keys act on, or null (see the class comment).
        public static Int32? TargetPid
        {
            get
            {
                lock (Sync)
                {
                    return Target()?.Pid;
                }
            }
        }

        // Every connected editor, for haptics: each one's transitions are followed separately.
        public static IReadOnlyList<EditorSnapshot> Editors
        {
            get
            {
                lock (Sync)
                {
                    return Sessions.Where(s => s.State != null)
                        .Select(s => new EditorSnapshot(s.Pid, s.State, s.Reloading && !s.ReloadingForPlayMode))
                        .ToList();
                }
            }
        }

        // unityInFront: whether a Unity Editor is the frontmost app (Options+ knows the app, not which project).
        public static void Start(Func<Boolean> unityInFront)
        {
            _unityInFront = unityInFront;
            _token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
            _cancellation = new CancellationTokenSource();
            _listener = Listen();
            WriteEndpointFile(((IPEndPoint)_listener.LocalEndpoint).Port);
            _ = AcceptLoopAsync(_cancellation.Token);
            PluginLog.Info($"Unity bridge listening on 127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}");
        }

        public static void Stop()
        {
            _cancellation?.Cancel();
            _listener?.Stop();
            _reloadTimer?.Dispose();
            try
            {
                File.Delete(EndpointFilePath());
            }
            catch (IOException)
            {
            }
        }

        // Switching between Unity and another app raises no editor event but can change the target; polled on a timer.
        public static void CheckFrontApplication()
        {
            var inFront = UnityInFront();
            if (inFront != _lastUnityInFront)
            {
                _lastUnityInFront = inFront;
                NotifyChanged();
            }
        }

        // True when the target editor runs the Unity package and handles this command itself;
        // false sends the key as a keystroke instead.
        public static Boolean TrySend(String commandId)
        {
            EditorSession session;
            lock (Sync)
            {
                session = Target();
            }

            if (session == null || !session.Supports(commandId) || !session.Send(new { type = "command", id = commandId }))
            {
                return false;
            }

            PluginLog.Verbose($"'{commandId}' → {session.Project}");
            return true;
        }

        // Folder and bridge-only actions: open a scene, run a tool, load a layout, ... in the target editor.
        public static Boolean TrySend(Object message)
        {
            EditorSession session;
            lock (Sync)
            {
                session = Target();
            }

            return session != null && session.Send(message);
        }

        internal static void Register(EditorSession session)
        {
            List<EditorSession> stale;
            lock (Sync)
            {
                // A domain reload can leave the previous connection of the same editor half-open; the new one replaces it.
                stale = Sessions.Where(s => s.Pid == session.Pid).ToList();
                Sessions.Add(session);
                _reloadingUntilUtc = DateTime.MinValue;
            }

            stale.ForEach(s => s.Close());
            NotifyChanged();
        }

        internal static void Unregister(EditorSession session)
        {
            lock (Sync)
            {
                if (!Sessions.Remove(session))
                {
                    return;
                }

                if (session.Reloading)
                {
                    _reloadingUntilUtc = DateTime.UtcNow + ReloadGrace;
                    _reloadingForPlayMode = session.ReloadingForPlayMode;
                    _reloadingWasFocused = session.State?.Focused == true;
                    _reloadTimer?.Dispose();
                    _reloadTimer = new Timer(_ => NotifyChanged(), null, ReloadGrace, Timeout.InfiniteTimeSpan);
                }
            }

            PluginLog.Info($"Unity disconnected: {session.Project}{(session.Reloading ? " (domain reload)" : "")}");
            NotifyChanged();
        }

        internal static void NotifyChanged()
        {
            try
            {
                Changed?.Invoke();
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Bridge state handler failed");
            }
        }

        private static EditorSession Target()
        {
            var inFront = Sessions.Where(s => s.State?.Focused == true).OrderByDescending(s => s.LastFocusedUtc).FirstOrDefault();
            if (inFront != null)
            {
                return inFront;
            }

            return UnityInFront() ? null : Sessions.OrderByDescending(s => s.LastFocusedUtc).FirstOrDefault();
        }

        private static Boolean UnityInFront()
        {
            try
            {
                return _unityInFront();
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static TcpListener Listen()
        {
            try
            {
                var preferred = new TcpListener(IPAddress.Loopback, PreferredPort);
                preferred.Start();
                return preferred;
            }
            catch (SocketException)
            {
                var any = new TcpListener(IPAddress.Loopback, 0);
                any.Start();
                return any;
            }
        }

        private static async Task AcceptLoopAsync(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync(cancellation);
                    client.NoDelay = true;
                    _ = new EditorSession(client).RunAsync(_token, cancellation);
                }
                catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException or SocketException)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }
        }

        private static void WriteEndpointFile(Int32 port)
        {
            var path = EndpointFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonSerializer.Serialize(new { protocol = Protocol, port, token = _token }));
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }

        // Must match BridgeClient.EndpointPath() in the Unity package.
        private static String EndpointFilePath() => OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EditorControlsForUnity", "bridge.json")
            : Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "", "Library", "Application Support", "EditorControlsForUnity", "bridge.json");
    }
}

namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;

    // One connected editor as haptics see it. CompileReload: the domain is reloading with freshly compiled code.
    public sealed record EditorSnapshot(Int32 Pid, EditorState State, Boolean CompileReload);
}
