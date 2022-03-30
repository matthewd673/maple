using System;
using System.Text;
using System.IO;

namespace maple
{
    class Program
    {

        static void Main(string[] args)
        {
            //initialize logger
            //note: maple directory will never be relative, since flag is set later
            Log.InitializeLogger();

            PrepareWindow();

            Log.Write("Parsing arguments", "program");
            //turn args into string for parser
            string argString = "";
            foreach(string s in args)
                argString += s + " ";
            argString.Trim(' ');

            CommandParser.CommandInfo runInfo = CommandParser.Parse(
                argString,
                defaultPrimaryCommand: "load",
                combineArgs: true
                );

            Log.Write("Loading settings from switches", "program");
            //load settings from switches
            foreach(string sw in runInfo.Switches)
            {
                switch(sw)
                {
                    case "--debug-tokens":
                        Settings.DebugTokens = true;
                        Settings.IgnoreSetting("debugtokens");
                        break;
                    case "--no-highlight":
                        Settings.NoHighlight = true;
                        Settings.IgnoreSetting("nohighlight");
                        break;
                    case "--cli-no-highlight":
                        Settings.CliNoHighlight = true;
                        Settings.IgnoreSetting("clinohighlight");
                        break;
                    case "-ro":
                    case "--relative-path":
                        Settings.RelativePath = true;
                        Settings.IgnoreSetting("relativepath");
                        break;
                    case "--readonly":
                        Input.ReadOnly = true;
                        Settings.IgnoreSetting("readonly");
                        break;
                    case "-log":
                    case "--enable-logging":
                        Settings.EnableLogging = true;
                        Settings.IgnoreSetting("enablelogging");
                        break;
                    case "--summarize-log":
                        Settings.EnableLogging = true;
                        Settings.IgnoreSetting("summarizelog");
                        break;
                    default:
                        Log.Write("Encountered unknown switch '" + sw + "'", "program", important: true);
                        break;
                }
            }

            Log.Write("Loading settings & aliases", "program");
            //load settings
            Settings.LoadSettings();
            Settings.LoadAliases();
            Settings.LoadShortcuts();

            //delete log file and pretend nothing happened if logging is actually disabled
            if (!Settings.EnableLogging)
                Log.DisableLogging();

            Log.Write("Loading theme", "program");
            //prepare styler
            Styler.LoadMapleTheme();

            //handle input
            //load file
            if (args.Length > 0)
            {
                Log.Write("Initializing editor with file '" + runInfo.Args[0] + "'", "program");
                Editor.Initialize(runInfo.Args[0]);
            }
            else //no argument provided
            {
                Log.Write("No file provided in args, defaulting to about", "program", important: true);
                Printer.PrintLineSimple("maple - terminal text editor | https://github.com/matthewd673/maple", Styler.AccentColor);
                Printer.PrintLineSimple("No arguments provided: 'maple [filename]' to begin editing", Styler.ErrorColor);
                Console.ResetColor();
                return;
            }

            //send control to editor
            Log.Write("Entering editor input loop", "program");
            Editor.BeginInputLoop();

        }

        static void PrepareWindow()
        {
            Printer.Initialize();
            Printer.Clear();
            Console.Title = "maple";
            Console.OutputEncoding = Encoding.UTF8;
            Printer.Resize();
        }

        public static void Close()
        {
            Log.Write("Session ended, cleaning up", "program");
            Console.Clear();
            Console.ForegroundColor = Styler.AccentColor;
            Console.WriteLine("maple session ended");

            if (Editor.CurrentDoc.GetAllLines().Count == 1 && Editor.CurrentDoc.GetLine(0).Length == 0 && Editor.CurrentDoc.NewlyCreated)
            {
                File.Delete(Editor.CurrentDoc.Filepath);
                Log.Write("Cleaned empty, newly-created file '" + Editor.CurrentDoc.Filepath + "'", "program", important: true);
            }

            if (Settings.SummarizeLog)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                if (Settings.EnableLogging)
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
