using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal static class Utilities
    {
        public const string PATH_BANIDS = "../../data/banids.txt";

        private static readonly string banHandlerCommandList =
            "**Ban ID Management Commands:**\n" +
            "```" +
            $"{Data.PREFIX} utility ban add       Adds an entry to the blacklist\n" +
            $"{Data.PREFIX} utility ban remove    Removes a matching entry from the blacklist\n" +
            $"{Data.PREFIX} utility ban list      Lists the current blacklist\n" +
            "```";

        private static readonly List<ulong> banIDs;

        private static readonly string commandList =
            "**Utility Command List:**\n" +
            "```" +
            "general:\n" +
           $"{Data.PREFIX} utility blacklist [command]\n" +
           $"{Data.PREFIX} utility prependbackupchatters\n" +
            "``````" +
            "data/debug:\n" +
           $"{Data.PREFIX} utility dumpchatters\n" +
           $"{Data.PREFIX} utility dumplogs\n" +
           $"{Data.PREFIX} utility dumpportals\n" +
           $"{Data.PREFIX} utility listservers\n" +
            "``````" +
            "dangerous:\n" +
           $"{Data.PREFIX} utility ban [command]" +
           $"{Data.PREFIX} utility leaveserver [ID]\n" +
           $"{Data.PREFIX} utility shutdown\n" +
            "```";

        private static readonly List<ulong> operatorIDs = new List<ulong>{   //Discord user IDs of allowed operators, in ulong format.
            238735900597682177, //Dex
            206920373952970753, //Jack
        };

        private static bool shutdownEnabled = false;

        private static long shutdownTimeout = 0;

        static Utilities()
        {
            string fileData = Data.Files_Read_String(PATH_BANIDS);
            if (fileData == null || fileData.Length == 0 || fileData == "[]")
            {
                banIDs = new List<ulong>();
            }
            else
            {
                banIDs = new List<ulong>(JsonConvert.DeserializeObject<List<ulong>>(fileData));
            }
        }

        public static async Task BanHandler(SocketMessage msg, string command) //Expects to be called from the utilities chain with the keyword 'ban'.
        {
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

            switch (keyword)
            {
                case "list":
                    {
                        string outStr = "```**Current Banned IDs:**";
                        int count = 0;
                        foreach (ulong entry in banIDs)
                        {
                            outStr += $"\n{count}: {entry}";
                            count++;
                        }
                        outStr += "```";
                        await msg.Channel.SendMessageAsync(outStr);
                    }
                    break;

                case "add":
                    {
                        if (args.Length == 18 && ulong.TryParse(args, out ulong id))
                        {
                            if (!banIDs.Contains(id))
                            {
                                banIDs.Add(id);
                                SaveBanIDs();
                                await msg.Channel.SendMessageAsync($"{id} added to banlist successfully.");
                            }
                            else { await msg.Channel.SendMessageAsync("ID already in banlist."); }
                        }
                        else { await msg.Channel.SendMessageAsync("Invalid ban ID."); }
                    }
                    break;

                case "remove":
                    {
                        if (args.Length == 18 && ulong.TryParse(args, out ulong id))
                        {
                            if (banIDs.Remove(id))
                            {
                                SaveBanIDs();
                                await msg.Channel.SendMessageAsync($"{id} removed from banlist successfully.");
                            }
                            else { await msg.Channel.SendMessageAsync("ID not found in banlist."); }
                        }
                        else { await msg.Channel.SendMessageAsync("Invalid ban ID."); }
                    }
                    break;

                default:
                    await msg.Channel.SendMessageAsync(banHandlerCommandList);
                    break;
            }
        }

        public static async Task Client_JoinedGuild(SocketGuild guild)
        {
            if (IsBanned(guild.Owner))
            {
                await guild.LeaveAsync();
                await Logging.LogtoConsoleandFileAsync($"Attempted to join guild owned by banned user {guild.OwnerId}, guild left.");
            }
        }

        public static bool IsBanned(SocketUser user)
        {
            return banIDs.Contains(user.Id);
        }

        public static bool IsOperator(SocketUser user)
        {
            foreach (ulong op in operatorIDs)
                if (user.Id == op)
                    return true;
            return false;
        }

        public static void SaveBanIDs()
        {
            List<ulong> newBanIDs = new List<ulong>();
            foreach (ulong banID in banIDs)
            {
                if (!newBanIDs.Contains(banID))
                    newBanIDs.Add(banID);
            }
            banIDs.Clear();
            banIDs.AddRange(newBanIDs);
            Data.Files_Write(PATH_BANIDS, JsonConvert.SerializeObject(banIDs, Formatting.Indented));
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
                    await Chatterbot.PrependBackupChatters(src.Channel);
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
                case "ban":
                    await BanHandler(src, args);
                    break;

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
                    await src.Channel.SendMessageAsync(commandList);
                    break;
            }
        }
    }
}