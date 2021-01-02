using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace AmongUsReader
{
    public class Player
    {
        public byte Id { get; set; }
        public string Name { get; set; }
        public byte ColorId { get; set; }
        public uint HatId { get; set; }
        public uint PetId { get; set; }
        public uint SkinId { get; set; }
        public bool Disconnected { get; set; }
        public bool IsImposter { get; set; }
        public bool IsDead { get; set; }
        public bool InVent { get; set; }

        public float X { get; set; }
        public float Y { get; set; }

        internal IntPtr TaskPtr { get; set; }
        internal IntPtr Ptr { get; set; }
        internal IntPtr ObjectPtr { get; set; }
        internal bool IsLocal { get; set; }

        public Player(byte[] data, GameReader reader)
        {
            GCHandle gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var plr = (PlayerInfo)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(PlayerInfo));
            gcHandle.Free();

            this.ColorId = plr.ColorId;
            this.Disconnected = plr.Disconnected == 1;
            this.HatId = plr.HatId;
            this.Name = reader.ReadString((IntPtr)plr.PlayerName);
            this.Id = plr.PlayerId;
            this.IsImposter = plr.IsImpostor == 1;
            this.IsDead = plr.IsDead == 1;

            this.ObjectPtr = plr._object;
            this.TaskPtr = plr.Tasks;

            var nullRef = ObjectPtr == IntPtr.Zero;

            this.IsLocal = nullRef ? false : reader.ReadMemory<int>(this.ObjectPtr, reader.Offsets.offsets.player.isLocal) != 0;

            var posOffsets = IsLocal ? new List<List<int>>
            {
                 reader.Offsets.offsets.player.localX,
                 reader.Offsets.offsets.player.localY
            } : new List<List<int>>
            {
                 reader.Offsets.offsets.player.remoteX,
                 reader.Offsets.offsets.player.remoteY
            };

            this.X = nullRef ? 0 :reader.ReadMemory<float>(this.ObjectPtr, posOffsets[0]);
            this.Y = nullRef ? 0 :reader.ReadMemory<float>(this.ObjectPtr, posOffsets[1]);
        }

        public override string ToString()
        {
            return $"Id: {this.Id} - {this.Name}";
        }
    }
}
