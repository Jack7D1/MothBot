using Discord;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal static class Custom   //A spaghetti containment module used for custom coded directives. These are all entirely server specific.
    {
        private static readonly Dictionary<ulong, GuildSettings> guilds = new Dictionary<ulong, GuildSettings>  {
            { 733421105448222771, new GuildSettings() },  //Moths
            { 404972021676507138, new GuildSettings() },  //Testing
            { 831287924133593090, new GuildSettings() }   //Apini
        };  //Which servers should these custom directives be enabled on? What directives should be enabled?

        public static void Init()
        {
            Program.client.MessageReceived += MessageRecieved;
        }

        private static async Task MessageRecieved(IMessage msg)
        {
            //if (msg.Channel is IGuildChannel && guilds.TryGetValue((msg.Channel as IGuildChannel).GuildId, out GuildSettings settings))
            {
            }
            if (Regex.IsMatch($"{msg.Content}{msg.Author.Status}{msg.Author.Username}", @":[^\s\n]{0,16}trans(?=[^a-fh-rt-zA-FH-RT-Z0-9])[^\s\n]{0,16}:|(\(|^)[a-zA-Z]{2,6}\/[a-zA-Z]{2,6}(\)|$)"))   //Supremacy movements will not be tolerated.
                Chatterbot.AddBlacklister(msg.Author.Username);
        }

        private struct GuildSettings
        {
            public readonly bool a2;

            public GuildSettings(bool a = false)
            {
                a2 = a;
            }
        }
    }
}