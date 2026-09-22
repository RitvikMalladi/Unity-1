//──────────────────────────────────────────────────────────────
// RemoteInputData.cs
// Shared packet — Game APK and Controller APK both use this.
//
// Packet layout v2 (10 bytes, little-endian):
//   [0..3]  float  motor      -1.0 (brake/reverse) → +1.0 (throttle)
//   [4..7]  float  steer      -1.0 (left)           → +1.0 (right)
//   [8]     byte   buttons    bit-0 = handBrake
//                             bit-1 = nitro
//   [9]     byte   version    always 2
//
// v1 packets (9 bytes, no version byte) are still accepted —
// handBrake is read from byte[8] bit-0, nitro defaults to false.
//──────────────────────────────────────────────────────────────

using System;

namespace ALIyerEdon.RemoteInput
{
    public struct RemoteInputData
    {
        // ── Packet constants ──────────────────────────────────
        public const int PacketSize   = 10;   // v2
        public const int PacketSizeV1 = 9;    // legacy
        public const byte Version     = 2;

        // ── Fields ────────────────────────────────────────────
        public float motor;       // -1 … +1
        public float steer;       // -1 … +1
        public bool  handBrake;
        public bool  nitro;

        // ── Serialize ─────────────────────────────────────────
        /// <summary>Returns a 10-byte v2 packet ready to send over UDP.</summary>
        public byte[] Serialize()
        {
            byte[] buffer = new byte[PacketSize];

            Buffer.BlockCopy(BitConverter.GetBytes(motor), 0, buffer, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(steer), 0, buffer, 4, 4);

            byte buttons = 0;
            if (handBrake) buttons |= 1;
            if (nitro)     buttons |= 2;
            buffer[8] = buttons;
            buffer[9] = Version;

            return buffer;
        }

        // ── Deserialize ───────────────────────────────────────
        /// <summary>
        /// Parses a raw UDP payload.  Accepts both v1 (9 bytes) and v2 (10 bytes).
        /// Returns false when the buffer is too short to be valid.
        /// </summary>
        public static bool TryDeserialize(byte[] buffer, int length,
                                          out RemoteInputData result)
        {
            result = default;

            if (buffer == null || length < PacketSizeV1)
                return false;

            result.motor = BitConverter.ToSingle(buffer, 0);
            result.steer = BitConverter.ToSingle(buffer, 4);

            // Guard NaN / Infinity
            if (float.IsNaN(result.motor)  || float.IsInfinity(result.motor))  result.motor = 0f;
            if (float.IsNaN(result.steer)  || float.IsInfinity(result.steer))  result.steer = 0f;

            // Clamp to valid range
            result.motor = Math.Max(-1f, Math.Min(1f, result.motor));
            result.steer = Math.Max(-1f, Math.Min(1f, result.steer));

            if (length >= PacketSize)
            {
                // v2 – buttons byte
                byte buttons   = buffer[8];
                result.handBrake = (buttons & 1) != 0;
                result.nitro     = (buttons & 2) != 0;
            }
            else
            {
                // v1 legacy – byte 8 was 0/1 for handBrake only
                result.handBrake = buffer[8] != 0;
                result.nitro     = false;
            }

            return true;
        }
    }
}
