using System;
using System.IO;

namespace maple
{
    static class Log
    {

        static string logPath;

        public static void InitializeLogger()
        {
            if (!Settings.EnableLogging) return;

            logPath = Settings.MapleDirectory + "\\log.txt";
            File.CreateText(logPath).Close();
            Write("New logging session started at " + DateTime.Now.ToString("HH:mm:ss"), "logger");
        }

        public static void Write(string text, string speaker)
        {
            if (!Settings.EnableLogging) return;
            File.AppendAllText(logPath, String.Format("[{0}]: {1}\n", speaker, text));
        }

        public static void WriteDebug(string text, string speaker)
        {
            if (!Settings.EnableLogging) return;
            #if DEBUG
                File.AppendAllText(logPath, String.Format("[DEBUG-{0}]: {1}\n", speaker, text));
            #endif
        }
    }
}