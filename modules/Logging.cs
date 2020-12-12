using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Logging
    {
        private const string LOGPATH = @"..\..\log.txt";
        private readonly string[] recentLogs = new string[256];
        private uint commandIndex = 0;
        private StreamWriter log = new StreamWriter(LOGPATH, true);
        private byte recentIndex = 0;
        private bool recentIndexRollover = false;

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

        public Task Log(SocketMessage src)
        {
            ToLogs($"{commandIndex}: [{src.Timestamp.DateTime}] {src.Author.Username}({src.Author.Id}): \"{src.Content}\"");
            commandIndex++;
            return Task.CompletedTask;
        }

        public Task Log(string str)
        {
            ToLogs(str);
            return Task.CompletedTask;
        }

        public Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public Task LogtoConsoleandFile(string str)
        {
            Console.WriteLine(str);
            Log(str);
            return Task.CompletedTask;
        }

        private Task ToLogs(string str)
        {
            log.WriteLineAsync(str);
            ToRecentLogs(str);
            log.Close();
            log = new StreamWriter(LOGPATH, true);
            return Task.CompletedTask;
        }

        private Task ToRecentLogs(string str)
        {
            if (recentIndex == 255)
                recentIndexRollover = true;

            recentLogs[recentIndex] = str;
            recentIndex++;

            return Task.CompletedTask;
        }
    }
}