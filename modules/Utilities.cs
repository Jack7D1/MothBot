using Discord.WebSocket;

namespace MothBot.modules
{
    internal class Utilities
    {
        public Program _parent;

        private readonly ulong[] operatorIDs = {   //Discord user IDs of allowed operators, in ulong format.
            238735900597682177,   //Dex
            206920373952970753,   //Jack
        };

        private string args = "";
        private string command = "";

        public void CommandHandler(SocketMessage src)
        {
            if (!IsOperator(src))   //You do not have permission
                return;
            if (src.Content.Substring(src.Content.ToLower().IndexOf("utility")).Length + 8 >= src.Content.Length)
            {
                command = "commands";
            }
            else
            {
                command = src.Content.ToLower().Substring(src.Content.ToLower().IndexOf("utility") + 8); //We now have all text that follows the word 'utility'.
                command = command.Substring(0, command.IndexOf('('));
                args = command.Substring(command.IndexOf('(', command.IndexOf(')') - command.IndexOf('(')));
            }
            string prefix = _parent.PREFIX + " utility ";
            switch (command)
            {
                case "commands":
                    src.Channel.SendMessageAsync("**Utility Command List:**\n" +
                             "```general:\n" +
                             prefix + "commands\n" +
                             prefix + "reboot\n" +
                             prefix + "gitpullandreboot\n" +
                             "```\n" +

                             "```modules.Imagesearch:\n" +
                             prefix + "Imagesearch.togglefallback\n" +
                             prefix + "Imagesearch.maxretries(0-255)\n" +
                             "```\n" +

                             "```modules.Minesweeper:\n" +
                             prefix + "Minesweeper.setbombs(0-65335)\n" +
                             prefix + "Minesweeper.setsize(0-255)\n" +
                             "```");
                    break;

                default:
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
    }
}