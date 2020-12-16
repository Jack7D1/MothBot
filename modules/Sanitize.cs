using Discord.WebSocket;

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

        public static string ScrubRoleMentions(SocketMessage src)
        {   //Removes any mentions for roles.
            if (src.MentionedRoles.Count > 0)
            {
                string content = ScrubEveryoneandHereMentions(src.Content);
                foreach (SocketRole mention in src.MentionedRoles)
                    content = content.Replace($"<@&{mention.Id}>", mention.Name);
                return content;
            }
            else
                return ScrubEveryoneandHereMentions(src.Content);
        }

        private static string ScrubEveryoneandHereMentions(string inStr)  //Outputs a role ping scrubbed string, recieves target string for santization.
        {
            //Blacklist for @everyone, @here.
            return inStr.Replace("@everyone", "everyone").Replace("@here", "here");
        }
    }
}