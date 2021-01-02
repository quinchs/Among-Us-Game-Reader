using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AmongUsReader
{
    public class AmongUsState
    {
        public GameState State { get; private set; }
        public string GameCode { get; }
        public List<Player> Players { get; } = new List<Player>();
        public bool IsHost { get; }
        public uint ClientId { get; }
        public uint HostId { get; }
        public int PlayerCount { get; }
        public int Imposters { get; }
        public int Crewmates { get; }
        public int DeadPlayers { get; }

        internal bool ExiledCausesEnd = false;

        internal uint allPlayers;

        public bool InGame
            => this.State == GameState.Tasks
            || this.State == GameState.Lobby
            || this.State == GameState.Discussion;

        public bool ConnectedToServer
            => this.GameCode != null || this.State != GameState.Menu;

        internal bool CanTalk
           => this.State == GameState.Discussion
           || this.State == GameState.Lobby
           || this.State == GameState.Menu;


        public AmongUsState(GameReader reader)
        {
            // Game state
            var st = reader.GameState;
            switch (st)
            {
                case 0:
                    this.State = GameState.Menu;
                    ExiledCausesEnd = false;
                    break;
                case 1 or 3:
                    this.State = GameState.Lobby;
                    ExiledCausesEnd = false;
                    break;
                default:
                    if (this.ExiledCausesEnd)
                        State = GameState.Lobby;
                    else if (reader.MeetingHudState < 4)
                        State = GameState.Discussion;
                    else State = GameState.Tasks;
                    break;

            }

            // GameCode
            this.GameCode = reader.GameCode;

            // Player count
            this.PlayerCount = reader.PlayerCount;

            // Host
            this.HostId = reader.ReadMemory<uint>(reader.GameAssemblyAddr, reader.Offsets.offsets.hostId);

            // Client
            this.ClientId = reader.ReadMemory<uint>(reader.GameAssemblyAddr, reader.Offsets.offsets.clientId);

            this.IsHost = HostId == ClientId;

            // players
            var PlayerAddAdr = reader.AllPlayers + (uint)reader.Offsets.offsets.playerAddrPtr;
            for(int i = 0; i != this.PlayerCount; i++)
            {
                var adr = reader.OffsetAddress((IntPtr)PlayerAddAdr, reader.Offsets.offsets.player.offsets);

                byte[] buffer = new byte[Marshal.SizeOf<PlayerInfo>()];
                GameReader.ReadProcessMemory(reader.amongUsHandle, adr.address + adr.last, buffer, buffer.Length, out var read);

                var plr = new Player(buffer, reader);
                
                this.Players.Add(plr);

                if (plr.IsImposter)
                    this.Imposters++;
                else
                    this.Crewmates++;

                if (plr.IsDead)
                    this.DeadPlayers++;

                PlayerAddAdr += 4;
            }

            if (Imposters == 0 || Imposters >= Crewmates)
            {
                this.ExiledCausesEnd = true;
                this.State = GameState.Lobby;
            }    
        }

        public void Update(AmongUsState previous, GameReader reader)
        {
            if(previous.State == GameState.Menu &&
                this.State == GameState.Lobby &&
                (previous.allPlayers == reader.AllPlayers || this.Players.Count == 1 || this.Players.Any(x => x.IsLocal)))
            {
                this.State = GameState.Menu;
            }

            allPlayers = reader.AllPlayers;
        }
    }
}
