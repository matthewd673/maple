using System;
using System.Collections.Generic;

namespace maple
{
    class Program
    {

        static Cursor docCursor;
        static Cursor cmdCursor;
        static Document document;

        static List<int> refreshedLines = new List<int>();
        static bool fullClearNext = false;

        static void Main(string[] args)
        {
            PrepareWindow();
            
            //create cursor
            docCursor = new Cursor(0, 0);
            cmdCursor = new Cursor(0, 0);

            cmdCursor.contentOffsetX = 7;
            cmdCursor.contentOffsetY = Cursor.maxScreenY;

            //prepare styler
            Styler.LoadMapleTheme();

            //load file
            if(args.Length > 0)
            {
                document = new Document(args[0], loadStyleInfo: true);
                document.CalculateGutterWidth();
                document.PrintFileLines();
                docCursor.MoveCursor(docCursor.dX, docCursor.dY);
            }

            //render initial footer
            Printer.DrawFooter("maple", foregroundColor: Styler.accentColor, backgroundColor: ConsoleColor.Black);
            //reset to initial position
            docCursor.MoveCursor(docCursor.dX, docCursor.dY);

            while(true)
            {
                //render initial footer
                PrintFooter();

                //set actual cursor position
                GetCursor().MoveCursor();

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

                //reset to user cursor position
                if(Input.GetInputTarget() == Input.InputTarget.Document)
                    docCursor.MoveCursor(docCursor.dX, docCursor.dY);
                else if(Input.GetInputTarget() == Input.InputTarget.Command)
                    cmdCursor.ForceDocumentPosition(cmdCursor.dX, cmdCursor.dY);
            }

        }

        static void PrepareWindow()
        {
            Console.Clear();
            Console.Title = "maple";
            Printer.Resize();
        }

        public static Cursor GetCursor()
        {
            if(Input.GetInputTarget() == Input.InputTarget.Document)
                return docCursor;
            else if(Input.GetInputTarget() == Input.InputTarget.Command)
                return cmdCursor;
            
            return docCursor;
        }

        public static Cursor GetDocumentCursor()
        {
            return docCursor;
        }

        public static Document GetDocument()
        {
            return document;
        }

        public static void Close()
        {
            Console.Clear();
            Console.ForegroundColor = Styler.accentColor;
            Console.WriteLine("maple session ({0})", GetDocument().GetFilePath());
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
            String defaultFooterContent = "maple (" + docCursor.dX + ", " + docCursor.dY + ") - " + document.GetFilePath();

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
