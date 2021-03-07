using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Chatterbot
    {
        private const ushort CHANCE_TO_CHAT = 20;         //Value is an inverse, (1 out of CHANCE_TO_CHAT chance)
        private const ushort CHATTER_MAX_LENGTH = 4096;
        private const string CHATTER_PATH = @"../../data/chatters.txt";
        private static readonly List<string> blacklist = new List<string> { "discord.gg" };
        private static List<string> chatters = new List<string>();
        //Contains strings that will be filtered out of chatters, such as discord invite links.

        public Chatterbot()
        {
            chatters = Lists.ReadFile(CHATTER_PATH);
            CleanupChatters();
        }

        public bool AcceptableChatter(string inStr)
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
            return true;
        }

        public async Task BlacklistHandler(SocketMessage msg, string command) //Expects to be called from the utilities chain with the keyword 'blacklist'.
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
                    if (blacklist.Contains(args.ToLower()))
                        await msg.Channel.SendMessageAsync("Entry already in blacklist.");
                    else if (args.Length < 3)
                        await msg.Channel.SendMessageAsync("Minimum 3 characters for blacklist entries");
                    else
                        blacklist.Add(args.ToLower());
                    break;

                case "remove":
                    if (!args.Contains(args))
                        await msg.Channel.SendMessageAsync("Entry not found");
                    else
                        blacklist.Remove(args);
                    break;

                default:
                    await Lists.Chatterbot_PrintBlacklistCommands(msg.Channel, $"{Program._prefix} utility blacklist ");
                    break;
            }
        }

        public async Task AddChatter(SocketMessage src)
        {
            if (Program.rand.Next(3) == 0 && !ShouldIgnore(src) && AcceptableChatter(src.Content))
            {
                if (chatters.Count >= CHATTER_MAX_LENGTH)
                    chatters.RemoveAt(0);
                chatters.Add(Sanitize.ScrubRoleMentions(src.Content));
                if (Program.rand.Next(4) == 0)     //1/4 chance to autosave chatters any time this is triggered.
                    await SaveChatters();
            }
        }

        public async Task ChatterHandler(SocketMessage src)
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
                if (outStr != null && AcceptableChatter(outStr))
                    await src.Channel.SendMessageAsync(Sanitize.ReplaceAllMentionsWithID(outStr, src.Author.Id));
            }
        }

        public Task SaveChatters()
        {
            CleanupChatters();
            Lists.WriteFile(CHATTER_PATH, chatters);
            return Task.CompletedTask;
        }

        private Task CleanupChatters()
        {
            chatters = new HashSet<string>(chatters).ToList();  //Kill duplicates
            List<bool> validMap = new List<bool>();

            foreach (string chatter in chatters)                 //Test every entry
                validMap.Add(AcceptableChatter(chatter));

            for (int i = validMap.Count - 1; i > 0; i--)
                if (!validMap[i])
                    chatters.RemoveAt(i);
            return Task.CompletedTask;
        }

        private string GetChatter() //Requires output sanitization still
        {
            if (chatters.Count == 0)
                return null;
            string outStr = chatters[Program.rand.Next(0, chatters.Count)];
            if (outStr == null)
                return null;
            return outStr;
        }

        private bool ShouldIgnore(SocketMessage src)
        {
            if (src.Channel.Id == 735266952129413211)
                return true;
            foreach (SocketUser mention in src.MentionedUsers)
                if (mention.IsBot)
                    return true;
            string inStr = src.Content.ToLower();
            char[] firstCharBlacklist = { '!', '@', '.', ',', '>', ';', ':', '`', '$', '%', '^', '&', '*', '?', '~' };
            if (inStr.Contains(Program._prefix) || inStr.IndexOfAny(firstCharBlacklist) < 2)
                return true;
            return false;
        }
    }
}