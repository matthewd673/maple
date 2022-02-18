using System;

namespace maple
{
    class Program
    {

        static void Main(string[] args)
        {
            PrepareWindow();

            //initialize logger
            //note: maple directory will never be relative, since flag is set later (this may be a good thing?)
            Log.InitializeLogger();

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
                    case "--quick-cli":
                        Settings.QuickCli = true;
                        Settings.IgnoreSetting("quickcli");
                        break;
                    case "--debug-tokens":
                        Settings.DebugTokens = true;
                        Settings.IgnoreSetting("debugtokens");
                        break;
                    case "--no-highlight":
                        Settings.NoHighlight = true;
                        Settings.IgnoreSetting("nohighlight");
                        break;
                    case "--relative-path":
                        Settings.RelativePath = true;
                        Settings.IgnoreSetting("relativepath");
                        break;
                    case "--navigate-past-tabs":
                        Settings.NavigatePastTabs = true;
                        Settings.IgnoreSetting("navigatepasttabs");
                        break;
                    case "--delete-entire-tabs":
                        Settings.DeleteEntireTabs = true;
                        Settings.IgnoreSetting("deleteentiretabs");
                        break;
                    case "--readonly":
                        Input.ReadOnly = true;
                        Settings.IgnoreSetting("readonly");
                        break;
                    case "--enable-logging":
                        Settings.EnableLogging = true;
                        Settings.IgnoreSetting("enablelogging");
                        break;
                    default:
                        Log.Write("Encountered unknown switch '" + sw + "'", "program");
                        break;
                }
            }

            Log.Write("Loading settings & aliases", "program");
            //load settings
            Settings.LoadSettings();
            Settings.LoadAliases();

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
                Log.Write("No file provided in args, defaulting to about", "program");
                Printer.PrintLine("maple - terminal text editor | github.com/matthewd673/maple", Styler.AccentColor);
                Printer.PrintLine("No arguments provided: 'maple [filename]' to begin editing", Styler.ErrorColor);
                return;
            }

            //send control to editor
            Log.Write("Entering editor input loop", "program");
            Editor.BeginInputLoop();

        }

        static void PrepareWindow()
        {
            Console.Clear();
            Console.Title = "maple";
            Printer.Resize();
        }

        public static void Close()
        {
            Log.Write("Session ended, cleaning up", "program");
            Console.Clear();
            Console.ForegroundColor = Styler.AccentColor;
            Console.WriteLine("maple session ended");
            Console.ForegroundColor = Styler.TextColor;
            Environment.Exit(0);
        }

    }
}
