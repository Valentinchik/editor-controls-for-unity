using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Valentinchik.EditorControls
{
    // Connects the editor to the Editor Controls plugin in Logi Options+. The plugin is the server;
    // every domain reload tears this client down and the next one reconnects within a couple of seconds.
    [InitializeOnLoad]
    internal static class BridgeClient
    {
        private const int Protocol = 1;
        private const string PackageVersion = "0.5.0";
        private const int RetryMilliseconds = 2000;
        private const string ConnectedOnceKey = "Valentinchik.EditorControls.ConnectedOnce";

        private static readonly ConcurrentQueue<string> Incoming = new ConcurrentQueue<string>();
        private static readonly object WriteLock = new object();
        private static readonly ManualResetEvent StopSignal = new ManualResetEvent(false);
        private static readonly ManualResetEvent HelloReady = new ManualResetEvent(false);
        private static readonly HelloMessage Hello;
        private static readonly string EndpointFile;
        private static readonly Thread Worker;

        private static StreamWriter _writer;
        private static TcpClient _client;
        private static volatile bool _connected;
        private static volatile bool _sendFullState;
        private static string _lastSent;
        private static string _lastLayout;
        private static volatile bool _resendLists;
        private static double _nextProbeTime;

        static BridgeClient()
        {
            // Unity APIs are main-thread only, so everything the worker thread needs is captured here.
            Hello = new HelloMessage
            {
                protocol = Protocol,
                pid = System.Diagnostics.Process.GetCurrentProcess().Id,
                project = Application.productName,
                projectPath = Path.GetDirectoryName(Application.dataPath),
                unityVersion = Application.unityVersion,
                packageVersion = PackageVersion,
            };
            EndpointFile = EndpointPath();

            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.quitting += Stop;
            EditorBuildSettings.sceneListChanged += () => EditorCatalog.MarkDirty(EditorCatalog.ScenesList);
            EditorApplication.projectChanged += OnProjectChanged;
            EditorControlsSettings.Changed += () => EditorCatalog.MarkDirty(EditorCatalog.ToolsList);
            EditorApplication.focusChanged += focused => EditorCatalog.MarkDirty(EditorCatalog.LayoutsList);
            AppDomain.CurrentDomain.DomainUnload += (_, __) => Stop();

            Worker = new Thread(Run) { IsBackground = true, Name = "Editor Controls bridge" };
            Worker.Start();
        }

        private static void Run()
        {
            WaitHandle.WaitAny(new WaitHandle[] { StopSignal, HelloReady });
            while (!StopSignal.WaitOne(0))
            {
                try
                {
                    var endpoint = ReadEndpoint();
                    if (endpoint != null && endpoint.protocol == Protocol)
                    {
                        RunSession(endpoint);
                    }
                }
                catch (Exception)
                {
                    // Plugin not running, socket dropped, or Stop() closed it — the loop decides whether to retry.
                }
                finally
                {
                    lock (WriteLock)
                    {
                        _connected = false;
                        _writer = null;
                        _client?.Close();
                        _client = null;
                    }
                }

                StopSignal.WaitOne(RetryMilliseconds);
            }
        }

        private static void RunSession(BridgeEndpoint endpoint)
        {
            var client = new TcpClient { NoDelay = true };
            client.Connect(IPAddress.Loopback, endpoint.port);
            var stream = client.GetStream();
            var reader = new StreamReader(stream, new UTF8Encoding(false));
            var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" };

            lock (WriteLock)
            {
                _client = client;
                _writer = writer;
                Hello.token = endpoint.token;
                writer.WriteLine(JsonUtility.ToJson(Hello));
                _connected = true;
                _sendFullState = true;
                _resendLists = true;
            }

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Incoming.Enqueue(line);
            }
        }

        private static void Update()
        {
            if (!HelloReady.WaitOne(0))
            {
                // Right after a domain reload the menus are still being rebuilt, so menu lookups wait for the first editor tick.
                Hello.commands = EditorCommands.SupportedIds();
                HelloReady.Set();
            }

            while (Incoming.TryDequeue(out var line))
            {
                Handle(line);
            }

            if (!_connected || EditorApplication.timeSinceStartup < _nextProbeTime)
            {
                return;
            }

            _nextProbeTime = EditorApplication.timeSinceStartup + 0.2;
            var probe = EditorStateProbe.Capture();
            if (probe.layout != _lastLayout)
            {
                // Saving a layout makes it the current one, so a new name may mean a new file.
                _lastLayout = probe.layout;
                EditorCatalog.MarkDirty(EditorCatalog.LayoutsList);
            }

            if (_resendLists)
            {
                _resendLists = false;
                EditorCatalog.ResendAll();
            }

            foreach (var list in EditorCatalog.TakeChanged())
            {
                Send(list);
            }

            var state = JsonUtility.ToJson(probe);
            if (_sendFullState || state != _lastSent)
            {
                _sendFullState = false;
                _lastSent = state;
                Send(state);
            }
        }

        private static void Handle(string line)
        {
            var header = JsonUtility.FromJson<MessageHeader>(line);
            var message = header?.type == "welcome" ? null : JsonUtility.FromJson<CommandMessage>(line);
            switch (header?.type)
            {
                case "welcome":
                    if (!SessionState.GetBool(ConnectedOnceKey, false))
                    {
                        SessionState.SetBool(ConnectedOnceKey, true);
                        Debug.Log("[Editor Controls] Connected to Logi Options+.");
                    }
                    break;
                case "command":
                    EditorCommands.Run(message.id);
                    break;
                case "openScene":
                    EditorCatalog.OpenScene(message.path);
                    break;
                case "runTool":
                    EditorCatalog.RunTool(message.id);
                    break;
                case "listMenus":
                    Send(EditorCatalog.Menus());
                    break;
                case "loadLayout":
                    WindowLayouts.Load(message.path);
                    break;
                case "openAsset":
                    AssetShortcuts.Open(message.id);
                    break;
                case "bookmark":
                    CameraBookmarks.Use(message.index, message.save);
                    break;
                case "timeScale":
                    TimeScale.Set(message.value);
                    break;
                case "timeScaleStep":
                    TimeScale.Step(message.index);
                    break;
            }
        }

        private static void Send(string json)
        {
            lock (WriteLock)
            {
                try
                {
                    _writer?.WriteLine(json);
                }
                catch (Exception)
                {
                    _client?.Close(); // the worker notices and reconnects
                }
            }
        }

        private static void OnProjectChanged()
        {
            // Assets were added, moved or renamed: scene names, tools and asset labels may be stale.
            EditorCatalog.MarkDirty(EditorCatalog.ScenesList);
            EditorCatalog.MarkDirty(EditorCatalog.ToolsList);
            EditorCatalog.MarkDirty(EditorCatalog.RecentList);
            EditorCatalog.MarkDirty(EditorCatalog.FavoritesList);
        }

        private static void OnBeforeAssemblyReload()
        {
            // Lets the plugin show "compiling" instead of "disconnected" while the domain reloads.
            Send(JsonUtility.ToJson(new ReloadingMessage { playMode = EditorApplication.isPlayingOrWillChangePlaymode }));
            Stop();
        }

        private static void Stop()
        {
            StopSignal.Set();
            lock (WriteLock)
            {
                _client?.Close();
            }

            Worker.Join(500);
        }

        private static BridgeEndpoint ReadEndpoint()
        {
            return File.Exists(EndpointFile) ? JsonUtility.FromJson<BridgeEndpoint>(File.ReadAllText(EndpointFile)) : null;
        }

        // Written by the plugin on start; the same location on both sides, so not SpecialFolder-based on macOS.
        private static string EndpointPath() =>
            Application.platform == RuntimePlatform.WindowsEditor
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EditorControlsForUnity", "bridge.json")
                : Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "", "Library", "Application Support", "EditorControlsForUnity", "bridge.json");
    }
}
