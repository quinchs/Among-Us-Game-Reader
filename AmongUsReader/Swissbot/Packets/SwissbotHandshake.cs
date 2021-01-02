using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsReader.Packets
{
    public class SwissbotHandshake : ISendable
    {
        public string type { get; } = "amongus_reader";

        public string auth { get; } = Swissbot.SwissbotAuthToken;

        public string session { get; } = Swissbot.UserAuth;
    }
    public class HandshakeResult : IRecieveable
    {
        public string type { get; set; }
        public bool valid { get; set; }
        public string user { get; set; }
        public string reason { get; set; }
    }
}
