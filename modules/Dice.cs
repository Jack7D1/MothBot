using Discord;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Dice
    {
        public static async Task Roll(IMessageChannel channel, int quantity, int sides, int offset)
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

        public static async Task Roll(IMessageChannel channel, int quantity, int sides)
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
            else
            {
                byte[] rolls = DiceMaster((byte)quantity, (byte)sides);

                await channel.SendMessageAsync(StringBuilder(rolls, 0));
            }
        }

        public static async Task Roll(IMessageChannel channel, string args)
        {
            try
            {
                string[] Quantity_Sides = args.Split('d');
                if (Quantity_Sides[0].Length == 0)  //Allows for shorthand rolling with just d20 instead of 1d20
                    Quantity_Sides[0] = "1";
                if (args.Contains('+'))
                {
                    string[] Sides_Offset = Quantity_Sides[1].Split('+');
                    await Roll(channel, int.Parse(Quantity_Sides[0]), int.Parse(Sides_Offset[0]), int.Parse(Sides_Offset[1]));
                }
                else
                {
                    await Roll(channel, int.Parse(Quantity_Sides[0]), int.Parse(Quantity_Sides[1]), 0);
                }
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
                outstring += $"+{offset} = {sum + offset}";
            return outstring;
        }
    }
}