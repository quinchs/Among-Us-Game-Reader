using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsReader
{
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
    public struct PlayerInfo
    {
        [System.Runtime.InteropServices.FieldOffset(8)]  public byte PlayerId;
        [System.Runtime.InteropServices.FieldOffset(12)] public uint PlayerName;
        [System.Runtime.InteropServices.FieldOffset(16)] public byte ColorId;
        [System.Runtime.InteropServices.FieldOffset(20)] public uint HatId;
        [System.Runtime.InteropServices.FieldOffset(24)] public uint PetId;
        [System.Runtime.InteropServices.FieldOffset(28)] public uint SkinId;
        [System.Runtime.InteropServices.FieldOffset(32)] public byte Disconnected;
        [System.Runtime.InteropServices.FieldOffset(36)] public IntPtr Tasks;
        [System.Runtime.InteropServices.FieldOffset(40)] public byte IsImpostor;
        [System.Runtime.InteropServices.FieldOffset(41)] public byte IsDead;
        [System.Runtime.InteropServices.FieldOffset(44)] public IntPtr _object;
    }
}
