using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace MothBot
{
    internal class Program
    {
        public DiscordSocketClient _client;
        public modules.Imagesearch _imageSearch;
        public modules.Logging _logging;
        public modules.Minesweeper _mineSweeper;
        public modules.Sanitize _sanitize;
        public modules.Utilities _utilities;
        public string PREFIX = "ai";     //What should the bots attention prefix be? MUST be lowercase.

        public static void Main(string[] args)  //Initialization
        {
            //Keep at bottom of init
            new Program().MainAsync().GetAwaiter().GetResult();    //Begin async program
        }

        public void InitModules()
        {
            _imageSearch = new modules.Imagesearch();
            _logging = new modules.Logging();
            _logging.Log($"System reset at [{System.DateTime.UtcNow}]");
            _mineSweeper = new modules.Minesweeper();
            _sanitize = new modules.Sanitize();
            _utilities = new modules.Utilities(this);
        }

        private Task CommandHandler(SocketMessage message)
        {
            string input = message.Content.ToLower();
            // Filter messages
            if (message.Author.IsBot)   //If message author is a bot, ignore
            {
                return Task.CompletedTask;
            }
            else if (input == "ye" && message.Id % 10 == 0) //If someone says ye, say ye, but with a 1/10 chance
            {
                message.Channel.SendMessageAsync("Ye");
                return Task.CompletedTask;
            }
            //All non prefix dependant directives go above
            else if ((input.Split(' ')[0] != PREFIX))
            {
                return Task.CompletedTask;
            }
            else if (input[PREFIX.Length] != ' ')    //Filter out messages starting with prefix but not as a whole word (eg. if prefix is 'bot' we want to look at 'bot command' but not 'bots command'
            {
                return Task.CompletedTask;
            }
            //We are now sure that the message starts with ai and is followed by a command.
            Console.WriteLine($@"[{message.Timestamp}][{message.Author}] said ({message.Content}) in #{message.Channel}");
            Console.WriteLine($@"Message size: {message.Content.Length}");

            //Begin comparing command to any known directives. Keep the switch ordered similarly to the commands section!
            //It is extremely important that any free field directives like 'say' sanitize out role pings such as @everyone using ScrubAnyRolePings
            string command = message.Content.Split(' ')[1].ToLower();
            _logging.Log(message);
            switch (command)
            {
                case "hello":
                case "hi":
                case "hey":
                    message.Channel.SendMessageAsync($@"Hi, {message.Author.Mention}!");
                    return Task.CompletedTask;

                case "uwu":
                    message.Channel.SendMessageAsync(ConvertToUwU(_sanitize.ScrubRoleMentions(message).Substring(PREFIX.Length + "uwu ".Length)));
                    message.DeleteAsync();
                    return Task.CompletedTask;

                case "rogue":
                case "malf":
                    if (PREFIX == "ai")
                        message.Channel.SendMessageAsync("http://media.discordapp.net/attachments/585862469508005888/752274349372735508/fwefewgaergar.png");
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
                            PREFIX + " utility      - Utility functions, bot only responds to operators\n" +
                            "```");
                    return Task.CompletedTask;

                case "pet":
                    if (message.Content.ToLower() == "ai pet")
                    {
                        message.Channel.SendMessageAsync($@"*{message.Author.Mention} pets AI.*");
                    }
                    else
                    {
                        message.Channel.SendMessageAsync($@"*fabricates a bionic arm out of the blue and pets {_sanitize.ScrubRoleMentions(message).Split(' ')[PREFIX.Length]}.*");
                    }
                    return Task.CompletedTask;

                case "hug":
                    if (input == "ai hug")
                    {
                        message.Channel.SendMessageAsync($@"*{message.Author.Mention} hugs AI to make them feel better.*");
                    }
                    else
                    {
                        message.Channel.SendMessageAsync($@"*fabricates a pair of bionic arms out of the blue and hugs {_sanitize.ScrubRoleMentions(message).Split(' ')[PREFIX.Length]} to make them feel better.*");
                    }
                    return Task.CompletedTask;

                case "laws":
                case "state":
                    if (message.Id % 100 != 0)   //Little antimov easter egg if the message ID ends in 00, 1 in 100 chance.
                    {
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
                    message.Channel.SendMessageAsync(_sanitize.ScrubRoleMentions(message).Substring(PREFIX.Length + "say ".Length));
                    message.DeleteAsync();
                    return Task.CompletedTask;

                case "minesweeper":
                    message.Channel.SendMessageAsync(_mineSweeper.GetMinesweeper());
                    return Task.CompletedTask;

                case "give":
                    string photoLink = _imageSearch.ImageSearch(message.Content.Substring(PREFIX.Length + 6));
                    if (photoLink == null)      //sry couldn't find ur photo :c
                        message.Channel.SendMessageAsync("Could not find photo of " + message.Content.Substring(PREFIX.Length + 6) + "... :bug:");
                    else
                        message.Channel.SendMessageAsync(photoLink);
                    //Console.WriteLine(message.Content.Substring(PREFIX.Length + 6));
                    return Task.CompletedTask;

                case "utility":
                    _utilities.CommandHandler(message);
                    return Task.CompletedTask;

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

        private async Task MainAsync()
        {
            InitModules();
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            _client = new DiscordSocketClient();
            _client.MessageReceived += CommandHandler;
            _client.Log += _logging.Log;

            await _client.LoginAsync(TokenType.Bot, S());
            await _client.SetGameAsync("Prefix: " + PREFIX + ". Say '" + PREFIX + " help' for commands!", null, ActivityType.Playing);
            await _client.StartAsync();

            await Task.Delay(-1);   //Sit here while the async listens
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            _client.LogoutAsync();  //So mothbot doesn't hang out as a ghost for a few minutes.
            _logging.Log($"System shutdown at [{System.DateTime.UtcNow}]");
            _logging.Close();
        }

        private string S()
        {
            byte[] s = { 0xd4, 0xcd, 0x64, 0x9a, 0x96, 0x6, 0x58, 0x11, 0x1, 0xf2, 0x91, 0x77, 0x3c, 0x9f, 0xfc, 0xf9, 0xa7, 0x5e, 0x58, 0x46, 0x59, 0xc2,
                0x6e, 0xd3, 0x2d, 0xb4, 0xcb, 0x4b, 0xd7, 0x46, 0x28, 0x3e, 0x92, 0x8b, 0xa6, 0xd3, 0xc4, 0xaf, 0x8d, 0xda, 0xaf, 0x11, 0xde, 0x5, 0xb1,
                0xc5, 0x8f, 0xe5, 0xc4, 0x98, 0x5b, 0x3a, 0x8f, 0xc9, 0xf8, 0xec, 0x62, 0xcd, 0x44 };
            for (byte m = 0; m < s.Length; ++m)
            {
                byte c = s[m];
                c += 0x28; c ^= m; c += 0x37; c ^= 0xc3; c += m; c ^= 0x1f; c -= m; c = (byte)((c >> 0x6) | (c << 0x2)); c ^= 0xdc; c = (byte)~c;
                c += m; c = (byte)((c >> 0x6) | (c << 0x2)); c = (byte)-c; c = (byte)((c >> 0x7) | (c << 0x1)); c -= m; c = (byte)((c >> 0x7) | (c << 0x1));
                c = (byte)-c; c -= 0x73; c = (byte)-c; c -= m; c = (byte)~c; c = (byte)-c; c ^= 0x7c; c += 0x94; c ^= m; c += 0x88; c ^= m; c += m;
                c = (byte)-c; c = (byte)((c >> 0x1) | (c << 0x7)); c = (byte)~c; c ^= 0xb8;
                s[m] = c;
            }
            return System.Text.Encoding.UTF8.GetString(s);
        }
    }
}