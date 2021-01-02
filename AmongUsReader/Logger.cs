using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AmongUsReader
{
    public class Logger
    {
        public static void Create()
        {
            _logEvent += Logger__logEvent;
           // Task.Run(() => { while (true) { DrawCard } });
        }

        private static void Logger__logEvent(object sender, (object data, Severity[] sev) e)
        {
            _queue.Enqueue(new KeyValuePair<object, Severity[]>(e.data, e.sev));
            if (_queue.Count > 0 && !inProg)
            {
                inProg = true;
                HandleQueueWrite();
            }
        }

        private static event EventHandler<(object data, Severity[] sev)> _logEvent;

        public enum Severity
        {
            Log,
            Error,
            Warning,
            Critical,
            Game,
            Socket,
            Core
        }
        private static ConcurrentQueue<KeyValuePair<object, Severity[]>> _queue = new ConcurrentQueue<KeyValuePair<object, Severity[]>>();

        private static string RightCard = "";

        public static void Write(object data, Severity sev = Severity.Log)
            => _logEvent?.Invoke(null, (data, new Severity[] { sev }));
        public static void Write(object data, params Severity[] sevs)
            => _logEvent?.Invoke(null, (data, sevs));

        static bool inProg = false;

        private static Dictionary<Severity, ConsoleColor> SeverityColorParser = new Dictionary<Severity, ConsoleColor>()
        {
            { Severity.Log, ConsoleColor.Green },
            { Severity.Error, ConsoleColor.Red },
            { Severity.Warning, ConsoleColor.Yellow },
            { Severity.Critical, ConsoleColor.DarkRed },
            { Severity.Game, ConsoleColor.Cyan },
            { Severity.Socket, ConsoleColor.Gray },
            { Severity.Core, ConsoleColor.Blue }

        };

        private static int WriteIndex = 0;
        private static void HandleQueueWrite()
        {
            while (_queue.Count > 0)
            {
                if (_queue.TryDequeue(out var res))
                {
                    var sev = res.Value;
                    var data = res.Key;

                    var enumsWithColors = "";
                    foreach (var item in sev)
                    {
                        if (enumsWithColors == "")
                            enumsWithColors = $"<{(int)SeverityColorParser[item]}>{item}</{(int)SeverityColorParser[item]}>";
                        else
                            enumsWithColors += $" -> <{(int)SeverityColorParser[item]}>{item}</{(int)SeverityColorParser[item]}>";
                    }

                    var items = ProcessColors($"{DateTime.UtcNow.ToString("O")} " + $"[{enumsWithColors}] - {data}");

                    foreach (var item in items)
                    {
                        Console.ForegroundColor = item.color;
                        Console.Write(item.value);
                        
                    }
                    Console.Write("\n");
                }
            }
            inProg = false;
        }

        private static Regex ColorRegex = new Regex(@"<(.*)>(.*?)<\/\1>");
        private static List<(ConsoleColor color, string value)> ProcessColors(string input)
        {
            var returnData = new List<(ConsoleColor color, string value)>();

            var mtch = ColorRegex.Matches(input);

            if(mtch.Count == 0)
            {
                returnData.Add((ConsoleColor.White, input));
                return returnData;
            }

            for(int i = 0; i != mtch.Count; i++)
            {
                var match = mtch[i];
                var color = GetColor(match.Groups[1].Value) ?? ConsoleColor.White;

                if (i == 0)
                {
                    if(match.Index != 0)
                    {
                        returnData.Add((ConsoleColor.White, new string(input.Take(match.Index).ToArray())));
                    }
                    returnData.Add((color, match.Groups[2].Value));
                }
                else
                {
                    var previousMatch = mtch[i - 1];
                    var start = previousMatch.Index + previousMatch.Length;
                    var end = match.Index;

                    returnData.Add((ConsoleColor.White, new string(input.Skip(start).Take(end - start).ToArray())));

                    returnData.Add((color, match.Groups[2].Value));
                }

                if (i + 1 == mtch.Count)
                {
                    // check remainder
                    if (match.Index + match.Length < input.Length)
                    {
                        returnData.Add((ConsoleColor.White, new string(input.Skip(match.Index + match.Length).ToArray())));
                    }
                }
            }

            return returnData;
        }

        private static List<string> prevCard = new List<string>();
        private static void DrawCard(bool isNew = true)
        {
            if (string.IsNullOrEmpty(RightCard))
                return;

            if (Console.BufferWidth <= 19)
                return;

            List<string> items;

            if (!isNew)
                items = prevCard;
            else
            {
                items = Split(RightCard, 15);
                prevCard = items;
            }

            if (items.Count + 1 >= Console.BufferHeight)
                return;

            var count = items.Count > 3 ? items.Count : 3;
            Console.SetCursorPosition(Console.BufferWidth - 17, 0);
            Console.Write($"|  ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Status".PadRight(15));
            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i != count; i++)
            {
                var item = items.Count - 1 < i ? "" : items[i];

                Console.SetCursorPosition(Console.BufferWidth - 17, i + 1);
                Console.Write($"| {item}".PadRight(15));
            }
            Console.SetCursorPosition(Console.BufferWidth - 17, count + 1);
            Console.Write("+".PadRight(16, '-'));
        }

        static List<string> Split(string str, int chunkSize)
        {
            if (str.Length <= chunkSize)
                return new List<string>() { str };

            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize)).ToList();
        }

        public static void WriteRightCard(string cardText)
        {
            RightCard = cardText;
           // DrawCard();
        }

        private static ConsoleColor? GetColor(string tag)
        {
            if (Enum.TryParse(typeof(ConsoleColor), tag, true, out var res))
            {
                return (ConsoleColor)res;
            }
            else
            {
                return null;
            }
        }
    }
}
