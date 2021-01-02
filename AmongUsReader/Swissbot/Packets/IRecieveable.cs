using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsReader.Packets
{
    public interface IRecieveable
    {
        string type { get; }
    }
    public class RawRecieveable : IRecieveable
    {
        public string type { get; set; }
    }
}
