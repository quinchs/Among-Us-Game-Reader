using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsReader.Packets
{
    public interface ISendable
    {
        string type { get; }
        string auth { get; }
        string session { get; }
    }
}
