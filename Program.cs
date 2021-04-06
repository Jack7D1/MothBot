﻿using Discord;
using Discord.Rest;
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
        public static DiscordRestClient restClient = new DiscordRestClient();

        public static void Main(string[] args)  //Initialization
        {
            _ = new Logging();
            Logging.Log($"System rebooted at [{DateTime.UtcNow}] {args}");
            //Keep at bottom of init
            new Program().MainAsync().GetAwaiter().GetResult();    //Begin async program
        }

        private static async Task Client_MessageRecieved(SocketMessage msg)
        {
            //Protect designated server from everyone mentions.
            if (msg.Channel is ITextChannel && (msg.Channel as ITextChannel).GuildId == 733421105448222771)
                if (msg.MentionedEveryone)
                {
                    await msg.DeleteAsync();
                    return;
                }
            // Filter messages
            if (msg.Author.IsBot)   //If message author is a bot, ignore
                return;

            await Portals.BroadcastHandlerAsync(msg);
            string input = msg.Content.ToLower();
            if (input.StartsWith($"{Data.PREFIX} "))    //Filter out messages starting with prefix but not as a whole word (eg. if prefix is 'bot' we want to look at 'bot command' but not 'bots command'
                try
                {
                    await RootCommandHandler(msg);
                }
                catch (Exception ex)
                {
                    await msg.Channel.SendMessageAsync($"**Command Failed!** Error: \"{ex.Message}\"");
                }
            else if (!Portals.IsPortal(msg.Channel))
                await Chatterbot.ChatterHandler(msg);
        }

        private static void ExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            client.StopAsync();   //Prevent further inputs immediately.
            Exception ex = e.ExceptionObject as Exception;
            Logging.LogtoConsoleandFile("\n\n******[FATAL EXCEPTION]******\n" +
                $"EXCEPTION TYPE: {ex.GetType()} (\"{ex.Message}\")\n" +
                $"FROM: {sender}\n" +
                $"**STACKTRACE:\n{ex.StackTrace}\n\n" +
                "Crash logging finished, saving data and shutting down safely...\n");
            Environment.Exit(ex.HResult);
        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            Portals.SavePortals();
            Chatterbot.SaveChatters();
            Chatterbot.SaveBlacklist();
            client.LogoutAsync(); //So mothbot doesn't hang out as a ghost for a few minutes
            restClient.LogoutAsync();
        }

        private static Task Ready()  //Init any objects here that are dependant on the client having logged in.
        {
            _ = new Chatterbot();
            _ = new Portals();
            return Task.CompletedTask;
        }

        private static async Task RootCommandHandler(SocketMessage msg)
        {
            string guildid = "";
            if (msg.Channel is ITextChannel)
                guildid = $", {(msg.Channel as ITextChannel).Guild.Name} [{(msg.Channel as ITextChannel).GuildId}]";

            await Logging.LogtoConsoleandFileAsync($@"[{msg.Timestamp.UtcDateTime}][{msg.Author}][{msg.Author.Id}] said ({msg.Content}) in #{msg.Channel}{guildid}");
            await Logging.LogtoConsoleandFileAsync($@"Message size: {msg.Content.Length}");

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
                    await msg.Channel.SendMessageAsync(Data.Program_GetCommandList());
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

                case "good":
                case "bad":
                    await Chatterbot.VoteHandler(msg, keyword);
                    break;

                case "chatter":
                    await Chatterbot.VoteHandler(msg, args);
                    break;

                case "ping":
                    await msg.Channel.SendMessageAsync($"Ping: {client.Latency}ms");
                    break;

                case "portal":
                case "portals":
                    await Portals.PortalManagement(msg, args);
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
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler);
            client.MessageReceived += Client_MessageRecieved;
            client.Log += Logging.Log;
            client.Ready += Ready;
            await client.LoginAsync(TokenType.Bot, File.ReadAllText(Data.PATH_TOKEN));
            await restClient.LoginAsync(TokenType.Bot, File.ReadAllText(Data.PATH_TOKEN));
            await Data.Program_SetStatus();
            await client.StartAsync();

            await Task.Delay(-1);   //Sit here while the async listens
        }
    }
}