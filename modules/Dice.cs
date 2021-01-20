using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Dice
    {
        public static async Task Roll(ISocketMessageChannel channel, int quantity, int sides, int offset)
        {
            if (quantity <= 0)
            {
                await channel.SendMessageAsync("I can't roll 0 dice!");
                return;
            }
            else if (sides <= 1)
            {
                await channel.SendMessageAsync("I'm not going to roll a die with that few sides.");
                return;
            }
            else if (sides > 128 || quantity > 32)
            {
                await channel.SendMessageAsync("Max dice count is 32, max side count is 128!");
                return;
            }
            else if (offset > 127 || offset < -128)
            {
                await channel.SendMessageAsync($"Offset can only range from -128 to 127!");
                return;
            }
            else
            {
                byte[] rolls = DiceMaster((byte)quantity, (byte)sides);

                await channel.SendMessageAsync(StringBuilder(rolls, (sbyte)offset));
            }
        }

        public static async Task Roll(ISocketMessageChannel channel, string args)
        {
            try
            {
                if (args.IndexOf('d') == 0)  //Handles the usage of just d20 instead of 1d20
                {
                    args = "1" + args;
                }
                string[] Quantity_Sides = args.Split('d');
                int quantity = int.Parse(Quantity_Sides[0]), sides, offset;
                if (args.Contains('+') || args.Contains('-'))
                {
                    int splitIndex = Quantity_Sides[1].LastIndexOfAny("+-".ToCharArray());
                    sides = int.Parse(Quantity_Sides[1].Substring(0, splitIndex));
                    offset = int.Parse(Quantity_Sides[1].Substring(splitIndex));
                }
                else
                {
                    offset = 0;
                    sides = int.Parse(Quantity_Sides[1]);
                }
                await Roll(channel, quantity, sides, offset);
            }
            catch (System.FormatException)
            {
                await channel.SendMessageAsync("Error parsing arguments, proper format for dice rolling is <count>d<sides> (ie 1d20), or append with a +# for an offset.");
                return;
            }
            catch (System.OverflowException)
            {
                await channel.SendMessageAsync("A number provided was too large!");
                return;
            }
        }

        private static byte[] DiceMaster(byte quantity, byte sides)
        {
            byte[] outresults = new byte[quantity];
            for (byte i = 0; i < quantity; i++)
                outresults[i] = (byte)Program.rand.Next(1, sides + 1);
            return outresults;
        }

        private static string StringBuilder(byte[] results, sbyte offset)
        {
            string outstring = $"{results[0]}";
            byte count = (byte)results.Length;
            if (count == 1 && offset == 0)
                return outstring;

            ushort sum = results[0];
            for (byte i = 1; i < count; i++)
            {
                outstring += $" + {results[i]}";
                sum += results[i];
            }
            outstring += $" = {sum}";
            if (offset != 0)
            {
                if (offset > 0)
                    outstring += "+";
                else
                    outstring += "-";
                outstring += $"{Math.Abs(offset)} = {sum + offset}";
            }
            return outstring;
        }
    }
}