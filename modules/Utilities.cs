using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Utilities
    {
        private static readonly List<ulong> operatorIDs = new List<ulong>{   //Discord user IDs of allowed operators, in ulong format.
            206920373952970753, //Jack
            144308736083755009, //Hirohito
            238735900597682177, //Dex
            489965198531362838, //Argema
        };

        private static bool shutdownEnabled = false;
        private static long shutdownTimeout = 0;

        public static bool IsOperator(SocketUser user)
        {
            foreach (ulong op in operatorIDs)
                if (user.Id == op)
                    return true;
            return false;
        }

        public static async Task UtilitiesHandlerAsync(SocketMessage src)
        {
            if (!IsOperator(src.Author))   //You do not have permission
            {
                await src.Channel.SendMessageAsync("You do not have access to Utilities.");
                return;
            }

            string command, keyword, args;
            command = src.Content.ToLower().Substring(Program._prefix.Length + " utility".Length); //We now have all text that follows the prefix and the word utility.
            if (command.IndexOf(' ') == 0)
                command = command.Substring(1);
            if (command.Contains(' '))
            {
                keyword = command.Substring(0, command.IndexOf(' '));
                args = command.Substring(command.IndexOf(' ') + 1);
            }
            else
            {
                keyword = command;
                args = "";
            }
            if (keyword == "")
                keyword = "commands";
            switch (keyword)    //Ensure switch is ordered similarly to command list
            {
                //General
                case "commands":
                case "help":
                    string prefix = Program._prefix + " utility ";
                    await Lists.Utilities_PrintCommandList(src.Channel, prefix);
                    return;

                case "setprefix":
                    Program._prefix = args.ToLower();
                    await Logging.LogtoConsoleandFileAsync($"PREFIX CHANGED TO \"{Program._prefix}\"");
                    await src.Channel.SendMessageAsync($"Prefix changed to {Program._prefix}!");
                    await Lists.SetDefaultStatus();
                    return;

                case "blacklist":
                    await Chatterbot.BlacklistHandler(src, args);
                    return;

                //dangerous
                case "shutdown":
                    if (args == "confirm")
                    {
                        if (DateTime.Now.Ticks > shutdownTimeout)
                            shutdownEnabled = false;

                        if (shutdownEnabled)
                        {
                            await Logging.LogtoConsoleandFileAsync($"SHUTTING DOWN: ordered by {src.Author.Username}");
                            await src.Channel.SendMessageAsync($"{src.Author.Mention} Shutdown confirmed, terminating bot.");
                            Environment.Exit(0);
                        }
                        else
                        {
                            shutdownEnabled = true;
                            shutdownTimeout = DateTime.Now.AddSeconds(12).Ticks;
                            await src.Channel.SendMessageAsync($"Shutdown safety disabled, {src.Author.Mention} confirm shutdown again to shut down bot, or argument anything else to re-enable safety.");
                            await Logging.LogtoConsoleandFileAsync($"{src.Author.Username} disabled shutdown safety.");
                        }
                    }
                    else
                    {
                        shutdownEnabled = false;
                        await src.Channel.SendMessageAsync("Shutdown safety (re)enabled, argument 'confirm' to disable.");
                    }
                    return;

                default:
                    await src.Channel.SendMessageAsync("Function does not exist or error in syntax.");
                    return;
            }
        }
    }
}