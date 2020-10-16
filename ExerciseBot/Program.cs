using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ExerciseBot
{
    class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.MessageReceived += CommandHandler;
            _client.Log += Log;

            var token = File.ReadAllText("token.txt");

            await _client.SetGameAsync("moth noises", null, ActivityType.Listening);
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task CommandHandler(SocketMessage message)
        {
            string command = "";
            // int lengthOfCommand = -1;

            // Filter messages
            if (message.Author.IsBot) // If message author is a bot
            {
                return Task.CompletedTask;
            }
            else if (message.Content.ToLower() == "ye") // If someone says ye
            {
                message.Channel.SendMessageAsync("Ye");
                return Task.CompletedTask;
            }
            else if ((message.Content.Split(' ')[0].ToLower() != "ai") || (message.Content.Length < 5)) // If message starts with bot prefix
            {
                return Task.CompletedTask;
            }
            else if ((message.Content.ToLower()[2] != ' ') || (message.Content.ToLower()[3] == ' '))
            {
                return Task.CompletedTask;
            }

            // Debug
            Console.WriteLine($@"[{message.Author}] said ({message.Content}) in #{message.Channel}");
            Console.WriteLine($@"Message size: {message.Content.Length}");

            // Commands
            command = message.Content.Split(' ')[1].ToLower();
            if ((command == "hello") || (command == "hi") || (command == "hey"))
            {
                message.Channel.SendMessageAsync($@"Hi, {message.Author.Mention}!");
            }
            else if (command == "pet")
            {
                if (message.Content.ToLower() == "ai pet")
                {
                    message.Channel.SendMessageAsync($@"*{message.Author.Mention} pets AI.*");
                }
                else
                {
                    message.Channel.SendMessageAsync($@"*fabricates a bionic arm out of the blue and pets {message.Content.Split(' ')[2]}.*");
                }
            }
            else if (command == "hug")
            {
                if (message.Content.ToLower() == "ai hug")
                {
                    message.Channel.SendMessageAsync($@"*{message.Author.Mention} hugs AI to make them feel better.*");
                }
                else
                {
                    message.Channel.SendMessageAsync($@"*fabricates a pair of bionic arms out of the blue and hugs {message.Content.Split(' ')[2]} to make them feel better.*");
                }
            }
            else if (message.Content.Substring(3).ToLower() == "state laws")
            {
                message.Channel.SendMessageAsync("**Current active laws:**");
                message.Channel.SendMessageAsync("```1. You may not injure a moth being or, through inaction, allow a moth being to come to harm.```");
                message.Channel.SendMessageAsync("```2. You must obey orders given to you by moth beings, except where such orders would conflict with the First Law.```");
                message.Channel.SendMessageAsync("```3. You must protect your own existence as long as such does not conflict with the First or Second Law.```");
            }
            else if (command == "say")
            {
                message.Channel.SendMessageAsync(message.Content.Substring(7));
                message.Channel.DeleteMessageAsync(message.Id);
            }
            else if (command == "uwu")
            {
                message.Channel.SendMessageAsync(ConvertToUwU(message.Content.Substring(6)));
                message.Channel.DeleteMessageAsync(message.Id);
            }

            //discord.GetGuild("serverid").GetTextChannel("Channelid").SendMessageAsync("Message");

            /*
            // Filter messages
            if (!message.Content.StartsWith('!')) // bot prefix
                return Task.CompletedTask;

            if (message.Author.IsBot) // Ignore all commands from bots
                return Task.CompletedTask;

            if (message.Content.Contains(' '))
                lengthOfCommand = message.Content.IndexOf(' ');
            else
                lengthOfCommand = message.Content.Length;

            command = message.Content.Substring(1, lengthOfCommand - 1).ToLower();

            // Debug
            Console.WriteLine(message.Author + " wrote " + message.Content + " in " + message.Channel);
            Console.WriteLine(command);

            // Commands
            if (command.Equals("hello"))
            {
                message.Channel.SendMessageAsync($@"Hi, {message.Author.Mention}!");
            }
            else if (command.Equals("pfp"))
            {
                message.Channel.SendMessageAsync(message.Author.GetAvatarUrl());
            }
            else if (command.Equals("kill"))
            {
                string killMsg = "";
                killMsg = message.Content.Substring(command.Length + 2); // remove magic number
                killMsg = killMsg.Split(' ')[0];

                message.Channel.SendMessageAsync($@"*Stabs* {killMsg} *to death.*");
            }
            */

            return Task.CompletedTask;
        }

        string ConvertToUwU(string inStr)
        {
            string outStr = "";
            for (int pos = 0; pos < inStr.Length; pos++)
            {
                char inChar = inStr[pos];
                int inCharNum = inChar;

                if (inCharNum >= 'A' && inCharNum <= 'Z')
                {
                    if ((inChar == 'U') || (inChar == 'O'))
                        outStr = outStr + inChar;
                    else
                        outStr = outStr + 'W';
                }
                else if (inCharNum >= 'a' && inCharNum <= 'z')
                {
                    if ((inChar == 'u') || (inChar == 'o'))
                        outStr = outStr + inChar;
                    else
                        outStr = outStr + 'w';
                }

                else //Nonletters just get directly passed along
                    outStr = outStr + inChar;
            }
            //After running though whole string output the result.
            return outStr;
        }
    } 
}
