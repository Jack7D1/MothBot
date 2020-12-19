using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace MothBot.modules
{
    internal class Sanitize
    {
        private const byte ID_LENGTH = 18;
        private static readonly char[] number = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        private static readonly List<string> publicWebsiteTags = new List<string>{  //Used for detecting a possible advertising link.
            ".com", ".net", ".org", ".io" };

        private static readonly List<string> whiteListedDomains = new List<string>{ //Image hosting domains that are definitely not for advertising. (probably)
            "discordapp", "discord", "twitter", "google", "tenor", "imgur", "youtube", "github", "4chan" };

        public static bool AcceptableChatter(string inStr)
        {
            inStr = inStr.ToLower();
            if (inStr.Length < 2 || inStr.Replace(" ", "").Length == 0) // Catch empty strings
                return false;

            ushort uniqueChars = 0;
            string clearChars = inStr.Replace(inStr[0].ToString(), "");     //One unique character deleted
            if (clearChars.Length != 0)
                uniqueChars++;
            while (clearChars.Length != 0)
            {
                clearChars = clearChars.Replace(clearChars[0].ToString(), "");
                uniqueChars++;
            }
            if (uniqueChars < 4)    //A message with less than four unique characters is probably just keyboard mash.
                return false;

            for (byte i = 0; i < publicWebsiteTags.Count; i++)
                if (inStr.Contains(publicWebsiteTags[i]))   //This part of the string might contain a website
                {
                    if (inStr.Length <= 16)
                        return false;
                    int index = inStr.IndexOf(publicWebsiteTags[i]);
                    string preamble = inStr.Substring(Math.Max(index - 16, 0), 16);
                    bool isWhitelisted = false;
                    for (byte k = 0; k < whiteListedDomains.Count; k++) //This is a whitelisted website
                        if (preamble.Contains(whiteListedDomains[k]))
                        {
                            isWhitelisted = true;
                            break;
                        }
                    return isWhitelisted;
                }
            return true;
        }

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