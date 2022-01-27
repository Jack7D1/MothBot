using Discord;
using System.Text.RegularExpressions;

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
            string webData = _webClient.GetStringAsync(searchTerm).Result, link;
            MatchCollection results = Regex.Matches(webData, "<img alt=\"\" src=\"\\/\\/i\\.imgur\\.com\\/(\\w{7})");
            if (results.Count == 0) { return null; }
            int retries = 100;
            do
            {
                link = results[Master.rand.Next(0, results.Count)].Groups[1].Value;
                retries--;
            } while (!CheckValid(link) && retries > 0);
            if (retries > 0) { return linkHeader + link + linkFooter; }
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