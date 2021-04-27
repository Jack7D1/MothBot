using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal static class Custom   //A spaghetti containment module used for custom coded directives. These are all entirely server specific.
    {
        private static readonly Dictionary<ulong, GuildSettings> guilds = new Dictionary<ulong, GuildSettings>  { 
            { 733421105448222771, new GuildSettings(true ) },  //Moths
            { 404972021676507138, new GuildSettings(true ) },  //Testing
            { 831287924133593090, new GuildSettings(false) }   //Apini
        };  //Which servers should these custom directives be enabled on? What directives should be enabled?

        public static void Init()
        {
            Program.client.MessageReceived += MessageRecieved;
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
            public readonly bool massPingsBlocked;

            public GuildSettings(bool blockMassPings)
            {
                massPingsBlocked = blockMassPings;
            }
        }
    }
}