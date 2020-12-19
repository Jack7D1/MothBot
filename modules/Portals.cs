using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Portal
    {
        public readonly ulong id;
        public readonly string portalName;
        public readonly bool visible = false;

        public Portal(ulong inId, string str)
        {
            portalName = str;
            id = inId;
            if (Program.client.GetChannel(id) is IMessageChannel messageChannel)
            {
                visible = true;
                if (messageChannel.Name != portalName)
                    portalName = messageChannel.Name;
            }
        }
    }

    internal class Portals
    {
        private const long COOLDOWN_MS = 1000;  //1 second cooldown, to avoid inevitable super spam.
        private const string PORTALS_PATH = @"..\..\data\portals.txt";
        private static long timeReady = 0;
        private readonly List<Portal> portals = new List<Portal>();
        /*Savedata structure:
         * File header contains 1 line:
         * The string "BEGIN PORTALS"
         *
         * Each portal channel entry contains 2 lines:
         * The ulong 'channelID'
         * The string 'portalName;
         *
         * If there are no portal entries the string "NO PORTALS" is saved instead
         *
         * Footer is the string "END PORTALS"
         */

        public Portals()
        {
            Program.client.ChannelDestroyed += CheckPortals;
            try
            {
                List<string> fileData = Lists.ReadFile(PORTALS_PATH);
                if (fileData == null)
                    throw new Exception("NO FILEDATA");
                int endIndex = fileData.IndexOf("END PORTALS");
                //Check Header
                if (fileData.Count == 0 || fileData[0] != "BEGIN PORTALS" || endIndex == -1)
                    throw new Exception("ERROR PARSING PORTALS");

                string portalIds = "";
                //Main data parsing
                if (fileData[1] != "NO PORTALS")
                {
                    for (int i = 1; i < endIndex; i += 2)
                        portals.Add(new Portal(ulong.Parse(fileData[i]), fileData[i + 1]));
                    foreach (Portal portal in portals)
                        portalIds += portal.id;
                }
                else
                {
                    portals.Clear();
                    return;
                }
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

        public async Task PortalHandlerAsync(SocketMessage msg) //Recieves every message the bot sees
        {
            if (IsPortal(msg.Channel))
            {
                if (DateTime.Now.Ticks < timeReady)
                    await msg.Channel.SendMessageAsync("The portal is cooling down!");
                else
                {
                    timeReady = DateTime.Now.AddMilliseconds(COOLDOWN_MS).Ticks;
                    await BroadcastAsync(msg);
                }
            }
        }

        public Task PortalManagement(SocketMessage msg, string args)    //Expects to be called when the keyword is "portal"
        {
            SocketGuildUser user = msg.Author as SocketGuildUser;
            if (user.GuildPermissions.Administrator)    //Only guild admins can designate a portal channel.
                switch (args)
                {
                    case "create":
                        if (!IsPortal(msg.Channel))
                        {
                            Portal portal = new Portal(msg.Channel.Id, msg.Channel.Name);
                            portals.Add(portal);
                            msg.Channel.SendMessageAsync($"This channel successfully added as a portal! To remove as a portal say \"{Program._prefix} portal delete\" or delete this channel!");
                        }
                        else
                            msg.Channel.SendMessageAsync("This channel is already a portal!");
                        return Task.CompletedTask;

                    case "delete":
                        foreach (Portal portal in portals)
                            if (portal.id == msg.Channel.Id)
                            {
                                portals.Remove(portal);
                                msg.Channel.SendMessageAsync("Portal successfully deleted");
                                return Task.CompletedTask;
                            }
                        msg.Channel.SendMessageAsync("This channel is not a portal!");
                        return Task.CompletedTask;

                    default:
                        msg.Channel.SendMessageAsync($"Unknown command, say \"{Program._prefix} portal create\" or \"{Program._prefix} portal delete\" to manage portals!");
                        return Task.CompletedTask;
                }
            else
                msg.Channel.SendMessageAsync("You lack the required permissions to manage portals for this server!");
            return Task.CompletedTask;
        }

        public void SavePortals()
        {
            List<string> outList = new List<string> { "BEGIN PORTALS" };  //header
            string portalIds = "";
            if (portals.Count != 0)
                foreach (Portal portal in portals)
                {
                    portalIds += portal.id;
                    outList.Add($"{portal.id}");
                    outList.Add(portal.portalName);
                }
            else
                outList.Add("NO PORTALS");
            outList.Add("END PORTALS");
            Task.Delay(500);
            Lists.WriteFile(PORTALS_PATH, outList);
        }

        private async Task BroadcastAsync(SocketMessage msg) //Passing a socketmessage to here will cause it to be relayed to every portal channel instance.
        {
            List<Portal> portaldupe = portals;
            foreach (Portal portal in portaldupe)
                if (portal.visible == true && Program.client.GetChannel(portal.id) is IMessageChannel ch)
                {
                    if (msg.Channel != ch)
                        await ch.SendMessageAsync($"*{msg.Author.Username} in [{msg.Channel.Name}] says* \"{msg.Content}\"");
                }
                else
                    portals.Remove(portal);
        }

        private Task CheckPortals(SocketChannel ch)
        {
            List<Portal> portaldupe = portals;
            foreach (Portal portal in portaldupe)
                if (ch.Id == portal.id)
                {
                    portals.Remove(portal);
                    break;
                }
            return Task.CompletedTask;
        }

        private bool IsPortal(ISocketMessageChannel ch)
        {
            foreach (Portal portal in portals)
                if (portal.id == ch.Id)
                    return true;
            return false;
        }
    }
}