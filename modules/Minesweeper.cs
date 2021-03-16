using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Minesweeper
    {
        private const ushort DEFAULT_BOMBS = 16;
        private const byte DEFAULT_GRIDSIZE = 8;

        //Program creates a minesweeper for discord, given by input parameters.
        private static readonly string[] bombCounts = { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:" };

        private static readonly string bombString = ":bomb:";
        private static readonly string[] spoilerTag = { "||", "||" };

        public async static Task MinesweeperHandlerAsync(ISocketMessageChannel ch, byte gridHeight = DEFAULT_GRIDSIZE, byte gridWidth = DEFAULT_GRIDSIZE, ushort bombs = DEFAULT_BOMBS)
        {
            bool[,] bombSpace = new bool[8, 8];
            byte[,] numSpace = new byte[8, 8];

            gridHeight = Math.Min(gridHeight, (byte)8);
            gridWidth = Math.Min(gridWidth, (byte)8);
            bombs = Math.Min(bombs, (ushort)(gridWidth * gridHeight));

            await PopulateBombs(bombs, gridWidth, gridHeight, ref bombSpace);
            await PopulateNums(gridWidth, gridHeight, ref bombSpace, ref numSpace);
            await ch.SendMessageAsync($"```MINESWEEPER: Size-{Math.Max(gridWidth, gridHeight)} Bombs-{bombs}```" +
                GetMineMap(gridWidth, gridHeight, ref bombSpace, ref numSpace));
        }

        private static string GetMineMap(byte gridWidth, byte gridHeight, ref bool[,] bombSpace, ref byte[,] numSpace) //Prints and spoilers game and returns as string
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

        private static byte GetNearbyBombs(byte x, byte y, byte gridWidth, byte gridHeight, ref bool[,] bombSpace)  //Checks target cell for bombs nearby. Does not read target cell.
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

        private static Task PopulateBombs(ushort bombs, byte gridWidth, byte gridHeight, ref bool[,] bombSpace)  //Uses numBombs and plots the number of bombs in random positions in bombSpace.
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
                    xRand = (byte)(Program.rand.Next(0, gridWidth));
                    yRand = (byte)(Program.rand.Next(0, gridHeight));
                } while (bombSpace[xRand, yRand]);
                bombSpace[xRand, yRand] = true;
            }
            return Task.CompletedTask;
        }

        private static Task PopulateNums(byte gridWidth, byte gridHeight, ref bool[,] bombSpace, ref byte[,] numSpace)  //Calculates nearby bombs and saves the nums to numSpace for easy printing. Bombspace must be populated before this is called.
        {//This is the heaviest task, so it's best to keep it seperate.
            //Effectively calls getNearbyBombs for every demanded space in numSpace
            for (byte y = 0; y < gridHeight; y++)
            {
                for (byte x = 0; x < gridWidth; x++)
                {
                    numSpace[x, y] = GetNearbyBombs(x, y, gridWidth, gridHeight, ref bombSpace);
                }
            }
            return Task.CompletedTask;
        }
    }
}
