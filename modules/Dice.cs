using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Dice
    {
        private static readonly Random rand = new Random();

        public async Task<Task> Roll(ISocketMessageChannel channel, int quantity, int sides, int offset)
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
                await channel.SendMessageAsync("Max side count is 128, max dice count is 32!");
                return Task.CompletedTask;
            }
            if (offset > 127 || offset < -128)
            {
                await channel.SendMessageAsync($"Offset can only range from -128 to 127!");
                return Task.CompletedTask;
            }
            byte[] rolls;
            if (offset == 0)
            {
                rolls = DiceMaster((byte)quantity, (byte)sides);
            }
            else
            {
                rolls = DiceMaster((byte)quantity, (byte)sides, (sbyte)offset);
            }
            await channel.SendMessageAsync(StringBuilder(rolls));
            return Task.CompletedTask;
        }

        public async Task<Task> Roll(ISocketMessageChannel channel, int quantity, int sides)
        {
            await Roll(channel, quantity, sides, 0);
            return Task.CompletedTask;
        }

        public async Task<Task> Roll(ISocketMessageChannel channel, string args)
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
                    await Roll(channel, int.Parse(Quantity_Sides[0]), int.Parse(Quantity_Sides[1]));
                }
                return Task.CompletedTask;
            }
            catch (System.FormatException)
            {
                await channel.SendMessageAsync("Error parsing arguments, proper format for dice rolling is <count>d<sides> (ie 1d20), or append with a +# for an offset.");
                return Task.CompletedTask;
            }
        }

        private byte[] DiceMaster(byte quantity, byte sides)
        {
            byte[] outresults = new byte[quantity];
            for (byte i = 0; i < quantity; i++)
            {
                outresults[i] = (byte)rand.Next(1, sides + 1);
            }
            return outresults;
        }

        private byte[] DiceMaster(byte quantity, byte sides, sbyte offset)
        {
            byte[] outresults = DiceMaster(quantity, sides);
            for (byte i = 0; i < quantity; i++)
            {
                short offsetRoll = (short)(outresults[i] + offset);
                offsetRoll = (short)Math.Max((int)offsetRoll, 0);
                outresults[i] = (byte)offsetRoll;
            }
            return outresults;
        }

        private string StringBuilder(byte[] results)
        {
            string outstring = $"{results[0]}";
            byte count = (byte)results.Length;
            if (count == 1)
            {
                return outstring;
            }
            ushort sum = results[0];
            for (byte i = 1; i < count; i++)
            {
                outstring += $" + {results[i]}";
                sum += results[i];
            }
            return outstring + " = " + sum;
        }
    }
}