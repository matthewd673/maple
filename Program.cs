﻿using System;
using System.Collections.Generic;

namespace maple
{
    class Program
    {

        static void Main(string[] args)
        {
            PrepareWindow();

            //prepare styler
            Styler.LoadMapleTheme();

            //turn args into string for parser
            String argString = "";
            foreach(String s in args)
                argString += s + " ";
            argString.Trim(' ');

            CommandParser.CommandInfo runInfo = CommandParser.Parse(
                argString,
                defaultPrimaryCommand: "load",
                combineArgs: true
                );

            foreach(String sw in runInfo.switches)
            {
                switch(sw)
                {
                    case "--quick-cli":
                        Editor.quickCli = true;
                        break;
                    case "--debug-tokens":
                        Editor.debugTokens = true;
                        break;
                }
            }

            //handle input
            //load file
            if(args.Length > 0)
            {
                Editor.Initialize(runInfo.args[0]);
            }
            else //no argument provided
            {
                Printer.PrintLine("maple - terminal text editor | github.com/matthewd673/maple", Styler.accentColor);
                Printer.PrintLine("No arguments provided: 'maple [filename]' to begin editing", Styler.errorColor);
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
            Console.ForegroundColor = Styler.accentColor;
            Console.WriteLine("maple session ended");
            Console.ForegroundColor = Styler.textColor;
            Environment.Exit(0);
        }

    }
}
