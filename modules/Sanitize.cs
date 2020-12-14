using Discord.WebSocket;

namespace MothBot.modules
{
    internal class Sanitize
    {
        private const byte ID_LENGTH = 18;

        public static string ReplaceAllMentionsWithID(string inStr, ulong ID)
        {
            int startIndex = inStr.IndexOf("<@!"), stopIndex = inStr.LastIndexOf("<@!");
            if (startIndex == -1)
                return ScrubEveryoneandHereMentions(inStr);
            if (startIndex == stopIndex)
            {
                if (inStr.Substring(startIndex).Length <= ID_LENGTH || !inStr.Substring(startIndex + ID_LENGTH).Contains('>'))
                    return ScrubEveryoneandHereMentions(inStr);
                string outStr = inStr.Replace($"<@!{inStr.Substring(startIndex + 3, ID_LENGTH)}>", $"<@!{ID}>");
                return ScrubEveryoneandHereMentions(outStr);
            }
            else
            {
                while (true)
                {
                    if (inStr.Substring(startIndex).Length <= ID_LENGTH || !inStr.Substring(startIndex + ID_LENGTH).Contains('>'))
                        return ScrubEveryoneandHereMentions(inStr);
                    string outStr = inStr.Replace($"<@!{inStr.Substring(startIndex + 3, ID_LENGTH)}>", $"<@!{ID}>");
                    if (outStr.IndexOf("<@!") == -1 || outStr[outStr.IndexOf("<@!") + ID_LENGTH + 4] != '>')
                        return ScrubEveryoneandHereMentions(outStr);
                }
            }
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

        private static string ScrubEveryoneandHereMentions(string inStr = "")  //Outputs a role ping scrubbed string, recieves target string for santization.
        {
            //Blacklist for @everyone, @here.
            if (inStr.ToLower().Contains("@everyone") || inStr.ToLower().Contains("@here"))
            {
                return inStr.Replace("@everyone", "everyone").Replace("@here", "here");
            }
            else
                return inStr;
        }
    }
}