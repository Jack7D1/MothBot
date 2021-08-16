using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal static class Chatterbot
    {
        public const string PATH_CHATTERS = "../../data/chatters.json";
        public const string PATH_CHATTERS_BACKUP = "../../resources/backupchatters.txt";
        public const string PATH_CHATTERS_BLACKLIST = "../../data/blacklist.txt";
        private const ushort CHATTERS_CHANCE_TO_CHAT = 128;

        //Value is an inverse, (1 out of CHANCE_TO_CHAT chance), similar for CHANCE_TO_SAVE
        private const ushort CHATTERS_CHANCE_TO_SAVE = 8;

        private const ushort CHATTERS_MAX_COUNT = 2048;
        private static readonly List<string> blacklist = new List<string>();  //Contains strings that will be filtered out of chatters, such as discord invite links. Also used for filtering mature materials from bot output.

        private static readonly List<Chatter> chatters;

        static Chatterbot()
        {
            Program.client.ReactionAdded += ReactionAdded;
            Program.client.ReactionRemoved += ReactionRemoved;

            foreach (string blacklister in Data.Files_Read(PATH_CHATTERS_BLACKLIST))
                blacklist.Add(Sanitize.Dealias(blacklister));
            chatters = new List<Chatter>();
            string fileData = File.ReadAllText(PATH_CHATTERS, Encoding.UTF8);
            if (fileData == null || fileData.Length == 0 || fileData == "[]")
            {
                Logging.LogtoConsoleandFile($"No chatters found at {PATH_CHATTERS}, running with empty...");
                chatters.Clear();
            }
            else
            {
                chatters = JsonConvert.DeserializeObject<List<Chatter>>(fileData.Replace("☼", "").Replace("™️", "(tm)").Normalize(NormalizationForm.FormKC));
                CleanupChatters();
                if (chatters.Count > CHATTERS_MAX_COUNT)
                {
                    int overflow = chatters.Count - CHATTERS_MAX_COUNT;
                    Logging.LogtoConsoleandFile($"CHATTERS: Chatter overflow found, deleting {overflow} lowest rated entries.");
                    for (int i = overflow; i > 0; i--)
                        RemoveLowestRated();
                }
            }
        }

        public static bool AddBlacklister(string entry)  //Returns false if already present
        {
            entry = Sanitize.Dealias(entry);
            if (blacklist.Contains(entry))
                return false;
            else
            {
                blacklist.Add(entry);
                SaveBlacklist();
                SaveChatters();
                return true;
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
                    if (args.Length < 3)
                        await msg.Channel.SendMessageAsync("Minimum 3 characters for blacklist entries");
                    else if (AddBlacklister(args))
                        await msg.Channel.SendMessageAsync($"\"{args}\" added to blacklist successfully");
                    else
                        await msg.Channel.SendMessageAsync("Entry already in blacklist.");
                    break;

                case "remove":
                    if (RemoveBlacklister(args))
                        await msg.Channel.SendMessageAsync($"\"{args}\" removed from blacklist successfully");
                    else
                        await msg.Channel.SendMessageAsync("Entry not found in blacklist");
                    break;

                default:
                    await msg.Channel.SendMessageAsync(Data.Chatterbot_GetBlacklistCommands());
                    break;
            }
        }

        public static async Task ChatterHandler(SocketMessage src)
        {
            if (src.Author.IsBot)
                return;
            bool mentionsMe = false, doNotSave = false;
            if ((src.Channel is ITextChannel && (src.Channel as ITextChannel).IsNsfw))
                doNotSave = true;
            foreach (SocketUser mention in src.MentionedUsers)
            {
                if (mention.Id == Data.MY_ID)
                {
                    mentionsMe = true;
                    break;
                }
            }

            if (mentionsMe || (Program.rand.Next(CHATTERS_CHANCE_TO_CHAT) == 0))
            {
                //Send Chatter
                Chatter outChatter = GetChatter();
                if (outChatter != null)
                {
                    outChatter.Channel_last_used = src.Channel.Id;
                    outChatter.Time_last_used = DateTime.Now.Ticks;
                    RestMessage chatter = await src.Channel.SendMessageAsync(Sanitize.ReplaceAllMentionsWithID(outChatter.Content, src.Author.Id));
                    await chatter.AddReactionAsync(new Emoji("⬆️"));
                    await chatter.AddReactionAsync(new Emoji("⬇️"));
                }
            }
            //Save Chatter
            Chatter candidate = new Chatter(Sanitize.ScrubMentions(Encoding.UTF8.GetString(Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(src.Content))), false).Replace('\n', ' '), src.Author.Id, src.Id, src.Channel.Id, (src.Author as IGuildUser).GuildId);

            if (!doNotSave && Program.rand.Next(CHATTERS_CHANCE_TO_SAVE) == 0 && AcceptableChatter(candidate))  //checks to see if it's a valid and acceptable chatter then saves if applicable.
            {
                if (chatters.Count >= CHATTERS_MAX_COUNT)
                    RemoveLowestRated();
                else
                    chatters.Add(candidate);
                SaveChatters();
            }
        }

        public static async Task CommandHandler(SocketMessage msg, string args)    //Expects to be called from main command tree with the keyword chatter to vote on the most recently used chatter in the sent channel.
        {
            Chatter latestChatter = null;
            if (args.Length > 0 && args != "leaderboard")
            {
                long latestTime = 0;
                foreach (Chatter chatter in chatters)
                {
                    if (chatter.Channel_last_used == msg.Channel.Id && chatter.Time_last_used > latestTime)
                        latestTime = chatter.Time_last_used;
                }
                if (latestTime == 0 || (DateTime.Now - DateTime.FromBinary(latestTime)).TotalHours > 6)
                {
                    await msg.Channel.SendMessageAsync("I can't find any recently sent chatters in this channel, check if this is the correct channel or that you aren't refrencing a non-chatter message.");
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
                case "clearvote":
                    if (latestChatter.ClearVote(msg.Author.Id))
                        await msg.AddReactionAsync(new Emoji("\u2705"));
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

                case "leaderboard":
                    {
                        if (!(GetLeaders() is List<Chatter> places))
                        {
                            await msg.Channel.SendMessageAsync("Could not generate chatter leaderboard, ratings too similar or too few chatters!");
                            break;
                        }
                        List<string> outmsgs = new List<string> { "**Leaderboard**" };
                        string[] ribbon = { ":first_place:", ":second_place:", ":third_place:" };

                        for (int i = 0; i < 3; i++)
                        {
                            string username = "Unknown";
                            if (places[i].Author() != null)
                            {
                                username = places[i].Author().Username;
                            }

                            string creditstr = $"Accreddited to {username}.\n";
                            outmsgs.Add($"{ribbon[i]} \nChatter: \" {places[i].Content} \" \n{creditstr}Which scored a rating of {places[i].Rating()} out of {places[i].Votes.Count} total votes.");
                        }
                        foreach (string outmsg in outmsgs)
                            await msg.Channel.SendMessageAsync(outmsg);
                    }
                    break;

                default:
                    await msg.Channel.SendMessageAsync(Data.Chatterbot_GetVotingCommands());
                    break;
            }
        }

        public static bool ContentsBlacklisted(string inStr)
        {
            inStr = Sanitize.Dealias(inStr);
            foreach (string blacklister in blacklist)
                if (inStr.Contains(blacklister))
                    return true;
            return false;
        }

        public static async Task PrependBackupChatters(IMessageChannel ch = null)    //Does what it says, this can mess with the chatters length however so it should only be called by operators
        {
            List<Chatter> chatterstoprepend = new List<Chatter>();
            List<string> backupchatters = Data.Files_Read(PATH_CHATTERS_BACKUP);
            int overshoot = chatters.Count + backupchatters.Count - CHATTERS_MAX_COUNT;
            if (overshoot > 0)
            {
                if (ch != null)
                    await ch.SendMessageAsync($"Warning: Prepending with {backupchatters.Count} lines overshot the max count [{CHATTERS_MAX_COUNT}] by {overshoot}. Deleting excess from prepend before saving.");
                backupchatters.RemoveRange(0, overshoot);
            }
            foreach (string chatter in backupchatters)
                chatterstoprepend.Add(new Chatter(chatter));

            chatters.InsertRange(0, chatterstoprepend);

            if (ch != null)
                await ch.SendMessageAsync("Prepend successful");
            await Logging.LogtoConsoleandFile($"CHATTERS: Prepend Completed with {overshoot} items removed due to limit of {CHATTERS_MAX_COUNT} entries.");
            SaveChatters();
        }

        public static void SaveBlacklist()
        {
            List<string> newBlacklist = new List<string>();
            foreach (string blacklister in blacklist)
            {
                if (!newBlacklist.Contains(blacklister))
                    newBlacklist.Add(blacklister);
            }
            blacklist.Clear();
            blacklist.AddRange(newBlacklist);
            Data.Files_Write(PATH_CHATTERS_BLACKLIST, newBlacklist);
        }

        public static void SaveChatters()
        {
            CleanupChatters();
            string outStr = JsonConvert.SerializeObject(chatters, Formatting.Indented);
            File.WriteAllText(PATH_CHATTERS, outStr, Encoding.UTF8);
        }

        private static bool AcceptableChatter(Chatter chatter)
        {
            string inStr = chatter.Content.ToLower();
            if (inStr.Length < 2 || inStr.Replace(" ", "").Length == 0)     //Catch empty strings
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
            if (Utilities.IsBanned(chatter.Origin_author) || ContentsBlacklisted(inStr))
                return false;
            return true;
        }

        private static void CleanupChatters()
        {
            List<Chatter> chattersout = new List<Chatter>();
            List<string> chattersContents = new List<string>();
            foreach (Chatter chatter in chatters)                //Test every entry for acceptableness and kill possible duplicates
                if (!chattersContents.Contains(chatter.Content) && AcceptableChatter(chatter))
                {
                    chattersout.Add(chatter);
                    chattersContents.Add(chatter.Content);
                }
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

        private static Chatter GetChatterFromMessage(IUserMessage msg)
        {
            Chatter targetChatter = null;
            if (msg.Author.Id == Data.MY_ID)    //If it's a chatter I would have sent it
            {
                foreach (Chatter chatter in chatters)
                    if (chatter.Content == msg.Content)
                    {
                        targetChatter = chatter;
                        break;
                    }
            }
            return targetChatter;
        }

        private static List<Chatter> GetLeaders()    //Gets the three highest rated chatters, returns null if unsuccessful.
        {
            List<Chatter> places = new List<Chatter> { null, null, null };
            int firstscore = int.MinValue, secondscore = int.MinValue, thirdscore = int.MinValue;

            foreach (Chatter chatter in chatters)   //Get the highest rating
                if (chatter.Rating() > firstscore)
                    firstscore = chatter.Rating();
            foreach (Chatter chatter in chatters)   //Second highest rating
                if (chatter.Rating() > secondscore && chatter.Rating() < firstscore)
                    secondscore = chatter.Rating();
            foreach (Chatter chatter in chatters)   //Third highest rating
                if (chatter.Rating() > thirdscore && chatter.Rating() < secondscore)
                    thirdscore = chatter.Rating();
            if (firstscore == int.MinValue || secondscore == int.MinValue || thirdscore == int.MinValue)
                return null;

            foreach (Chatter chatter in chatters)
            {
                if (places[0] == null && chatter.Rating() == firstscore)
                    places[0] = chatter;
                else if (places[1] == null && chatter.Rating() == secondscore)
                    places[1] = chatter;
                else if (places[2] == null && chatter.Rating() == thirdscore)
                    places[2] = chatter;
                if (!places.Contains(null))
                    break;
            }
            return places;
        }

        private static Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel ch, SocketReaction reaction)
        {
            IUserMessage msg = message.DownloadAsync().Result;
            IUser usr = Program.client.Rest.GetUserAsync(reaction.UserId).Result;
            Chatter chatter = GetChatterFromMessage(msg);
            if (chatter == null || usr.IsBot)
                return Task.CompletedTask;
            bool vote;
            switch (reaction.Emote.Name)
            {
                case "⬆️":
                    vote = true;
                    break;

                case "⬇️":
                    vote = false;
                    break;

                default:
                    return Task.CompletedTask;
            }
            chatter.AddVote(usr.Id, vote);
            return Task.CompletedTask;
        }

        private static Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel ch, SocketReaction reaction)
        {
            IUserMessage msg = message.DownloadAsync().Result;
            IUser usr = Program.client.Rest.GetUserAsync(reaction.UserId).Result;
            Chatter chatter = GetChatterFromMessage(msg);
            if (chatter == null || usr.IsBot)
                return Task.CompletedTask;

            switch (reaction.Emote.Name)
            {
                case "⬆️":
                case "⬇️":
                    chatter.ClearVote(usr.Id);
                    break;
            }
            return Task.CompletedTask;
        }

        private static bool RemoveBlacklister(string blacklister)   //Returns false if not found in blacklist
        {
            blacklister = Sanitize.Dealias(blacklister);
            if (!blacklist.Contains(blacklister))
                return false;
            else
            {
                blacklist.Remove(blacklister);
                SaveBlacklist();
                return true;
            }
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

            public RestUser Author()   //Returns null if NA
            {
                if (Origin_author == 0)
                    return null;

                return Program.client.Rest.GetUserAsync(Origin_author).Result;
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

            public RestGuild OriginGuild() //Returns null if NA
            {
                if (Origin_guild == 0)
                    return null;
                return Program.client.Rest.GetGuildAsync(Origin_guild).Result;
            }

            public IMessage OriginMsg() //Returns null if NA
            {
                if (Origin_msg == 0)
                    return null;
                return (Program.client.Rest.GetChannelAsync(Origin_channel) as IMessageChannel).GetMessageAsync(Origin_msg).Result;
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