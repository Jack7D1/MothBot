using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Portals
    {
        private static readonly List<Portal> portals = new List<Portal>();

        public Portals()
        {
            Program.client.ChannelDestroyed += ChannelDestroyed;
            Program.client.LeftGuild += LeftGuild;
            try
            {
                string fileData = Data.Files_Read_String(Data.PATH_PORTALS);
                if (fileData == null || fileData.Length == 0)
                    throw new Exception("NO FILEDATA");
                List<Portal> filePortals = JsonConvert.DeserializeObject<List<Portal>>(fileData);
                foreach (Portal portal in filePortals)
                    portals.Add(portal);
                CheckPortals();
            }
            catch (Exception e) when (e.Message == "NO FILEDATA")
            {
                Logging.LogtoConsoleandFile($"No portal data found at {Data.PATH_PORTALS}, running with empty list of portals.");
                portals.Clear();
                return;
            }
        }

        public static async Task BroadcastHandlerAsync(SocketMessage msg) //Recieves every message the bot sees
        {
            if (IsPortal(msg.Channel) && !msg.Content.StartsWith(Data.PREFIX))
            {
                if (Chatterbot.ContentsBlacklisted(msg.Content))
                    await BroadcastAsync(msg);
            }
        }

        public static bool IsPortal(IMessageChannel ch)
        {
            if (GetPortal(ch.Id) is Portal && !(ch as ITextChannel).IsNsfw)
                return true;
            return false;
        }

        public static async Task PortalManagement(SocketMessage msg, string args)    //Expects to be called when the keyword is "portal", 'args' is expected to be everything following keyword, minus space.
        {
            SocketGuildUser user = msg.Author as SocketGuildUser;
            if (args == "list")
            {
                await ListPortals(msg.Channel);
            }
            else if (user is SocketGuildUser && user.GuildPermissions.ManageChannels)    //Only guild admins can designate a portal channel.
                switch (args)
                {
                    case "open":
                        if (!GuildHasPortal(user.Guild.Id))
                        {
                            if (!(msg.Channel as ITextChannel).IsNsfw)
                            {
                                Portal portal = new Portal(user.Guild.Id, msg.Channel.Id);
                                portals.Add(portal);
                                await msg.Channel.SendMessageAsync($"Portal opened in this channel! To remove as a portal say \"{Data.PREFIX} portal close\" or delete this channel!");
                                await Logging.LogtoConsoleandFile($"Portal created at {user.Guild.Name} [{msg.Channel.Name}]");
                                await CheckPortals();
                                SavePortals();
                            }
                            else
                                await msg.Channel.SendMessageAsync("NSFW channels cannot be portals!");
                        }
                        else
                            await msg.Channel.SendMessageAsync("This server already has a portal!");
                        break;

                    case "close":
                        {
                            if (GetPortal(msg.Channel.Id) is Portal portal)
                            {
                                portals.Remove(portal);
                                await msg.Channel.SendMessageAsync("Portal successfully closed");
                                await Logging.LogtoConsoleandFile($"Portal deleted at {user.Guild.Name} [{msg.Channel.Name}]");
                                await CheckPortals();
                                SavePortals();
                            }
                            else
                                await msg.Channel.SendMessageAsync("This channel is not a portal!");
                            break;
                        }
                    default:
                        await msg.Channel.SendMessageAsync($"Unknown command, say \"{Data.PREFIX} portal open\" or \"{Data.PREFIX} portal close\" to manage portals!");
                        break;
                }
            else
                await msg.Channel.SendMessageAsync("You lack the required permissions to manage portals for this server! [ManageChannels]");
        }

        public static void SavePortals()
        {
            string outStr = JsonConvert.SerializeObject(portals, Formatting.Indented);
            Data.Files_Write(Data.PATH_PORTALS, outStr);
        }

        private static async Task BroadcastAsync(SocketMessage msg) //Passing a socketmessage to here will cause it to be relayed to every portal channel instance.
        {
            List<Portal> portaldupe = portals;
            foreach (Portal portal in portaldupe)
                if (portal.GetChannel() is IMessageChannel ch)
                {
                    if (msg.Channel != ch)
                        await ch.SendMessageAsync($"*{msg.Author.Username} in [{(msg.Author as IGuildUser).Guild.Name}] says* \"{Sanitize.ScrubRoleMentions(msg.Content)}\"");
                }
                else
                    portals.Remove(portal);
        }

        private static Task ChannelDestroyed(SocketChannel arg)
        {
            CheckPortals();
            return Task.CompletedTask;
        }

        private static Task CheckPortals()
        {
            List<Portal> toRemove = new List<Portal>();
            List<ulong> seenGuildIDs = new List<ulong>();   //Enforce one portal per guild
            foreach (Portal portal in portals)
            {
                if (!(portal.GetChannel() is IMessageChannel) || seenGuildIDs.Contains(portal.guildId))
                    toRemove.Add(portal);
                else
                    seenGuildIDs.Add(portal.guildId);
            }
            foreach (Portal portal in toRemove)
                portals.Remove(portal);
            return Task.CompletedTask;
        }

        private static Portal GetPortal(ulong ch) //If not a portal returns null
        {
            foreach (Portal portal in portals)
                if (portal.channelId == ch)
                    return portal;
            return null;
        }

        private static bool GuildHasPortal(ulong guildId)   //Returns true if the input guild has a portal already
        {
            foreach (Portal portal in portals)
                if (guildId == portal.guildId)
                    return true;
            return false;
        }

        private static Task LeftGuild(SocketGuild arg)
        {
            CheckPortals();
            return Task.CompletedTask;
        }

        private static Task ListPortals(ISocketMessageChannel ch)
        {
            CheckPortals();
            if (portals.Count > 0)
            {
                string outStr = "**OPEN PORTALS:** ```";
                for (int i = 0; i < portals.Count; i++)
                {
                    IMessageChannel portalCh = portals[i].GetChannel();
                    IGuild portalGuild = portals[i].GetGuild();
                    outStr += $"{i + 1}: \"{portalCh.Name}\" in {portalGuild.Name}.\n";
                }
                ch.SendMessageAsync(outStr + "```");
            }
            else
                ch.SendMessageAsync("No portals found!");
            return Task.CompletedTask;
        }

        private class Portal
        {
            public ulong channelId;

            public ulong guildId;

            [JsonConstructor]
            public Portal(ulong serverId, ulong chId)
            {
                guildId = serverId;
                channelId = chId;
            }

            public IMessageChannel GetChannel() //returns null if not found
            {
                return Program.restClient.GetChannelAsync(channelId).Result as IMessageChannel;
            }

            public IGuild GetGuild()    //returns null if not found
            {
                return Program.restClient.GetGuildAsync(guildId).Result;
            }
        }
    }
}