using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal static class Utilities
    {
        private static readonly ulong[] operatorIDs = {   //Discord user IDs of allowed operators, in ulong format.
            238735900597682177, //Dex
            206920373952970753, //Jack
        };

        private static bool shutdownEnabled = false;
        private static long shutdownTimeout = 0;

        public static bool IsOperator(SocketUser user)
        {
            foreach (ulong op in operatorIDs)
                if (user.Id == op)
                    return true;
            return false;
        }

        public static async Task UtilitiesHandlerAsync(SocketMessage src, string command)
        {
            if (!IsOperator(src.Author))   //You do not have permission
            {
                await src.Channel.SendMessageAsync("You do not have access to Utilities.");
                return;
            }

            string keyword, args;
            if (command.IndexOf(' ') == 0)
                command = command.Substring(1);
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
            args = args.ToLower();
            keyword = keyword.ToLower();
            if (keyword == "")
                keyword = "commands";

            switch (keyword)    //Ensure switch is ordered similarly to command list
            {
                //General
                case "blacklist":
                    await Chatterbot.BlacklistHandler(src, args);
                    break;

                case "prependbackupchatters":
                    await src.Channel.SendMessageAsync("Prepending backups...");
                    Chatterbot.PrependBackupChatters(src.Channel);
                    break;

                //Data/debug
                case "dumpchatters":
                    await src.Channel.SendMessageAsync("Dumping chatters file...");
                    await src.Channel.SendFileAsync(Chatterbot.PATH_CHATTERS);
                    break;

                case "dumplogs":
                    await src.Channel.SendMessageAsync("Dumping logs file...");
                    await src.Channel.SendFileAsync(Logging.PATH_LOGS);
                    break;

                case "dumpportals":
                    await src.Channel.SendMessageAsync("Dumping portals file...");
                    await src.Channel.SendFileAsync(Portals.PATH_PORTALS);
                    break;

                case "listservers":
                    {
                        string outStr = "```CURRENT JOINED SERVERS:\n";
                        foreach (SocketGuild guild in Program.client.Guilds)
                            outStr += $"{guild.Name} [{guild.Id}], owned by {Program.client.Rest.GetUserAsync(guild.OwnerId).Result.Username} [{guild.OwnerId}]\n";
                        await src.Channel.SendMessageAsync(outStr + "```");
                    }
                    break;

                //dangerous
                case "leaveserver":
                    await Program.client.GetGuild(ulong.Parse(args)).LeaveAsync();
                    await src.Channel.SendMessageAsync($"Left guild {args} successfully");
                    break;

                case "shutdown":
                    if (args == "confirm")
                    {
                        if (DateTime.Now.Ticks > shutdownTimeout)
                            shutdownEnabled = false;

                        if (shutdownEnabled)
                        {
                            await Logging.LogtoConsoleandFileAsync($"SHUTTING DOWN: ordered by {src.Author.Username}");
                            await src.Channel.SendMessageAsync($"{src.Author.Mention} Shutdown confirmed, terminating bot.");
                            Environment.Exit(0);
                        }
                        else
                        {
                            shutdownEnabled = true;
                            shutdownTimeout = DateTime.Now.AddSeconds(12).Ticks;
                            await src.Channel.SendMessageAsync($"Shutdown safety disabled, {src.Author.Mention} confirm shutdown again to shut down bot, or argument anything else to re-enable safety.");
                            await Logging.LogtoConsoleandFileAsync($"{src.Author.Username} disabled shutdown safety.");
                        }
                    }
                    else
                    {
                        shutdownEnabled = false;
                        await src.Channel.SendMessageAsync("Shutdown safety (re)enabled, argument 'confirm' to disable.");
                    }
                    break;

                default:
                    await src.Channel.SendMessageAsync(Data.Utilities_GetCommandList());
                    break;
            }
        }
    }
}