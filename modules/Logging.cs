using Discord;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal static class Logging
    {
        public const string PATH_LOGS = "../../data/log.txt";

        static Logging()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
        }

        public static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public static Task Log(string str)
        {
            File.AppendAllText(PATH_LOGS, $"{str}\n");
            return Task.CompletedTask;
        }

        public static async Task LogAsync(string str)
        {
            await File.AppendAllTextAsync(PATH_LOGS, $"{str}\n");
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

        private static async void ProcessExit(object sender, EventArgs e)
        {
            await Log($"System shutdown at [{DateTime.UtcNow}]");
        }
    }
}