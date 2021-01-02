using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Portal
    {
        public readonly ulong channelId;
        public readonly ulong guildId;

        public Portal(ulong serverId, ulong chId)
        {
            guildId = serverId;
            channelId = chId;
        }

        public IMessageChannel GetChannel() //returns null if not found
        {
            return Program.client.GetChannel(channelId) as IMessageChannel;
        }

        public IGuild GetGuild()    //returns null if not found
        {
            return Program.client.GetGuild(guildId);
        }

        public string GetJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    internal class Portals
    {
        private const long COOLDOWN_MS = 1000;  //Cooldown to try and avoid the inevitable super spam.
        private const string PORTALS_PATH = @"..\..\data\portals.json";
        private static List<Portal> portals = new List<Portal>();
        private static long timeReady = 0;

        public Portals()
        {
            Program.client.ChannelDestroyed += ChannelDestroyed;
            Program.client.LeftGuild += LeftGuild;
            try
            {
                string fileData = Lists.ReadFile(PORTALS_PATH)[0];
                portals = JsonConvert.DeserializeObject<List<Portal>>(fileData);   
            }
            catch (Exception e) when (e.Message == "NO FILEDATA")
            {
                Program.logging.LogtoConsoleandFile($"{this} reports: No file data found, running with empty list of portals.");
                portals.Clear();
                return;
            }
        }

        public static async Task BroadcastHandlerAsync(SocketMessage msg) //Recieves every message the bot sees
        {
            if (GetPortal(msg.Channel) is Portal)
            {
                if (DateTime.Now.Ticks < timeReady)
                {
                    RestMessage sentmsg = msg.Channel.SendMessageAsync("The portal is cooling down!").Result;
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                    await sentmsg.DeleteAsync();
                }
                else
                {
                    timeReady = DateTime.Now.AddMilliseconds(COOLDOWN_MS).Ticks;
                    await BroadcastAsync(msg);
                }
            }
        }

        public static Portal GetPortal(IMessageChannel ch) //If not a portal returns null
        {
            foreach (Portal portal in portals)
                if (portal.channelId == ch.Id)
                    return portal;
            return null;
        }

        public static async Task PortalManagement(SocketMessage msg, string args)    //Expects to be called when the keyword is "portal"
        {
            SocketGuildUser user = msg.Author as SocketGuildUser;
            if (args == "list")
            {
                await ListPortals(msg.Channel);
            }
            else if (user is SocketGuildUser && user.GuildPermissions.Administrator)    //Only guild admins can designate a portal channel.
                switch (args)
                {
                    case "open":
                        if (!GuildHasPortal(user.Guild))
                        {
                            Portal portal = new Portal(user.Guild.Id, msg.Channel.Id);
                            portals.Add(portal);
                            await msg.Channel.SendMessageAsync($"Portal opened in this channel! To remove as a portal say \"{Program._prefix} portal close\" or delete this channel!");
                            Program.logging.LogtoConsoleandFile($"Portal created at {user.Guild.Name} [{msg.Channel.Name}]");
                        }
                        else
                            await msg.Channel.SendMessageAsync("This server already has a portal!");
                        break;

                    case "close":
                        {
                            if (GetPortal(msg.Channel) is Portal portal)
                            {
                                portals.Remove(portal);
                                await msg.Channel.SendMessageAsync("Portal successfully closed");
                                Program.logging.LogtoConsoleandFile($"Portal deleted at {user.Guild.Name} [{msg.Channel.Name}]");
                            }
                            else
                                await msg.Channel.SendMessageAsync("This channel is not a portal!");
                            break;
                        }
                    default:
                        await msg.Channel.SendMessageAsync($"Unknown command, say \"{Program._prefix} portal open\" or \"{Program._prefix} portal close\" to manage portals!");
                        break;
                }
            else
                await msg.Channel.SendMessageAsync("You lack the required permissions to manage portals for this server! [Administrator]");
        }

        public static void SavePortals()
        {
            string outStr = JsonConvert.SerializeObject(portals);
            Lists.WriteFile(PORTALS_PATH, outStr.ToCharArray());
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
            List<Portal> portaldupe = portals;
            foreach (Portal portal in portaldupe)
                if (!(portal.GetChannel() is IMessageChannel))
                    portals.Remove(portal);
            return Task.CompletedTask;
        }

        private static bool GuildHasPortal(SocketGuild guild)   //Returns true if the input guild has a portal already
        {
            foreach (Portal portal in portals)
                if (guild.Id == portal.guildId)
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
    }
}