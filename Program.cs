using Discord;
using Discord.WebSocket;
using MothBot.modules;
using System;
using System.Threading.Tasks;

namespace MothBot
{
    internal class Program
    {
        public const ulong MY_ID = 765202973495656538;
        public static string _prefix = "ai";         //What should the bots attention prefix be? MUST be lowercase.
        public static Chatterbot chatter = new Chatterbot();
        public static DiscordSocketClient client = new DiscordSocketClient();
        public static Logging logging = new Logging();
        public static Minesweeper mineSweeper = new Minesweeper();
        public static Utilities utilities = new Utilities();

        public static void Main(string[] args)  //Initialization
        {
            logging.Log($"System rebooted at [{DateTime.UtcNow}] {args}");
            //Keep at bottom of init
            new Program().MainAsync().GetAwaiter().GetResult();    //Begin async program
        }

        private static string S()
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

        private async Task MainAsync()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            client.MessageReceived += MessageHandler;
            client.Log += logging.Log;

            await client.LoginAsync(TokenType.Bot, S());
            await client.SetGameAsync("Prefix: " + _prefix + ". Say '" + _prefix + " help' for commands! Invite at https://tinyurl.com/MOFFBOT1111", null, ActivityType.Playing);
            await client.StartAsync();

            await Task.Delay(-1);   //Sit here while the async listens
        }

        private async Task MessageHandler(SocketMessage message)
        {
            {
                string input = message.Content.ToLower();
                // Filter messages
                if (message.Author.IsBot)   //If message author is a bot, ignore
                    return;
                if (input == "ye") //If someone says ye, say ye, but with a 1/10 chance
                {
                    if (message.Id % 10 != 0)
                        return;
                    await message.Channel.SendMessageAsync("Ye");
                    return;
                }
                chatter.AddChatter(message);
                await chatter.ChatterHandler(message);
                //All non prefix dependant directives go above
                if ((input.Split(' ')[0] != _prefix))
                    return;
                if (input[_prefix.Length] != ' ')    //Filter out messages starting with prefix but not as a whole word (eg. if prefix is 'bot' we want to look at 'bot command' but not 'bots command'
                    return;
            }
            //We are now sure that the message starts with ai and is followed by a command.
            Console.WriteLine($@"[{message.Timestamp}][{message.Author}] said ({message.Content}) in #{message.Channel}");
            Console.WriteLine($@"Message size: {message.Content.Length}");

            string command, keyword, args;
            command = message.Content.ToLower().Substring(_prefix.Length + 1); //We now have all text that follows the prefix.
            if (command.Contains(' '))
            {
                keyword = command.Substring(0, command.IndexOf(' '));
                args = command.Substring(command.IndexOf(' ') + 1);
            }
            else
            {
                keyword = command;
                args = "";
            }

            //Begin comparing command to any known directives. Keep the switch ordered similarly to the commands section!
            //It is extremely important that any free field directives like 'say' sanitize out role pings such as @everyone using ScrubAnyRolePings
            await logging.LogAsync(message);
            switch (keyword)
            {
                case "hello":
                case "hi":
                case "hey":
                    await message.Channel.SendMessageAsync($@"Hi, {message.Author.Mention}!");
                    return;

                case "rogue":
                case "malf":
                    if (_prefix == "ai")
                        await message.Channel.SendMessageAsync("http://media.discordapp.net/attachments/585862469508005888/752274349372735508/fwefewgaergar.png");
                    return;

                case "help":
                case "commands":
                    await Lists.Program_PrintCommandList(message.Channel, _prefix);
                    return;

                case "pet":
                    if (args != "")
                        await message.Channel.SendMessageAsync($@"*Fabricates a bionic arm out of the blue and pets {Sanitize.ScrubRoleMentions(message).Split(' ')[_prefix.Length]}.*");
                    return;

                case "hug":
                    if (args != "")
                        await message.Channel.SendMessageAsync($@"*Fabricates a pair of bionic arms out of the blue and hugs {Sanitize.ScrubRoleMentions(message).Split(' ')[_prefix.Length]} to make them feel better.*");
                    return;

                case "laws":
                case "state":
                    await Lists.PrintLaws(message.Channel);
                    return;

                case "say":
                    await message.Channel.SendMessageAsync(Sanitize.ScrubRoleMentions(message).Substring(_prefix.Length + "say ".Length));
                    await message.DeleteAsync();
                    return;

                case "minesweeper":
                    await message.Channel.SendMessageAsync(mineSweeper.GetMinesweeper());
                    return;

                case "give":
                    await Imagesearch.ImageSearchHandler(message.Channel, args);
                    return;

                case "roll":
                    await Dice.Roll(message.Channel, args);
                    return;

                case "utility":
                    await utilities.CommandHandler(message);
                    return;

                default:
                    return;
            }
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            chatter.SaveChatters();
            logging.Log($"System shutdown at [{System.DateTime.UtcNow}]");
            client.LogoutAsync(); //So mothbot doesn't hang out as a ghost for a few minutes.
        }
    }
}