using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Chatterbot
    {
        public const string PATH_BLACKLIST = "../../data/blacklist.txt";
        public const string PATH_CHATTERS = "../../data/chatters.txt";
        public const string PATH_CHATTERS_BACKUP = "../../preloaded/backupchatters.txt";
        private const ushort CHANCE_TO_CHAT = 32;         //Value is an inverse, (1 out of CHANCE_TO_CHAT chance)
        private const ushort CHATTER_MAX_LENGTH = 4096;
        private static readonly List<string> blacklist = new List<string>(Data.Files_Read(PATH_BLACKLIST));
        private static readonly char[] firstCharBlacklist = { '!', '#', '$', '%', '&', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '\\', '^', '`', '|', '~' };
        private static List<string> chatters = new List<string>();

        //Contains strings that will be filtered out of chatters, such as discord invite links.
        public Chatterbot()
        {
            chatters = Data.Files_Read(PATH_CHATTERS);
            if (chatters.Count == 0)
                chatters = Data.Files_Read(PATH_CHATTERS_BACKUP);
            CleanupChatters();
        }

        public static bool AcceptableChatter(string inStr)
        {
            inStr = inStr.ToLower();
            if (inStr.Length < 2 || inStr.Replace(" ", "").Length == 0)     //Catch empty strings
                return false;

            foreach (char c in inStr.ToCharArray())     //Strings may not contain characters above UTF16 0000BF
                if (c > 0xBF)
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

            if (inStr.IndexOf(Program._prefix) == 0 || inStr.IndexOfAny(firstCharBlacklist) < 3)    //Check for characters in the char blacklist appearing too early int he straing, likely denoting a bot command
                return false;

            if (blacklist.Count != 0)                           //Check against strings in the blacklist
                foreach (string blacklister in blacklist)
                    if (inStr.Contains(blacklister.ToLower()))
                        return false;
            return true;
        }

        public static async Task AddChatterHandler(SocketMessage src)
        {
            if (Program.rand.Next(16) == 0 && !ShouldIgnore(src) && AcceptableChatter(src.Content))  //1/5 chance to save a message it sees, however checks to see if it's a valid and acceptable chatter first
            {
                if (chatters.Count >= CHATTER_MAX_LENGTH)
                    chatters.RemoveAt(0);
                chatters.Add(Sanitize.ScrubRoleMentions(src.Content).Replace('\n', ' '));
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
                    await msg.Channel.SendMessageAsync(Data.Chatterbot_GetBlacklistCommands($"{Program._prefix} utility blacklist "));
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

        public static async Task PrependBackupChatters(ISocketMessageChannel ch)    //Does what it says, this can mess with the chatters length however so it should only be called by operators
        {
            List<string> backupchatters = Data.Files_Read(PATH_CHATTERS_BACKUP);
            if (chatters.Count + backupchatters.Count > CHATTER_MAX_LENGTH)
            {
                int overshoot = chatters.Count + backupchatters.Count - CHATTER_MAX_LENGTH;
                await ch.SendMessageAsync($"Warning: Prepending with {backupchatters.Count} lines overshot the max line count [{CHATTER_MAX_LENGTH}] by {overshoot} lines. Deleting excess from prepend before saving.");
                backupchatters.RemoveRange(0, overshoot);
            }
            foreach (string chatter in backupchatters)
                chatters = chatters.Prepend(chatter).ToList();
            await ch.SendMessageAsync("Prepend successful");
            await SaveChatters();
        }

        public static Task SaveBlacklist()
        {
            Data.Files_Write(PATH_BLACKLIST, blacklist);
            return Task.CompletedTask;
        }

        public static Task SaveChatters()
        {
            CleanupChatters();
            Data.Files_Write(PATH_CHATTERS, chatters);
            return Task.CompletedTask;
        }

        private static Task CleanupChatters()
        {
            chatters = new HashSet<string>(chatters).ToList();  //Kill duplicates
            List<string> chattersout = new List<string>();

            foreach (string chatter in chatters)                //Test every entry
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
            if (Sanitize.IsChannelNsfw(src.Channel))
                return true;
            foreach (SocketUser mention in src.MentionedUsers)
                if (mention.IsBot)
                    return true;
            return false;
        }
    }
}