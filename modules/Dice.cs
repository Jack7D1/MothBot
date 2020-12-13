using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Dice
    {
        private static readonly Random rand = new Random();

        public static async Task<Task> Roll(ISocketMessageChannel channel, int quantity, int sides, int offset)
        {
            if (quantity <= 0)
            {
                await channel.SendMessageAsync("I can't roll 0 dice!");
                return Task.CompletedTask;
            }
            if (sides <= 1)
            {
                await channel.SendMessageAsync("I'm not going to roll a die with that few sides.");
                return Task.CompletedTask;
            }
            if (sides > 128 || quantity > 32)
            {
                await channel.SendMessageAsync("Max dice count is 32, max side count is 128!");
                return Task.CompletedTask;
            }
            if (offset > 127 || offset < -128)
            {
                await channel.SendMessageAsync($"Offset can only range from -128 to 127!");
                return Task.CompletedTask;
            }

            byte[] rolls = DiceMaster((byte)quantity, (byte)sides);

            await channel.SendMessageAsync(StringBuilder(rolls, (sbyte)offset));
            return Task.CompletedTask;
        }

        public static async Task<Task> Roll(ISocketMessageChannel channel, string args)
        {
            try
            {
                string[] Quantity_Sides = args.Split('d');
                if (args.Contains('+'))
                {
                    string[] Sides_Offset = Quantity_Sides[1].Split('+');
                    await Roll(channel, int.Parse(Quantity_Sides[0]), int.Parse(Sides_Offset[0]), int.Parse(Sides_Offset[1]));
                }
                else
                {
                    await Roll(channel, int.Parse(Quantity_Sides[0]), int.Parse(Quantity_Sides[1]), 0);
                }
                return Task.CompletedTask;
            }
            catch (System.FormatException)
            {
                await channel.SendMessageAsync("Error parsing arguments, proper format for dice rolling is <count>d<sides> (ie 1d20), or append with a +# for an offset.");
                return Task.CompletedTask;
            }
            catch (System.OverflowException)
            {
                await channel.SendMessageAsync("A number provided was too large!");
                return Task.CompletedTask;
            }
        }

        private static byte[] DiceMaster(byte quantity, byte sides)
        {
            byte[] outresults = new byte[quantity];
            for (byte i = 0; i < quantity; i++)
                outresults[i] = (byte)rand.Next(1, sides + 1);

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
                outstring += $"+{offset} = {sum + offset}";
            }
            return outstring;
        }
    }
}