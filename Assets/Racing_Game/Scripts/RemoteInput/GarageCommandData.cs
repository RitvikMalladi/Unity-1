//──────────────────────────────────────────────────────────────
// GarageCommandData.cs
// Shared command packet for garage / menu navigation.
// Used by GarageRemoteController (game side) and
// GarageControllerUI (controller phone side).
//
// Packet layout (2 bytes):
//   [0]  byte  command   — GarageCommand enum value
//   [1]  byte  param     — optional integer parameter (e.g. level ID)
//──────────────────────────────────────────────────────────────

using System;

namespace ALIyerEdon.RemoteInput
{
    public enum GarageCommand : byte
    {
        None        = 0,
        NextCar     = 1,   // browse to next car
        PrevCar     = 2,   // browse to previous car
        SelectCar   = 3,   // confirm / play with this car
        BuyCar      = 4,   // purchase current car
        SelectLevel = 5,   // param = level index
        NextLevel   = 6,   // browse to next level
        PrevLevel   = 7,   // browse to previous level
        Back        = 8,   // close popup / go back
        Confirm     = 9,   // generic confirm / ok
        SelectMode  = 10,  // param = 0:Sport 1:Truck 2:F1 3:Offroad
    }

    public struct GarageCommandData
    {
        public const int PacketSize = 2;

        public GarageCommand command;
        public byte          param;      // level ID or 0

        public byte[] Serialize()
        {
            return new byte[] { (byte)command, param };
        }

        public static bool TryDeserialize(byte[] buf, int len, out GarageCommandData result)
        {
            result = default;
            if (buf == null || len < PacketSize) return false;
            result.command = (GarageCommand)buf[0];
            result.param   = buf[1];
            return true;
        }
    }
}
