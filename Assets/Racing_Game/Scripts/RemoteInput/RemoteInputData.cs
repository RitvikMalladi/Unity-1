//──────────────────────────────────────────────────────────────
// RemoteInputData.cs
// Shared drive-input message — sent by the Controller app and
// read by the Game app as a JSON text frame over the relay
// WebSocket (see RelayLink.cs / RelayServer/).
//──────────────────────────────────────────────────────────────

using System;

namespace ALIyerEdon.RemoteInput
{
    [Serializable]
    public struct RemoteInputData
    {
        public string t;          // always "drive"
        public float  motor;      // -1 … +1
        public float  steer;      // -1 … +1
        public bool   handBrake;
        public bool   nitro;
    }
}
