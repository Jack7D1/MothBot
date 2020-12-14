using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Logging
    {
        private const string LOG_PATH = @"..\..\data\log.txt";
        private readonly StreamWriter log;
        private uint commandIndex = 0;

        public Logging()
        {
            if (!Directory.Exists(LOG_PATH))
                Directory.CreateDirectory(LOG_PATH.Substring(0, LOG_PATH.LastIndexOf('\\')));

            log = new StreamWriter(LOG_PATH, true);
        }

        public void Log(SocketMessage src)
        {
            Log($"{commandIndex}: [{src.Timestamp.DateTime}] {src.Author.Username}({src.Author.Id}): \"{src.Content}\"");
            commandIndex++;
        }

        public Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public void Log(string str)
        {
            log.WriteLineAsync(str);
            log.Flush();
        }

        public async Task LogAsync(SocketMessage src)
        {
            await LogAsync($"{commandIndex}: [{src.Timestamp.DateTime}] {src.Author.Username}({src.Author.Id}): \"{src.Content}\"");
            commandIndex++;
        }

        public async Task LogAsync(string str)
        {
            await log.WriteLineAsync(str);
            log.Flush();
        }

        public void LogtoConsoleandFile(string str)
        {
            Console.WriteLine(str);
            Log(str);
        }

        public async Task LogtoConsoleandFileAsync(string str)
        {
            Console.WriteLine(str);
            await LogAsync(str);
        }

    }
}