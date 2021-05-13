using Discord.WebSocket;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MothBot.modules
{
    internal static class Sanitize
    //Largely public functions used for text input/output cleanup and regulation
    {
        public static string Dealias(string inStr) //Returns a dealiased version of a string, destroys input detail in exchange for comparability.
        {   //While this is not entirely accurate, it is consistent, which is adequate for the purpose of comparison.
            inStr = inStr.Normalize(NormalizationForm.FormKC).ToUpperInvariant().Normalize(NormalizationForm.FormKC);
            //Confusable table:
            List<char> A = new List<char> { 'ᗅ', 'Ꭺ', 'ꓮ', 'а', 'a', 'ɑ', 'α', '@' };
            List<char> B = new List<char> { 'ꓐ', 'Ᏼ', 'ᗷ', 'b', 'ᑲ', 'Ꮟ', 'ᖯ' };
            List<char> C = new List<char> { 'ꓚ', 'Ꮯ', 'с', 'c', 'ᴄ', 'ⲥ' };
            List<char> D = new List<char> { 'Ꭰ', 'ꓓ', 'ᗪ', 'ᗞ', 'ԁ', 'ꓒ', 'd', 'Ꮷ', 'ᑯ' };
            List<char> E = new List<char> { 'ꓰ', 'ⴹ', 'Ꭼ', 'ꬲ', 'e', 'е', 'ҽ', '3' };
            List<char> F = new List<char> { 'ᖴ', 'ꓝ', 'ꬵ', 'f', 'ꞙ', 'ẝ' };
            List<char> G = new List<char> { 'Ꮐ', 'Ᏻ', 'ꓖ', 'ɡ', 'ց', 'ᶃ', 'g' };
            List<char> H = new List<char> { 'ꓧ', 'Ꮋ', 'ᕼ', 'հ', 'Ꮒ', 'h', 'һ' };
            List<char> I = new List<char> { 'ǀ', 'ᛁ', 'ߊ', 'l', 'ⵏ', '1', '۱', 'ꓲ', 'ו', 'ן', 'ı', 'Ꭵ', 'і', 'ꙇ', 'i', 'ɩ', 'ι', 'ɪ', 'ӏ', '!' };
            List<char> J = new List<char> { 'ꓙ', 'Ꭻ', 'ᒍ', 'ϳ', 'ј', 'j' };
            List<char> K = new List<char> { 'ᛕ', 'Ꮶ', 'ꓗ', 'k' };
            List<char> L = new List<char> { 'ꓡ', 'ᒪ', 'Ꮮ', 'ǀ', 'ߊ', 'l', 'ⵏ', '1', 'ꓲ' };
            List<char> M = new List<char> { 'ᗰ', 'ᛖ', 'Ꮇ', 'ꓟ', 'm' };
            List<char> N = new List<char> { 'ꓠ', 'ո', 'ռ', 'n' };
            List<char> O = new List<char> { '߀', 'ଠ', '০', '୦', '〇', 'ዐ', '0', 'ꓳ', 'ⵔ', '၀', 'σ', 'օ', 'ᴏ', '๐', '໐', 'ᴑ', 'ဝ', 'ⲟ', 'ഠ', '०', '੦', '૦', '௦', '౦', '೦', '൦', 'o', 'о', 'ο', 'ჿ' };
            List<char> P = new List<char> { 'ꓑ', 'Ꮲ', 'ᑭ', 'р', 'p', 'ρ', 'ⲣ' };
            List<char> Q = new List<char> { 'ⵕ', 'q', 'գ', 'զ', 'ԛ' };
            List<char> R = new List<char> { 'Ꭱ', 'Ꮢ', 'ꓣ', 'ᖇ', 'r', 'г', 'ⲅ', 'ᴦ', 'ꭇ', 'ꭈ' };
            List<char> S = new List<char> { 'ꓢ', 'Ꮪ', 'ꜱ', 's', 'ѕ', 'ƽ' };
            List<char> T = new List<char> { 'Ꭲ', 'ꓔ', 't' };
            List<char> U = new List<char> { 'ሀ', 'ꓴ', 'ᑌ', 'ꭒ', 'υ', 'u', 'ʋ', 'ᴜ', 'ս', 'ꭎ', 'ꞟ', 'µ' };
            List<char> V = new List<char> { 'ꓦ', '٧', '۷', 'ⴸ', 'Ꮩ', 'ᐯ', 'ᴠ', 'ѵ', 'v', 'ט', 'ν' };
            List<char> W = new List<char> { 'Ꮃ', 'Ꮤ', 'ꓪ', 'ᴡ', 'ѡ', 'ա', 'w', 'ԝ', 'ɯ' };
            List<char> X = new List<char> { 'ᚷ', 'ꓫ', 'ⵝ', 'ᕁ', 'х', 'x', 'ᕽ' };
            List<char> Y = new List<char> { 'Ꭹ', 'ꓬ', 'Ꮍ', 'у', 'ɣ', 'γ', 'ყ', 'y', 'ꭚ', 'ᶌ', 'ʏ', 'ү', 'ỿ' };
            List<char> Z = new List<char> { 'Ꮓ', 'ꓜ', 'ᴢ', 'z' };

            List<List<char>> confusables = new List<List<char>> { A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z };   //We need iterators for boilerplate.
            List<char> confusablesKey = new List<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
            //holy moly

            string outStr = "";
            for (int i = 0; i < inStr.Length; i++)
            {
                bool match = false;
                for (int k = 0; k < confusables.Count; k++)
                {
                    foreach (char c in confusables[k])
                        if (c == inStr[i])
                        {
                            outStr += confusablesKey[k];
                            match = true;
                            break;
                        }
                    if (match)
                        break;
                }
                if (!match)
                    outStr += inStr[i];
            }
            return outStr;
        }

        public static string ReplaceAllMentionsWithID(string inStr, ulong ID)   //regex saves the day
        {
            if (inStr.Length < (18 + "<@>").Length || !Regex.IsMatch(inStr, @"<@\d{18}>"))
                return ScrubEveryoneandHereMentions(inStr);

            MatchCollection mentions = Regex.Matches(inStr, @"<@\d{18}>");
            string outStr = inStr;
            foreach (Match mention in mentions)
                outStr = outStr.Replace(mention.Value, $"<@{ID}>");
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