using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Chatterbot
    {
        private const ushort CHANCE_TO_CHAT = 5;         //Value is an inverse, (1 out of CHANCE_TO_CHAT chance)
        private const string CHATTER_PATH = @"..\..\data\chatters.txt";
        private const ushort CHATTER_MAX_LENGTH = 1024;
        private static string nextChatter;
        private static List<string> chatters = new List<string>();
        private static readonly Random rand = new Random();

        public Chatterbot()
        {
            try
            {
                StreamReader reader = new StreamReader(CHATTER_PATH);
                ushort i = 0;
                while (!reader.EndOfStream && i < CHATTER_MAX_LENGTH)
                {
                    chatters.Add(reader.ReadLine());
                    i++;
                }
                reader.Close();
                nextChatter = GetChatter();
            }
            catch (FileNotFoundException)
            {
                _ = new StreamWriter(CHATTER_PATH, true);
                return;
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(CHATTER_PATH.Substring(0, CHATTER_PATH.LastIndexOf('\\')));
                _ = new StreamWriter(CHATTER_PATH, true);
                return;
            }
        }

        public void AddChatter(SocketMessage src)
        {
            if (rand.Next(0, CHANCE_TO_CHAT) != 0)
                return;
            if (ShouldIgnore(src.Content))
                return;
            if (chatters.Count >= CHATTER_MAX_LENGTH)
                chatters.RemoveAt(0);
            chatters.Add(Sanitize.ScrubRoleMentions(src));
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
            if(nextChatter != null)
                await src.Channel.SendMessageAsync(nextChatter);
            nextChatter = Sanitize.ReplaceAllMentionsWithID(GetChatter(), src.Author.Id);
        }

        public void SaveChatters()
        {
            StreamWriter writer = new StreamWriter(CHATTER_PATH, false);
            for (ushort i = 0; i < CHATTER_MAX_LENGTH; i++)
            {
                if (i == chatters.Count)
                    break;
                writer.WriteLine(chatters[i]);
            }
            writer.Flush();
            writer.Close();
        }

        private string GetChatter()
        {
            if (chatters.Count == 0)
                return null;
            string outStr = chatters[rand.Next(0, chatters.Count)];
            if (outStr == null)
                return null;
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