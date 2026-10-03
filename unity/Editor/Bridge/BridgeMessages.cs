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
        public string activeScene;
        public float timeScale;
        public string layout;

        // Bit n is set when camera bookmark n exists for the active scene.
        public int bookmarks;

        public TestRunState editModeTests = new TestRunState();
        public TestRunState playModeTests = new TestRunState();

        // About the active GameObject: -1 nothing selected, 0 no, 1 yes.
        public int selectionActive;
        public int selectionHidden;
        public int selectionUnpickable;
    }

    [Serializable]
    internal class TestRunState
    {
        public const int None = 0;
        public const int Running = 1;
        public const int Finished = 2;

        public int status;

        // Finished runs this editor session; a run shorter than one state update still bumps it.
        public int runs;
        public int total;
        public int done;
        public int passed;
        public int failed;
        public int skipped;
    }

    // One list for a dynamic folder or an action editor: scenes, tools, layouts, recent, favorites, menus.
    [Serializable]
    internal class ListMessage
    {
        public string type = "list";
        public string name;
        public ListItem[] items;
    }

    [Serializable]
    internal class ListItem
    {
        public string id;
        public string label;
        public string icon;
        public string group;
    }

    [Serializable]
    internal class ReloadingMessage
    {
        public string type = "reloading";

        // Entering Play Mode reloads the domain too; the plugin must not take that for a compilation.
        public bool playMode;
    }

    // Everything the plugin asks for; each type uses the fields it needs.
    [Serializable]
    internal class CommandMessage
    {
        public string type;
        public string id;
        public string path;
        public float value;
        public int index;
        public bool save;
    }
}
