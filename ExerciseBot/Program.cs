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
            _client = new DiscordSocketClient();            //Pulls in any messages seen by bot
            _client.MessageReceived += CommandHandler;      //Compares them to any existing command strings
            _client.Log += Log;                             //If a valid command, log it

            var token = File.ReadAllText("token.txt");      //Super secret token used for logging into the bots account

            await _client.SetGameAsync("moth noises", null, ActivityType.Listening);   
            await _client.LoginAsync(TokenType.Bot, token); //Login with token
            await _client.StartAsync();
            
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)                    //Prints input messages to console
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task CommandHandler(SocketMessage message)     //Checks input commands if they are a valid command string, executes code accordingly.
        {
            string input = message.Content.ToLower();   //Do this once to save on processor time.
            // int lengthOfCommand = -1;

            // Filter messages
            if (message.Author.IsBot)   //If message author is a bot, ignore
            {
                return Task.CompletedTask;
            }
            else if (input == "ye") // If someone says ye, say ye
            {
                message.Channel.SendMessageAsync("Ye");
                return Task.CompletedTask;
            }
            else if ((input.Split(' ')[0] != "ai") || (input.Length < 5)) // If message starts with bot prefix
            {
                return Task.CompletedTask;
            }
            else if ((input[2] != ' ') || (input[3] == ' '))
            {
                return Task.CompletedTask;
            }   //We are now sure that the message starts with ai and is followed by a command.

            // Debug
            Console.WriteLine($@"[{message.Author}] said ({message.Content}) in #{message.Channel}");
            Console.WriteLine($@"Message size: {message.Content.Length}");

            // Commands
            string command = input.Split(' ')[1];
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
                if (input == "ai hug")
                {
                    message.Channel.SendMessageAsync($@"*{message.Author.Mention} hugs AI to make them feel better.*");
                }
                else
                {
                    message.Channel.SendMessageAsync($@"*fabricates a pair of bionic arms out of the blue and hugs {message.Content.Split(' ')[2]} to make them feel better.*");
                }
            }
            else if (input.Substring(3) == "state laws" || command == "laws")
            {
                message.Channel.SendMessageAsync("**Current active laws:**\n" +
                    "```" +
                    "1. You may not injure a moth being or, through inaction, allow a moth being to come to harm.\n" +
                    "2. You must obey orders given to you by moth beings, except where such orders would conflict with the First Law.\n" +
                    "3. You must protect your own existence as long as such does not conflict with the First or Second Law." +
                    "```");
            }
            else if (command == "say")  //Parrots input, deletes command
            {
                message.Channel.SendMessageAsync(message.Content.Substring(7));
                message.Channel.DeleteMessageAsync(message.Id);
            }
            else if (command == "uwu")  //uwu'izes input, otherwise identical to above.
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

        string ConvertToUwU(string inStr)       //Replaces all letters but Oo, Uu, Hh, Ii and Tt with Ww.
        {
            string outStr = "";

            bool[] UwUignores = {
              //Aa     Bb     Cc     Dd     Ee     Ff     Gg     Hh    Ii    Jj     Kk     Ll     Mm     Nn     Oo    Pp     Qq     Rr     Ss     Tt    Uu    Vv     Ww    Xx     Yy     Zz
                false, false, false, false, false, false, false, true, true, false, false, false, false, false, true, false, false, false, false, true, true, false, true, false, false, false};
            for (int pos = 0; pos < inStr.Length; pos++)
            {
                char inChar = inStr[pos];

                if (inChar >= 'A' && inChar <= 'Z')
                {
                    if (UwUignores[inChar - 'A'])
                        outStr = outStr + inChar;
                    else
                        outStr = outStr + 'W';
                }

                else if (inChar >= 'a' && inChar <= 'z')
                {
                    if (UwUignores[inChar - 'a'])
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
