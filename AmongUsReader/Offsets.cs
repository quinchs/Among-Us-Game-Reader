using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AmongUsReader
{
    public class OffsetReader
    {
        public static Root ReadOffsets(string dllHash)
        {
            string json = File.ReadAllText($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}Offsets.json");
            var data = JsonConvert.DeserializeObject<Dictionary<string, Root>>(json);

            if(data.TryGetValue(dllHash, out var item))
            {
                return item;
            }
            else
            {
                Logger.Write("Unsupported version running! unable to attach to among us", Logger.Severity.Critical);
                return null;
            }
        }


        private static SHA256 Sha256 = SHA256.Create();
        public static string getHash(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                return Convert.ToBase64String(Sha256.ComputeHash(stream));
            }
        }
    }

    public class Struct
    {
        public string type { get; set; }
        public int skip { get; set; }
        public string name { get; set; }
    }

    public class PlayerData
    {
        public List<Struct> @struct { get; set; }
        public List<int> isLocal { get; set; }
        public List<int> localX { get; set; }
        public List<int> localY { get; set; }
        public List<int> remoteX { get; set; }
        public List<int> remoteY { get; set; }
        public int bufferLength { get; set; }
        public List<int> offsets { get; set; }
        public List<int> inVent { get; set; }
    }

    public class Offsets
    {
        public List<int> meetingHud { get; set; }
        public List<int> meetingHudCachePtr { get; set; }
        public List<int> meetingHudState { get; set; }
        public List<int> gameState { get; set; }
        public List<int> hostId { get; set; }
        public List<int> clientId { get; set; }
        public List<int> allPlayersPtr { get; set; }
        public List<int> allPlayers { get; set; }
        public List<int> playerCount { get; set; }
        public int playerAddrPtr { get; set; }
        public List<int> exiledPlayerId { get; set; }
        public List<int> gameCode { get; set; }
        public PlayerData player { get; set; }
    }

    public class Root
    {
        public string versionNumber { get; set; }
        public string versionSource { get; set; }
        public Offsets offsets { get; set; }
    }


    

}
