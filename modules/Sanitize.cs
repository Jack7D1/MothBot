using Discord.WebSocket;
using System.Collections.Generic;

namespace MothBot.modules
{
    internal class Sanitize
    {
        private const byte ID_LENGTH = 18;
        private static readonly char[] number = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public static string ReplaceAllMentionsWithID(string inStr, ulong ID)   //holy moly
        {
            if (inStr.Length < ID_LENGTH + "<@>".Length || !inStr.Contains("<@") || !inStr.Substring(inStr.IndexOf("<@")).Contains('>'))
                return ScrubEveryoneandHereMentions(inStr);
            string outStr = inStr, refStr = inStr;
            while (refStr.Length >= ID_LENGTH + "<@>".Length && refStr.Contains("<@") && refStr.Substring(refStr.IndexOf("<@")).Contains('>'))
            {
                int IDStartIndex = refStr.IndexOfAny(number, refStr.IndexOf("<@"), ID_LENGTH);
                outStr = outStr.Replace(refStr.Substring(IDStartIndex, ID_LENGTH), $"{ID}");
                refStr = refStr.Substring(refStr.IndexOf('>') + 1);
            }
            return ScrubEveryoneandHereMentions(outStr);
        }

        public static string ScrubRoleMentions(string inStr)
        {   //Removes any mentions for roles.
            List<SocketRole> roles = new List<SocketRole>();
            foreach (SocketGuild guild in Program.client.Guilds)    //Gets a list of any possible role the bot could feasibly ever mention
                foreach (SocketRole role in guild.Roles)
                    roles.Add(role);
            string outStr = inStr;
            foreach (SocketRole role in roles)
                if (outStr.Contains($"<@&{role.Id}>"))
                    outStr = outStr.Replace($"<@&{role.Id}>", role.Name);   //Replace with name of role instead
            return ScrubEveryoneandHereMentions(outStr);
        }

        private static string ScrubEveryoneandHereMentions(string inStr)  //Outputs a role ping scrubbed string, recieves target string for santization.
        {
            //Blacklist for @everyone, @here.
            return inStr.Replace("@everyone", "everyone").Replace("@here", "here");
        }
    }
}