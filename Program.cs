using Discord;
using Discord.WebSocket;
using MothBot.modules;
using System;
using System.IO;
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
        private const string TOKEN_PATH = @"..\..\data\token.txt";

        public static void Main(string[] args)  //Initialization
        {
            logging.Log($"System rebooted at [{DateTime.UtcNow}] {args}");
            //Keep at bottom of init
            new Program().MainAsync().GetAwaiter().GetResult();    //Begin async program
        }

        private async Task MainAsync()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            client.MessageReceived += MessageHandler;
            client.Log += logging.Log;

            await client.LoginAsync(TokenType.Bot, File.ReadAllText(TOKEN_PATH));
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
                    await mineSweeper.MinesweeperHandler(message.Channel);
                    return;

                case "give":
                    await Imagesearch.ImageSearchHandler(message.Channel, args);
                    return;

                case "roll":
                    await Dice.Roll(message.Channel, args);
                    return;

                case "ping":
                    await message.Channel.SendMessageAsync($"Ping: {client.Latency}ms");
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