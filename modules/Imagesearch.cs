using System;

namespace MothBot.modules
{
    internal class Imagesearch
    {
        private const string linkHeader = "https://i.imgur.com/";
        private const string linkFooter = ".jpg";
        private const string linkSearch = "https://imgur.com/search?q=";
        private const byte maxRetries = 255;

        public string ImageSearch(string searchTerm)   //Finds a random imgur photo that matches search term, returns null if no valid photos can be found.
        {
            searchTerm = linkSearch + searchTerm;
            searchTerm = searchTerm.Replace(' ', '+');
            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] raw = wc.DownloadData(searchTerm);
            string webData = System.Text.Encoding.UTF8.GetString(raw);
            Random rng = new Random();
            string link;
            int linkPtr = 0;

            byte retries = maxRetries;
            do
            {
                int randNum = rng.Next(1, 128);
                for (byte i = 0; i < randNum; i++)   //Get random image link. (Links can start breaking if method cant find enough images!)
                {
                    linkPtr = webData.IndexOf(@"<img alt="""" src=""//i.imgur.com/") + 31;
                    if (linkPtr == -1 || linkPtr >= webData.Length)
                        break;

                    webData = webData.Substring(linkPtr);
                }
                link = webData.Substring(linkPtr, 7);
                if (CheckValid(link))
                {
                    Console.WriteLine("Image found, took " + (maxRetries - retries) + " tries.");
                    return linkHeader + link + linkFooter;
                }
                retries--;
                webData = webData.Substring(linkPtr);
            } while (retries > 0);

            //Last ditch effort to some *something*! Shooting for the top result.
            webData = System.Text.Encoding.UTF8.GetString(raw);
            linkPtr = webData.IndexOf(@"<img alt="""" src=""//i.imgur.com/") + 31;
            link = webData.Substring(linkPtr, 7);
            if (CheckValid(link))
            {
                Console.WriteLine("First result image found in last ditch effort.");
                return linkHeader + link + linkFooter;
            }
            else
            {
                Console.WriteLine("No valid results found, returning null.");
                return null;
            }
        }

        private bool CheckValid(string inStr)
        {
            for (byte i = 0; i < 7; i++)
                if (!((inStr[i] >= '0' && inStr[i] <= '9') || (inStr[i] >= 'a' && inStr[i] <= 'z') || (inStr[i] >= 'A' && inStr[i] <= 'Z')))
                    return false;
            //Internet validate
            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] raw = wc.DownloadData(linkHeader + inStr + linkFooter);
            string webData = System.Text.Encoding.UTF8.GetString(raw);
            if (webData.Contains("https://i.imgur.com/removed.png"))
                return false;
            return true;
        }
    }
}