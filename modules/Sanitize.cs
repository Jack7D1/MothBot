using Discord.WebSocket;

namespace MothBot.modules
{
    internal class Sanitize
    {
        public static string ScrubEveryoneandHereMentions(string inStr = "")  //Outputs a role ping scrubbed string, recieves target string for santization.
        {
            //Blacklist for @everyone, @here.
            if (inStr.ToLower().Contains("@everyone") || inStr.ToLower().Contains("@here"))
            {
                return inStr.Replace("@everyone", "everyone").Replace("@here", "here");
            }
            else
                return inStr;
        }

        public static string ScrubRoleMentions(SocketMessage src)
        {   //Removes any mentions for roles.
            if (src.MentionedRoles.Count > 0)
            {
                string content = ScrubEveryoneandHereMentions(src.Content);
                foreach (SocketRole mention in src.MentionedRoles)
                {
                    content = content.Replace($"<@&{mention.Id}>", mention.Name);
                }
                return content;
            }
            else
                return ScrubEveryoneandHereMentions(src.Content);
        }
    }
}