using Discord;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Logging
    {
        private static StreamWriter log;

        public Logging()
        {
            if (!Directory.Exists(Data.PATH_LOGS))
                Directory.CreateDirectory(Data.PATH_LOGS.Substring(0, Data.PATH_LOGS.LastIndexOf('/')));
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
        }

        public static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public static Task Log(string str)
        {
            OpenLogs();
            log.WriteLineAsync(str);
            log.Close();
            return Task.CompletedTask;
        }

        public static async Task LogAsync(string str)
        {
            OpenLogs();
            await log.WriteLineAsync(str);
            log.Close();
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

        private static void OpenLogs()
        {
            log = new StreamWriter(Data.PATH_LOGS, true);
        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            Log($"System shutdown at [{System.DateTime.UtcNow}]");
        }
    }
}