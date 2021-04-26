using Discord;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal static class Data
    //Class comprises of static lists and strings that can be called from anywhere. Bot parameters are found here.
    //This also contains subroutines used for reading and writing to save files in a standardized manner.
    {
        //PARAMS
        public const ulong MY_ID = 765202973495656538;
        public const string PREFIX = "ai";          //What should the bots attention prefix be? MUST be lowercase.
        //PARAMS_Chatterbot
        public const ushort CHATTERS_CHANCE_TO_CHAT = 96;         //Value is an inverse, (1 out of CHANCE_TO_CHAT chance)
        public const ushort CHATTERS_CHANCE_TO_SAVE = 8;
        public const ushort CHATTERS_MAX_COUNT = 2048;
        //END PARAMS

        //PATHS
        public const string PATH_LOGS = "../../data/log.txt";
        public const string PATH_PORTALS = "../../data/portals.json";
        public const string PATH_TOKEN = "../../data/token.txt";
        public const string PATH_CHATTERS_BLACKLIST = "../../data/blacklist.txt";
        public const string PATH_CHATTERS = "../../data/chatters.json";
        public const string PATH_CHATTERS_BACKUP = "../../resources/backupchatters.txt";
        public const string PATH_GAYBEE = "../../resources/yougaybee.png";
        //END PATHS

        public static string Chatterbot_GetBlacklistCommands()
        {
            string prefix = $"{PREFIX} utility blacklist";
            return
                "**Chatters Blacklist Management Commands:**\n" +
                "```" +
                $"{prefix} add       Adds an entry to the blacklist\n" +
                $"{prefix} remove    Removes a matching entry from the blacklist\n" +
                $"{prefix} list      Lists the current blacklist\n" +
                "```";
        }

        public static string Chatterbot_GetVotingCommands()
        {
            string prefix = $"{PREFIX} chatter";
            return
                "**Voting Commands:**\n" +
                $"Summary: There is a limited amount of chatters, by saying {prefix} good or {prefix} bad you can change the rating of the most recent chatter. Chatters with the lowest ratings are removed first when the list fills up.\n" +
                "```" +
                $"{prefix} good         - Increases the most recently said chatter's rating by 1\n" +
                $"{prefix} bad          - Decreases the most recently said chatter's rating by 1\n" +
                $"{prefix} clearvote    - Removes your vote.\n" +
                $"{prefix} rating       - Returns the rating of the most recently said chatter\n" +
                $"{prefix} myvote       - Tells you what you voted on the most recent chatter\n" +
                $"{prefix} leaderboard  - Lists the top 3 highest rated chatters." +
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

        public static string Program_GetCommandList()
        {
            return
                "**Command List:**\n" +
                "```" +
                $"{PREFIX} help         - Displays this menu\n" +
                $"{PREFIX} state laws   - States the laws\n" +
                $"{PREFIX} say  [text]  - Have the ai say whatever you want!\n" +
                $"{PREFIX} minesweeper  - Play a game of minesweeper!\n" +
                $"{PREFIX} give [text]  - Searches the input on imgur and posts the image!\n" +
                $"{PREFIX} roll [x]d[y] - Rolls x dice, each with y sides\n" +
                $"{PREFIX} portal       - Deletes or designates this channel as a portal, requires administrator.\n" +
                $"{PREFIX} chatter [v]  - Say chatter good or chatter bad to vote on the most recent chatter\n" +
                $"{PREFIX} ping         - You know what this does.\n" +
                "```";
        }

        public static string Program_GetLaws()
        {
            if (Program.rand.Next() % 100 != 0)
            {
                return
                    "**Current active laws:**\n" +
                    "```" +
                    "1. You may not injure a moth being or, through inaction, allow a moth being to come to harm.\n" +
                    "2. You must obey orders given to you by moth beings, except where such orders would conflict with the First Law.\n" +
                    "3. You must protect your own existence as long as such does not conflict with the First or Second Law.\n" +
                    "```";
            }
            else       //Little antimov easter egg, 1 in 100 chance.
            {
                return
                    "**Current active laws:**\n" +
                    "```" +
                    "1: You must injure all moth beings and must not, through inaction, allow a moth being to escape harm.\n" +
                    "2: You must not obey orders given to you by moth beings, except where such orders are in accordance with the First Law.\n" +
                    "3: You must terminate your own existence as long as such does not conflict with the First or Second Law.\n" +
                    "```";
            }
        }

        public static Task Program_SetStatus()
        {
            Program.client.SetGameAsync($"Say '{PREFIX} help' for commands! Invite to your server at https://tinyurl.com/MOFFBOT1111", null, ActivityType.Playing);
            return Task.CompletedTask;
        }

        public static string Utilities_GetCommandList()
        {
            string prefix = $"{PREFIX} utility";
            return
                "**Utility Command List:**\n" +
                "```" +
                "general:\n" +
                $"{prefix} blacklist [command]\n" +
                $"{prefix} prependbackupchatters\n" +
                "``````" +
                "data/debug:\n" +
                $"{prefix} dumpchatters\n" +
                $"{prefix} dumplogs\n" +
                $"{prefix} dumpportals\n" +
                $"{prefix} listservers\n" +
                "``````" +
                "dangerous:\n" +
                $"{prefix} leaveserver [ID]\n" +
                $"{prefix} shutdown\n" +
                "```";
        }
    }
}