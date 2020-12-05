﻿using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Utilities
    {
        public static bool confirmNoAccess = false;
        public Program _parent;
        private static bool shutdownEnabled = false;

        private readonly ulong[] operatorIDs = {   //Discord user IDs of allowed operators, in ulong format.
            206920373952970753, //Jack
            144308736083755009, //Hirohito
            238735900597682177, //Dex
            489965198531362838, //Argema
        };

        private string command, keyword, arg;

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
                             "``````" +
                             "modules.Imagesearch:\n" +
                             prefix + "imagesearch.togglefallback\n" +
                             prefix + "imagesearch.maxretries(0-255)\n" +
                             "``````" +
                             "dangerous:\n" +
                             prefix + "shutdown\n" +
                             "```");
                    return Task.CompletedTask;

                case "togglenoaccessconfirmation":
                    confirmNoAccess = !confirmNoAccess;
                    Console.WriteLine($"confirmNoAccess toggled to {confirmNoAccess}");
                    src.Channel.SendMessageAsync($"confirmNoAccess toggled to {confirmNoAccess}");
                    return Task.CompletedTask;

                case "resetmodules":
                    Console.WriteLine("RESETTING MODULES");
                    this._parent.InitModules();
                    src.Channel.SendMessageAsync("Modules reset!");
                    return Task.CompletedTask;

                case "setprefix":
                    arg = arg.ToLower();    //JUST in case
                    this._parent.PREFIX = arg;
                    Console.WriteLine($"PREFIX CHANGED TO \"{this._parent.PREFIX}\"");
                    src.Channel.SendMessageAsync($"Prefix changed to {this._parent.PREFIX}!");
                    this._parent._client.SetGameAsync("Prefix: " + arg + ". Say '" + arg + " help' for commands!", null, ActivityType.Playing);
                    return Task.CompletedTask;

                //Debug info
                case "showvars":
                    src.Channel.SendMessageAsync("**Set Vars:**\n" +
                        "```" +
                        "general:\n" +
                        $"Current Prefix: \"{this._parent.PREFIX}\"\n" +
                        $"confirmNoAccess: {confirmNoAccess}\n" +
                        $"shutdownEnabled: {shutdownEnabled}\n" +
                        "``````" +
                        "modules.Imagesearch:\n" +
                        $"imagesearch.firstResultFallback: {this._parent._imageSearch.firstResultFallback}\n" +
                        $"imagesearch.maxRetries: {this._parent._imageSearch.maxRetries}\n" +
                        "```");
                    return Task.CompletedTask;

                case "showguilds":
                    int i = 0;
                    string guildstring = "";
                    foreach (SocketGuild guild in this._parent._client.Guilds)
                    {
                        guildstring += $"GUILD {i + 1}: {guild.Name} [{guild.Id}]. Members: {guild.MemberCount}\n";
                        i++;
                    }
                    src.Channel.SendMessageAsync(guildstring);
                    return Task.CompletedTask;

                case "ping":
                    src.Channel.SendMessageAsync($"Ping: {this._parent._client.Latency}ms");
                    return Task.CompletedTask;

                //modules.Imagesearch:
                case "imagesearch.togglefallback":
                    this._parent._imageSearch.ToggleFallback();
                    src.Channel.SendMessageAsync("firstResultFallback toggled to: " + this._parent._imageSearch.firstResultFallback);
                    Console.WriteLine("firstResultFallback toggled to: " + this._parent._imageSearch.firstResultFallback);
                    return Task.CompletedTask;

                case "imagesearch.maxretries":
                    this._parent._imageSearch.maxRetries = byte.Parse(arg);
                    src.Channel.SendMessageAsync("maxRetries set to: " + this._parent._imageSearch.maxRetries);
                    Console.WriteLine("maxRetries set to: " + this._parent._imageSearch.maxRetries);
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