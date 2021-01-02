using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsReader.Packets
{
    public class BrowserHandshake : IRecieveable
    {
        public string type { get; set; }
        public string session { get; set; }
    }
    public class BrowserHandshakeResult : IBrowserSendable
    {
        public string type { get; set; } = "handshake_result";

        public bool isValid { get; set; }
        public string reason { get; set; }

        public BrowserHandshakeResult Accept()
        {
            this.isValid = true;
            return this;
        }

        public BrowserHandshakeResult Deny(string reason)
        {
            this.isValid = false;
            this.reason = reason;
            return this;
        }
    }
}
