﻿using Discord;
using Discord.WebSocket;
using System;

namespace MothBot.modules
{
    internal class Utilities
    {
        public Program _parent;

        public bool confirmNoAccess = false;

        private readonly ulong[] operatorIDs = {   //Discord user IDs of allowed operators, in ulong format.
            206920373952970753, //Jack
            144308736083755009, //Hirohito
            238735900597682177, //Dex
            489965198531362838, //Argema
        };

        private string arg = "";
        private string command = "";
        private string keyword = "";
        private bool shutdownEnabled = false;

        public void CommandHandler(SocketMessage src)
        {
            if (!IsOperator(src))   //You do not have permission
                if (confirmNoAccess)
                {
                    src.Channel.SendMessageAsync("You do not have access to Utilities.");
                    return;
                }
                else
                    return;
            if (src.Content.Length < (_parent.PREFIX + " utility ").Length)
            {
                keyword = "commands";
            }
            else
            {
                command = src.Content.ToLower().Substring(src.Content.ToLower().IndexOf("utility") + "utility ".Length); //We now have all text that follows the word 'utility'.
                if (command.Contains('('))
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
                case "commands":
                    src.Channel.SendMessageAsync("**Utility Command List:**\n" +
                             "```" +
                             "general:\n" +
                             prefix + "commands\n" +
                             prefix + "togglenoaccess" +
                             prefix + "showvars\n" +
                             prefix + "resetmodules\n" +
                             prefix + "setprefix(string)\n" +
                             "``````" +
                             "modules.Imagesearch:\n" +
                             prefix + "imagesearch.togglefallback\n" +
                             prefix + "imagesearch.maxretries(0-255)\n" +
                             "``````" +
                             "modules.Minesweeper:\n" +
                             prefix + "minesweeper.setbombs(0-65335)\n" +
                             //prefix + "minesweeper.setsize(0-16)\n" +
                             "```````" +
                             "dangerous:\n" +
                             prefix + "shutdown\n" +
                             "```");
                    return;

                case "togglenoaccess":
                    confirmNoAccess = !confirmNoAccess;
                    Console.WriteLine($"confirmNoAccess toggled to {confirmNoAccess}");
                    src.Channel.SendMessageAsync($"confirmNoAccess toggled to {confirmNoAccess}");
                    return;

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
                        "``````" +
                        "modules.Minesweeper:\n" +
                        $"minesweeper.defaultBombs(0-65335)\n" +
                        //$"minesweeper.defaultGridsize(0-16)\n" +
                        "```");
                    return;

                case "resetmodules":
                    Console.WriteLine("RESETTING MODULES");
                    this._parent.InitModules();
                    src.Channel.SendMessageAsync("Modules reset!");
                    return;

                case "setprefix":
                    arg = arg.ToLower();    //JUST in case
                    this._parent.PREFIX = arg;
                    Console.WriteLine($"PREFIX CHANGED TO \"{this._parent.PREFIX}\"");
                    src.Channel.SendMessageAsync($"Prefix changed to {this._parent.PREFIX}!");
                    this._parent._client.SetGameAsync("Prefix: " + arg + ". Say '" + arg + " help' for commands!", null, ActivityType.Playing);
                    return;

                case "imagesearch.togglefallback":
                    this._parent._imageSearch.ToggleFallback();
                    src.Channel.SendMessageAsync("firstResultFallback toggled to: " + this._parent._imageSearch.firstResultFallback);
                    Console.WriteLine("firstResultFallback toggled to: " + this._parent._imageSearch.firstResultFallback);
                    return;

                case "imagesearch.maxretries":
                    this._parent._imageSearch.maxRetries = byte.Parse(arg);
                    src.Channel.SendMessageAsync("maxRetries set to: " + this._parent._imageSearch.maxRetries);
                    Console.WriteLine("maxRetries set to: " + this._parent._imageSearch.maxRetries);
                    return;

                case "minesweeper.setbombs":
                    ushort Sarg = (ushort)Math.Abs(Math.Min(ushort.Parse(arg), ushort.MaxValue));
                    this._parent._mineSweeper.defaultBombs = Sarg;
                    src.Channel.SendMessageAsync("defaultBombs set to: " + this._parent._mineSweeper.defaultBombs);
                    Console.WriteLine("defaultBombs set to: " + this._parent._mineSweeper.defaultBombs);
                    return;

                /*case "minesweeper.setsize":
                    this._parent._mineSweeper.defaultGridsize = byte.Parse(arg);
                    src.Channel.SendMessageAsync("defaultGridsize set to: " + this._parent._mineSweeper.defaultGridsize);
                    Console.WriteLine("defaultGridsize set to: " + this._parent._mineSweeper.defaultGridsize);
                    return;*/

                case "shutdown":
                    if (arg == "confirm")
                    {
                        if (shutdownEnabled)
                        {
                            Console.WriteLine($"SHUTTING DOWN: ordered by {src.Author.Username}");
                            src.Channel.SendMessageAsync($"{src.Author.Mention} Shutdown confirmed, terminating bot.");
                            Environment.Exit(13);
                            return;
                        }
                        else
                        {
                            src.Channel.SendMessageAsync($"Shutdown safety disabled, {src.Author.Mention} confirm shutdown again to shut down bot, or argument anything else to cancel.");
                            Console.WriteLine($"{src.Author.Username} disabled shutdown safety.");
                            return;
                        }
                    }
                    else
                    {
                        shutdownEnabled = false;
                        src.Channel.SendMessageAsync("Shutdown safety enabled, argument (confirm) to disable.");
                        return;
                    }

                default:
                    src.Channel.SendMessageAsync("Function does not exist or error in syntax.");
                    return;
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