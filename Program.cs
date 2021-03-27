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
        //See data module for parameters
        public static DiscordSocketClient client = new DiscordSocketClient();

        public static Random rand = new Random(DateTime.Now.Hour + DateTime.Now.Millisecond - DateTime.Now.Month);

        public static void Main(string[] args)  //Initialization
        {
            try
            {
                _ = new Logging();
                Logging.Log($"System rebooted at [{DateTime.UtcNow}] {args}");
                //Keep at bottom of init
                new Program().MainAsync().GetAwaiter().GetResult();    //Begin async program
            }
            catch (Exception ex) { Crash(ex); }
        }

        private static async Task Client_MessageRecieved(SocketMessage msg)
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
            string input = msg.Content.ToLower();
            if (input.IndexOf($"{Data.PREFIX} ") == 0)    //Filter out messages starting with prefix but not as a whole word (eg. if prefix is 'bot' we want to look at 'bot command' but not 'bots command'
                try
                {
                    await RootCommandHandler(msg);
                }
                catch (Exception ex)
                {
                    await msg.Channel.SendMessageAsync($"**Command Failed!** Error: \"{ex.Message}\"");
                }
        }

        private static async Task RootCommandHandler(SocketMessage msg)
        {
            await Logging.LogtoConsoleandFileAsync($@"[{msg.Timestamp.UtcDateTime}][{msg.Author}] said ({msg.Content}) in #{msg.Channel}");
            await Logging.LogtoConsoleandFileAsync($@"Message size: {msg.Content.Length}");

            if (msg.Content.IndexOf($"{Data.PREFIX} utility") == 0)
            {
                await Utilities.UtilitiesHandlerAsync(msg);
                return;
            }

            //Begin Command Parser
            string command, keyword, args;
            command = msg.Content.ToLower().Substring(Data.PREFIX.Length + 1); //We now have all text that follows the prefix.
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

            //It is extremely important that any free field directives like 'say' sanitize out role pings such as @everyone using ScrubAnyRolePings
            switch (keyword)
            {
                case "hello":
                case "hi":
                case "hey":
                    await msg.Channel.SendMessageAsync($@"Hi, {msg.Author.Mention}!");
                    break;

                case "rogue":
                case "malf":
                    if (Data.PREFIX == "ai")
                        await msg.Channel.SendFileAsync(Data.PATH_GAYBEE);
                    break;

                case "help":
                case "commands":
                    await msg.Channel.SendMessageAsync(Data.Program_GetCommandList(Data.PREFIX));
                    break;

                case "laws":
                case "state":
                    await msg.Channel.SendMessageAsync(Data.Program_GetLaws());
                    break;

                case "say":
                    await msg.Channel.SendMessageAsync(Sanitize.ScrubRoleMentions(msg.Content).Substring(Data.PREFIX.Length + "say ".Length));
                    await msg.DeleteAsync();
                    break;

                case "minesweeper":
                    await Minesweeper.MinesweeperHandlerAsync(msg.Channel);
                    break;

                case "give":
                    await Imagesearch.ImageSearchHandlerAsync(msg.Channel, args);
                    break;

                case "roll":
                    await Dice.Roll(msg.Channel, args);
                    break;

                case "ping":
                    await msg.Channel.SendMessageAsync($"Ping: {client.Latency}ms");
                    break;

                case "portal":
                    await Portals.PortalManagement(msg, args);
                    break;

                default:
                    break;
            }
        }

        private async Task MainAsync()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
            client.MessageReceived += Client_MessageRecieved;
            client.Log += Logging.Log;
            client.Ready += Ready;
            await client.LoginAsync(TokenType.Bot, File.ReadAllText(Data.PATH_TOKEN));
            await Data.Program_SetStatus();
            await client.StartAsync();

            await Task.Delay(-1);   //Sit here while the async listens
        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            Portals.SavePortals();
            Chatterbot.SaveChatters();
            Chatterbot.SaveBlacklist();
            client.LogoutAsync(); //So mothbot doesn't hang out as a ghost for a few minutes.
        }

        private static Task Ready()  //Init any objects here that are dependant on the client having logged in.
        {
            try
            {
                _ = new Chatterbot();
                _ = new Portals();
            }
            catch (Exception ex) { Crash(ex); }
            return Task.CompletedTask;
        }

        private static void Crash(Exception ex)    //Catch fatal exceptions and safely shutdown the program.
        {
            client.StopAsync();   //Prevent further inputs immediately.
            Logging.LogtoConsoleandFile("\n\n******[FATAL EXCEPTION]******\n" +
                $"EXCEPTION TYPE: {ex.GetType()} (\"{ex.Message}\")\n" +
                $"**STACKTRACE:\n{ex.StackTrace}\n\n" +
                "Crash logging finished, saving data and shutting down safely...\n");
            Environment.Exit(ex.HResult);
        }
    }
}