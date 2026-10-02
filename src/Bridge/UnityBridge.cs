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
    // survives Unity's domain reloads; each open editor connects as a client, and commands go to the one focused last.
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
        private static Timer _reloadTimer;

        // Raised on connect, disconnect and every state change of any editor; may come from a socket thread.
        public static event Action Changed;

        public static EditorView View
        {
            get
            {
                lock (Sync)
                {
                    var active = ActiveSession();
                    if (active?.State != null)
                    {
                        return new EditorView(active.State, active.Reloading);
                    }

                    return DateTime.UtcNow < _reloadingUntilUtc ? EditorView.ReloadingDomain : EditorView.Disconnected;
                }
            }
        }

        public static String ActiveProject
        {
            get
            {
                lock (Sync)
                {
                    return ActiveSession()?.Project;
                }
            }
        }

        public static void Start()
        {
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

        // True when the focused editor runs the Unity package and handles this command itself.
        public static Boolean TrySend(String commandId)
        {
            EditorSession session;
            lock (Sync)
            {
                session = ActiveSession();
            }

            return session != null && session.Supports(commandId) && session.Send(new { type = "command", id = commandId });
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

        private static EditorSession ActiveSession() => Sessions.OrderByDescending(s => s.LastFocusedUtc).FirstOrDefault();

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
