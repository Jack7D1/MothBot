using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal static class Whitelist
    {
        public const string PATH_WHITELISTS = "../data/whitelists.json";

        private static readonly string COMMANDS = "**Whitelist Commands:**\n```" +
                    "Summary: The whitelist is used to prevent non whitelisted users from joining the current server. Whitelists can only be operated by server administrators.\n" +
            $"{Data.PREFIX} whitelist add [userID]    - Adds user to whitelist in current server\n" +
            $"{Data.PREFIX} whitelist remove [userID] - Removes user from whitelist in current server\n" +
            $"{Data.PREFIX} whitelist populate        - Adds all current users to the whitelist in current server\n" +
            //$"{Data.PREFIX} whitelist list            - Lists all users in the current server's whitelist\n" +
            $"{Data.PREFIX} whitelist toggle          - Toggles the enable state of whitelist enforcement\n" +
            "```";

        private static readonly Dictionary<ulong, Server> whitelists;

        static Whitelist()
        {
            Master.client.LeftGuild += Client_LeftGuild;

            string fileData = Data.Files_Read(PATH_WHITELISTS);
            if (fileData == null || fileData.Length == 0 || fileData == "[]")
            {
                whitelists = new Dictionary<ulong, Server>();
            }
            else
            {
                whitelists = new Dictionary<ulong, Server>(JsonConvert.DeserializeObject<Dictionary<ulong, Server>>(fileData));
            }
        }

        //Ulong key is server ID
        public static async Task CommandHandler(SocketMessage msg, string command)    //Expects to be called from main command tree with the keyword whitelist.
        {
            SocketGuildUser user = msg.Author as SocketGuildUser;

            if (!(user is SocketGuildUser && user.GuildPermissions.Administrator))
            {
                await msg.Channel.SendMessageAsync("You do not have permission to manage the whitelist here! [Administrator]");
                return;
            }
            SocketGuild guild = user.Guild;
            bool haswhitelist = whitelists.TryGetValue(guild.Id, out Server server);

            if (haswhitelist && server.opsOnly && (!Utilities.IsOperator(user)))
            {
                await msg.Channel.SendMessageAsync("You do not have permission to manage the whitelist here! [Bot Operator]");
                return;
            }
            Data.CommandSplitter(command, out string keyword, out string args);

            switch (keyword)
            {
                case "add":
                    if (haswhitelist)
                    {
                        if (server.whitelistedIDs.Contains(ulong.Parse(args)))
                        {
                            await msg.Channel.SendMessageAsync($"User {Master.client.Rest.GetUserAsync(ulong.Parse(args)).Result.Username} already in the whitelist.");
                            break;
                        }
                        else
                            server.whitelistedIDs.Add(ulong.Parse(args));
                    }
                    else
                    {
                        whitelists.Add(guild.Id, new Server(guild.Id, new List<ulong> { ulong.Parse(args) }));
                    }
                    await msg.Channel.SendMessageAsync($"User {Master.client.Rest.GetUserAsync(ulong.Parse(args)).Result.Username} added to whitelist successfully.");
                    SaveWhitelists();
                    break;

                case "remove":
                    if (haswhitelist)
                    {
                        if (server.whitelistedIDs.Contains(ulong.Parse(args)))
                        {
                            server.whitelistedIDs.Remove(ulong.Parse(args));
                            SaveWhitelists();
                            await msg.Channel.SendMessageAsync($"User {Master.client.Rest.GetUserAsync(ulong.Parse(args)).Result.Username} removed from whitelist successfully.");
                        }
                        else
                            await msg.Channel.SendMessageAsync("Invalid input or user not in whitelist");
                    }
                    else
                        await msg.Channel.SendMessageAsync("This server does not have a whitelist! Add users to create one!");
                    break;

                case "populate":
                    if (!haswhitelist)
                    {
                        whitelists.Add(guild.Id, new Server(guild.Id, new List<ulong>()));
                        whitelists.TryGetValue(guild.Id, out server);
                    }
                    await guild.DownloadUsersAsync();
                    foreach (IGuildUser guildUser in guild.Users)
                    {
                        if (!server.whitelistedIDs.Contains(guildUser.Id))
                            server.whitelistedIDs.Add(guildUser.Id);
                    }
                    await msg.Channel.SendMessageAsync($"Whitelist populated successfully with {server.whitelistedIDs.Count} users.");
                    SaveWhitelists();
                    break;

                //case "list":
                //    if (haswhitelist)
                //    {
                //        string outStr = "Whitelisted Users:\n ```";
                //        int i = 0;
                //        foreach (ulong userID in server.whitelistedIDs)
                //        {
                //            i++;
                //            string username = "Unknown";
                //            if (Program.client.Rest.GetUserAsync(userID).Result is IUser usr)
                //                username = usr.Username;
                //            outStr += $"{i}: {username} [{userID}]\n";
                //        }
                //        await msg.Channel.SendMessageAsync(outStr + "```");
                //    }
                //    else
                //        await msg.Channel.SendMessageAsync("This server does not have a whitelist! Add users to create one!");
                //    break;

                case "toggle":
                    if (haswhitelist)
                    {
                        server.enabled = !server.enabled;
                        await msg.Channel.SendMessageAsync($"Whitelist enforcement toggled to {server.enabled}");
                        SaveWhitelists();
                    }
                    else
                        await msg.Channel.SendMessageAsync("This server does not have a whitelist! Add users to create one!");
                    break;

                case "opsonly":
                    if (haswhitelist)
                    {
                        if (Utilities.IsOperator(user))
                        {
                            server.opsOnly = !server.opsOnly;
                            await msg.Channel.SendMessageAsync($"Ops only mode toggled to {server.opsOnly}");
                            SaveWhitelists();
                        }
                        else
                            await msg.Channel.SendMessageAsync("You do not have permission to manage the whitelist here! [Bot Operator]");
                    }
                    else
                        await msg.Channel.SendMessageAsync("This server does not have a whitelist! Add users to create one!");
                    break;

                case "commands":
                    await msg.Channel.SendMessageAsync(COMMANDS);
                    break;
            }
        }

        public static void SaveWhitelists()
        {
            string outStr = JsonConvert.SerializeObject(whitelists, Formatting.Indented);
            Data.Files_Write(PATH_WHITELISTS, outStr);
        }

        public static async Task UserJoined(SocketGuildUser user)
        {
            if (whitelists.TryGetValue(user.Guild.Id, out Server server) && (!server.whitelistedIDs.Contains(user.Id)))
            {
                await user.KickAsync("User not whitelisted.");
            }
        }

        private static async Task Client_LeftGuild(SocketGuild guild)
        {
            whitelists.Remove(guild.Id);
            await Logging.LogtoConsoleandFile($"Left server {guild.Name}, removing from whitelists.");
        }

        private class Server
        {
            public bool enabled;
            public ulong ID;
            public bool opsOnly;
            public List<ulong> whitelistedIDs;

            [JsonConstructor]
            public Server(ulong id, List<ulong> whitelisted, bool opsonly = false, bool enable = true)
            {
                ID = id;
                if (whitelisted != null)
                    whitelistedIDs = whitelisted;
                else
                    whitelistedIDs = new List<ulong>();
                opsOnly = opsonly;
                enabled = enable;
            }
        }
    }
}