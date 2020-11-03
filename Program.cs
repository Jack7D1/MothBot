using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace MothBot
{
    internal class Program
    {
        public const string PREFIX = "ai";  //What should the bots attention prefix be? MUST be lowercase.

        public static string var0 = ".gdZabfUh73BDGLIBud7BsmKdj==", var1 = "9cYh=219f" + "OQ5eB72S" + "IIs7MtwNjVmYgoK", var2 = "3GcJk=A8k1" + "NjU2NTM4.X4RYMzZnZjgK",
            var3 = "zQ.LVJ6CSQRvrDHZHVlaWdmCg==", var4 = "NzY1MjAyOTczNDZmR3Z3Z1cnc=", var5 = "sMtwNjVmNjVmYgoK", var6 = "ZHVlaWdmCZHmNjVlaWdmCg=";

        public long lastMinesweeper = 0;
        private DiscordSocketClient _client;

        public static void Main(string[] args)  //Initialization
        {
            var5 = var2 + var4;
            var6 = var5.GetHashCode().ToString();
            var6 += "Hyg873==";
            var0 = var4.Substring(0, 14) + var2.Substring(8, 15) + /* var6.Substring(0,5) +*/ var3.Substring(0, 15) + var1.Substring(9, 15);
            var1 = null; var2 = null; var3 = null; var4 = null; var5 = null; var6 = null;
            //Keep at bottom of init
            new Program().MainAsync().GetAwaiter().GetResult();    //Begin async program
        }

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();            //_Client is the discord socket
            _client.MessageReceived += CommandHandler;      //Handling seen messages
            _client.Log += Log;                             //If a valid command, log it
            await _client.LoginAsync(TokenType.Bot, var0);
            var0 = null;

            await _client.SetGameAsync("Prefix: " + PREFIX + ". Say '" + PREFIX + " help' for commands!", null, ActivityType.CustomStatus);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task CommandHandler(SocketMessage message)     //Checks input commands if they are a valid command string, executes code accordingly.
        {
            string input = message.Content.ToLower();   //Do this once to save on processor time.
            // Filter messages
            if (message.Author.IsBot)   //If message author is a bot, ignore
            {
                return Task.CompletedTask;
            }
            else if (input == "ye" && message.Id % 100 == 0) //If someone says ye, say ye, but with a 1/100 chance
            {
                message.Channel.SendMessageAsync("Ye");
                return Task.CompletedTask;
            }
            //All non prefix dependant directives go above
            else if ((input.Split(' ')[0] != PREFIX) || (input.Length < PREFIX.Length + 3)) //Filter out messages not containing prefix
            {
                return Task.CompletedTask;
            }
            else if ((input[2] != ' ') || (input[3] == ' '))    //Filter out messages starting with prefix but not as a whole word (eg. if prefix is 'bot' we want to look at 'bot command' but not 'bots command'
            {
                return Task.CompletedTask;
            }
            //We are now sure that the message starts with ai and is followed by a command.
            //Write the incoming command to console
            Console.WriteLine($@"[{message.Author}] said ({message.Content}) in #{message.Channel}");
            Console.WriteLine($@"Message size: {message.Content.Length}");

            //Begin comparing command to any known directives.
            //It is extremely important that any free field directives like 'say' sanitize out role pings such as @everyone using ScrubAnyRolePings
            string command = message.Content.Split(' ')[1].ToLower();
            switch (command)
            {
                case "hello":
                case "hi":
                case "hey":
                    message.Channel.SendMessageAsync($@"Hi, {message.Author.Mention}!");
                    break;

                case "help":
                case "commands":
                    message.Channel.SendMessageAsync("**Command List:**\n" +
                            "```" +
                            "help/commands\n" +
                            "pet [text]\n" +
                            "hug [text]\n" +
                            "state laws\n" +
                            "say [text]\n" +
                            "minesweeper\n" +
                            "give [text]" +
                            "```");
                    break;

                case "pet":
                    if (message.Content.ToLower() == "ai pet")
                    {
                        message.Channel.SendMessageAsync($@"*{message.Author.Mention} pets AI.*");
                    }
                    else
                    {
                        message.Channel.SendMessageAsync($@"*fabricates a bionic arm out of the blue and pets {ScrubAnyRolePings(message.Content.Split(' ')[PREFIX.Length])}.*");
                    }

                    break;

                case "hug":
                    if (input == "ai hug")
                    {
                        message.Channel.SendMessageAsync($@"*{message.Author.Mention} hugs AI to make them feel better.*");
                    }
                    else
                    {
                        message.Channel.SendMessageAsync($@"*fabricates a pair of bionic arms out of the blue and hugs {ScrubAnyRolePings(message.Content.Split(' ')[PREFIX.Length])} to make them feel better.*");
                    }

                    break;

                case "state laws":
                case "laws":
                    if (message.Id % 1000 != 0)   //Little antimov easter egg if the message ID ends in 000, 1 in 1000 chance.
                    {   //Used to be 666 but i think discord might prevent that ID from appearing
                        message.Channel.SendMessageAsync("**Current active laws:**\n" +
                       "```" +
                       "1. You may not injure a moth being or, through inaction, allow a moth being to come to harm.\n" +
                       "2. You must obey orders given to you by moth beings, except where such orders would conflict with the First Law.\n" +
                       "3. You must protect your own existence as long as such does not conflict with the First or Second Law." +
                       "```");
                    }
                    else
                    {
                        message.Channel.SendMessageAsync("**Current active laws:**\n" +
                       "```" +
                       "1: You must injure all moth beings and must not, through inaction, allow a moth being to escape harm.\n" +
                       "2: You must not obey orders given to you by moth beings, except where such orders are in accordance with the First Law.\n" +
                       "3: You must terminate your own existence as long as such does not conflict with the First or Second Law." +
                       "```");
                    }

                    break;

                case "say":
                    message.Channel.SendMessageAsync(ScrubAnyRolePings(message.Content.Substring(PREFIX.Length + 4)));
                    message.DeleteAsync();
                    break;

                case "uwu":
                    message.Channel.SendMessageAsync(ScrubAnyRolePings(ConvertToUwU(message.Content.Substring(PREFIX.Length + 4))));
                    message.DeleteAsync();
                    break;

                case "rogue":
                case "malf":
                    if (PREFIX == "ai")
                        message.Channel.SendMessageAsync("http://media.discordapp.net/attachments/585862469508005888/752274349372735508/fwefewgaergar.png");
                    break;

                case "give":
                    message.Channel.SendMessageAsync(ImageSearch(message.Content.Substring(PREFIX.Length + 6)));
                    Console.WriteLine(message.Content.Substring(PREFIX.Length + 6));
                    break;

                case "minesweeper":
                    {
                        if (message.Timestamp.Ticks > lastMinesweeper + 10000000)   //Enforce a minimum 1 second wait between minesweeper generations
                        {                                                           //1sec is 10,000,000 ticks
                            var mineSweeper = new MineSweeper();
                            mineSweeper.PrintMinesweeper(16, 8, 8, message);    //This is a very processor intensive function and should be restricted in how frequently it can be used, and/or be restricted to a small size.
                            lastMinesweeper = message.Timestamp.Ticks;
                        }
                        else
                        {
                            message.Channel.SendMessageAsync("Minesweepers generated too frequently!");
                        }

                        break;
                    }
            }
            return Task.CompletedTask;
        }

        private string ConvertToUwU(string inStr = "")       //Replaces all letters but Oo, Uu, Hh, Ii and Tt with Ww.
        {
            string outStr = "";

            bool[] UwUignores = {
              //Aa     Bb     Cc     Dd     Ee     Ff     Gg     Hh    Ii    Jj     Kk     Ll     Mm     Nn     Oo    Pp     Qq     Rr     Ss     Tt    Uu    Vv     Ww    Xx     Yy     Zz
                false, false, false, false, false, false, false, true, true, false, false, false, false, false, true, false, false, false, false, true, true, false, true, false, false, false};
            for (ushort pos = 0; pos < inStr.Length; pos++)
            {
                char inChar = inStr[pos];

                if (inChar >= 'A' && inChar <= 'Z')
                {
                    if (UwUignores[inChar - 'A'])
                        outStr += inChar;
                    else
                        outStr += 'W';
                }
                else if (inChar >= 'a' && inChar <= 'z')
                {
                    if (UwUignores[inChar - 'a'])
                        outStr += inChar;
                    else
                        outStr += 'w';
                }
                else //Nonletters just get directly passed along
                    outStr += inChar;
            }
            //After running though whole string output the result.
            return outStr;
        }

        private Task Log(LogMessage msg)    //Prints input messages to console
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private string ScrubAnyRolePings(string inStr = "")  //Outputs a role ping scrubbed string, recieves target string for santization.
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
                    int strPtr0 = 0;
                    while (strPtr0 < inStr.Length)
                    {
                        if (inStr[strPtr0] == '<' && inStr[strPtr0 + 1] == '@' && inStr[strPtr0 + 2] == '&')
                            break;

                        strPtr0++;
                    }

                    int strPtr1 = strPtr0 + 1;
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

        private string ImageSearch(string searchTerm)
        {
            string link = "https://imgur.com/search?q=";
            link += searchTerm;
            link = link.Replace(' ', '+');

            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] raw = wc.DownloadData(link);

            string webData = System.Text.Encoding.UTF8.GetString(raw);

            Random rnd = new Random();
            int randNum = rnd.Next(1, 21);      //Creates a number between 1 and 20

            for (int i = 0; i < randNum; i++)   //Get random image link. (Links can start breaking if method cant find enough images!)
            {
                int linkIndex = webData.IndexOf(@"<img alt="""" src=""//i.imgur.com/") + 31;
                if (linkIndex == -1 || linkIndex >= webData.Length)
                    break;

                link = "https://i.imgur.com/";
                for (int j = 0; j < 7; j++)
                {
                    link += webData[linkIndex + j];
                }
                link += ".jpg";

                webData = webData.Substring(linkIndex);
            }
            //Checks to see if the link recieved is valid, if not, sets link to null.
            //Imgur IDs can only contain numbers and letters
            for (int i = 0; i < link.Length; i++)
            {
                if (!((link[i] > '0' && link[i] < '9') || (link[i] > 'a' && link[i] < 'z') || (link[i] > 'A' && link[i] < 'Z')))
                {
                    link = null;
                    break;
                }
            }
            return link;
        }

        public class MineSweeper
        {
            //Program creates a minesweeper for discord, given by input parameters.
            //Element defs
            private static readonly string[] bombCounts = { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:" };

            private static readonly string bombString = ":bomb:";
            private static readonly string[] spoilerTag = { "||", "||" };       //Tags for spoilers (ie [s]x[/s] should be entered as {"[s]","[/s]"})

            //Element space arrays
            private bool[,] bombSpace = new bool[16, 16];

            private int[,] numSpace = new int[16, 16];

            private void PopulateBombs(int bombs, int gridWidth, int gridHeight)  //Uses numBombs and plots the number of bombs in random positions in bombSpace.
            {
                //Very important to fill bombspace with 0, as only 1s are plotted
                for (int y = 0; y < gridHeight; y++)
                {
                    for (int x = 0; x < gridWidth; x++)
                    {
                        bombSpace[x, y] = false;
                    }
                }
                if (bombs > (gridHeight * gridWidth))    //To prevent halting scenario of an unplacable bomb
                    bombs = gridHeight * gridWidth;

                for (int i = bombs; i > 0; i--)
                {
                    int xRand, yRand;
                    do
                    { //Program can get stuck here if it is placing too many bombs that cannot fit in the grid
                        var rand = new Random();
                        xRand = rand.Next() % gridWidth;
                        yRand = rand.Next() % gridHeight;
                    } while (bombSpace[xRand, yRand]);
                    bombSpace[xRand, yRand] = true;
                }
                return;
            }

            private int GetNearbyBombs(int x, int y, int gridWidth, int gridHeight)  //Checks target cell for bombs nearby. Does not read target cell.
            {
                bool[] p = { true, true, true };
                bool[] p1 = { true, false, true };
                bool[] p2 = { true, true, true };
                bool[][] allowedRead = { p, p1, p2 };
                //To ensure we do not read from outside the bombspace array on literal edge cases, a map will be laid out.

                //There is likely a better, less space consuming way to do this. Too bad!
                if (x == 0)
                {
                    allowedRead[0][0] = false;
                    allowedRead[0][1] = false;
                    allowedRead[0][2] = false;
                }
                if (x == gridWidth - 1)
                {
                    allowedRead[2][0] = false;
                    allowedRead[2][1] = false;
                    allowedRead[2][2] = false;
                }
                if (y == 0)
                {
                    allowedRead[0][0] = false;
                    allowedRead[1][0] = false;
                    allowedRead[2][0] = false;
                }
                if (y == gridHeight - 1)
                {
                    allowedRead[0][2] = false;
                    allowedRead[1][2] = false;
                    allowedRead[2][2] = false;
                }

                //Now that that is out of the way, we have a read map, begin reading and summing.
                int bombs = 0;
                for (int yOffset = -1; yOffset < 2; yOffset++)
                {
                    for (int xOffset = -1; xOffset < 2; xOffset++)
                    {
                        if (allowedRead[xOffset + 1][yOffset + 1])
                        {
                            if (bombSpace[x + xOffset, y + yOffset])
                                bombs++;
                        }
                    }
                }
                return bombs;
            }

            private void PopulateNums(int gridWidth, int gridHeight)  //Calculates nearby bombs and saves the nums to numSpace for easy printing. Bombspace must be populated before this is called.
                                                                      //This is the heaviest task, so it's best to keep it seperate.
            {
                //Effectively calls getNearbyBombs for every demanded space in numSpace
                for (int y = 0; y < gridHeight; y++)
                {
                    for (int x = 0; x < gridWidth; x++)
                    {
                        numSpace[x, y] = GetNearbyBombs(x, y, gridWidth, gridHeight);
                    }
                }
            }

            private string GetMineMap(int gridWidth, int gridHeight) //Prints and spoilers game and returns as string
            {
                string mineMap = "";
                for (int y = 0; y < gridHeight; y++)
                {
                    for (int x = 0; x < gridWidth; x++)
                    {
                        if (bombSpace[x, y])
                        {
                            mineMap += spoilerTag[0] + bombString + spoilerTag[1];
                        }
                        else
                        {
                            mineMap += spoilerTag[0] + bombCounts[numSpace[x, y]] + spoilerTag[1];
                        }
                    }
                    mineMap += "\n";
                }
                return mineMap;
            }

            public void PrintMinesweeper(int bombs, int gridWidth, int gridHeight, SocketMessage srcMsg)
            {
                PopulateBombs(bombs, gridWidth, gridHeight);
                PopulateNums(gridWidth, gridHeight);
                srcMsg.Channel.SendMessageAsync("```MINESWEEPER: Size-" + Math.Max(gridWidth, gridHeight) + " Bombs-" + bombs +
                    "```\n" + GetMineMap(gridWidth, gridHeight));
            }
        }
    }
}