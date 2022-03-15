using System;
using System.IO;

namespace maple
{
    static class Log
    {

        public static string LogPath { get; private set; }

        public static int ImportantEvents { get; private set; } = 0;
        public static int DebugEvents { get; private set; } = 0;

        public static void InitializeLogger()
        {
            if (!Settings.EnableLogging) return;

            LogPath = Settings.MapleDirectory + "\\log.txt";
            File.CreateText(LogPath).Close();
            Write("New logging session started at " + DateTime.Now.ToString("HH:mm:ss"), "logger");
            #if DEBUG
                Write("Maple is running under a debugger, additional events may be logged", "logger");
            #endif
        }

        public static void Write(string text, string speaker, bool important = false)
        {
            if (!Settings.EnableLogging) return;
            string template = "[{0}]: {1}\n";
            if (important)
            {
                template = "!!! " + template;
                ImportantEvents++;
            }
            File.AppendAllText(LogPath, String.Format(template, speaker, text));
        }

        public static void WriteDebug(string text, string speaker)
        {
            if (!Settings.EnableLogging) return;
            #if DEBUG
                File.AppendAllText(LogPath, String.Format("DEBUG [{0}]: {1}\n", speaker, text));
                DebugEvents++;
            #endif
        }
    }
}