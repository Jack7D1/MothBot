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

        public static Random rand = new Random(DateTime.Now.Hour + DateTime.Now.Millisecond - DateTime.Now.Month);
        public static DiscordSocketClient client = new DiscordSocketClient();
        public static Logging logging = new Logging();  //Sequence sensitive inits here

        public static Chatterbot chatter = new Chatterbot();
        public static Minesweeper mineSweeper = new Minesweeper();
        public static Utilities utilities = new Utilities();
        public static Portals portal = new Portals();
        private const string TOKEN_PATH = @"..\..\data\token.txt";

        public static void Main(string[] args)  //Initialization
        {
            logging.Log($"System rebooted at [{DateTime.UtcNow}] {args}");
            //Keep at bottom of init
            new Program().MainAsync().GetAwaiter().GetResult();    //Begin async program
        }

        private async Task Client_MessageRecieved(SocketMessage message)
        {
            {
                string input = message.Content.ToLower();
                // Filter messages
                if (message.Author.IsBot)   //If message author is a bot, ignore
                    return;
                if (input == "ye") //If someone says ye, say ye, but with a 1/10 chance
                {
                    if (rand.Next(10) == 0)
                        await message.Channel.SendMessageAsync("Ye");
                    return;
                }
                await chatter.AddChatter(message);
                await chatter.ChatterHandler(message);

                //All non prefix dependant directives go above
                if (input.IndexOf($"{_prefix} ") != 0)    //Filter out messages starting with prefix but not as a whole word (eg. if prefix is 'bot' we want to look at 'bot command' but not 'bots command'
                    return;
                if (input.IndexOf($"{_prefix} utility") == 0)
                {
                    await utilities.UtilitiesHandlerAsync(message);
                    return;
                }
            }
            await logging.LogtoConsoleandFileAsync($@"[{message.Timestamp}][{message.Author}] said ({message.Content}) in #{message.Channel}");
            await logging.LogtoConsoleandFileAsync($@"Message size: {message.Content.Length}");

            //Begin Command Parser
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
                    await mineSweeper.MinesweeperHandlerAsync(message.Channel);
                    return;

                case "give":
                    await Imagesearch.ImageSearchHandlerAsync(message.Channel, args);
                    return;

                case "roll":
                    await Dice.Roll(message.Channel, args);
                    return;

                case "ping":
                    await message.Channel.SendMessageAsync($"Ping: {client.Latency}ms");
                    return;

                default:
                    return;
            }
        }

        private async Task MainAsync()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
            client.MessageReceived += Client_MessageRecieved;
            client.Log += logging.Log;
            await client.LoginAsync(TokenType.Bot, File.ReadAllText(TOKEN_PATH));
            await Lists.SetDefaultStatus();
            await client.StartAsync();

            await Task.Delay(-1);   //Sit here while the async listens
        }

        private void ProcessExit(object sender, EventArgs e)
        {
            client.LogoutAsync(); //So mothbot doesn't hang out as a ghost for a few minutes.
        }
    }
}