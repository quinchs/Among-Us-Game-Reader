using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsReader
{
    public partial class GameReader
    {
        public event EventHandler<AmongUsState> GameStarted;
        public event EventHandler<AmongUsState> MeetingCalled;
        public event EventHandler<PlayerDiedEventArgs> PlayerDied;
        public event EventHandler<AmongUsState> GameEnded;
        public event EventHandler<PlayerJoinedEventArgs> PlayerJoined;
        public event EventHandler<PlayerLeftEventArgs> PlayerLeft;

        // For voip related stuff
        public event EventHandler<bool> VoipSettingUpdate;
    }
    public class PlayerLeftEventArgs
    {
        public AmongUsState GameState;
        public Player Player;
    }
    public class PlayerJoinedEventArgs
    {
        public AmongUsState GameState;
        public Player Player;
    }
    public class PlayerDiedEventArgs
    {
        public AmongUsState GameState;
        public Player Player;
    }
}
