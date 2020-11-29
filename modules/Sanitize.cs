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
                do    //Find all occurances of '<@&', select up to the next '>' and simply remove it.
                {
                    ushort strPtr0 = (ushort)inStr.IndexOf("<@&");
                    ushort strPtr1 = (ushort)(strPtr0 + 1);
                    while (strPtr1 < inStr.Length)
                    {
                        if (inStr[strPtr1] == '>' || strPtr1 >= inStr.Length - 1)
                            break;
                        strPtr1++;
                    }
                    if (inStr[strPtr1] != '>')
                        break;
                    //Remove this section between strPtr0 to strPtr1 inclusive
                    string strFirst = inStr.Substring(0, strPtr0);
                    string strSecond = inStr.Substring(strPtr1 + 1);
                    outStr = strFirst + strSecond;
                } while (outStr.Contains("<@&"));
            }
            if (outStr.Length == 0)
                outStr = " ";
            return outStr;
        }
    }
}