using System;
using System.Collections.Generic;

namespace maple
{
    class Program
    {

        //static DocumentCursor docCursor;
        //static Cursor cmdCursor;
        //static Document document;

        //static List<int> refreshedLines = new List<int>();
        //static bool fullClearNext = false;

        static void Main(string[] args)
        {
            PrepareWindow();

            //prepare styler
            Styler.LoadMapleTheme();

            //handle input
            //load file
            if(args.Length > 0)
            {
                Editor.Initialize(args[0]);
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
