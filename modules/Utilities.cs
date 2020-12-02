using Discord;
using Discord.WebSocket;
using System;

namespace MothBot.modules
{
    internal class Utilities
    {
        public Program _parent;

        private readonly ulong[] operatorIDs = {   //Discord user IDs of allowed operators, in ulong format.
            206920373952970753, //Jack
            144308736083755009, //Hirohito
            238735900597682177, //Dex
            489965198531362838, //Argema
        };

        private string command = "";
        private string keyword = "";
        private string arg = "";

        public void CommandHandler(SocketMessage src)
        {
            if (!IsOperator(src))   //You do not have permission
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
                             "```general:\n" +
                             prefix + "commands\n" +
                             prefix + "showvars\n" +
                             prefix + "reboot\n" +
                             prefix + "gitpullandreboot\n" +
                             prefix + "resetmodules\n" +
                             prefix + "setprefix(string)\n" +
                             "```\n" +

                             "```modules.Imagesearch:\n" +
                             prefix + "imagesearch.togglefallback\n" +
                             prefix + "imagesearch.maxretries(0-255)\n" +
                             "```\n" +

                             "```modules.Minesweeper:\n" +
                             prefix + "minesweeper.setbombs(0-65335)\n" +
                             //prefix + "minesweeper.setsize(0-16)\n" +
                             "```");
                    return;

                case "showvars":
                    src.Channel.SendMessageAsync("**Set Vars:**\n```" +
                        $"FirstResultFallback: {this._parent._imageSearch.firstResultFallback}\n" +
                        $"maxRetries: {this._parent._imageSearch.maxRetries}\n" +
                        $"defaultBombs: {this._parent._mineSweeper.defaultBombs}\n" +
                        //$"defaultGridsize: {this._parent._mineSweeper.defaultGridsize}\n" +
                        $"```");
                    return;

                case "reboot":
                    Placeholder(src);
                    return;

                case "gitpullandreboot":
                    Placeholder(src);
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
                    this._parent._mineSweeper.defaultBombs = ushort.Parse(arg);
                    src.Channel.SendMessageAsync("defaultBombs set to: " + this._parent._mineSweeper.defaultBombs);
                    Console.WriteLine("defaultBombs set to: " + this._parent._mineSweeper.defaultBombs);
                    return;

                /*case "minesweeper.setsize":
                    this._parent._mineSweeper.defaultGridsize = byte.Parse(arg);
                    src.Channel.SendMessageAsync("defaultGridsize set to: " + this._parent._mineSweeper.defaultGridsize);
                    Console.WriteLine("defaultGridsize set to: " + this._parent._mineSweeper.defaultGridsize);
                    return;*/

                default:
                    Placeholder(src);
                    break;
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

        private void Placeholder(SocketMessage src)
        {
            src.Channel.SendMessageAsync("Function does not exist or error in syntax.");
        }
    }
}