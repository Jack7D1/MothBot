using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Logging
    {
        public ulong logIndex = 0;
        private readonly StreamWriter _log = new StreamWriter(@"..\..\log.txt", true);

        public void Close()
        {
            _log.Close();
        }

        public Task Log(SocketMessage src)
        {
            _log.WriteLineAsync($"{logIndex}: [{src.Timestamp.DateTime}] {src.Author.Username}({src.Author.Id}): \"{src.Content}\"");
            logIndex++;
            return Task.CompletedTask;
        }

        public Task Log(string str)
        {
            _log.WriteLineAsync(str);
            return Task.CompletedTask;
        }

        public Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public Task LogConsoleAndFile(string str)
        {
            Console.WriteLine(str);
            _log.WriteLineAsync(str);
            return Task.CompletedTask;
        }
    }
}