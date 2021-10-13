using Discord.WebSocket;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MothBot.modules
{
    internal static class Sanitize
    //Largely public functions used for text input/output cleanup and regulation
    {
        public static string Dealias(string inStr) //Returns a dealiased version of a string, destroys input detail in exchange for comparability.
        {   //While this is not entirely accurate, it is consistent, which is adequate for the purpose of comparison.
            {
                inStr = inStr.Normalize(NormalizationForm.FormKC);
                StringBuilder stringBuilder = new StringBuilder();
                foreach (char c in inStr)
                {
                    var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                        stringBuilder.Append(c);
                }
                inStr = stringBuilder.ToString().Replace(" ", "").Replace("_", "").Replace("-", "");
            }
            //Massive table of manually mapped characters based on their alphabet lookalikes
            //Confusable table:
            List<char> A = new List<char> { 'ᗅ', 'Ꭺ', 'ꓮ', 'а', 'a', 'ɑ', 'α', '@', 'ä', 'ᴀ', 'å', 'λ' };
            List<char> B = new List<char> { 'ꓐ', 'Ᏼ', 'ᗷ', 'b', 'ᑲ', 'Ꮟ', 'ᖯ', 'в', 'ʙ', 'ß', '฿' };
            List<char> C = new List<char> { 'ꓚ', 'Ꮯ', 'с', 'c', 'ᴄ', 'ⲥ', '©', 'ς', 'ᥴ', '¢', '₵', '匚', 'ç' };
            List<char> D = new List<char> { 'Ꭰ', 'ꓓ', 'ᗪ', 'ᗞ', 'ԁ', 'ꓒ', 'd', 'Ꮷ', 'ᑯ', '∂', 'ᦔ', 'ᴅ', 'đ', 'ð' };
            List<char> E = new List<char> { 'ꓰ', 'ⴹ', 'Ꭼ', 'ꬲ', 'e', 'е', 'ҽ', '3', 'Ҽ', 'є', 'ᴇ', 'Σ', 'è', 'ê', 'ɇ', 'ë', 'é' };
            List<char> F = new List<char> { 'ᖴ', 'ꓝ', 'ꬵ', 'f', 'ꞙ', 'ẝ', 'ƒ', 'ᠻ', 'ꜰ', '₣' };
            List<char> G = new List<char> { 'Ꮐ', 'Ᏻ', 'ꓖ', 'ɡ', 'ց', 'ᶃ', 'g', 'ǥ', 'ᧁ', 'ɢ' };
            List<char> H = new List<char> { 'ꓧ', 'Ꮋ', 'ᕼ', 'հ', 'Ꮒ', 'h', 'һ', '卄', 'ʜ', 'н', 'ⱨ' };
            List<char> I = new List<char> { 'ǀ', 'ᛁ', 'ߊ', 'ⵏ', '1', '۱', 'ꓲ', 'ו', 'ן', 'ı', 'Ꭵ', 'і', 'ꙇ', 'i', 'ɩ', 'ι', 'ɪ', 'ӏ', '!', 'ɨ', 'ï', 'ł', 'ì' };
            List<char> J = new List<char> { 'ꓙ', 'Ꭻ', 'ᒍ', 'ϳ', 'ј', 'j', 'ĵ', 'ᴊ', 'נ' };
            List<char> K = new List<char> { 'ᛕ', 'Ꮶ', 'ꓗ', 'k', 'ҝ', 'к', 'ᴋ', '₭' };
            List<char> L = new List<char> { 'ꓡ', 'ᒪ', 'Ꮮ', 'ǀ', 'ߊ', 'l', 'ⵏ', '1', 'ꓲ', 'Ƚ', 'ʅ', 'L', 'ʟ', 'ᄂ' };
            List<char> M = new List<char> { 'ᗰ', 'ᛖ', 'Ꮇ', 'ꓟ', 'm', '爪', 'м', 'ᴍ', '₥' };
            List<char> N = new List<char> { 'ꓠ', 'ո', 'ռ', 'n', 'ɳ', 'η', 'ɴ', 'п', 'ñ', '₦' };
            List<char> O = new List<char> { '߀', 'ଠ', '০', '୦', '〇', 'ዐ', '0', 'ꓳ', 'ⵔ', '၀', 'σ', 'օ', 'ᴏ', '๐', '໐', 'ᴑ', 'ဝ', 'ⲟ', 'ഠ', '०', '੦', '૦', '௦', '౦', '೦', '൦',
                'o', 'о', 'ο', 'ჿ', 'ø', 'ө', 'ö', 'ㄖ' };
            List<char> P = new List<char> { 'ꓑ', 'Ꮲ', 'ᑭ', 'р', 'p', 'ρ', 'ⲣ', 'ᴘ', 'þ', '₱' };
            List<char> Q = new List<char> { 'ⵕ', 'q', 'գ', 'զ', 'ԛ', 'ɋ' };
            List<char> R = new List<char> { 'Ꭱ', 'Ꮢ', 'ꓣ', 'ᖇ', 'r', 'г', 'ⲅ', 'ᴦ', 'ꭇ', 'ꭈ', 'ɾ', 'ʀ', 'я', 'ɽ', '尺' };
            List<char> S = new List<char> { 'ꓢ', 'Ꮪ', 'ꜱ', 's', 'ѕ', 'ƽ', 'ʂ' };
            List<char> T = new List<char> { 'Ꭲ', 'ꓔ', 't', 'ƚ', 'т', 'ŧ', 'ᴛ', '†', '₮' };
            List<char> U = new List<char> { 'ሀ', 'ꓴ', 'ᑌ', 'ꭒ', 'υ', 'u', 'ʋ', 'ᴜ', 'ս', 'ꭎ', 'ꞟ', 'µ', 'μ', 'ʉ', 'ú' };
            List<char> V = new List<char> { 'ꓦ', '٧', '۷', 'ⴸ', 'Ꮩ', 'ᐯ', 'ᴠ', 'ѵ', 'v', 'ט', 'ν' };
            List<char> W = new List<char> { 'Ꮃ', 'Ꮤ', 'ꓪ', 'ᴡ', 'ѡ', 'ա', 'w', 'ԝ', 'ɯ', 'ω', 'щ', '₩', '山' };
            List<char> X = new List<char> { 'ᚷ', 'ꓫ', 'ⵝ', 'ᕁ', 'х', 'x', 'ᕽ', 'χ', '×', 'ӿ' };
            List<char> Y = new List<char> { 'Ꭹ', 'ꓬ', 'Ꮍ', 'у', 'ɣ', 'γ', 'ყ', 'y', 'ꭚ', 'ᶌ', 'ʏ', 'ү', 'ỿ', '¥', 'ㄚ' };
            List<char> Z = new List<char> { 'Ꮓ', 'ꓜ', 'ᴢ', 'z', 'ƶ', '乙' };

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
            const string pingRegex = @"(<@\d{18}>)|(<@!\d{18}>)";
            if (inStr == null || !Regex.IsMatch(inStr, pingRegex))
                return ScrubEveryoneandHereMentions(inStr);

            MatchCollection mentions = Regex.Matches(inStr, pingRegex);
            string outStr = inStr;
            foreach (Match mention in mentions)
                outStr = outStr.Replace(mention.Value, $"<@!{ID}>");
            return ScrubEveryoneandHereMentions(outStr);
        }

        public static string ScrubMentions(string inStr, bool usermentions = true, bool rolementions = true)
        {
            if (!(usermentions || rolementions))
                return ScrubEveryoneandHereMentions(inStr);

            foreach (SocketGuild guild in Master.client.Guilds)
            {
                if (usermentions)
                    foreach (SocketUser user in guild.Users)
                        inStr = inStr.Replace($"<@{user.Id}>", user.Username);
                if (rolementions)
                    foreach (SocketRole role in guild.Roles)
                        inStr = inStr.Replace($"<@&{role.Id}>", role.Name);
            }
            return ScrubEveryoneandHereMentions(inStr);
        }

        private static string ScrubEveryoneandHereMentions(string inStr)  //Outputs a role ping scrubbed string, recieves target string for santization.
        {
            //Blacklist for @everyone, @here.
            return inStr.Replace("@everyone", "everyone").Replace("@here", "here");
        }
    }
}