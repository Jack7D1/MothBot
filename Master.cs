using Discord;
using Discord.WebSocket;
using MothBot.modules;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MothBot
{
    internal class Master
    {
        public const string PATH_TOKEN = "../../data/token.txt";

        //See data module for parameters
        public static DiscordSocketClient client = new DiscordSocketClient();

        public static Random rand = new Random((int)(DateTime.Now.Ticks % int.MaxValue));

        public static void Main(string[] args)  //Initialization
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
            Console.CancelKeyPress += new ConsoleCancelEventHandler(ProcessExit);

            Logging.Log($"System rebooted at [{DateTime.UtcNow}] {args}");
            //Keep at bottom of init
            try
            {
                new Master().MainAsync().GetAwaiter().GetResult();    //Start Runtime
            }
            catch (Exception e)
            {
                Logging.LogtoConsoleandFile($"**********\nPROGRAM CRASH: \n{e.Message}\n**********");
                throw e;
            }
        }

        private static async Task Client_MessageRecieved(SocketMessage msg)
        {
            // Filter messages
            if (msg.Author.IsBot)   //If message author is a bot, ignore
                return;

            string input = msg.Content.ToLower();
            if (input.StartsWith($"{Data.PREFIX} "))    //Filter out messages starting with prefix but not as a whole word (eg. if prefix is 'bot' we want to look at 'bot command' but not 'bots command'
                try
                {
                    await RootCommandHandler(msg);
                }
                catch (Exception ex)
                {
                    string errmsg = $"**Command Failed!** Error: \"{ex.Message}\"";
                    await msg.Channel.SendMessageAsync(errmsg);
                    await Logging.LogtoConsoleandFileAsync(errmsg);
                }
        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            client.LogoutAsync(); //So mothbot doesn't hang out as a ghost for a few minutes
        }

        private static async Task RootCommandHandler(SocketMessage msg)
        {
            string guildid = "";
            if (msg.Channel is ITextChannel)
                guildid = $", {(msg.Channel as ITextChannel).Guild.Name} [{(msg.Channel as ITextChannel).GuildId}]";

            await Logging.LogtoConsoleandFileAsync($@"[{msg.Timestamp.UtcDateTime}][{msg.Author}][{msg.Author.Id}] said ({msg.Content}) in #{msg.Channel}{guildid}");
            await Logging.LogtoConsoleandFileAsync($@"Message size: {msg.Content.Length}");

            Data.CommandSplitter(msg.Content.Substring($"{Data.PREFIX} ".Length), out string keyword, out string args);

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
                    await msg.Channel.SendMessageAsync(Data.Program_GetCommandList());
                    break;

                case "laws":
                case "state":
                    await msg.Channel.SendMessageAsync(Data.Program_GetLaws());
                    break;

                case "say":
                    if (!Chatterbot.ContentsBlacklisted(msg.Content))
                        await msg.Channel.SendMessageAsync(Sanitize.ScrubMentions(msg.Content, false).Substring(Data.PREFIX.Length + "say ".Length));
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

                case "chatter":
                    await Chatterbot.CommandHandler(msg, args);
                    break;

                case "ping":
                    await msg.Channel.SendMessageAsync($"Ping: {client.Latency}ms");
                    break;

                case "portal":
                case "portals":
                    await Portals.PortalManagement(msg, args);
                    break;

                case "whitelist":
                    await Whitelist.CommandHandler(msg, args);
                    break;
                //Below are not listed in the commands block.
                case "dealias":
                    await msg.Channel.SendMessageAsync(Sanitize.Dealias(args));
                    break;

                case "utility":
                    await Utilities.UtilitiesHandlerAsync(msg, args);
                    break;

                default:
                    break;
            }
        }

        private async Task MainAsync()
        {
            client.MessageReceived += Client_MessageRecieved;
            client.Log += Logging.Log;
            client.Ready += Users.Client_Ready;
            client.MessageReceived += Portals.MessageRecieved;
            client.UserJoined += Whitelist.UserJoined;
            client.MessageReceived += Chatterbot.ChatterHandler;
            await client.LoginAsync(TokenType.Bot, File.ReadAllText(PATH_TOKEN));

            await Data.Program_SetStatus();
            await client.StartAsync();

            await Task.Delay(-1);   //Sit here while the async listens
        }
    }
}