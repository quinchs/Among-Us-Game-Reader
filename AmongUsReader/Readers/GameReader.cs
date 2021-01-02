using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsReader
{
    public partial class GameReader
    {
        public bool IsAmongUsRunning
            => checkAmongUs();

        public bool IsAttached = false;

        internal Root Offsets;

        internal IntPtr GameAssemblyAddr;
        internal IntPtr AmongUsAddr;

        internal bool isReading;

        internal IntPtr amongUsHandle;
        private ModuleReader moduleReader;

        private bool checkAmongUs()
        {
            var procs = Process.GetProcessesByName("Among Us");
            return procs.Length > 0;
        }

        private Process GetAmongUs()
            => Process.GetProcessesByName("Among Us").FirstOrDefault();

        public GameReader()
        {
            moduleReader = new ModuleReader();
        }

        public async Task Attach()
        {
            while (!IsAttached)
            {
                if (IsAmongUsRunning)
                {
                    Logger.Write("Among us running! Starting to attach...", Logger.Severity.Core);

                    var amongUs = GetAmongUs();

                    amongUs.Exited += (object sender, EventArgs e) =>
                    {
                        IsAttached = false;
                        Logger.Write("Among us has exited, killing reading loop and waiting for process to start", Logger.Severity.Core);
                        Task.Run(() => Attach().ConfigureAwait(false));
                    };

                    amongUsHandle = OpenProcess(PROCESS_WM_READ, false, amongUs.Id);

                    // Get the GameAssembly module
                    var modules = moduleReader.CollectModules(amongUs);

                    //foreach (var mod in modules)
                    //    Console.WriteLine($"{mod.ModuleName} - 0x{mod.BaseAddress} / {mod.Size}");

                    var gameAssembly = modules.FirstOrDefault(x => x.ModuleName == "GameAssembly.dll");
                    var amongUsArr = modules.FirstOrDefault(x => x.ModuleName == "Among Us.exe");

                    this.GameAssemblyAddr = gameAssembly.BaseAddress;
                    this.AmongUsAddr = amongUsArr.BaseAddress;

                    Logger.Write($"Got process handle -> {amongUsHandle}", Logger.Severity.Core);

                    string gameAssemblyPath = amongUs.MainModule.FileName.Replace("Among Us.exe", "GameAssembly.dll");

                    string hashKey = OffsetReader.getHash(gameAssemblyPath);

                    Offsets = OffsetReader.ReadOffsets(hashKey);

                    if (Offsets == null)
                        return;

                    this.IsAttached = true;

                    if (!isReading)
                    {

                    }
                }
                else
                {
                    Logger.Write("Waiting for among us to start...", Logger.Severity.Core);
                    await Task.Delay(1000);
                }
            }
        }

        public void Start()
        {
            Task.Run(() => ReadLoop().ConfigureAwait(false));
        }

        private AmongUsState PreviousState;
        private async Task ReadLoop()
        {
            if (!this.IsAttached)
                return;

            if (!isReading)
                isReading = true;


            try
            {
                PreviousState = new AmongUsState(this);
                
                Logger.Write("Starting read..\n" +
                             "Game State:\n" +
                             $"  Game state: <Green>{PreviousState.State}</Green>\n" +
                             $"  Code: <Green>{PreviousState.GameCode}</Green>\n" +
                             $"  Players: <Green>{PreviousState.PlayerCount}</Green>:\n" +
                             $"   - " + string.Join("\n   - ", PreviousState.Players), Logger.Severity.Game

                );

                while (this.IsAttached)
                {
                    try
                    {
                        var currentState = new AmongUsState(this);
                        currentState.Update(PreviousState, this);

                        if(currentState.State != PreviousState.State)
                        {
                            Logger.Write($"Gamestate updated from <Gray>{PreviousState.State}</Gray> to <Green>{currentState.State}</Green>", Logger.Severity.Game);
                        }

                        if(currentState.ConnectedToServer != PreviousState.ConnectedToServer)
                        {
                            if (currentState.ConnectedToServer)
                            {
                                Logger.Write($"Connected to the game <Cyan>{currentState.GameCode}</Cyan> with <Cyan>{currentState.PlayerCount}</Cyan> players", Logger.Severity.Game);
                            }
                            else
                            {
                                Logger.Write($"Disconnected from the game with <Cyan>{PreviousState.Players.Count(x => !x.IsDead)}</Cyan> players", Logger.Severity.Game);
                            }
                        }

                        if (currentState.InGame != PreviousState.InGame)
                        {
                            if (currentState.InGame)
                            {
                                GameStarted?.Invoke(null, currentState);
                                Logger.Write($"Game <Cyan>{currentState.GameCode}</Cyan> <Green>Started</Green> with <Green>{currentState.PlayerCount}</Green> players.", Logger.Severity.Game);
                            }
                            else
                            {
                                if (currentState.ConnectedToServer)
                                {
                                    GameEnded?.Invoke(null, currentState);
                                    var isCrewmateWin = this.IsCrewMateWin(currentState);
                                    Logger.Write($"Game <Cyan>{currentState.GameCode}</Cyan> <Red>Ended</Red> with {(isCrewmateWin ? $"<Green>Crewmate Victory</Green>" : "<Red>Imposter Win</Red>")}", Logger.Severity.Game);
                                }

                            }
                        }

                        if (currentState.ConnectedToServer)
                        {
                            var Joined = currentState.Players.Select(x => (x.Name, x.Id)).Except(PreviousState.Players.Select(x => (x.Name, x.Id))).ToList();

                            if (Joined.Count > 0)
                            {
                                foreach (var item in Joined.Where(x => x.Name != ""))
                                {
                                    var newPlayer = currentState.Players.FirstOrDefault(x => x.Id == item.Id);
                                    PlayerJoined?.Invoke(null, new PlayerJoinedEventArgs()
                                    {
                                        GameState = currentState,
                                        Player = newPlayer
                                    });

                                    Logger.Write($"Player <Gray>{newPlayer.Id}: \'{newPlayer.Name}\'</Gray> joined <Cyan>{currentState.GameCode}</Cyan>", Logger.Severity.Game);
                                }
                            }


                            var LeftIngame = currentState.Players.Where(x =>
                            {
                                var plr = PreviousState.Players.FirstOrDefault(y => y.Id == x.Id);

                                if (plr == null)
                                    return false;

                                if (x.Disconnected && !plr.Disconnected)
                                    return true;
                                return false;
                            }).Select(x => x.Id);

                            var Left = PreviousState.Players.Select(x => x.Id).Except(currentState.Players.Select(x => x.Id)).Concat(LeftIngame).ToList();

                            if (Left.Count > 0)
                            {
                                foreach (var item in Left)
                                {
                                    var oldPlayer = PreviousState.Players.FirstOrDefault(x => x.Id == item);
                                    PlayerLeft?.Invoke(null, new PlayerLeftEventArgs()
                                    {
                                        GameState = currentState,
                                        Player = oldPlayer
                                    });

                                    Logger.Write($"Player <Gray>{oldPlayer.Id}: \'{oldPlayer.Name}\'</Gray> Left <Cyan>{currentState.GameCode}</Cyan>", Logger.Severity.Game);
                                }
                            }

                            if (PreviousState.CanTalk != currentState.CanTalk)
                            {
                                VoipSettingUpdate?.Invoke(null, currentState.CanTalk);
                                Logger.Write($"Voice state changed from {(PreviousState.CanTalk ? "<Green>Can talk</Green>" : "<Red>Cannot Talk</Red>")} to {(currentState.CanTalk ? "<Green>Can talk</Green>" : "<Red>Cannot Talk</Red>")}", Logger.Severity.Game);
                            }

                            if (currentState.State == AmongUsReader.GameState.Discussion && currentState.State != PreviousState.State)
                            {
                                MeetingCalled?.Invoke(null, currentState);
                                Logger.Write("Meeting called, Entered discussion", Logger.Severity.Game);
                            }
                        }

                        PreviousState = currentState;
                    }
                    catch(Exception x)
                    {
                        Logger.Write($"Error on read loop: {x}");
                    }
                    finally
                    {
                        await Task.Delay(50);
                    }
                }
            }
            catch(Exception x)
            {
                Logger.Write($"Fatal before read loop: {x}", Logger.Severity.Critical);
            }
            finally
            {
                isReading = false;
            }
        }
    }
}
