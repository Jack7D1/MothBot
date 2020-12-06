using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Logging
    {
        private readonly StreamWriter _log = new StreamWriter(@"..\..\log.txt", true);
        private readonly string[] recentLogs = new string[256];
        private uint commandIndex = 0;
        private byte recentIndex = 0;
        private bool recentIndexRollover = false;

        public void Close()
        {
            _log.Close();
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

        public Task Log(SocketMessage src)
        {
            _log.WriteLineAsync($"{commandIndex}: [{src.Timestamp.DateTime}] {src.Author.Username}({src.Author.Id}): \"{src.Content}\"");
            commandIndex++;
            ToIndex(src.Content);
            return Task.CompletedTask;
        }

        public Task Log(string str)
        {
            _log.WriteLineAsync(str);
            ToIndex(str);
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
            _log.WriteLineAsync(str);
            ToIndex(str);
            return Task.CompletedTask;
        }

        private Task ToIndex(string str)
        {
            if (recentIndex == 255)
                recentIndexRollover = true;

            recentLogs[recentIndex] = str;
            recentIndex++;

            return Task.CompletedTask;
        }
    }
}