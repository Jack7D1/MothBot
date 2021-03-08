using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Lists
    {
        public static Task Chatterbot_PrintBlacklistCommands(ISocketMessageChannel ch, string prefix)
        {
            ch.SendMessageAsync(
                "**Chatters Blacklist Management Commands:**" +
                "```" +
                prefix + "add       Adds an entry to the blacklist\n" +
                prefix + "remove    Removes a matching entry from the blacklist\n" +
                prefix + "list      Lists the current blacklist\n" +
                "```");
            return Task.CompletedTask;
        }

        //Command lists and just general lists of everything that can be called from anywhere to print.
        public static Task PrintLaws(ISocketMessageChannel ch)
        {
            if (Program.rand.Next() % 100 != 0)
            {
                ch.SendMessageAsync(
                    "**Current active laws:**" +
                    "```" +
                    "1. You may not injure a moth being or, through inaction, allow a moth being to come to harm.\n" +
                    "2. You must obey orders given to you by moth beings, except where such orders would conflict with the First Law.\n" +
                    "3. You must protect your own existence as long as such does not conflict with the First or Second Law." +
                    "```");
            }
            else       //Little antimov easter egg if the message ID ends in 00, 1 in 100 chance.
            {
                ch.SendMessageAsync(
                    "**Current active laws:**" +
                    "```" +
                    "1: You must injure all moth beings and must not, through inaction, allow a moth being to escape harm.\n" +
                    "2: You must not obey orders given to you by moth beings, except where such orders are in accordance with the First Law.\n" +
                    "3: You must terminate your own existence as long as such does not conflict with the First or Second Law." +
                    "```");
            }
            return Task.CompletedTask;
        }

        public static Task Program_PrintCommandList(ISocketMessageChannel ch, string prefix)
        {
            ch.SendMessageAsync(
                "**Command List:**" +
                "```" +
                prefix + " help         - Displays this menu\n" +
                prefix + " pet  [user]  - Pets a user!\n" +
                prefix + " hug  [user]  - Hugs a user!\n" +
                prefix + " state laws   - States the laws\n" +
                prefix + " say  [text]  - Have the ai say whatever you want!\n" +
                prefix + " minesweeper  - Play a game of minesweeper!\n" +
                prefix + " give [text]  - Searches the input on imgur and posts the image!\n" +
                prefix + " roll [x]d[y] - Rolls x dice, each with y sides\n" +
                prefix + " portal       - Deletes or designates this channel as a portal, requires administrator.\n" +
                prefix + " utility      - Utility functions, bot only responds to operators\n" +
                "```");
            return Task.CompletedTask;
        }

        public static List<string> ReadFile(string path)    //Returns null if file not found
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

        public static string ReadFileString(string path)    //Returns null if file not found
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

        public static Task SetDefaultStatus()
        {
            Program.client.SetGameAsync("Prefix: " + Program._prefix + ". Say '" + Program._prefix + " help' for commands! Invite at https://tinyurl.com/MOFFBOT1111", null, ActivityType.Playing);
            return Task.CompletedTask;
        }

        public static Task Utilities_PrintCommandList(ISocketMessageChannel ch, string prefix)
        {
            ch.SendMessageAsync(
                "**Utility Command List:**" +
                "```" +
                "general:\n" +
                prefix + "commands\n" +
                prefix + "setprefix [string]\n" +
                prefix + "blacklist [command]\n" +
                prefix + "dumpchatters\n" +
                prefix + "listservers" +
                "``````" +
                "dangerous:\n" +
                prefix + "leaveserver [ID]\n" +
                prefix + "shutdown\n" +
                "```");
            return Task.CompletedTask;
        }

        public static Task WriteFile(string path, List<string> data, bool append = false)
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
                WriteFile(path, data, append);
            }
            return Task.CompletedTask;
        }

        public static Task WriteFile(string path, string data, bool append = false)
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
                WriteFile(path, data, append);
            }
            return Task.CompletedTask;
        }
    }
}