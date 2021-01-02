using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AmongUsReader
{
    class Program
    {
        private static GameReader reader;
        private static Swissbot swissbot;
        static void Main(string[] args)
        {
            new Program().Run().GetAwaiter().GetResult();
        }

        public async Task Run()
        {
            Logger.Create();
            Logger.Write("Starting init of services...", Logger.Severity.Core);

            reader = new GameReader();

            await  reader.Attach();

            reader.Start();

            Logger.Write("Starting swissbot connections...", Logger.Severity.Core);

            swissbot = new Swissbot();

            await Task.Delay(-1);
        }
    }
}
