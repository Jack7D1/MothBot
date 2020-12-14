using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Chatterbot
    {
        private const ushort CHANCE_TO_CHAT = 30;         //Value is an inverse, (1 out of CHANCE_TO_CHAT chance)
        private const string CHATTER_PATH = @"..\..\data\chatters.txt";

        private static readonly string[] chatters = new string[4096];
        private static readonly Random rand = new Random();
        private static ushort chatterIndex = 0;

        public Chatterbot()
        {
            try
            {
                StreamReader reader = new StreamReader(CHATTER_PATH);
                ushort i = 0;
                while (!reader.EndOfStream && i < chatters.Length)
                {
                    chatters[i] = reader.ReadLine();
                    i++;
                    chatterIndex++;
                }
                reader.Close();
            }
            catch (FileNotFoundException)
            {
                _ = new StreamWriter(CHATTER_PATH, true);
                return;
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(CHATTER_PATH.Substring(0, CHATTER_PATH.LastIndexOf('\\')));
                return;
            }
        }

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
            bool mentionsMothbot = false;
            foreach (SocketUser user in src.MentionedUsers)
                if (user.Id == Program.MY_ID)
                {
                    mentionsMothbot = true;
                    break;
                }

            if ((rand.Next(0, CHANCE_TO_CHAT) != 0 || ShouldIgnore(src.Content)) && !mentionsMothbot)
                return;
            string outStr = Sanitize.ReplaceAllMentionsWithID(GetChatter(), src.Author.Id);
            await src.Channel.SendMessageAsync(outStr);
            return;
        }

        public void SaveChatters()
        {
            StreamWriter writer = new StreamWriter(CHATTER_PATH, false);
            for (ushort i = 0; i < chatters.Length; i++)
            {
                writer.WriteLine(chatters[i]);
            }
            writer.Flush();
            writer.Close();
        }

        private string GetChatter()
        {
            string outStr = chatters[rand.Next(0, chatterIndex)];
            if (outStr == null)
                return "";
            return outStr;
        }

        private bool ShouldIgnore(string inStr)
        {
            char[] firstCharBlacklist = { '!', '@', '.', ',', '>', ';', ':', '`', '$', '%', '^', '&', '*', '?', '~' };
            inStr = inStr.ToLower();
            if (inStr.Contains(Program._prefix))
                return true;
            if (inStr.IndexOfAny(firstCharBlacklist) == 1)
                return true;
            return false;
        }
    }
}