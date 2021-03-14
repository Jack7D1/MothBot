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
        public static DiscordSocketClient client = new DiscordSocketClient();
        public static Random rand = new Random(DateTime.Now.Hour + DateTime.Now.Millisecond - DateTime.Now.Month);
        private const string TOKEN_PATH = @"../../data/token.txt";

        public static void Main(string[] args)  //Initialization
        {
            _ = new Logging();
            Logging.Log($"System rebooted at [{DateTime.UtcNow}] {args}");
            //Keep at bottom of init
            try
            {
                new Program().MainAsync().GetAwaiter().GetResult();    //Begin async program
            }
            catch (Exception ex)   //Catch unhandled exceptions and safely shutdown the program.
            {
                client.StopAsync();   //Prevent further inputs immediately.
                Logging.LogtoConsoleandFile("\n\n******[UNHANDLED EXCEPTION]******\n" +
                    $"EXCEPTION TYPE: {ex.GetType()} (\"{ex.Message}\")\n" +
                    $"**STACKTRACE:\n{ex.StackTrace}\n\n" +
                    "Crash logging finished, saving data and shutting down safely...");
                Environment.Exit(ex.HResult);
            }
        }

        private async Task Client_MessageRecieved(SocketMessage msg)
        {
            {
                // Filter messages
                if (msg.Author.IsBot)   //If message author is a bot, ignore
                    return;
                if (!Portals.IsPortal(msg.Channel))
                {
                    await Chatterbot.AddChatterHandler(msg);
                    await Chatterbot.ChatterHandler(msg);
                }
                await Portals.BroadcastHandlerAsync(msg);

                //All non prefix dependant directives go above
                string input = msg.Content.ToLower();
                if (input.IndexOf($"{_prefix} ") != 0)    //Filter out messages starting with prefix but not as a whole word (eg. if prefix is 'bot' we want to look at 'bot command' but not 'bots command'
                    return;
                if (input.IndexOf($"{_prefix} utility") == 0)
                {
                    await Utilities.UtilitiesHandlerAsync(msg);
                    return;
                }
            }
            await Logging.LogtoConsoleandFileAsync($@"[{msg.Timestamp.UtcDateTime}][{msg.Author}] said ({msg.Content}) in #{msg.Channel}");
            await Logging.LogtoConsoleandFileAsync($@"Message size: {msg.Content.Length}");

            //Begin Command Parser
            string command, keyword, args;
            command = msg.Content.ToLower().Substring(_prefix.Length + 1); //We now have all text that follows the prefix.
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
                    await msg.Channel.SendMessageAsync($@"Hi, {msg.Author.Mention}!");
                    return;

                case "rogue":
                case "malf":
                    if (_prefix == "ai")
                        await msg.Channel.SendMessageAsync("http://media.discordapp.net/attachments/585862469508005888/752274349372735508/fwefewgaergar.png");
                    return;

                case "help":
                case "commands":
                    await msg.Channel.SendMessageAsync(Data.Program_GetCommandList(_prefix));
                    return;

                case "laws":
                case "state":
                    await msg.Channel.SendMessageAsync(Data.Program_GetLaws());
                    return;

                case "say":
                    await msg.Channel.SendMessageAsync(Sanitize.ScrubRoleMentions(msg.Content).Substring(_prefix.Length + "say ".Length));
                    await msg.DeleteAsync();
                    return;

                case "minesweeper":
                    await Minesweeper.MinesweeperHandlerAsync(msg.Channel);
                    return;

                case "give":
                    await Imagesearch.ImageSearchHandlerAsync(msg.Channel, args);
                    return;

                case "roll":
                    await Dice.Roll(msg.Channel, args);
                    return;

                case "ping":
                    await msg.Channel.SendMessageAsync($"Ping: {client.Latency}ms");
                    return;

                case "portal":
                    await Portals.PortalManagement(msg, args);
                    return;

                default:
                    return;
            }
        }

        private async Task MainAsync()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
            client.MessageReceived += Client_MessageRecieved;
            client.Log += Logging.Log;
            client.Ready += Ready;
            await client.LoginAsync(TokenType.Bot, File.ReadAllText(TOKEN_PATH));
            await Data.Program_SetStatus();
            await client.StartAsync();

            await Task.Delay(-1);   //Sit here while the async listens
        }

        private void ProcessExit(object sender, EventArgs e)
        {
            Portals.SavePortals();
            Chatterbot.SaveChatters();
            Chatterbot.SaveBlacklist();
            client.LogoutAsync(); //So mothbot doesn't hang out as a ghost for a few minutes.
        }

        private Task Ready()  //Init any objects here that are dependant on the client having logged in.
        {
            _ = new Chatterbot();
            _ = new Portals();
            return Task.CompletedTask;
        }
    }
}