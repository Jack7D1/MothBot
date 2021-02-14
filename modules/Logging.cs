﻿using Discord;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Logging
    {
        private const string LOG_PATH = @"../../data/log.txt";
        private static StreamWriter log;

        public Logging()
        {
            if (!Directory.Exists(LOG_PATH))
                Directory.CreateDirectory(LOG_PATH.Substring(0, LOG_PATH.LastIndexOf('\\')));

            log = new StreamWriter(LOG_PATH, true);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
        }

        public static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public static void Log(string str)
        {
            log.WriteLineAsync(str);
            log.Flush();
        }

        public static async Task LogAsync(string str)
        {
            await log.WriteLineAsync(str);
            log.Flush();
        }

        public static void LogtoConsoleandFile(string str)
        {
            Console.WriteLine(str);
            Log(str);
        }

        public static async Task LogtoConsoleandFileAsync(string str)
        {
            Console.WriteLine(str);
            await LogAsync(str);
        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            Log($"System shutdown at [{System.DateTime.UtcNow}]");
        }
    }
}