using Discord;

namespace MothBot.modules
{
    internal static class Imagesearch
    {
        private const string linkFooter = ".jpg";
        private const string linkHeader = "https://i.imgur.com/";
        private const string linkSearch = "https://imgur.com/search?q=";
        private static readonly HttpClient _webClient = new HttpClient();

        public static string ImageSearch(string searchTerm)   //Finds a random imgur photo that matches search term, returns null if no valid photos can be found.
        {
            searchTerm = (linkSearch + searchTerm).Replace(' ', '+');
            string webData = _webClient.GetStringAsync(searchTerm).Result, link = "";
            int linkPtr = -1;
            int maxretries = 1024;
            for (int retries = maxretries; retries > 0; retries--)
            {
                bool EOF = false;
                string webDatatemp = webData;
                int randNum = Master.rand.Next(1, retries);
                for (int i = 0; i < randNum; i++)   //Get random image link. (Links can start breaking if method cant find enough images!)
                {
                    linkPtr = webDatatemp.IndexOf(@"<img alt="""" src=""//i.imgur.com/");
                    if (linkPtr == -1 || linkPtr + 7 >= webDatatemp.Length)
                    {    
                        EOF = true;
                        break;
                    }
                    link = webDatatemp.Substring(linkPtr + 31, 7);
                    webDatatemp = webDatatemp.Substring(linkPtr + 32);
                }
                if (!EOF && CheckValid(link))
                {
                    Console.WriteLine($"Image found, took {maxretries - retries} tries.");
                    return linkHeader + link + linkFooter;
                }
                else
                    webDatatemp = webDatatemp.Substring(linkPtr + 32);
            }
             Console.WriteLine("No valid results found, returning null.");
                return null;
        }

        public static async Task ImageSearchHandlerAsync(IMessageChannel channel, string searchquery)
        {
            string photoLink = ImageSearch(searchquery);
            if (photoLink == null)      //sry couldn't find ur photo :c
                await channel.SendMessageAsync($"Could not find photo of {searchquery}... :bug:");
            else
                await channel.SendMessageAsync(photoLink);
        }

        private static bool CheckValid(string inStr)
        {
            if (inStr.Length < 7 || Chatterbot.ContentsBlacklisted(inStr))
                return false;
            for (int i = 0; i < 7; i++)
                if (!((inStr[i] >= '0' && inStr[i] <= '9') || (inStr[i] >= 'a' && inStr[i] <= 'z') || (inStr[i] >= 'A' && inStr[i] <= 'Z')))
                    return false;
            //Internet validate
            string webData = _webClient.GetStringAsync(linkHeader + inStr + linkFooter).Result;
            if (webData.Length < 1000)   //Imgur fallback page is very small compared to normal pages
                return false;
            return true;
        }
    }
}