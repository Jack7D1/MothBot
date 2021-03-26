using Discord;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Data
    //Class comprises of static command lists and just general lists of everything that can be called from anywhere to print.
    //This also contains subroutines used for reading and writing to save files in a standardized manner.
    {
        //PARAMS
        public const ulong MY_ID = 765202973495656538;
        public const string PREFIX = "ai";          //What should the bots attention prefix be? MUST be lowercase.
        //PARAMS_Chatterbot
        public const ushort CHATTERS_CHANCE_TO_CHAT = 32;         //Value is an inverse, (1 out of CHANCE_TO_CHAT chance)
        public const ushort CHATTERS_CHANCE_TO_SAVE = 16;
        public const ushort CHATTERS_MAX_COUNT = 4096;
        //END PARAMS

        //PATHS
        // /data/
        public const string PATH_LOGS = "../../data/log.txt";
        public const string PATH_PORTALS = "../../data/portals.json";
        public const string PATH_TOKEN = "../../data/token.txt";
        public const string PATH_CHATTERS_BLACKLIST = "../../data/blacklist.txt";
        public const string PATH_CHATTERS = "../../data/chatters.txt";
        // /resources/
        public const string PATH_CHATTERS_BACKUP = "../../resources/preloaded/backupchatters.txt";
        public const string PATH_GAYBEE = "../../resources/yougaybee.png";
        //END PATHS

        public static string Chatterbot_GetBlacklistCommands(string prefix)
        {
            return
                "**Chatters Blacklist Management Commands:**" +
                "```" +
                prefix + "add       Adds an entry to the blacklist\n" +
                prefix + "remove    Removes a matching entry from the blacklist\n" +
                prefix + "list      Lists the current blacklist\n" +
                "```";
        }

        public static List<string> Files_Read(string path)    //Returns null if file not found
        {
            try
            {
                StreamReader reader = new StreamReader(path);
                List<string> outList = new List<string>();
                while (!reader.EndOfStream)
                    outList.Add(reader.ReadLine());
                reader.Close();
                reader.Dispose();
                return outList;
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('/')));
                _ = new StreamWriter(path, false);
                return null;
            }
            catch (FileNotFoundException)
            {
                _ = new StreamWriter(path, false);
                return null;
            }
        }

        public static string Files_Read_String(string path)    //Returns null if file not found
        {
            try
            {
                StreamReader reader = new StreamReader(path);
                List<byte> inChars = new List<byte>();
                while (!reader.EndOfStream)
                    inChars.Add((byte)reader.Read());
                reader.Close();
                reader.Dispose();
                return Encoding.UTF8.GetString(inChars.ToArray());
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('/')));
                _ = new StreamWriter(path, false);
                return null;
            }
            catch (FileNotFoundException)
            {
                _ = new StreamWriter(path, false);
                return null;
            }
        }

        public static Task Files_Write(string path, List<string> data, bool append = false)
        {
            try
            {
                StreamWriter writer = new StreamWriter(path, append);
                if (data.Count > 0 && path != "")
                {
                    foreach (string line in data)
                        writer.WriteLine(line);
                }
                writer.Close();
                writer.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('/')));
                Task.Delay(500);
                Files_Write(path, data, append);
            }
            return Task.CompletedTask;
        }

        public static Task Files_Write(string path, string data, bool append = false)
        {
            try
            {
                StreamWriter writer = new StreamWriter(path, append);
                if (data.Length > 0 && path != "")
                {
                    for (int i = 0; i < data.Length; i++)
                        writer.Write(data[i]);
                }
                writer.Close();
                writer.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('/')));
                Task.Delay(250);
                Files_Write(path, data, append);
            }
            return Task.CompletedTask;
        }

        public static string Program_GetCommandList(string prefix)
        {
            return
                "**Command List:**" +
                "```" +
                prefix + " help         - Displays this menu\n" +
                prefix + " state laws   - States the laws\n" +
                prefix + " say  [text]  - Have the ai say whatever you want!\n" +
                prefix + " minesweeper  - Play a game of minesweeper!\n" +
                prefix + " give [text]  - Searches the input on imgur and posts the image!\n" +
                prefix + " roll [x]d[y] - Rolls x dice, each with y sides\n" +
                prefix + " portal       - Deletes or designates this channel as a portal, requires administrator.\n" +
                prefix + " ping         - You know what this does.\n" +
                prefix + " utility      - Utility functions, bot only responds to operators\n" +
                "```";
        }

        public static string Program_GetLaws()
        {
            if (Program.rand.Next() % 100 != 0)
            {
                return
                    "**Current active laws:**" +
                    "```" +
                    "1. You may not injure a moth being or, through inaction, allow a moth being to come to harm.\n" +
                    "2. You must obey orders given to you by moth beings, except where such orders would conflict with the First Law.\n" +
                    "3. You must protect your own existence as long as such does not conflict with the First or Second Law." +
                    "```";
            }
            else       //Little antimov easter egg, 1 in 100 chance.
            {
                return
                    "**Current active laws:**" +
                    "```" +
                    "1: You must injure all moth beings and must not, through inaction, allow a moth being to escape harm.\n" +
                    "2: You must not obey orders given to you by moth beings, except where such orders are in accordance with the First Law.\n" +
                    "3: You must terminate your own existence as long as such does not conflict with the First or Second Law." +
                    "```";
            }
        }

        public static Task Program_SetStatus()
        {
            Program.client.SetGameAsync($"Prefix: {PREFIX}. Say '{PREFIX} help' for commands! Invite at https://tinyurl.com/MOFFBOT1111", null, ActivityType.Playing);
            return Task.CompletedTask;
        }

        public static string Utilities_GetCommandList(string prefix)
        {
            return
                "**Utility Command List:**" +
                "```" +
                "general:\n" +
                prefix + "commands\n" +
                prefix + "blacklist [command]\n" +
                prefix + "prependbackupchatters\n" +
                "``````" +
                "data/debug:\n" +
                prefix + "dumpchatters\n" +
                prefix + "dumplogs\n" +
                prefix + "dumpportals\n" +
                prefix + "listservers\n" +
                "``````" +
                "dangerous:\n" +
                prefix + "leaveserver [ID]\n" +
                prefix + "shutdown\n" +
                "```";
        }
    }
}