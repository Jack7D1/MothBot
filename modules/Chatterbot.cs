using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Chatterbot
    {
        private static readonly List<string> blacklist = new List<string>(Data.Files_Read(Data.PATH_CHATTERS_BLACKLIST));
        private static readonly List<Chatter> chatters = new List<Chatter>();

        //Contains strings that will be filtered out of chatters, such as discord invite links.
        public Chatterbot()
        {
            try
            {
                string fileData = Data.Files_Read_String(Data.PATH_CHATTERS);
                if (fileData == null || fileData.Length == 0 || fileData == "[]")
                    throw new Exception("NO FILEDATA");
                List<Chatter> fileChatters = JsonConvert.DeserializeObject<List<Chatter>>(fileData);
                foreach (Chatter chatter in fileChatters)
                    chatters.Add(chatter);
                CleanupChatters();
            }
            catch (Exception ex) when (ex.Message == "NO FILEDATA")
            {
                Logging.LogtoConsoleandFile($"No chatters found at {Data.PATH_CHATTERS}, loading prepend.");
                PrependBackupChatters();
                return;
            }
        }

        public static bool AcceptableChatter(string inStr)
        {
            inStr = inStr.ToLower();
            if (inStr.Length < 2 || inStr.Replace(" ", "").Length == 0)     //Catch empty strings
                return false;

            foreach (char c in inStr.ToCharArray())     //Strings may not contain characters above UTF16 0000BF
                if (c > 0xBF)
                    return false;
            {
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
            }
            {
                char[] firstCharBlacklist = { '!', '#', '$', '%', '&', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '\\', '^', '`', '|', '~', '\'' };
                if (inStr.IndexOf(Data.PREFIX) == 0 || inStr.IndexOfAny(firstCharBlacklist) < 3)    //Check for characters in the char blacklist appearing too early int he straing, likely denoting a bot command
                    return false;
            }

            if (blacklist.Count != 0)                           //Check against strings in the blacklist
                foreach (string blacklister in blacklist)
                    if (inStr.Contains(blacklister.ToLower()))
                        return false;
            return true;
        }

        public static async Task AddChatterHandler(SocketMessage src)
        {
            if (Program.rand.Next(Data.CHATTERS_CHANCE_TO_SAVE) == 0 && !ShouldIgnore(src) && AcceptableChatter(src.Content))  //checks to see if it's a valid and acceptable chatter then saves if applicable.
            {
                if (chatters.Count >= Data.CHATTERS_MAX_COUNT)
                    chatters.RemoveAt(0);
                chatters.Add(new Chatter(Sanitize.ScrubRoleMentions(src.Content).Replace('\n', ' '), src.Author.Id, (src.Author as SocketGuildUser).Guild.Id));
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
                    await msg.Channel.SendMessageAsync(Data.Chatterbot_GetBlacklistCommands($"{Data.PREFIX} utility blacklist "));
                    break;
            }
        }

        public static async Task ChatterHandler(SocketMessage src)
        {
            bool mentionsMothbot = false;
            foreach (SocketUser user in src.MentionedUsers)
                if (user.Id == Data.MY_ID)
                {
                    mentionsMothbot = true;
                    break;
                }

            if (mentionsMothbot || (Program.rand.Next(0, Data.CHATTERS_CHANCE_TO_CHAT) == 0 && !ShouldIgnore(src)))
            {
                string outStr = GetChatter();
                if (outStr != null)
                    await src.Channel.SendMessageAsync(Sanitize.ReplaceAllMentionsWithID(outStr, src.Author.Id));
            }
        }
        private static Chatter GetLowestRated()
        {
            int worst = GetLowestRating();
            List<Chatter> candidates = chatters;

            foreach(Chatter chatter in chatters)
            {
                if (chatter.Rating > worst)
                    candidates.Remove(chatter);//no clue if this actually works
            }
            return candidates[Program.rand.Next(0, candidates.Count)];
        }

        private static int GetLowestRating()
        {
            int bottom = 0;
            foreach (Chatter chatter in chatters)
                if (chatter.Rating < bottom)
                    bottom = chatter.Rating;
            return bottom;
        }

        public static Task PrependBackupChatters(ISocketMessageChannel ch = null)    //Does what it says, this can mess with the chatters length however so it should only be called by operators
        {
            List<string> backupchatters = Data.Files_Read(Data.PATH_CHATTERS_BACKUP);
            if (chatters.Count + backupchatters.Count > Data.CHATTERS_MAX_COUNT)
            {
                int overshoot = chatters.Count + backupchatters.Count - Data.CHATTERS_MAX_COUNT;
                if(ch != null)
                    ch.SendMessageAsync($"Warning: Prepending with {backupchatters.Count} lines overshot the max line count [{Data.CHATTERS_MAX_COUNT}] by {overshoot} lines. Deleting excess from prepend before saving.");
                backupchatters.RemoveRange(0, overshoot);
            }

            foreach (string chatter in backupchatters)
                if (chatters.Count == 0)
                    chatters.Add(new Chatter(chatter));
                else
                    chatters.Insert(0, new Chatter(chatter));
            if(ch != null)
                ch.SendMessageAsync("Prepend successful");
            Logging.LogtoConsoleandFile("CHATTERS: Prepend successful.");
            SaveChatters();
            return Task.CompletedTask;
        }

        public static Task SaveBlacklist()
        {
            Data.Files_Write(Data.PATH_CHATTERS_BLACKLIST, blacklist);
            return Task.CompletedTask;
        }

        public static Task SaveChatters()
        {
            CleanupChatters();
            string outStr = JsonConvert.SerializeObject(chatters, Formatting.Indented);
            Data.Files_Write(Data.PATH_CHATTERS, outStr);
            return Task.CompletedTask;
        }

        private static Task CleanupChatters()
        {
            List<Chatter> chattersout = new List<Chatter>();
            foreach (Chatter chatter in chatters)                //Test every entry for acceptableness and kill possible duplicates
                if (AcceptableChatter(chatter.Content) && !chattersout.Contains(chatter))
                    chattersout.Add(chatter);
            //Move them over
            chatters.Clear();
            foreach (Chatter chatter in chattersout)
                chatters.Add(chatter);
            return Task.CompletedTask;
        }

        private static string GetChatter() //Requires output sanitization still
        {
            if (chatters.Count == 0)
                return null;
            else
                return chatters[Program.rand.Next(0, chatters.Count)].Content;
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

        private class Chatter   
        {
            public ulong Channel_last_used;
            public int Rating;  //When implementing rating ensure to create some kind of restriction
            public ulong Time_last_used;
            public readonly string Content;
            public readonly ulong Origin_guild;
            public readonly ulong Origin_user;

            [JsonConstructor]
            public Chatter(string content, ulong origin_user = 0, ulong origin_guild = 0, int rating = 0, ulong time_last_used = 0, ulong channel_last_used = 0)
            {
                Content = content;
                Origin_user = origin_user;
                Origin_guild = origin_guild;
                Rating = rating;
                Time_last_used = time_last_used;
                Channel_last_used = channel_last_used;
            }
        }
    }
}