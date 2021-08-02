using Discord;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Logging
    {
        public const string PATH_LOGS = "../../data/log.txt";
        private static readonly StreamWriter log = new StreamWriter(PATH_LOGS, true);

        public Logging()
        {
            if (!Directory.Exists(PATH_LOGS))
                Directory.CreateDirectory(PATH_LOGS.Substring(0, PATH_LOGS.LastIndexOf('/')));
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
        }

        public static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public static Task Log(string str)
        {
            log.WriteLineAsync(str);
            log.Flush();
            return Task.CompletedTask;
        }

        public static async Task LogAsync(string str)
        {
            await log.WriteLineAsync(str);
            log.Flush();
        }

        public static Task LogtoConsoleandFile(string str)
        {
            Console.WriteLine(str);
            Log(str);
            return Task.CompletedTask;
        }

        public static async Task LogtoConsoleandFileAsync(string str)
        {
            Console.WriteLine(str);
            await LogAsync(str);
        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            Log($"System shutdown at [{System.DateTime.UtcNow}]");
        }
    }
}