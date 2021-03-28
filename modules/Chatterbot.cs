using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Chatterbot
    {
        private static readonly List<string> blacklist = new List<string>(Data.Files_Read(Data.PATH_CHATTERS_BLACKLIST));  //Contains strings that will be filtered out of chatters, such as discord invite links.
        private static readonly List<Chatter> chatters;

        static Chatterbot()
        {
            try
            {
                chatters = new List<Chatter>();
                string fileData = Data.Files_Read_String(Data.PATH_CHATTERS);
                if (fileData == null || fileData.Length == 0 || fileData == "[]")
                    throw new Exception("NO FILEDATA");
                chatters = JsonConvert.DeserializeObject<List<Chatter>>(fileData);

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
            bool mentionsMe = false, doNotSave = false;
            if (Sanitize.IsChannelNsfw(src.Channel))
                doNotSave = true;
            foreach (SocketUser mention in src.MentionedUsers)
            {
                if (mention.IsBot)
                    doNotSave = true;
                if (mention.Id == Data.MY_ID)
                    mentionsMe = true;
            }

            if (mentionsMe || (Program.rand.Next(Data.CHATTERS_CHANCE_TO_CHAT) == 0))
            {
                //Send Chatter
                string outStr = GetChatter();
                if (outStr != null)
                    await src.Channel.SendMessageAsync(Sanitize.ReplaceAllMentionsWithID(outStr, src.Author.Id));
                //Save Chatter
                if (!doNotSave && Program.rand.Next(Data.CHATTERS_CHANCE_TO_SAVE) == 0 && AcceptableChatter(src.Content))  //checks to see if it's a valid and acceptable chatter then saves if applicable.
                {
                    if (chatters.Count >= Data.CHATTERS_MAX_COUNT)
                        chatters.RemoveAt(0);
                    chatters.Add(new Chatter(Sanitize.ScrubRoleMentions(src.Content).Replace('\n', ' '), src.Author.Id, src.Id, src.Channel.Id, (src.Author as IGuildUser).GuildId));
                    await SaveChatters();
                }
            }
        }

        /*private static Chatter GetLowestRated()
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
        }*/

        public static Task PrependBackupChatters(ISocketMessageChannel ch = null)    //Does what it says, this can mess with the chatters length however so it should only be called by operators
        {
            List<Chatter> chatterstoprepend = new List<Chatter>();
            List<string> backupchatters = Data.Files_Read(Data.PATH_CHATTERS_BACKUP);
            if (chatters.Count + backupchatters.Count > Data.CHATTERS_MAX_COUNT)
            {
                int overshoot = chatters.Count + backupchatters.Count - Data.CHATTERS_MAX_COUNT;
                if (ch != null)
                    ch.SendMessageAsync($"Warning: Prepending with {backupchatters.Count} lines overshot the max line count [{Data.CHATTERS_MAX_COUNT}] by {overshoot} lines. Deleting excess from prepend before saving.");
                backupchatters.RemoveRange(0, overshoot);
            }
            foreach (string chatter in backupchatters)
                chatterstoprepend.Add(new Chatter(chatter));

            chatters.InsertRange(0, chatterstoprepend);

            if (ch != null)
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

        private class Chatter
        {
            [JsonRequired]
            public readonly string Content;

            public readonly ulong Origin_author;
            public readonly ulong Origin_channel;
            public readonly ulong Origin_guild;
            public readonly ulong Origin_msg;
            public readonly List<Voter> Votes;
            public ulong Channel_last_used;
            public ulong Msg_last_used;

            [JsonConstructor]
            public Chatter(string content, ulong origin_author = 0, ulong origin_msg = 0, ulong origin_channel = 0, ulong origin_guild = 0, ulong msg_last_used = 0, ulong channel_last_used = 0, List<Voter> votes = null)
            {
                Content = content;
                Msg_last_used = msg_last_used;
                Channel_last_used = channel_last_used;
                Origin_author = origin_author;
                Origin_msg = origin_msg;
                Origin_channel = origin_channel;
                Origin_guild = origin_guild;
                if (votes != null)
                    Votes = votes;
                else
                    Votes = new List<Voter>();
            }

            public bool AddVote(ulong voterID, bool vote)  //Returns true or false based on if vote was counted.
            {
                if (HasVoted(voterID))
                    return false;
                Votes.Add(new Voter(vote, voterID));
                return true;
            }

            public IUser Author()
            {
                return Program.client.GetUser(Origin_author);
            }

            public bool ClearVote(ulong voterID)    //Returns true or false based on if voter was found.
            {
                foreach (Voter voter in Votes)
                    if (voter.VoterID == voterID)
                    {
                        Votes.Remove(voter);
                        return true;
                    }
                return false;
            }

            public bool HasVoted(ulong userID)
            {
                foreach (Voter voter in Votes)
                    if (voter.VoterID == userID)
                        return true;
                return false;
            }

            public IMessage LastUsed()
            {
                return (Program.client.GetChannel(Channel_last_used) as IMessageChannel).GetMessageAsync(Msg_last_used).Result;
            }

            public IGuild OriginGuild()
            {
                return Program.client.GetGuild(Origin_guild);
            }

            public IMessage OriginMessage()
            {
                return (Program.client.GetChannel(Origin_channel) as IMessageChannel).GetMessageAsync(Origin_msg).Result;
            }

            public int Rating()   //Calculates and returns rating
            {
                int rating = 0;
                foreach (Voter voter in Votes)
                    if (voter.Vote)
                        rating++;
                    else
                        rating--;
                return rating;
            }

            public class Voter
            {
                public readonly bool Vote;
                public readonly ulong VoterID;

                [JsonConstructor]
                public Voter(bool vote, ulong voterID)
                {
                    Vote = vote;
                    VoterID = voterID;
                }
            }
        }
    }
}