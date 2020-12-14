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
        private readonly string[] recentLogs = new string[256];
        private uint commandIndex = 0;
        private byte recentIndex = 0;
        private bool recentIndexRollover = false;

        public Logging()
        {
            if (!Directory.Exists(LOG_PATH))
                Directory.CreateDirectory(LOG_PATH.Substring(0, LOG_PATH.LastIndexOf('\\')));

            log = new StreamWriter(LOG_PATH, true);
        }

        public string[] GetLogs()
        {
            string[] outIndex = new string[256];
            if (recentIndexRollover)
            {
                for (byte i = 255; i >= recentIndex; i--)
                {
                    outIndex[i] = recentLogs[recentIndex - i];
                }
            }

            for (byte i = 0; i < recentIndex; i++)
            {
                outIndex[i] = recentLogs[i];
            }

            return outIndex;
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
            ToRecentLogs(str);
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
            ToRecentLogs(str);
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

        private void ToRecentLogs(string str)
        {
            if (recentIndex == 255)
            {
                recentIndexRollover = true;
                recentIndex = 0;
            }
            recentLogs[recentIndex] = str;
            recentIndex++;
        }
    }
}