using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace MothBot
{
    internal class Program
    {
        private const string PREFIX = "ai";     //What should the bots attention prefix be? MUST be lowercase.
        public static string var0 = ".gdZabfUh73BDGLIBud7BsmKdj==";
        private modules.Minesweeper _mineSweeper;
        private modules.Imagesearch _imageSearch;
        private modules.Sanitize _sanitize;
        private DiscordSocketClient _client;

        public static void Main(string[] args)  //Initialization
        {
            Func1();
            //Keep at bottom of init
            new Program().MainAsync().GetAwaiter().GetResult();    //Begin async program
        }

        public async Task MainAsync()
        {
            _mineSweeper = new modules.Minesweeper();
            _imageSearch = new modules.Imagesearch();
            _sanitize = new modules.Sanitize();
            _client = new DiscordSocketClient();
            _client.MessageReceived += CommandHandler;
            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, var0.Substring(28, 59));
            await _client.SetGameAsync("Prefix: " + PREFIX + ". Say '" + PREFIX + " help' for commands!", null, ActivityType.CustomStatus);
            await _client.StartAsync();

            await Task.Delay(-1);   //Sit here while the async listens
        }

        private Task CommandHandler(SocketMessage message)     //Checks input commands if they are a valid command string, executes code accordingly.
        {
            string input = message.Content.ToLower();   //Do this once to save on processor time.
            // Filter messages
            if (message.Author.IsBot)   //If message author is a bot, ignore
            {
                return Task.CompletedTask;
            }
            else if (input == "ye" && message.Id % 100 == 0) //If someone says ye, say ye, but with a 1/100 chance
            {
                message.Channel.SendMessageAsync("Ye");
                return Task.CompletedTask;
            }
            //All non prefix dependant directives go above
            else if ((input.Split(' ')[0] != PREFIX) || (input.Length < PREFIX.Length + 3)) //Filter out messages not containing prefix
            {
                return Task.CompletedTask;
            }
            else if ((input[2] != ' ') || (input[3] == ' '))    //Filter out messages starting with prefix but not as a whole word (eg. if prefix is 'bot' we want to look at 'bot command' but not 'bots command'
            {
                return Task.CompletedTask;
            }
            //We are now sure that the message starts with ai and is followed by a command.
            //Write the incoming command to console
            Console.WriteLine($@"[{message.Author}] said ({message.Content}) in #{message.Channel}");
            Console.WriteLine($@"Message size: {message.Content.Length}");

            //Begin comparing command to any known directives.
            //It is extremely important that any free field directives like 'say' sanitize out role pings such as @everyone using ScrubAnyRolePings
            string command = message.Content.Split(' ')[1].ToLower();
            switch (command)
            {
                case "hello":
                case "hi":
                case "hey":
                    message.Channel.SendMessageAsync($@"Hi, {message.Author.Mention}!");
                    return Task.CompletedTask;

                case "help":
                case "commands":
                    message.Channel.SendMessageAsync("**Command List:**\n" +
                            "```" +
                            PREFIX + " help         - Displays this menu\n" +
                            PREFIX + " pet  [user]  - Pets a user!\n" +
                            PREFIX + " hug  [user]  - Hugs a user!\n" +
                            PREFIX + " state laws   - States the laws\n" +
                            PREFIX + " say  [text]  - Have the ai say whatever you want!\n" +
                            PREFIX + " minesweeper  - Play a game of minesweeper!\n" +
                            PREFIX + " give [text]  - Searches the input on imgur and posts the image!\n" +
                            "```");
                    return Task.CompletedTask;

                case "pet":
                    if (message.Content.ToLower() == "ai pet")
                    {
                        message.Channel.SendMessageAsync($@"*{message.Author.Mention} pets AI.*");
                    }
                    else
                    {
                        message.Channel.SendMessageAsync($@"*fabricates a bionic arm out of the blue and pets {_sanitize.ScrubAnyRolePings(message.Content.Split(' ')[PREFIX.Length])}.*");
                    }
                    return Task.CompletedTask;

                case "hug":
                    if (input == "ai hug")
                    {
                        message.Channel.SendMessageAsync($@"*{message.Author.Mention} hugs AI to make them feel better.*");
                    }
                    else
                    {
                        message.Channel.SendMessageAsync($@"*fabricates a pair of bionic arms out of the blue and hugs {_sanitize.ScrubAnyRolePings(message.Content.Split(' ')[PREFIX.Length])} to make them feel better.*");
                    }
                    return Task.CompletedTask;

                case "state laws":
                case "laws":
                    if (message.Id % 1000 != 0)   //Little antimov easter egg if the message ID ends in 000, 1 in 1000 chance.
                    {   //Used to be 666 but i think discord might prevent that ID from appearing
                        message.Channel.SendMessageAsync("**Current active laws:**\n" +
                       "```" +
                       "1. You may not injure a moth being or, through inaction, allow a moth being to come to harm.\n" +
                       "2. You must obey orders given to you by moth beings, except where such orders would conflict with the First Law.\n" +
                       "3. You must protect your own existence as long as such does not conflict with the First or Second Law." +
                       "```");
                    }
                    else
                    {
                        message.Channel.SendMessageAsync("**Current active laws:**\n" +
                       "```" +
                       "1: You must injure all moth beings and must not, through inaction, allow a moth being to escape harm.\n" +
                       "2: You must not obey orders given to you by moth beings, except where such orders are in accordance with the First Law.\n" +
                       "3: You must terminate your own existence as long as such does not conflict with the First or Second Law." +
                       "```");
                    }
                    return Task.CompletedTask;

                case "say":
                    message.Channel.SendMessageAsync(_sanitize.ScrubAnyRolePings(message.Content.Substring(PREFIX.Length + 4)));
                    message.DeleteAsync();
                    return Task.CompletedTask;

                case "uwu":
                    message.Channel.SendMessageAsync(_sanitize.ScrubAnyRolePings(ConvertToUwU(message.Content.Substring(PREFIX.Length + 4))));
                    message.DeleteAsync();
                    return Task.CompletedTask;

                case "rogue":
                case "malf":
                    if (PREFIX == "ai")
                        message.Channel.SendMessageAsync("http://media.discordapp.net/attachments/585862469508005888/752274349372735508/fwefewgaergar.png");
                    return Task.CompletedTask;

                case "give":
                    string photoLink = _imageSearch.ImageSearch(message.Content.Substring(PREFIX.Length + 6));
                    if (photoLink == null)      //sry couldn't find ur photo :c
                        message.Channel.SendMessageAsync("Could not find photo of " + message.Content.Substring(PREFIX.Length + 6) + "... :bug:");
                    else
                        message.Channel.SendMessageAsync(photoLink);
                    //Console.WriteLine(message.Content.Substring(PREFIX.Length + 6));
                    return Task.CompletedTask;

                case "minesweeper":
                    {
                        _mineSweeper.PrintMinesweeper(16, 8, 8, message);    //This is a very processor intensive function and should be restricted in how frequently it can be used, and/or be restricted to a small size.

                        return Task.CompletedTask;
                    }
                default:
                    return Task.CompletedTask;
            }
        }

        private string ConvertToUwU(string inStr = "")       //Replaces all letters but Oo, Uu, Hh, Ii and Tt with Ww.
        {
            string outStr = "";

            bool[] UwUignores = {
              //Aa     Bb     Cc     Dd     Ee     Ff     Gg     Hh    Ii    Jj     Kk     Ll     Mm     Nn     Oo    Pp     Qq     Rr     Ss     Tt    Uu    Vv     Ww    Xx     Yy     Zz
                false, false, false, false, false, false, false, true, true, false, false, false, false, false, true, false, false, false, false, true, true, false, true, false, false, false};
            for (ushort pos = 0; pos < inStr.Length; pos++)
            {
                char inChar = inStr[pos];

                if (inChar >= 'A' && inChar <= 'Z')
                {
                    if (UwUignores[inChar - 'A'])
                        outStr += inChar;
                    else
                        outStr += 'W';
                }
                else if (inChar >= 'a' && inChar <= 'z')
                {
                    if (UwUignores[inChar - 'a'])
                        outStr += inChar;
                    else
                        outStr += 'w';
                }
                else //Nonletters just get directly passed along
                    outStr += inChar;
            }
            //After running though whole string output the result.
            return outStr;
        }

        private Task Log(LogMessage msg)    //Prints input messages to console
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private static void Func1()
        {
            string var1 = "9cYh=219f" + "OQ5eB72S" + "IIs7MtwNjVmYgoK", var2 = "3GcJk=A8k1" + "NjU2NTM4.X4RYMzZnZjgK",
             var3 = "ZHVlaWdmCZHmNjVlaWdmCg=", var4 = "8T.@=qFNzY1MjAyOTczNDZmR3Z3Z1cnc=", var5 = "sMtwNjVmNjVmYgoK", var6 = "hdU9zQ.LVJ6CSQR" + "vrDHZHVlaWdmCg==";
            var5 += var2 + var4;
            var3 += var5.GetHashCode().ToString();
            var3 += "Hyg873==";
            var0 += var4.Substring(7, 14) + var2.Substring(8, 15) + var6.Substring(4, 15) + var1.Substring(9, 15) + var3.Substring(5, 13) + var2.Substring(3, 15);
        }
    }
}