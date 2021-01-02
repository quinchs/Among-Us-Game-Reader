using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsReader.Packets
{
    public class ValidateCredentials : ISendable
    {
        public string type { get; } = "validate_credentials";

        public string auth { get; } = null;

        public string session { get; set; }

        public ValidateCredentials(string session)
        {
            this.session = session;
        }
    }
    public class CredentialResponse : IRecieveable
    {
        public string type { get; } = "validate_credentials_result";
        public bool isValid { get; set; }
        public string auth { get; set; }
    }
}
