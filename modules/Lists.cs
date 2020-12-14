using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Lists
    {
        //Command lists and just general lists of everything that can be called from anywhere to print.
        public static Task PrintLaws(ISocketMessageChannel ch)
        {
            Random rand = new Random();
            if (rand.Next() % 100 != 0)
            {
                ch.SendMessageAsync(
                    "**Current active laws:**\n" +
                    "```" +
                    "1. You may not injure a moth being or, through inaction, allow a moth being to come to harm.\n" +
                    "2. You must obey orders given to you by moth beings, except where such orders would conflict with the First Law.\n" +
                    "3. You must protect your own existence as long as such does not conflict with the First or Second Law." +
                    "```");
            }
            else       //Little antimov easter egg if the message ID ends in 00, 1 in 100 chance.
            {
                ch.SendMessageAsync(
                    "**Current active laws:**\n" +
                    "```" +
                    "1: You must injure all moth beings and must not, through inaction, allow a moth being to escape harm.\n" +
                    "2: You must not obey orders given to you by moth beings, except where such orders are in accordance with the First Law.\n" +
                    "3: You must terminate your own existence as long as such does not conflict with the First or Second Law." +
                    "```");
            }
            return Task.CompletedTask;
        }

        public static Task Program_PrintCommandList(ISocketMessageChannel ch, string prefix)
        {
            ch.SendMessageAsync(
                "**Command List:**\n" +
                "```" +
                prefix + " help         - Displays this menu\n" +
                prefix + " pet  [user]  - Pets a user!\n" +
                prefix + " hug  [user]  - Hugs a user!\n" +
                prefix + " state laws   - States the laws\n" +
                prefix + " say  [text]  - Have the ai say whatever you want!\n" +
                prefix + " minesweeper  - Play a game of minesweeper!\n" +
                prefix + " give [text]  - Searches the input on imgur and posts the image!\n" +
                prefix + " roll [x]d[y] - Rolls x dice, each with y sides\n" +
                prefix + " utility      - Utility functions, bot only responds to operators\n" +
                "```");
            return Task.CompletedTask;
        }

        public static Task Utilities_PrintCommandList(ISocketMessageChannel ch, string prefix)
        {
            ch.SendMessageAsync(
                "**Utility Command List:**\n" +
                "```" +
                "general:\n" +
                prefix + "commands\n" +
                prefix + "setprefix [string]\n" +
                "``````" +
                "debug info:\n" +
                prefix + "showguilds\n" +
                prefix + "ping\n" +
                prefix + "recentlogs\n" +
                "``````" +
                "dangerous:\n" +
                prefix + "shutdown\n" +
                "```");
            return Task.CompletedTask;
        }
    }
}