using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Utilities
    {
        private static readonly ulong[] operatorIDs = {   //Discord user IDs of allowed operators, in ulong format.
            206920373952970753, //Jack
            144308736083755009, //Hirohito
            238735900597682177, //Dex
            489965198531362838, //Argema
        };

        private static bool shutdownEnabled = false;

        public async Task<Task> CommandHandler(SocketMessage src)
        {
            if (!IsOperator(src.Author))   //You do not have permission
            {
                await src.Channel.SendMessageAsync("You do not have access to Utilities.");
                return Task.CompletedTask;
            }

            string command, keyword, args;
            command = src.Content.ToLower().Substring(Program._prefix.Length + " utility ".Length); //We now have all text that follows the prefix and the word utility.
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
            string prefix = Program._prefix + " utility ";
            switch (keyword)    //Ensure switch is ordered similarly to command list
            {
                //General
                case "commands":
                case "help":
                    await Lists.Utilities_PrintCommandList(src.Channel, prefix);
                    return Task.CompletedTask;

                case "setprefix":
                    Program._prefix = args.ToLower();
                    await Program.logging.LogtoConsoleandFile($"PREFIX CHANGED TO \"{Program._prefix}\"");
                    await src.Channel.SendMessageAsync($"Prefix changed to {Program._prefix}!");
                    await Program.client.SetGameAsync("Prefix: " + args + ". Say '" + args + " help' for commands! Invite at https://tinyurl.com/MOFFBOT1111", null, ActivityType.Playing);
                    return Task.CompletedTask;

                //Debug info

                case "showguilds":
                    {
                        string guildstring = "";

                        byte i = 0;
                        foreach (SocketGuild guild in Program.client.Guilds)
                        {
                            guildstring += $"GUILD {i + 1}: {guild.Name} [{guild.Id}]. Members: {guild.MemberCount}\n";
                            i++;
                        }

                        await src.Channel.SendMessageAsync(guildstring);
                        return Task.CompletedTask;
                    }

                case "ping":
                    await src.Channel.SendMessageAsync($"Ping: {Program.client.Latency}ms");
                    return Task.CompletedTask;

                case "recentlogs":
                    {
                        string[] inlogs = Program.logging.GetLogs();
                        string outstr = "**Recent Log Entries**```";
                        for (byte i = 0; i < 255; i++)
                        {
                            outstr += $"{i + 1}: \"{inlogs[i]}\"\n";
                            if (i < 255)
                                if (inlogs[i + 1] == null)
                                    break;
                        }
                        await src.Channel.SendMessageAsync(outstr + "```");
                        return Task.CompletedTask;
                    }

                //dangerous
                case "shutdown":
                    if (args == "confirm")
                    {
                        if (shutdownEnabled)
                        {
                            await Program.logging.LogtoConsoleandFile($"SHUTTING DOWN: ordered by {src.Author.Username}");
                            await src.Channel.SendMessageAsync($"{src.Author.Mention} Shutdown confirmed, terminating bot.");
                            Environment.Exit(13);
                            return Task.CompletedTask;
                        }
                        else
                        {
                            shutdownEnabled = true;
                            await src.Channel.SendMessageAsync($"Shutdown safety disabled, {src.Author.Mention} confirm shutdown again to shut down bot, or argument anything else to re-enable safety.");
                            await Program.logging.LogtoConsoleandFile($"{src.Author.Username} disabled shutdown safety.");
                            return Task.CompletedTask;
                        }
                    }
                    else
                    {
                        shutdownEnabled = false;
                        await src.Channel.SendMessageAsync("Shutdown safety (re)enabled, argument 'confirm' to disable.");
                        return Task.CompletedTask;
                    }
                default:
                    await src.Channel.SendMessageAsync("Function does not exist or error in syntax.");
                    return Task.CompletedTask;
            }
        }

        private bool IsOperator(SocketUser user)
        {
            for (byte i = 0; i < operatorIDs.Length; i++)
            {
                if (user.Id == operatorIDs[i])
                    return true;
            }
            return false;
        }
    }
}