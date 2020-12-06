using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Utilities
    {
        public static bool confirmNoAccess = false;
        private static bool shutdownEnabled = false;
        private readonly Imagesearch _imageSearch;
        private readonly Logging _logging;
        private readonly Minesweeper _mineSweeper;
        private readonly Program _parent;
        private readonly Sanitize _sanitize;

        private readonly ulong[] operatorIDs = {   //Discord user IDs of allowed operators, in ulong format.
            206920373952970753, //Jack
            144308736083755009, //Hirohito
            238735900597682177, //Dex
            489965198531362838, //Argema
        };

        private string command, keyword, arg;

        public Utilities(Program parentRef)
        {
            _parent = parentRef;
            _imageSearch = _parent._imageSearch;
            _logging = _parent._logging;
            _mineSweeper = _parent._mineSweeper;
            _sanitize = _parent._sanitize;
        }

        public Task CommandHandler(SocketMessage src)
        {
            command = "";
            keyword = "";
            arg = "";
            if (!IsOperator(src))   //You do not have permission
                if (confirmNoAccess)
                {
                    src.Channel.SendMessageAsync("You do not have access to Utilities.");
                    return Task.CompletedTask;
                }
                else
                    return Task.CompletedTask;
            if (src.Content.Length < (_parent.PREFIX + " utility ").Length)
            {
                keyword = "commands";
            }
            else
            {
                command = src.Content.ToLower().Substring(src.Content.ToLower().IndexOf("utility") + "utility ".Length); //We now have all text that follows the word 'utility'.
                if (command.Contains('(') && command.Contains(')'))
                {
                    keyword = command.Substring(0, command.IndexOf('('));
                    arg = command[(command.IndexOf('(') + 1)..command.IndexOf(')')];
                }
                else
                {
                    keyword = command;
                }
            }
            string prefix = _parent.PREFIX + " utility ";
            switch (keyword)    //Ensure switch is ordered similarly to command list
            {
                //General
                case "commands":
                    src.Channel.SendMessageAsync("**Utility Command List:**\n" +
                             "```" +
                             "general:\n" +
                             prefix + "commands\n" +
                             prefix + "togglenoaccessconfirmation\n" +
                             prefix + "resetmodules\n" +
                             prefix + "setprefix(string)\n" +
                             "``````" +
                             "debug info:\n" +
                             prefix + "showvars\n" +
                             prefix + "showguilds\n" +
                             prefix + "ping\n" +
                             prefix + "recentlogs\n" +
                             "``````" +
                             "modules.Imagesearch:\n" +
                             prefix + "imagesearch.togglefallback\n" +
                             prefix + "imagesearch.maxretries(0-255)\n" +
                             "``````" +
                             "modules.Minesweeper:\n" +
                             prefix + "minesweeper.setbombs(0-64)" +
                             "``````" +
                             "dangerous:\n" +
                             prefix + "shutdown\n" +
                             "```");
                    return Task.CompletedTask;

                case "togglenoaccessconfirmation":
                    confirmNoAccess = !confirmNoAccess;
                    _logging.LogtoConsoleandFile($"confirmNoAccess toggled to {confirmNoAccess}");
                    src.Channel.SendMessageAsync($"confirmNoAccess toggled to {confirmNoAccess}");
                    return Task.CompletedTask;

                case "resetmodules":
                    _logging.LogtoConsoleandFile("RESETTING MODULES");
                    _parent.InitModules();
                    src.Channel.SendMessageAsync("Modules reset!");
                    return Task.CompletedTask;

                case "setprefix":
                    arg = arg.ToLower();    //JUST in case
                    _parent.PREFIX = arg;
                    _logging.LogtoConsoleandFile($"PREFIX CHANGED TO \"{_parent.PREFIX}\"");
                    src.Channel.SendMessageAsync($"Prefix changed to {_parent.PREFIX}!");
                    _parent._client.SetGameAsync("Prefix: " + arg + ". Say '" + arg + " help' for commands!", null, ActivityType.Playing);
                    return Task.CompletedTask;

                //Debug info
                case "showvars":
                    src.Channel.SendMessageAsync("**Set Vars:**\n" +
                        "```" +
                        "general:\n" +
                        $"Current Prefix: \"{_parent.PREFIX}\"\n" +
                        $"confirmNoAccess: {confirmNoAccess}\n" +
                        $"shutdownEnabled: {shutdownEnabled}\n" +
                        "``````" +
                        "modules.Imagesearch:\n" +
                        $"imagesearch.firstResultFallback: {_imageSearch.firstResultFallback}\n" +
                        $"imagesearch.maxRetries: {_imageSearch.maxRetries}\n" +
                        "``````" +
                        "modules.Minesweeper:\n" +
                        $"minesweeper.defaultBombs: {_mineSweeper.defaultBombs}\n" +
                        "```");
                    return Task.CompletedTask;

                case "showguilds":
                    {
                        string guildstring = "";

                        byte i = 0;
                        foreach (SocketGuild guild in _parent._client.Guilds)
                        {
                            guildstring += $"GUILD {i + 1}: {guild.Name} [{guild.Id}]. Members: {guild.MemberCount}\n";
                            i++;
                        }

                        src.Channel.SendMessageAsync(guildstring);
                        return Task.CompletedTask;
                    }

                case "ping":
                    src.Channel.SendMessageAsync($"Ping: {_parent._client.Latency}ms");
                    return Task.CompletedTask;

                case "recentlogs":
                    {
                        string[] inlogs = _logging.GetLogs();
                        string outstr = "**Recent Log Entries**```";
                        for (byte i = 0; i < 255; i++)
                        {
                            outstr += $"{i + 1}: \"{inlogs[i]}\"\n";
                            if (i < 255)
                                if (inlogs[i + 1] == null)
                                    break;
                        }
                        src.Channel.SendMessageAsync(outstr + "```");
                        return Task.CompletedTask;
                    }

                //modules.Imagesearch:
                case "imagesearch.togglefallback":
                    _imageSearch.ToggleFallback();
                    src.Channel.SendMessageAsync("firstResultFallback toggled to: " + _imageSearch.firstResultFallback);
                    _logging.LogtoConsoleandFile("firstResultFallback toggled to: " + _imageSearch.firstResultFallback);
                    return Task.CompletedTask;

                case "imagesearch.maxretries":
                    _imageSearch.maxRetries = byte.Parse(arg);
                    src.Channel.SendMessageAsync($"maxRetries set to: {_imageSearch.maxRetries}");
                    _logging.LogtoConsoleandFile($"maxRetries set to: {_imageSearch.maxRetries}");
                    return Task.CompletedTask;

                //modules.Minesweeper:
                case "minesweeper.setbombs":
                    _mineSweeper.defaultBombs = ushort.Parse(arg);
                    src.Channel.SendMessageAsync("minesweeper.defaultBombs set to: " + _mineSweeper.defaultBombs);
                    _logging.LogtoConsoleandFile($"defaultBombs set to: {_mineSweeper.defaultBombs}");
                    return Task.CompletedTask;

                //dangerous
                case "shutdown":
                    if (arg == "confirm")
                    {
                        if (shutdownEnabled)
                        {
                            Console.WriteLine($"SHUTTING DOWN: ordered by {src.Author.Username}");
                            src.Channel.SendMessageAsync($"{src.Author.Mention} Shutdown confirmed, terminating bot.");
                            Environment.Exit(13);
                            return Task.CompletedTask;
                        }
                        else
                        {
                            shutdownEnabled = true;
                            src.Channel.SendMessageAsync($"Shutdown safety disabled, {src.Author.Mention} confirm shutdown again to shut down bot, or argument anything else to re-enable safety.");
                            Console.WriteLine($"{src.Author.Username} disabled shutdown safety.");
                            return Task.CompletedTask;
                        }
                    }
                    else
                    {
                        shutdownEnabled = false;
                        src.Channel.SendMessageAsync("Shutdown safety (re)enabled, argument (confirm) to disable.");
                        return Task.CompletedTask;
                    }

                default:
                    src.Channel.SendMessageAsync("Function does not exist or error in syntax.");
                    return Task.CompletedTask;
            }
        }

        public bool IsOperator(SocketMessage src)
        {
            for (byte i = 0; i < operatorIDs.Length; i++)
            {
                if (src.Author.Id == operatorIDs[i])
                    return true;
            }
            return false;
        }
    }
}