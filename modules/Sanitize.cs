namespace MothBot.modules
{
    internal class Sanitize
    {
        public string ScrubAnyRolePings(string inStr = "")  //Outputs a role ping scrubbed string, recieves target string for santization.
        {
            string outStr = inStr;
            //Blacklist for @everyone, @here and all role pings. Waste minimal processor time by simply skipping santization if these arent found.
            if (inStr.ToLower().Contains("@everyone") || inStr.ToLower().Contains("@here"))
            {
                outStr = inStr.Replace('@', ' ');
            }
            else if (inStr.Contains("<@&"))
            {
                while (true)    //Find all occurances of '<@&', select up to the next '>' and simply remove it.
                {
                    ushort strPtr0 = 0;
                    while (strPtr0 < inStr.Length)
                    {
                        if (inStr[strPtr0] == '<' && inStr[strPtr0 + 1] == '@' && inStr[strPtr0 + 2] == '&')
                            break;

                        strPtr0++;
                    }

                    ushort strPtr1 = (ushort)(strPtr0 + 1);
                    while (strPtr1 < inStr.Length)
                    {
                        if (inStr[strPtr1] == '>')
                            break;
                        strPtr1++;
                    }

                    //Remove this section between strPtr0 to strPtr1 inclusive
                    string strFirst = inStr.Substring(0, strPtr0);
                    string strSecond = inStr.Substring(strPtr1 + 1);
                    outStr = strFirst + strSecond;
                    if (strPtr0 <= inStr.Length || strPtr1 <= inStr.Length)
                        break;
                }
            }
            return outStr;
        }
    }
}