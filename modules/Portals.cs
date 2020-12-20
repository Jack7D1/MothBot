using Discord;
using Discord.Rest;
using Discord.WebSocket;
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
    }

    internal class Portals
    {
        private const long COOLDOWN_MS = 1000;  //Cooldown to try and avoid the inevitable super spam.
        private const string PORTALS_PATH = @"..\..\data\portals.txt";
        private static long timeReady = 0;
        private readonly List<Portal> portals = new List<Portal>();
        /*Savedata structure:
         * File header contains 1 line:
         * The string "BEGIN PORTALS"
         *
         * Each portal channel entry contains 2 lines:
         * The ulong 'guildID' which represents the ID of the guild the portal belongs to
         * The ulong 'channelID' which represents the ID of the channel the portal targets
         *
         * If there are no portal entries the string "NO PORTALS" is saved instead
         *
         * Footer is the string "END PORTALS"
         */

        public Portals()
        {
            Program.client.ChannelDestroyed += ChannelDestroyed;
            Program.client.LeftGuild += LeftGuild;
            try
            {
                List<string> fileData = Lists.ReadFile(PORTALS_PATH);
                if (fileData == null)
                    throw new Exception("NO FILEDATA");
                int endIndex = fileData.IndexOf("END PORTALS");
                //Check Header
                if (fileData.Count == 0 || fileData[0] != "BEGIN PORTALS" || endIndex == -1)
                    throw new Exception("ERROR PARSING PORTALS");

                //Main data parsing
                if (fileData[1] != "NO PORTALS")
                    for (int i = 1; i < endIndex; i += 2)
                        portals.Add(new Portal(ulong.Parse(fileData[i]), ulong.Parse(fileData[i + 1])));
            }
            catch (Exception e) when (e.Message == "ERROR PARSING PORTALS")
            {
                Program.logging.LogtoConsoleandFile($"FATAL IN {this}: Error parsing \"{PORTALS_PATH}\", data is either corrupted or missing, file will be overwritten on application close.");
                portals.Clear();
                return;
            }
            catch (Exception e) when (e.Message == "NO FILEDATA")
            {
                Program.logging.LogtoConsoleandFile($"{this} reports: No file data found, running with empty list of portals.");
                portals.Clear();
                return;
            }
        }

        public async Task BroadcastHandlerAsync(SocketMessage msg) //Recieves every message the bot sees
        {
            if (GetPortal(msg.Channel) is Portal)
            {
                if (DateTime.Now.Ticks < timeReady)
                {
                    RestMessage sentmsg = msg.Channel.SendMessageAsync("The portal is cooling down!").Result;
                    await Task.Delay(500);
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

        public Portal GetPortal(IMessageChannel ch) //If not a portal returns null
        {
            foreach (Portal portal in portals)
                if (portal.channelId == ch.Id)
                    return portal;
            return null;
        }

        public Task PortalManagement(SocketMessage msg, string args)    //Expects to be called when the keyword is "portal"
        {
            SocketGuildUser user = msg.Author as SocketGuildUser;
            if (user.GuildPermissions.Administrator)    //Only guild admins can designate a portal channel.
                switch (args)
                {
                    case "create":
                        if (!GuildHasPortal(user.Guild))
                        {
                            Portal portal = new Portal(user.Guild.Id, msg.Channel.Id);
                            portals.Add(portal);
                            msg.Channel.SendMessageAsync($"This channel successfully added as a portal! To remove as a portal say \"{Program._prefix} portal delete\" or delete this channel!");
                            Program.logging.LogtoConsoleandFile($"Portal created at {user.Guild.Name} [{msg.Channel.Name}]");
                        }
                        else
                            msg.Channel.SendMessageAsync("This server already has a portal!");
                        return Task.CompletedTask;

                    case "delete":
                        {
                            if (GetPortal(msg.Channel) is Portal portal)
                            {
                                portals.Remove(portal);
                                msg.Channel.SendMessageAsync("Portal successfully deleted");
                                Program.logging.LogtoConsoleandFile($"Portal deleted at {user.Guild.Name} [{msg.Channel.Name}]");
                            }
                            else
                                msg.Channel.SendMessageAsync("This channel is not a portal!");
                            return Task.CompletedTask;
                        }
                    default:
                        msg.Channel.SendMessageAsync($"Unknown command, say \"{Program._prefix} portal create\" or \"{Program._prefix} portal delete\" to manage portals!");
                        return Task.CompletedTask;
                }
            else
                msg.Channel.SendMessageAsync("You lack the required permissions to manage portals for this server! [Administrator]");
            return Task.CompletedTask;
        }

        public void SavePortals()
        {
            List<string> outList = new List<string> { "BEGIN PORTALS" };  //header
            if (portals.Count != 0)
                foreach (Portal portal in portals)
                {
                    outList.Add($"{portal.guildId}");
                    outList.Add($"{portal.channelId}");
                }
            else
                outList.Add("NO PORTALS");
            outList.Add("END PORTALS");
            Lists.WriteFile(PORTALS_PATH, outList);
        }

        private async Task BroadcastAsync(SocketMessage msg) //Passing a socketmessage to here will cause it to be relayed to every portal channel instance.
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

        private Task ChannelDestroyed(SocketChannel arg)
        {
            CheckPortals();
            return Task.CompletedTask;
        }

        private Task CheckPortals()
        {
            List<Portal> portaldupe = portals;
            foreach (Portal portal in portaldupe)
                if (!(portal.GetChannel() is IMessageChannel))
                    portals.Remove(portal);
            return Task.CompletedTask;
        }

        private bool GuildHasPortal(SocketGuild guild)   //Returns true if the input guild has a portal already
        {
            foreach (Portal portal in portals)
                if (guild.Id == portal.guildId)
                    return true;
            return false;
        }

        private Task LeftGuild(SocketGuild arg)
        {
            CheckPortals();
            return Task.CompletedTask;
        }
    }
}