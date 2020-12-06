using System;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Minesweeper
    {
        public ushort defaultBombs = 16;
        private const byte defaultGridsize = 8;
        private static readonly Random _rand = new Random();

        //Program creates a minesweeper for discord, given by input parameters.
        //Element defs
        private static readonly string[] bombCounts = { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:" };

        private static readonly string bombString = ":bomb:";
        private static readonly string[] spoilerTag = { "||", "||" };
        private static long lastMinesweeper = 0;
        //Tags for spoilers (ie [s]x[/s] should be entered as {"[s]","[/s]"})

        //Element space arrays
        private readonly bool[,] bombSpace = new bool[8, 8];

        private readonly byte[,] numSpace = new byte[8, 8];

        public string GetMinesweeper()
        {
            return GetMinesweeper(defaultBombs, defaultGridsize);
        }

        public string GetMinesweeper(ushort bombs, byte gridWidth, byte gridHeight)
        {
            if (System.DateTime.Now.Ticks < lastMinesweeper + 10000000) //1 sec is 10,000,000 ticks
                return "Minesweepers generated too frequently!";

            lastMinesweeper = System.DateTime.Now.Ticks;

            gridHeight = Math.Min(gridHeight, (byte)8);
            gridWidth = Math.Min(gridWidth, (byte)8);
            bombs = Math.Min(bombs, (ushort)(gridWidth * gridHeight));

            PopulateBombs(bombs, gridWidth, gridHeight);
            PopulateNums(gridWidth, gridHeight);
            return $"```MINESWEEPER: Size-{Math.Max(gridWidth, gridHeight)} Bombs-{bombs}```" +
                GetMineMap(gridWidth, gridHeight);
        }

        public string GetMinesweeper(ushort bombs, byte size)
        {
            return GetMinesweeper(bombs, size, size);
        }

        private string GetMineMap(byte gridWidth, byte gridHeight) //Prints and spoilers game and returns as string
        {
            string mineMap = "";
            for (byte y = 0; y < gridHeight; y++)
            {
                for (byte x = 0; x < gridWidth; x++)
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

        private byte GetNearbyBombs(byte x, byte y, byte gridWidth, byte gridHeight)  //Checks target cell for bombs nearby. Does not read target cell.
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
            byte bombs = 0;
            for (sbyte yOffset = -1; yOffset < 2; yOffset++)
            {
                for (sbyte xOffset = -1; xOffset < 2; xOffset++)
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

        private Task PopulateBombs(ushort bombs, byte gridWidth, byte gridHeight)  //Uses numBombs and plots the number of bombs in random positions in bombSpace.
        {
            //Very important to fill bombspace with 0, as only 1s are plotted
            for (byte y = 0; y < gridHeight; y++)
            {
                for (byte x = 0; x < gridWidth; x++)
                {
                    bombSpace[x, y] = false;
                }
            }
            for (ushort i = bombs; i > 0; i--)
            {
                byte xRand, yRand;
                do
                { //Program can get stuck here if it is placing too many bombs that cannot fit in the grid
                    xRand = (byte)(_rand.Next(0, gridWidth));
                    yRand = (byte)(_rand.Next(0, gridHeight));
                } while (bombSpace[xRand, yRand]);
                bombSpace[xRand, yRand] = true;
            }
            return Task.CompletedTask;
        }

        private Task PopulateNums(byte gridWidth, byte gridHeight)  //Calculates nearby bombs and saves the nums to numSpace for easy printing. Bombspace must be populated before this is called.
                                                                    //This is the heaviest task, so it's best to keep it seperate.
        {
            //Effectively calls getNearbyBombs for every demanded space in numSpace
            for (byte y = 0; y < gridHeight; y++)
            {
                for (byte x = 0; x < gridWidth; x++)
                {
                    numSpace[x, y] = GetNearbyBombs(x, y, gridWidth, gridHeight);
                }
            }
            return Task.CompletedTask;
        }
    }
}