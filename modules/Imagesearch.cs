using System;

namespace MothBot.modules
{
    internal class Imagesearch
    {
        public string ImageSearch(string searchTerm)   //Finds a random imgur photo that matches search term, returns null if no valid photos can be found.
        {
            string link = "https://imgur.com/search?q=";
            link += searchTerm;
            link = link.Replace(' ', '+');
            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] raw = wc.DownloadData(link);
            string webData = System.Text.Encoding.UTF8.GetString(raw);

            Random rng = new Random();
            byte maxRetries = 32;
            do
            {
                int randNum = rng.Next(1, 64);
                for (byte i = 0; i < randNum; i++)   //Get random image link. (Links can start breaking if method cant find enough images!)
                {
                    int linkPtr = webData.IndexOf(@"<img alt="""" src=""//i.imgur.com/") + 31;
                    if (linkPtr == -1 || linkPtr >= webData.Length)
                        break;

                    link = "https://i.imgur.com/";
                    for (byte j = 0; j < 7; j++)
                    {
                        link += webData[linkPtr + j];
                        if (!((webData[linkPtr + j] >= '0' && webData[linkPtr + j] <= '9') || (webData[linkPtr + j] >= 'a' && webData[linkPtr + j] <= 'z') || (webData[linkPtr + j] >= 'A' && webData[linkPtr + j] <= 'Z')))
                        {
                            Console.WriteLine("Link invalid, Retries left:" + maxRetries);
                            link = null;    //Return null if given link is invalid.
                            maxRetries--;
                            break;
                        }
                    }
                    if (link == null)
                        break;
                    link += ".jpg";

                    webData = webData.Substring(linkPtr);
                }
                if (link != null)
                    break;
            } while (maxRetries > 0);   //Null link = invalid and requires another try
            return link;
        }
    }
}