using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace MothBot.modules
{
    class Portals
    {
        private const string PORTALS_PATH = @"..\..\data\portals.txt";
        private readonly Dictionary<string, Portal> portals = new Dictionary<string, Portal>();
        /*Savedata structure:
         * File header contains 1 line:
         * The string "BEGIN PORTALS"
         * 
         * Each portal channel entry contains 2 lines:
         * The ulong 'channelID'
         * The string 'portalName;
         * 
         * Footer contains 3 lines:
         * The string "EOF"
         * The md5 hash of all portalnames placed end-to-end in a string, cast as a hex to base64 value.
         * The md5 hash of all channelIDs placed end-to-end in a string, cast as a hex to base64 value.
         */

        public Portals()
        {
            Program.client.ChannelDestroyed += ChannelDestroyed;
            Program.client.LeftGuild += LeftGuild;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(SavePortals);
            try
            {
                StreamReader reader = new StreamReader(PORTALS_PATH);
                //Check Header
                if (reader.ReadLine() != "BEGIN PORTALS")
                { reader.Close(); throw new Exception(); }
                //Main data parsing
                string nextStr;
                List<string> portalNames = new List<string>();
                List<ulong> portalIds = new List<ulong>();
                while(!reader.EndOfStream)
                {
                    nextStr = reader.ReadLine();
                    if (nextStr == "EOF")
                        break;
                    ulong chId = ulong.Parse(nextStr);
                    string portalName = reader.ReadLine();
                    Portal portal = new Portal(portalName, chId);
                    portals.Add(portalName, portal);
                    portalNames.Add(portalName);
                    portalIds.Add(chId);
                }
                //Check against footer
                reader.Close();
            }
            catch (FileNotFoundException)
            {
                portals.Clear();
                _ = new StreamWriter(PORTALS_PATH, false);
                return;
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(PORTALS_PATH.Substring(0, PORTALS_PATH.LastIndexOf('\\')));
                _ = new StreamWriter(PORTALS_PATH, false);
                return;
            }
            finally
            {
                Program.logging.LogtoConsoleandFile($"FATAL IN {this}: Error parsing {PORTALS_PATH}, data is either corrupted or missing, file will be overwritten on application close.");
                portals.Clear();
            }
        }

        private void SavePortals(object sender, EventArgs e)
        {
            
        }

        private Task ChannelDestroyed(SocketChannel arg)
        {
            return Task.CompletedTask;
        }

        private static Task LeftGuild(SocketGuild guild)
        {
            return Task.CompletedTask;
        }
    }

    class Portal
    {
        public readonly string portalName;
        public readonly ulong channelID;

        public Portal(string str, ulong id)
        {
            portalName = str;
            channelID = id;
        }
    }
}
