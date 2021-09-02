using System;
using System.Collections.Generic;

namespace maple
{
    class Program
    {

        static void Main(string[] args)
        {
            PrepareWindow();

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
                }
            }

            //load settings
            Settings.LoadSettings();

            //prepare styler
            Styler.LoadMapleTheme();

            //handle input
            //load file
            if (args.Length > 0)
            {
                Editor.Initialize(runInfo.Args[0]);
            }
            else //no argument provided
            {
                Printer.PrintLine("maple - terminal text editor | github.com/matthewd673/maple", Styler.AccentColor);
                Printer.PrintLine("No arguments provided: 'maple [filename]' to begin editing", Styler.ErrorColor);
                return;
            }

            //send control to editor
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
            Console.Clear();
            Console.ForegroundColor = Styler.AccentColor;
            Console.WriteLine("maple session ended");
            Console.ForegroundColor = Styler.TextColor;
            Environment.Exit(0);
        }

    }
}
