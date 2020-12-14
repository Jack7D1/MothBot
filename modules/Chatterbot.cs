using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Chatterbot
    {
        public const ushort CHANCE_TO_CHAT = 100;    //Value is an inverse, (1 out of CHANCE_TO_CHAT chance)
        private static readonly string[] chatters = new string[ushort.MaxValue + 1];
        private static readonly Random rand = new Random();
        private static ushort chatterIndex = 0;

        public void AddChatter(SocketMessage src)
        {
            if (ShouldIgnore(src.Content))
                return;
            if (chatterIndex == chatters.Length)
                chatterIndex = 0;
            chatters[chatterIndex] = Sanitize.ScrubRoleMentions(src);
            chatterIndex++;
            return;
        }

        public async Task ChatterHandler(SocketMessage src)
        {
            if (rand.Next(0, CHANCE_TO_CHAT) != 0 || ShouldIgnore(src.Content))
                return;
            string outStr = Sanitize.ReplaceAllMentionsWithID(GetChatter(), src.Author.Id);
            await src.Channel.SendMessageAsync(outStr);
            return;
        }

        private string GetChatter()
        {
            string outStr = chatters[rand.Next(0, chatterIndex)];
            if (outStr == null)
                return "";
            return outStr;
        }

        private void ShiftChattersLeft(ushort shiftPtr)    //Left implies big endian
        {
            if (chatters[shiftPtr] != null)
                return;
            for (int i = shiftPtr; i < chatterIndex; i++)
            {
                if (chatters[i + 1] == null)
                    break;
                chatters[i] = chatters[i + 1];
            }
            return;
        }

        private bool ShouldIgnore(string inStr)
        {
            char[] firstCharBlacklist = { '!', '@', '.', ',', '>', ';', ':', '`', '$', '%', '^', '&', '*', '?', '~' };
            inStr = inStr.ToLower();
            if (inStr.Length < Program._prefix.Length)
                return true;
            if (inStr.Contains(Program._prefix))
                return true;
            if (inStr.IndexOfAny(firstCharBlacklist) == 1)
                return true;
            return false;
        }
    }
}