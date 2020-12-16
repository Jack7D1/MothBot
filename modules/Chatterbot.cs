using Discord.WebSocket;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Chatterbot
    {
        private const ushort CHANCE_TO_CHAT = 16;         //Value is an inverse, (1 out of CHANCE_TO_CHAT chance)
        private const ushort CHATTER_MAX_LENGTH = 4096;
        private const string CHATTER_PATH = @"..\..\data\chatters.txt";
        private static readonly List<string> chatters = new List<string>();

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

        public async Task AddChatter(SocketMessage src)
        {
            if (!ShouldIgnore(src))
            {
                if (chatters.Count >= CHATTER_MAX_LENGTH)
                    chatters.RemoveAt(0);
                chatters.Add(Sanitize.ScrubRoleMentions(src));
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

            if (Program.rand.Next(0, CHANCE_TO_CHAT) == 0 && !ShouldIgnore(src) || mentionsMothbot)
            {
                string outStr = GetChatter();
                if (outStr != null)
                    await src.Channel.SendMessageAsync(outStr);
            }
        }

        public Task SaveChatters()
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
            return Task.CompletedTask;
        }

        private string GetChatter()
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