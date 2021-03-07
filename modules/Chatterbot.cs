using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Chatterbot
    {
        private const string BLACKLIST_PATH = "../../data/blacklist.txt";
        private const ushort CHANCE_TO_CHAT = 24;         //Value is an inverse, (1 out of CHANCE_TO_CHAT chance)
        private const ushort CHATTER_MAX_LENGTH = 4096;
        private const string CHATTER_PATH = "../../data/chatters.txt";
        private static List<string> blacklist = new List<string>(Lists.ReadFile(CHATTER_PATH));
        private static List<string> chatters = new List<string>(Lists.ReadFile(CHATTER_PATH));

        //Contains strings that will be filtered out of chatters, such as discord invite links.

        public Chatterbot()
        {
            chatters = Lists.ReadFile(CHATTER_PATH);
            blacklist = Lists.ReadFile(BLACKLIST_PATH);
            CleanupChatters();
        }

        public static bool AcceptableChatter(string inStr)
        {
            inStr = inStr.ToLower();
            if (inStr.Length < 2 || inStr.Replace(" ", "").Length == 0) // Catch empty strings
                return false;

            ushort uniqueChars = 0;
            string clearChars = inStr.Replace(inStr[0].ToString(), "");     //One unique character deleted
            if (clearChars.Length != 0)
                uniqueChars++;
            while (clearChars.Length != 0)
            {
                clearChars = clearChars.Replace(clearChars[0].ToString(), "");
                uniqueChars++;
            }
            if (uniqueChars < 5)    //A message with less than five unique characters is probably just keyboard mash or a single word.
                return false;
            if(blacklist.Count != 0)
                foreach (string blacklister in blacklist)
                    if (inStr.Contains(blacklister.ToLower()))
                        return false;
            return true;
        }

        public static async Task AddChatter(SocketMessage src)
        {
            if (Program.rand.Next(5) == 0 && !ShouldIgnore(src) && AcceptableChatter(src.Content))  //1/5 chance to save a message it sees, however checks to see if it's a valid and acceptable chatter first
            {
                if (chatters.Count >= CHATTER_MAX_LENGTH)
                    chatters.RemoveAt(0);
                chatters.Add(Sanitize.ScrubRoleMentions(src.Content));
                await SaveChatters();
            }
        }

        public static async Task BlacklistHandler(SocketMessage msg, string command) //Expects to be called from the utilities chain with the keyword 'blacklist'.
        {
            string keyword, args;
            if (command.IndexOf(' ') == 0)
                command = command.Substring(1);
            if (command.Contains(' '))
            {
                keyword = command.Substring(0, command.IndexOf(' '));
                args = command.Substring(command.IndexOf(' ') + 1);
            }
            else
            {
                keyword = command;
                args = "";
            }
            args = args.ToLower();
            switch (keyword)
            {
                case "list":
                    {
                        string outStr = "```**Current Blacklist Entries:**";
                        int count = 0;
                        foreach (string entry in blacklist)
                        {
                            outStr += $"\n{count}: {entry}";
                            count++;
                        }
                        outStr += "```";
                        await msg.Channel.SendMessageAsync(outStr);
                    }
                    break;

                case "add":
                    if (blacklist.Contains(args))
                        await msg.Channel.SendMessageAsync("Entry already in blacklist.");
                    else if (args.Length < 3)
                        await msg.Channel.SendMessageAsync("Minimum 3 characters for blacklist entries");
                    else
                    {
                        blacklist.Add(args);
                        await SaveBlacklist();
                        await SaveChatters();
                        await msg.Channel.SendMessageAsync($"\"{args}\" added to chatters blacklist successfully");
                    }
                    break;

                case "remove":
                    if (!blacklist.Contains(args))
                        await msg.Channel.SendMessageAsync("Entry not found in blacklist");
                    else
                    {
                        blacklist.Remove(args);
                        await SaveBlacklist();
                        await msg.Channel.SendMessageAsync($"\"{args}\" removed from chatters blacklist successfully");
                    }
                    break;

                default:
                    await Lists.Chatterbot_PrintBlacklistCommands(msg.Channel, $"{Program._prefix} utility blacklist ");
                    break;
            }
        }

        public static async Task ChatterHandler(SocketMessage src)
        {
            bool mentionsMothbot = false;
            foreach (SocketUser user in src.MentionedUsers)
                if (user.Id == Program.MY_ID)
                {
                    mentionsMothbot = true;
                    break;
                }

            if (mentionsMothbot || (Program.rand.Next(0, CHANCE_TO_CHAT) == 0 && !ShouldIgnore(src)))
            {
                string outStr = GetChatter();
                if (outStr != null)
                    await src.Channel.SendMessageAsync(Sanitize.ReplaceAllMentionsWithID(outStr, src.Author.Id));
            }
        }

        public static Task SaveBlacklist()
        {
            blacklist = new HashSet<string>(blacklist).ToList();  //Kill duplicates
            Lists.WriteFile(BLACKLIST_PATH, blacklist);
            return Task.CompletedTask;
        }

        public static Task SaveChatters()
        {
            CleanupChatters();
            Lists.WriteFile(CHATTER_PATH, chatters);
            return Task.CompletedTask;
        }

        private static Task CleanupChatters()
        {
            chatters = new HashSet<string>(chatters).ToList();  //Kill duplicates
            List<string> chattersout = new List<string>();

            foreach (string chatter in chatters)                 //Test every entry
                if (AcceptableChatter(chatter))
                    chattersout.Add(chatter);
            chatters = chattersout;
            return Task.CompletedTask;
        }

        private static string GetChatter() //Requires output sanitization still
        {
            if (chatters.Count == 0)
                return null;
            string outStr = chatters[Program.rand.Next(0, chatters.Count)];
            if (outStr == null)
                return null;
            return outStr;
        }

        private static bool ShouldIgnore(SocketMessage src)
        {
            if (src.Channel.Id == 735266952129413211)
                return true;
            foreach (SocketUser mention in src.MentionedUsers)
                if (mention.IsBot)
                    return true;
            string inStr = src.Content.ToLower();
            char[] firstCharBlacklist = { '!', '@', '.', ',', '>', ';', ':', '`', '$', '%', '^', '&', '*', '?', '~' };
            if (inStr.Contains(Program._prefix) || inStr.IndexOfAny(firstCharBlacklist) < 3)
                return true;
            return false;
        }
    }
}