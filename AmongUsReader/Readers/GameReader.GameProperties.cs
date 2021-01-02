using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AmongUsReader
{
    public partial class GameReader
    {
        public int GameState
           => this.ReadMemory<int>(this.GameAssemblyAddr, Offsets.offsets.gameState);

        public IntPtr MeetingHud
            => this.ReadMemory<IntPtr>(this.GameAssemblyAddr, Offsets.offsets.meetingHud);

        public uint MeetingHudCachePtr
            => MeetingHud == IntPtr.Zero ? 0 : this.ReadMemory<uint>((IntPtr)MeetingHud, Offsets.offsets.meetingHudCachePtr);

        public int MeetingHudState
            => MeetingHudCachePtr == 0 ? 4 : this.ReadMemory<int>((IntPtr)MeetingHud, Offsets.offsets.meetingHudState);

        public IntPtr AllPlayersPtr
            => (IntPtr)this.ReadMemory<int>(this.GameAssemblyAddr, Offsets.offsets.allPlayersPtr);

        public uint AllPlayers
            => this.ReadMemory<uint>((IntPtr)AllPlayersPtr, Offsets.offsets.allPlayers);

        public int PlayerCount
            => this.ReadMemory<int>((IntPtr)AllPlayersPtr, Offsets.offsets.playerCount);

        public string GameCode
            => GetGameCode();

        private string GetGameCode()
        {
            var addr = (IntPtr)this.ReadMemory<int>(this.GameAssemblyAddr, Offsets.offsets.gameCode);

            if (addr == IntPtr.Zero)
                return null;

            var rawCode = this.ReadString(addr);

            string code = null;

            if (rawCode == null)
                return code;

            var data = rawCode.Split("\r\n");


            if (data.Length == 2)
            {
                code = data[1];
            }

            if (code == null)
                return null;
            if (Regex.IsMatch(code, @"^[A-Z]{6}$"))
            {
                return code;
            }
            else return null;
        }

        public bool IsCrewMateWin(AmongUsState state)
        {
            if (state.Players.Where(x => x.IsImposter).Count() == 0)
                return true;

            return false;
        }

    }
}
