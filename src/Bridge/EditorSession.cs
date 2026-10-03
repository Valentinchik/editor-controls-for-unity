namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    // One connected Unity editor.
    internal sealed class EditorSession
    {
        private readonly TcpClient _client;
        private readonly Object _writeLock = new();
        private StreamWriter _writer;
        private HashSet<String> _commands = new();

        public EditorSession(TcpClient client)
        {
            this._client = client;
        }

        public Int32 Pid { get; private set; }

        public String Project { get; private set; }
        public String ProjectPath { get; private set; }
        public String UnityVersion { get; private set; }
        public EditorState State { get; private set; }
        public EditorCatalog Catalog { get; private set; } = EditorCatalog.Empty;
        public Boolean Reloading { get; private set; }
        public Boolean ReloadingForPlayMode { get; private set; }
        public DateTime LastFocusedUtc { get; private set; }

        public Boolean Supports(String commandId) => this._commands.Contains(commandId);

        public async Task RunAsync(String token, CancellationToken cancellation)
        {
            try
            {
                using var stream = this._client.GetStream();
                using var reader = new StreamReader(stream, new UTF8Encoding(false));
                this._writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" };

                if (!this.TryAcceptHello(await reader.ReadLineAsync(cancellation), token))
                {
                    return;
                }

                UnityBridge.Register(this);
                this.Send(new { type = "welcome", protocol = UnityBridge.Protocol });

                String line;
                while ((line = await reader.ReadLineAsync(cancellation)) != null)
                {
                    this.Handle(line);
                }
            }
            catch (Exception ex) when (ex is IOException or SocketException or OperationCanceledException or JsonException or ObjectDisposedException)
            {
                // Editor closed, domain reload or plugin shutdown.
            }
            finally
            {
                this._client.Close();
                UnityBridge.Unregister(this);
            }
        }

        public void Close() => this._client.Close();

        public Boolean Send(Object message)
        {
            lock (this._writeLock)
            {
                try
                {
                    this._writer?.WriteLine(JsonSerializer.Serialize(message));
                    return this._writer != null;
                }
                catch (Exception ex) when (ex is IOException or ObjectDisposedException)
                {
                    return false;
                }
            }
        }

        private Boolean TryAcceptHello(String line, String token)
        {
            if (line == null)
            {
                return false;
            }

            var hello = JsonDocument.Parse(line).RootElement;
            if (hello.GetProperty("type").GetString() != "hello" || hello.GetProperty("token").GetString() != token)
            {
                PluginLog.Warning("Rejected a bridge connection with a wrong handshake");
                return false;
            }

            this.Pid = hello.GetProperty("pid").GetInt32();
            this.Project = hello.GetProperty("project").GetString();
            this.ProjectPath = hello.GetProperty("projectPath").GetString();
            this.UnityVersion = hello.GetProperty("unityVersion").GetString();
            this._commands = new HashSet<String>();
            foreach (var id in hello.GetProperty("commands").EnumerateArray())
            {
                this._commands.Add(id.GetString());
            }

            PluginLog.Info($"Unity connected: {this.Project} ({this.UnityVersion}), package {hello.GetProperty("packageVersion").GetString()}, {this._commands.Count} commands");
            return true;
        }

        private void Handle(String line)
        {
            var message = JsonDocument.Parse(line).RootElement;
            switch (message.GetProperty("type").GetString())
            {
                case "state":
                    this.State = EditorState.Parse(message);
                    if (this.State.Focused)
                    {
                        this.LastFocusedUtc = DateTime.UtcNow;
                    }

                    UnityBridge.NotifyChanged();
                    break;
                case "reloading":
                    this.Reloading = true;
                    this.ReloadingForPlayMode = message.TryGetProperty("playMode", out var playMode) && playMode.GetBoolean();
                    break;
                case "list":
                    var (name, items) = EditorCatalog.ParseList(message);
                    this.Catalog = this.Catalog.With(name, items);
                    PluginLog.Verbose($"List '{name}' from {this.Project}: {items.Count} items");
                    UnityBridge.NotifyChanged();
                    break;
            }
        }
    }
}
