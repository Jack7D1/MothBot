using Discord.WebSocket;

namespace MothBot.modules
{
    internal class Utilities
    {
        public Program _parent;

        private readonly ulong[] operatorIDs = {   //Discord user IDs of allowed operators, in ulong format.
            238735900597682177, //Dex
            206920373952970753, //Jack
            489965198531362838, //Argema
        };

        private string args = "";
        private string command = "";

        public void CommandHandler(SocketMessage src)
        {
            if (!IsOperator(src))   //You do not have permission
                return;
            if (src.Content.Length < (_parent.PREFIX + " utility ").Length)
            {
                command = "commands";
            }
            else
            {
                command = src.Content.ToLower().Substring(src.Content.ToLower().IndexOf("utility") + "utility ".Length); //We now have all text that follows the word 'utility'.
                if (command.Contains('('))
                {
                    command = command.Substring(0, command.IndexOf('('));
                    args = command.Substring(command.IndexOf('(', command.IndexOf(')') - command.IndexOf('(')));
                }
            }
            string prefix = _parent.PREFIX + " utility ";
            switch (command)    //Ensure switch is ordered similarly to command list
            {
                case "commands":
                    src.Channel.SendMessageAsync("**Utility Command List:**\n" +
                             "```general:\n" +
                             prefix + "commands\n" +
                             prefix + "reboot\n" +
                             prefix + "gitpullandreboot\n" +
                             "```\n" +

                             "```modules.Imagesearch:\n" +
                             prefix + "imagesearch.togglefallback\n" +
                             prefix + "imagesearch.maxretries(0-255)\n" +
                             "```\n" +

                             "```modules.Minesweeper:\n" +
                             prefix + "minesweeper.setbombs(0-65335)\n" +
                             prefix + "minesweeper.setsize(0-255)\n" +
                             "```");
                    return;

                case "reboot":
                    Placeholder(src);
                    return;

                case "gitpullandreboot":
                    Placeholder(src);
                    return;

                case "imagesearch.togglefallback":
                    this._parent._imageSearch.ToggleFallback();
                    src.Channel.SendMessageAsync("First result image fallback toggled to: " + this._parent._imageSearch.enable_firstResultFallback);
                    return;

                case "imagesearch.maxretries":
                    this._parent._imageSearch.maxRetries = byte.Parse(args);
                    src.Channel.SendMessageAsync("Maxretries set to: " + this._parent._imageSearch.maxRetries);
                    return;

                case "minesweeper.setbombs":
                    Placeholder(src);
                    return;

                case "minesweeper.setsize":
                    Placeholder(src);
                    return;

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