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
                if (chatters.Count > Data.CHATTERS_MAX_COUNT)
                    throw new Exception("CHATTER OVERFLOW");
            }
            catch (Exception ex) when (ex.Message == "NO FILEDATA")
            {
                Logging.LogtoConsoleandFile($"No chatters found at {Data.PATH_CHATTERS}, running with empty...");
                chatters.Clear();
                return;
            }
            catch (Exception ex) when (ex.Message == "CHATTER OVERFLOW")
            {
                int overflow = chatters.Count - Data.CHATTERS_MAX_COUNT;
                Logging.LogtoConsoleandFile($"CHATTERS: Chatter overflow found, deleting {overflow} lowest rated entries.");
                for (int i = overflow; i > 0; i--)
                    RemoveLowestRated();
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
            if (ContentsBlacklisted(inStr))
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
                        SaveBlacklist();
                        SaveChatters();
                        await msg.Channel.SendMessageAsync($"\"{args}\" added to chatters blacklist successfully");
                    }
                    break;

                case "remove":
                    if (!blacklist.Contains(args))
                        await msg.Channel.SendMessageAsync("Entry not found in blacklist");
                    else
                    {
                        blacklist.Remove(args);
                        SaveBlacklist();
                        await msg.Channel.SendMessageAsync($"\"{args}\" removed from chatters blacklist successfully");
                    }
                    break;

                default:
                    await msg.Channel.SendMessageAsync(Data.Chatterbot_GetBlacklistCommands());
                    break;
            }
        }

        public static async Task ChatterHandler(SocketMessage src)
        {
            bool mentionsMe = false, doNotSave = false;
            if (src.Channel is ITextChannel && (src.Channel as ITextChannel).IsNsfw)
                doNotSave = true;
            foreach (SocketUser mention in src.MentionedUsers)
            {
                if (mention.IsBot)
                    if (mention.Id == Data.MY_ID)
                        mentionsMe = true;
                    else
                        return;
            }

            if (mentionsMe || (Program.rand.Next(Data.CHATTERS_CHANCE_TO_CHAT) == 0))
            {
                //Send Chatter
                Chatter outChatter = GetChatter();
                if (outChatter != null)
                {
                    outChatter.Channel_last_used = src.Channel.Id;
                    outChatter.Time_last_used = DateTime.Now.Ticks;
                    await src.Channel.SendMessageAsync(Sanitize.ReplaceAllMentionsWithID(outChatter.Content, src.Author.Id));
                }
                //Save Chatter
                if (!doNotSave && Program.rand.Next(Data.CHATTERS_CHANCE_TO_SAVE) == 0 && AcceptableChatter(src.Content))  //checks to see if it's a valid and acceptable chatter then saves if applicable.
                {
                    if (chatters.Count == Data.CHATTERS_MAX_COUNT)
                        RemoveLowestRated();
                    if (chatters.Count > Data.CHATTERS_MAX_COUNT)
                        RemoveLowestRated();
                    else
                        chatters.Add(new Chatter(Sanitize.ScrubRoleMentions(src.Content).Replace('\n', ' '), src.Author.Id, src.Id, src.Channel.Id, (src.Author as IGuildUser).GuildId));
                    SaveChatters();
                }
            }
        }

        public static bool ContentsBlacklisted(string inStr)
        {
            foreach (string blacklister in blacklist)
                if (inStr.Contains(blacklister.ToLower()))
                    return true;
            return false;
        }

        public static void PrependBackupChatters(IMessageChannel ch = null)    //Does what it says, this can mess with the chatters length however so it should only be called by operators
        {
            List<Chatter> chatterstoprepend = new List<Chatter>();
            List<string> backupchatters = Data.Files_Read(Data.PATH_CHATTERS_BACKUP);
            int overshoot = chatters.Count + backupchatters.Count - Data.CHATTERS_MAX_COUNT;
            if (overshoot > 0)
            {
                if (ch != null)
                    ch.SendMessageAsync($"Warning: Prepending with {backupchatters.Count} lines overshot the max count [{Data.CHATTERS_MAX_COUNT}] by {overshoot}. Deleting excess from prepend before saving.");
                backupchatters.RemoveRange(0, overshoot);
            }
            foreach (string chatter in backupchatters)
                chatterstoprepend.Add(new Chatter(chatter));

            chatters.InsertRange(0, chatterstoprepend);

            if (ch != null)
                ch.SendMessageAsync("Prepend successful");
            Logging.LogtoConsoleandFile($"CHATTERS: Prepend Completed with {overshoot} items removed due to limit of {Data.CHATTERS_MAX_COUNT} entries.");
            SaveChatters();
        }

        public static void SaveBlacklist()
        {
            Data.Files_Write(Data.PATH_CHATTERS_BLACKLIST, blacklist);
        }

        public static void SaveChatters()
        {
            CleanupChatters();
            string outStr = JsonConvert.SerializeObject(chatters, Formatting.Indented);
            Data.Files_Write(Data.PATH_CHATTERS, outStr);
        }

        public static async Task VoteHandler(SocketMessage msg, string args)    //Expects to be called from main command tree with the keyword chatter to vote on the most recently used chatter in the sent channel.
        {
            Chatter latestChatter = null;
            if (args.Length > 0)
            {
                long latestTime = 0;
                foreach (Chatter chatter in chatters)
                {
                    if (chatter.Channel_last_used == msg.Channel.Id && chatter.Time_last_used > latestTime)
                        latestTime = chatter.Time_last_used;
                }
                if (latestTime == 0)
                {
                    await msg.Channel.SendMessageAsync("I can't find any chatters in this channel, check if this is the correct channel.");
                    return;
                }

                foreach (Chatter chatter in chatters)
                    if (chatter.Time_last_used == latestTime)
                    {
                        latestChatter = chatter;
                        break;
                    }
            }

            switch (args)
            {
                case "good":
                case "bad":
                    {
                        bool vote = false;
                        if (args == "good")
                            vote = true;
                        if (latestChatter.AddVote(msg.Author.Id, vote))
                            await msg.Channel.SendMessageAsync($"Vote of \"{args}\" placed successfully.");
                        else
                            await msg.Channel.SendMessageAsync("You have already placed a vote for this chatter!");
                    }
                    break;

                case "clearvote":
                    if (latestChatter.ClearVote(msg.Author.Id))
                        await msg.Channel.SendMessageAsync("Vote successfully removed.");
                    else
                        await msg.Channel.SendMessageAsync("You haven't voted on this chatter!");
                    break;

                case "rating":
                    await msg.Channel.SendMessageAsync($"Most recent chatter has a rating of {latestChatter.Rating()}, with {latestChatter.Votes.Count} vote(s).");
                    break;

                case "myvote":
                    if (latestChatter.HasVoted(msg.Author.Id))
                    {
                        string vote = "bad";
                        if (latestChatter.GetVote(msg.Author.Id))
                            vote = "good";
                        await msg.Channel.SendMessageAsync($"You voted \"{vote}\" on the most recent chatter.");
                    }
                    else
                        await msg.Channel.SendMessageAsync("You haven't voted on this chatter!");
                    break;

                default:
                    await msg.Channel.SendMessageAsync(Data.Chatterbot_GetVotingCommands());
                    break;
            }
        }

        private static void CleanupChatters()
        {
            List<Chatter> chattersout = new List<Chatter>();
            foreach (Chatter chatter in chatters)                //Test every entry for acceptableness and kill possible duplicates
                if (AcceptableChatter(chatter.Content) && !chattersout.Contains(chatter))
                    chattersout.Add(chatter);
            //Move them over
            chatters.Clear();
            foreach (Chatter chatter in chattersout)
                chatters.Add(chatter);
        }

        private static Chatter GetChatter() //Requires output sanitization still
        {
            if (chatters.Count == 0)
                return null;
            else
                return chatters[Program.rand.Next(0, chatters.Count)];
        }

        private static void RemoveLowestRated()   //Deletes one member of chatters with the lowest rating, will choose at random between multiple members with the same lowest rating.
        {
            int lowest = int.MaxValue;
            foreach (Chatter chatter in chatters)
                if (chatter.Rating() < lowest)
                    lowest = chatter.Rating();
            List<Chatter> candidates = new List<Chatter>();
            foreach (Chatter chatter in chatters)
            {
                if (chatter.Rating() == lowest)
                    candidates.Add(chatter);
            }

            chatters.Remove(candidates[Program.rand.Next(candidates.Count)]);
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
            public long Time_last_used;

            [JsonConstructor]
            public Chatter(string content, ulong origin_author = 0, ulong origin_msg = 0, ulong origin_channel = 0, ulong origin_guild = 0, long time_last_used = 0, ulong channel_last_used = 0, List<Voter> votes = null)
            {
                Content = content;
                Time_last_used = time_last_used;
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

            public IUser Author()   //Returns null if NA
            {
                if (Origin_author == 0)
                    return null;
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

            public bool GetVote(ulong userID)   //Returns false if voter not found, make sure to also check with HasVoted
            {
                foreach (Voter voter in Votes)
                    if (voter.VoterID == userID)
                        return voter.Vote;
                return false;
            }

            public bool HasVoted(ulong userID)
            {
                foreach (Voter voter in Votes)
                    if (voter.VoterID == userID)
                        return true;
                return false;
            }

            public IGuild OriginGuild() //Returns null if NA
            {
                if (Origin_guild == 0)
                    return null;
                return Program.client.GetGuild(Origin_guild);
            }

            public IMessage OriginMsg() //Returns null if NA
            {
                if (Origin_msg == 0)
                    return null;
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