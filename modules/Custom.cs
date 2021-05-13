using Discord;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal static class Custom   //A spaghetti containment module used for custom coded directives. These are all entirely server specific.
    {
        private static readonly Dictionary<ulong, GuildSettings> guilds = new Dictionary<ulong, GuildSettings>  {
            { 733421105448222771, new GuildSettings(true, false ) },  //Moths
            { 404972021676507138, new GuildSettings(true, false ) },  //Testing
            { 831287924133593090, new GuildSettings(false, false) }   //Apini
        };  //Which servers should these custom directives be enabled on? What directives should be enabled?

        public static bool ContainsBad(string input, out List<string> matches) //i'm so bloody sick of this, least now the bot won't participate.
        {
            matches = new List<string>();
            string pattern = @"\([a-zA-Z]{2,6}\/[a-zA-Z]{2,6}\)";
            MatchCollection rawmatches = Regex.Matches(input, pattern);
            if (rawmatches.Count > 0)
            {
                foreach (Match match in rawmatches)
                    matches.Add(match.Value);
                return true;
            }
            return false;
        }

        public static bool ContainsBad(string input)
        {
            return ContainsBad(input, out List<string> _);
        }

        public static void Init()
        {
            Program.client.MessageReceived += MessageRecieved;
            Program.client.GuildMemberUpdated += GuildMemberUpdated;
        }

        private static async Task GuildMemberUpdated(IGuildUser prev, IGuildUser current)
        {
            if (ContainsBad($"{current.Activity}{current.Nickname}{current.Status}{current.Username}"))
                Chatterbot.AddBlacklister(current.Username);
        }

        private static async Task MessageRecieved(IMessage msg)
        {
            if (msg.Channel is IGuildChannel && guilds.TryGetValue((msg.Channel as IGuildChannel).GuildId, out GuildSettings settings))
            {
                //Protect designated servers from @everyone and @here mentions.
                if (settings.massPingsBlocked && msg.MentionedEveryone)
                    await msg.DeleteAsync();
            }
        }

        private struct GuildSettings
        {
            public readonly bool filteringEnabled;
            public readonly bool massPingsBlocked;

            public GuildSettings(bool blockMassPings, bool filterAllMessages)
            {
                massPingsBlocked = blockMassPings;
                filteringEnabled = filterAllMessages;
            }
        }
    }
}