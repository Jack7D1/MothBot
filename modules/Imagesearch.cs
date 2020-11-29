using System;

namespace MothBot.modules
{
    internal class Imagesearch
    {
        private string linkHeader = "https://i.imgur.com/";
        private string linkFooter = ".jpg";
        private string linkSearch = "https://imgur.com/search?q=";
        public byte maxRetries = 255;
        public bool enable_firstResultFallback = true;
        private System.Net.WebClient _webClient = new System.Net.WebClient();

        public string ImageSearch(string searchTerm)   //Finds a random imgur photo that matches search term, returns null if no valid photos can be found.
        {
            searchTerm = linkSearch + searchTerm;
            searchTerm = searchTerm.Replace(' ', '+');
            byte[] raw = _webClient.DownloadData(searchTerm);
            string webData = System.Text.Encoding.UTF8.GetString(raw);
            Random rng = new Random();
            string link = "";
            int linkPtr = -1;
            byte retries = maxRetries;
            do
            {
                short randNum = (short)rng.Next(1, 64);
                for (short i = 0; i < randNum; i++)   //Get random image link. (Links can start breaking if method cant find enough images!)
                {
                    linkPtr = webData.IndexOf(@"<img alt="""" src=""//i.imgur.com/");
                    link = webData.Substring(linkPtr + 31, 7);
                    if (linkPtr == -1 || linkPtr + 7 >= webData.Length)
                        break;

                    webData = webData.Substring(linkPtr + 32);
                }
                if (CheckValid(link))
                {
                    Console.WriteLine("Image found, took " + (maxRetries - retries) + " tries.");
                    return linkHeader + link + linkFooter;
                }
                retries--;
                webData = webData.Substring(linkPtr + 32);
            } while (retries > 0);

            if (!enable_firstResultFallback)
            {
                Console.WriteLine("No valid results found, returning null.");
                return null;
            }
            //Last ditch effort to find *something*! Shooting for the top result.
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
            if (inStr == null)
                return false;
            for (byte i = 0; i < 7; i++)
                if (!((inStr[i] >= '0' && inStr[i] <= '9') || (inStr[i] >= 'a' && inStr[i] <= 'z') || (inStr[i] >= 'A' && inStr[i] <= 'Z')))
                    return false;
            //Internet validate
            byte[] raw = _webClient.DownloadData(linkHeader + inStr + linkFooter);
            if (raw.Length < 1000)   //Imgur fallback page is very small compared to normal pages
                return false;
            else
                return true;
        }
    }
}