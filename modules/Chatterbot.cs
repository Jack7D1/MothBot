using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Chatterbot
    {
        private const ushort CHANCE_TO_CHAT = 16;         //Value is an inverse, (1 out of CHANCE_TO_CHAT chance)
        private const ushort CHATTER_MAX_LENGTH = 4096;
        private const string CHATTER_PATH = @"..\..\data\chatters.txt";
        private static List<string> chatters = new List<string>();

        public Chatterbot()
        {
            chatters = Data.ReadFileString(CHATTER_PATH);
            CleanupChatters();
        }

        public async Task AddChatter(SocketMessage src)
        {
            if (Program.rand.Next(2) == 0 && !ShouldIgnore(src) && Sanitize.AcceptableChatter(src.Content))
            {
                if (chatters.Count >= CHATTER_MAX_LENGTH)
                    chatters.RemoveAt(0);
                chatters.Add(Sanitize.ScrubRoleMentions(src.Content));
                if (Program.rand.Next(5) == 0)
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
                if (outStr != null)
                    await src.Channel.SendMessageAsync(Sanitize.ReplaceAllMentionsWithID(outStr, src.Author.Id));
            }
        }

        public Task SaveChatters()
        {
            CleanupChatters();
            Data.WriteFileString(CHATTER_PATH, chatters);
            return Task.CompletedTask;
        }

        private Task CleanupChatters()
        {
            chatters = new HashSet<string>(chatters).ToList();  //Kill duplicates
            List<bool> validMap = new List<bool>();

            foreach (string chatter in chatters)                 //Test every entry
                validMap.Add(Sanitize.AcceptableChatter(chatter));

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
            if (inStr.Contains(Program._prefix) || inStr.IndexOfAny(firstCharBlacklist) == 0)
                return true;
            return false;
        }
    }
}