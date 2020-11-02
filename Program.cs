using System;
using System.Collections.Generic;

namespace maple
{
    class Program
    {

        static DocumentCursor docCursor;
        static Cursor cmdCursor;
        static Document document;

        static List<int> refreshedLines = new List<int>();
        static bool fullClearNext = false;

        static void Main(string[] args)
        {
            PrepareWindow();
            
            //create cursor
            //docCursor = new Cursor(0, 0);
            cmdCursor = new Cursor(0, 0);

            cmdCursor.contentOffsetX = 7;
            cmdCursor.contentOffsetY = Cursor.maxScreenY;

            //prepare styler
            Styler.LoadMapleTheme();

            //handle input
            //load file
            if(args.Length > 0)
            {
                document = new Document(args[0], loadStyleInfo: true);
            }
            else //no argument provided
            {
                Printer.PrintLine("maple - terminal text editor | github.com/matthewd673/maple", Styler.accentColor);
                Printer.PrintLine("No arguments provided: maple [filename] to begin editing", Styler.errorColor);
                return;
            }
        
            docCursor = new DocumentCursor(document, 0, 0);
            docCursor.Move(0, 0);
            docCursor.ApplyPosition();

            document.CalculateGutterWidth();

            //render initial footer
            Printer.DrawFooter("maple", foregroundColor: Styler.accentColor, backgroundColor: ConsoleColor.Black);
            //reset to initial position
            docCursor.Move(docCursor.dX, docCursor.dY);


            RedrawLines();
            RefreshAllLines();

            //primary loop
            while(true)
            {
                //render initial footer
                PrintFooter();

                //set actual cursor position
                GetActiveCursor().ApplyPosition();

                //accept input
                Input.AcceptInput(Console.ReadKey());

                //redraw lines that have changed
                if(fullClearNext)
                {
                    Console.Clear();
                    fullClearNext = false;
                }
                RedrawLines();

                //render footer
                PrintFooter();

                //apply user cursor position
                if(Input.GetInputTarget() == Input.InputTarget.Document)
                    docCursor.Move(docCursor.dX, docCursor.dY);
                else if(Input.GetInputTarget() == Input.InputTarget.Command)
                    cmdCursor.Move(cmdCursor.dX, cmdCursor.dY);
            }
        }

        static void PrepareWindow()
        {
            Console.Clear();
            Console.Title = "maple";
            Printer.Resize();
        }

        public static Cursor GetActiveCursor()
        {
            if(Input.GetInputTarget() == Input.InputTarget.Document)
                return docCursor;
            else if(Input.GetInputTarget() == Input.InputTarget.Command)
                return cmdCursor;
            
            return docCursor;
        }

        public static DocumentCursor GetDocCursor() { return docCursor; }

        public static Cursor GetCommandCursor() { return cmdCursor; }

        public static void Close()
        {
            Console.Clear();
            Console.ForegroundColor = Styler.accentColor;
            Console.WriteLine("maple session ({0})", document.GetFilePath());
            Console.ForegroundColor = Styler.textColor;
            Environment.Exit(0);
        }

        public static void RefreshLine(int lineIndex)
        {
            refreshedLines.Add(lineIndex);
        }

        public static void RefreshAllLines()
        {
            for(int i = 0; i < document.GetMaxLine() + 1; i++)
                refreshedLines.Add(i);
            fullClearNext = true;
        }

        public static void RedrawLines()
        {
            //redraw lines that have changed
            foreach(int lineIndex in refreshedLines)
            {
                if(lineIndex <= document.GetMaxLine())
                    document.PrintLine(lineIndex);
                else
                    Printer.ClearLine(lineIndex - document.GetScrollY());
            }
            refreshedLines.Clear(); //clear for next time
        }

        public static void PrintFooter()
        {
            //generate footer string
            String defaultFooterContent = "maple (" + (docCursor.dX + 1) + ", " + (docCursor.dY + 1) + ") - " + document.GetFilePath();

            if(!CommandLine.HasOutput())
            {
                if(Input.GetInputTarget() == Input.InputTarget.Document) //render default footer
                    Printer.DrawFooter(defaultFooterContent, foregroundColor: Styler.accentColor, backgroundColor: ConsoleColor.Black);
                else if(Input.GetInputTarget() == Input.InputTarget.Command) //render input footer
                    Printer.DrawFooter("maple: " + CommandLine.GetText(), backgroundColor: Styler.cmdinColor);
            }
            else //render output footer
                Printer.DrawFooter("maple: \"" + CommandLine.GetOutput() + "\"", foregroundColor: Styler.cmdoutColor, backgroundColor: ConsoleColor.Black);
        }

    }
}
