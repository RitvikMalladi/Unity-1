//──────────────────────────────────────────────────────────────
// GarageCommandData.cs
// Shared garage/menu-navigation message — sent by the Controller
// app and read by the Game app as a JSON text frame over the
// relay WebSocket (see RelayLink.cs / RelayServer/).
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

    [Serializable]
    public struct GarageCommandData
    {
        public string t;       // always "garage"
        public byte   command; // GarageCommand enum value
        public byte   param;   // level ID, mode ID, or 0
    }
}
