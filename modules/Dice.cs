using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Dice
    {
        private static readonly Random rand = new Random();

        public async Task<Task> Roll(ISocketMessageChannel channel, int quantity, int sides)
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
            if (sides > 255 || quantity > 255)
            {
                await channel.SendMessageAsync("Max sides and dice count is 255!");
                return Task.CompletedTask;
            }
            await channel.SendMessageAsync(StringBuilder(DiceMaster((byte)quantity, (byte)sides), (byte)quantity));
            return Task.CompletedTask;
        }

        public async Task<Task> Roll(ISocketMessageChannel channel, string args)
        {
            if (!args.Contains('d'))
            {
                await channel.SendMessageAsync("Error parsing arguments, proper format for dice rolling is <count>d<sides> (ie 1d20)");
                return Task.CompletedTask;
            }
            string[] dicecmd = args.Split('d');
            await Roll(channel, int.Parse(dicecmd[0]), int.Parse(dicecmd[1]));
            return Task.CompletedTask;
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

        private string StringBuilder(byte[] results, byte quantity)
        {
            string outstring = $"{results[0]}";
            if (quantity == 1)
            {
                return outstring;
            }
            ushort sum = results[0];
            for (byte i = 1; i < quantity; i++)
            {
                outstring += $" + {results[i]}";
                sum += results[i];
            }
            return outstring + " = " + sum;
        }
    }
}