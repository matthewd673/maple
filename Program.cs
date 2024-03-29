﻿using System;
using System.Text;
using System.IO;

namespace maple
{
    class Program
    {
        static void Main(string[] args)
        {
            // initialize logger
            // NOTE: maple directory will never be relative, since flag is set later
            Log.InitializeLogger();

            PrepareWindow();

            Log.Write("Parsing arguments", "program");
            // turn args into string for parser
            string argString = "";
            foreach(string s in args)
                argString += s + " ";
            argString.Trim(' ');

            CommandParser.CommandInfo runInfo = CommandParser.Parse(
                argString,
                defaultPrimaryCommand: "load",
                combineArgs: true
                );

            Log.Write("Loading settings", "program");

            // load settings
            Settings.LoadSettings();

            Log.Write("Loading settings from switches", "program");
            // load settings from switches
            foreach(string sw in runInfo.Switches)
            {
                switch(sw)
                {
                    case "--debug-tokens":
                        Settings.Properties.DebugTokens = true;
                        break;
                    case "--no-highlight":
                        Settings.Properties.NoHighlight = true;
                        break;
                    case "--no-tokenize":
                        Settings.Properties.NoTokenize = true;
                        break;
                    case "--cli-no-highlight":
                        Settings.Properties.CliNoHighlight = true;
                        break;
                    case "-ro":
                    case "--readonly":
                        Settings.Properties.ReadOnly = true;
                        break;
                    case "--relative-path":
                        Settings.Properties.RelativePath = true;
                        break;
                    case "-log":
                    case "--enable-logging":
                        Settings.Properties.EnableLogging = true;
                        break;
                    case "--summarize-log":
                        Settings.Properties.EnableLogging = true;
                        break;
                    default:
                        Log.Write("Encountered unknown switch '" + sw + "'", "program", important: true);
                        break;
                }
            }

            // delete log file and pretend nothing happened if logging is actually disabled
            if (!Settings.Properties.EnableLogging)
            {
                Log.DisableLogging();
            }

            // load file
            if (args.Length > 0)
            {
                Log.Write("Initializing editor with file '" + runInfo.Args[0] + "'", "program");
                Editor.Initialize(runInfo.Args[0]);
            }
            else // no argument provided
            {
                Log.Write("No file provided in args, creating Document with no filename", "program", important: true);
                Editor.Initialize("");
            }

            // send control to editor
            Log.Write("Entering editor input loop", "program");
            Editor.BeginInputLoop();

        }

        static void PrepareWindow()
        {
            Printer.Initialize();
            Printer.Clear();
            Input.Initialize();
            Console.Title = "maple";
            Console.OutputEncoding = Encoding.UTF8;
            Printer.Resize();
        }

        public static void ThrowFatalError(string message)
        {
            Log.Write("Throw fatal error: " + message, "program", important: true);

            Input.RestorePreviousConsoleMode();

            Console.ForegroundColor = Settings.Theme.ErrorColor;
            Console.WriteLine("A fatal error occurred: " + message);

            Console.ResetColor();
            Environment.Exit(1);
        }

        public static void Close()
        {
            Log.Write("Session ended, cleaning up", "program");

            Input.RestorePreviousConsoleMode();

            Console.Clear();
            Console.ForegroundColor = Settings.Theme.AccentColor;
            Console.WriteLine("maple session ended");

            if (Editor.CurrentDoc.GetAllLines().Count == 1 && Editor.CurrentDoc.GetLine(0).Length == 0 && Editor.CurrentDoc.NewlyCreated)
            {
                File.Delete(Editor.CurrentDoc.Filepath);
                Log.Write("Cleaned empty, newly-created file '" + Editor.CurrentDoc.Filepath + "'", "program", important: true);
            }

            if (Settings.Properties.SummarizeLog)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                if (Settings.Properties.EnableLogging)
                    Console.WriteLine("{0} important/unusual log event(s) occurred in the last session", Log.ImportantEvents);
                else
                    Console.WriteLine("Logging was disabled during this session; however, an empty log file still exists");
                #if DEBUG
                    Console.WriteLine("The last session was run in a debugger, {0} additional event(s) are available", Log.DebugEvents);
                #endif
                Console.ResetColor();
                Console.WriteLine("Log file is available at: {0}", Log.LogPath);
            }

            Console.ResetColor();
            Environment.Exit(0);
        }

    }
}
