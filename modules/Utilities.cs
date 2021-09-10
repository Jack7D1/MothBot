using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal static class Utilities
    {
        private static readonly string banHandlerCommandList =
            "**Ban ID Management Commands:**\n" +
            "```" +
            $"{Data.PREFIX} utility ban add       Ban ID from using bot in certain capacities.\n" +
            $"{Data.PREFIX} utility ban pardon    Lifts a ban on an ID if exists.\n" +
            "```";

        private static readonly string commandList =
            "**Utility Command List:**\n" +
            "```" +
            "general:\n" +
           $"{Data.PREFIX} utility blacklist [command]\n" +
           $"{Data.PREFIX} utility prependbackupchatters\n" +
            "``````" +
            "data/debug:\n" +
           $"{Data.PREFIX} utility dumpdata\n" +
           $"{Data.PREFIX} utility dumplogs\n" +
           $"{Data.PREFIX} utility listservers\n" +
           $"{Data.PREFIX} utility whois\n" +
            "``````" +
            "dangerous:\n" +
           $"{Data.PREFIX} utility ban [command]\n" +
           $"{Data.PREFIX} utility leaveserver [ID]\n" +
           $"{Data.PREFIX} utility shutdown\n" +
            "```";

        private static readonly List<ulong> operatorIDs = new List<ulong>{   //Discord user IDs of allowed operators, in ulong format.
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

            Data.CommandSplitter(command, out string keyword, out string args);

            switch (keyword)    //Ensure switch is ordered similarly to command list
            {
                //General
                case "blacklist":
                    await Chatterbot.BlacklistHandler(src, args);
                    break;

                case "prependbackupchatters":
                    await src.Channel.SendMessageAsync("Prepending backups...");
                    await Chatterbot.PrependBackupChatters(src.Channel);
                    break;

                //Data/debug
                case "dumpdata":
                    {
                        const string PATH_DATAZIP = "../../data.zip";
                        await src.Channel.SendMessageAsync("Zipping data files...");
                        if (File.Exists(PATH_DATAZIP))
                            File.Delete(PATH_DATAZIP);
                        ZipFile.CreateFromDirectory("../../data/", PATH_DATAZIP, CompressionLevel.Optimal, false);
                        await src.Channel.SendMessageAsync("Uploading...");
                        await src.Channel.SendFileAsync(PATH_DATAZIP);
                            File.Delete(PATH_DATAZIP);
                    }
                    break;

                case "dumplogs":
                    await src.Channel.SendMessageAsync("Dumping logs file...");
                    await src.Channel.SendFileAsync(Logging.PATH_LOGS);
                    break;

                case "listservers":
                    {
                        string outStr = "```CURRENT JOINED SERVERS:\n";
                        foreach (SocketGuild guild in Master.client.Guilds)
                            outStr += $"{guild.Name} [{guild.Id}], owned by {Master.client.Rest.GetUserAsync(guild.OwnerId).Result.Username} [{guild.OwnerId}]\n";
                        await src.Channel.SendMessageAsync(outStr + "```");
                    }
                    break;

                case "whois":
                    {
                        if (ulong.TryParse(args, out ulong id))
                        {
                            if (Users.GetUser(id, out Users.User user))
                            {
                                await src.Channel.SendMessageAsync($"ID resolves to username: {user.Username}");
                            }
                            else
                            {
                                await src.Channel.SendMessageAsync("ID not found");
                            }
                        }
                        else
                        {
                            if (Users.GetUser(args, out Users.User user))
                            {
                                await src.Channel.SendMessageAsync($"Username resolves to username: {user.Id}");
                            }
                            else
                            {
                                await src.Channel.SendMessageAsync("Username not found");
                            }
                        }
                    }
                    break;

                //dangerous
                case "ban":
                    await BanHandler(src, args);
                    break;

                case "leaveserver":
                    await Master.client.GetGuild(ulong.Parse(args)).LeaveAsync();
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
                    await src.Channel.SendMessageAsync(commandList);
                    break;
            }
        }

        private static async Task BanHandler(SocketMessage msg, string command) //Expects to be called from the utilities chain with the keyword 'ban'.
        {
            Data.CommandSplitter(command, out string keyword, out string args);

            switch (keyword)
            {
                case "add":
                    {
                        if (args.Length == 18 && ulong.TryParse(args, out ulong id) && Users.GetUser(id, out Users.User user))
                        {
                            if (!user.isBanned)
                            {
                                user.isBanned = true;
                                await msg.Channel.SendMessageAsync($"{id} added to banlist successfully.");
                            }
                            else { await msg.Channel.SendMessageAsync($"{id} already in banlist."); }
                        }
                        else { await msg.Channel.SendMessageAsync("Invalid ban ID or user not found."); }
                    }
                    break;

                case "pardon":
                    {
                        if (args.Length == 18 && ulong.TryParse(args, out ulong id) && Users.GetUser(id, out Users.User user))
                        {
                            if (user.isBanned)
                            {
                                user.isBanned = false;
                                await msg.Channel.SendMessageAsync($"{id} removed from banlist successfully.");
                            }
                            else { await msg.Channel.SendMessageAsync($"{id} not in banlist."); }
                        }
                        else { await msg.Channel.SendMessageAsync("Invalid ban ID or user not found."); }
                    }
                    break;

                default:
                    await msg.Channel.SendMessageAsync(banHandlerCommandList);
                    break;
            }
        }
    }
}