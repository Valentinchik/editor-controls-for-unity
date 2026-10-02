using System;

// Fields are filled by JsonUtility, which the compiler cannot see.
#pragma warning disable 0649

namespace Valentinchik.EditorControls
{
    // Wire format shared with the Logi plugin: one JSON object per line over a loopback TCP socket.
    // Keep field names in sync with src/Bridge/ on the plugin side.

    [Serializable]
    internal class BridgeEndpoint
    {
        public int protocol;
        public int port;
        public string token;
    }

    [Serializable]
    internal class MessageHeader
    {
        public string type;
    }

    [Serializable]
    internal class HelloMessage
    {
        public string type = "hello";
        public int protocol;
        public string token;
        public int pid;
        public string project;
        public string projectPath;
        public string unityVersion;
        public string packageVersion;
        public string[] commands;
    }

    [Serializable]
    internal class StateMessage
    {
        public string type = "state";
        public bool focused;
        public bool playing;
        public bool paused;
        public bool compiling;
        public bool compileFailed;
        public int errors;
        public int warnings;
        public string tool;
        public string pivotMode;
        public string pivotRotation;
        public bool sceneView2D;
        public string drawMode;
        public bool vertexSnap;
        public bool gridSnap;
        public bool isolated;
        public bool viewLocked;
        public bool overlays;
        public bool profilerRecording;

        // About the active GameObject: -1 nothing selected, 0 no, 1 yes.
        public int selectionActive;
        public int selectionHidden;
        public int selectionUnpickable;
    }

    [Serializable]
    internal class ReloadingMessage
    {
        public string type = "reloading";
    }

    [Serializable]
    internal class CommandMessage
    {
        public string type;
        public string id;
    }
}
