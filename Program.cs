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
            if ((command == "hello") || (command == "hi") || (command == "hey"))
            {
                message.Channel.SendMessageAsync($@"Hi, {message.Author.Mention}!");
            }
            else if (command == "help" || command == "commands")    //haha funny meme anime sakuri something cum chalice man else if
            {
                message.Channel.SendMessageAsync("**Command List:**\n" +
                    "```" +
                    "help/commands\n" +
                    "pet [text]\n" +
                    "hug [text]\n" +
                    "state laws\n" +
                    "say [text]\n" +
                    "minesweeper" +
                    "```");
            }
            else if (command == "pet")
            {
                if (message.Content.ToLower() == "ai pet")
                {
                    message.Channel.SendMessageAsync($@"*{message.Author.Mention} pets AI.*");
                }
                else
                {
                    message.Channel.SendMessageAsync($@"*fabricates a bionic arm out of the blue and pets {ScrubAnyRolePings(message.Content.Split(' ')[PREFIX.Length])}.*");
                }
            }
            else if (command == "hug")
            {
                if (input == "ai hug")
                {
                    message.Channel.SendMessageAsync($@"*{message.Author.Mention} hugs AI to make them feel better.*");
                }
                else
                {
                    message.Channel.SendMessageAsync($@"*fabricates a pair of bionic arms out of the blue and hugs {ScrubAnyRolePings(message.Content.Split(' ')[PREFIX.Length])} to make them feel better.*");
                }
            }
            else if (input.Substring(PREFIX.Length + 1) == "state laws" || command == "laws")
            {
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
            }
            else if (command == "say")  //Parrots input
            {
                message.Channel.SendMessageAsync(ScrubAnyRolePings(message.Content.Substring(PREFIX.Length + 4)));
                message.DeleteAsync();
            }
            else if (command == "uwu")  //uwu'izes input
            {
                message.Channel.SendMessageAsync(ScrubAnyRolePings(ConvertToUwU(message.Content.Substring(PREFIX.Length + 4))));
                message.DeleteAsync();
            }
            else if (PREFIX == "ai" && (command == "rogue" || command == "malf"))   //u gay
            {
                message.Channel.SendMessageAsync("http://media.discordapp.net/attachments/585862469508005888/752274349372735508/fwefewgaergar.png");
            }
            else if (command == "minesweeper")
            {
                var mineSweeper = new MineSweeper();
                mineSweeper.PrintMinesweeper(24, 8, 8, message); //This is a very processor intensive function and should be restricted in how frequently it can be used, or be restricted to a small size.
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
            if (inStr.ToLower().Contains("@everyone") || inStr.ToLower().Contains("@here")) //Scrubbing is easy and predefined for these.
            {
                outStr = inStr.Replace('@', ' ');
            }
            //Scrubbing requires finding the authors's name for these
            else if (inStr.Contains("<@&")) //<@& is the prefix for role pings
            {
                while (true) //Find all occurances of '<@&', select up to the next '>' and simply remove it.
                {
                    int strPtr0 = 0;
                    //Set strPtr0 to the next occurance of '<@&' or when reached end of string.
                    while (strPtr0 < inStr.Length)
                    {
                        if (inStr[strPtr0] == '<' && inStr[strPtr0 + 1] == '@' && inStr[strPtr0 + 2] == '&')
                            break;

                        strPtr0++;
                    }

                    int strPtr1 = strPtr0 + 1;
                    //Set strPtr1 to next occurance of > after strPtr0
                    while (strPtr1 < inStr.Length)
                    {
                        if (inStr[strPtr1] == '>')
                            break;
                        strPtr1++;
                    }

                    //Remove this section between strPtr0 to strPtr1 inclusive
                    string strFirst = inStr.Substring(0, strPtr0);  //Valid string before remove target
                    string strSecond = inStr.Substring(strPtr1 + 1);                    //Valid string afterwards
                    outStr = strFirst + strSecond;  //Remove this from the output string and continue.
                    if (strPtr0 <= inStr.Length || strPtr1 <= inStr.Length) //Break loop if at end of string
                        break;
                }
            }
            return outStr;
        }

        public class MineSweeper
        {
            //Program creates a minesweeper for discord, given by input parameters.
            //Element defs
            private static readonly string[] bombCounts = { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:" };   //Discord emotes for numbers, will be used for nearby indicators

            private static readonly string bombString = ":bomb:";               //Discord emote for bomb
            private static readonly string[] spoilerTag = { "||", "||" };       //Tags for spoilers (ie [s]x[/s] should be entered as {"[s]","[/s]"})

            //Element space arrays
            private bool[,] bombSpace = new bool[16, 16];

            private int[,] numSpace = new int[16, 16];

            private void PopulateBombs(int numBombs, int gridWidth, int gridHeight)  //Uses NUM_BOMBS and plots the number of bombs in random positions in bombSpace.
            {
                //Begin by initializing bombSpace to 0
                for (int y = 0; y < gridHeight; y++)
                {
                    for (int x = 0; x < gridWidth; x++)
                    {
                        bombSpace[x, y] = false;
                    }
                }
                //Begin plotting bombs
                if (numBombs > (gridHeight * gridWidth))    //To prevent halting scenario of an unplacable bomb
                    numBombs = gridHeight * gridWidth;

                for (int i = numBombs; i > 0; i--)
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

            private string GetMineMap(int gridWidth, int gridHeight) //Prints and spoilers game to console accordingly.
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
                string mineMap = GetMineMap(gridWidth, gridHeight);
                srcMsg.Channel.SendMessageAsync(mineMap);
            }
        }
    }
}