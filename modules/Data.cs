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
    //Generally just functions and variables that aren't necessarily tied to a single class/module and are also called by multiple classes/modules.
    {
        //PARAMS
        public const ulong MY_ID = 765202973495656538;

        //PATHS
        public const string PATH_GAYBEE = "../../resources/yougaybee.png";

        public const string PREFIX = "ai";          //What should the bots attention prefix be? MUST be lowercase.

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
                $"{prefix} clearvote    - Removes your vote.\n" +
                $"{prefix} rating       - Returns the rating of the most recently said chatter\n" +
                $"{prefix} myvote       - Tells you what you voted on the most recent chatter\n" +
                $"{prefix} leaderboard  - Lists the top 3 highest rated chatters." +
                "```";
        }

        public static string Files_Read(string path)
        {
            try
            {
                return File.ReadAllText(path, Encoding.UTF8);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public static List<string> Files_ReadList(string path)
        {
            try
            {
                return new List<string>(File.ReadAllLines(path, Encoding.UTF8));
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public static void Files_Write(string path, string data)
        {
            File.WriteAllText(path, data, Encoding.UTF8);
        }

        public static void Files_Write(string path, List<string> data)
        {
            File.WriteAllLines(path, data, Encoding.UTF8);
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
                $"{PREFIX} portal       - Access the portals module\n" +
                $"{PREFIX} chatter [v]  - Access the chatters module\n" +
                $"{PREFIX} whitelist    - Access the whitelists module\n" +
                $"{PREFIX} ping         - You know what this does.\n" +
                "```";
        }

        public static string Program_GetLaws()
        {
            if (Master.rand.Next() % 100 != 0)
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

        public static async Task Program_SetStatus()
        {
            await Master.client.SetGameAsync($"Say '{PREFIX} help' for commands! Invite to your server at https://tinyurl.com/MOFFBOT1111", null, ActivityType.Playing);
        }
    }
}