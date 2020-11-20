using System;
using System.Collections.Generic;

namespace maple
{
    static class Editor
    {
        static Cursor cmdCursor;
        static DocumentCursor docCursor;
        static List<int> refreshedLines = new List<int>();
        static bool fullClearNext = false;

        public static void Initialize(String filename)
        {
            //create cursor
            cmdCursor = new Cursor(0, 0);

            cmdCursor.contentOffsetX = 7;
            cmdCursor.contentOffsetY = Cursor.maxScreenY;

            //create doc cursor with document
            docCursor = new DocumentCursor(filename, 0, 0);
            docCursor.CalculateGutterWidth();
            docCursor.Move(0, 0);
            docCursor.ApplyPosition();

            //render initial footer
            Printer.DrawFooter("maple", foregroundColor: Styler.accentColor, backgroundColor: ConsoleColor.Black);
            docCursor.Move(docCursor.dX, docCursor.dY); //reset to original position

            //prep for initial full-draw
            RefreshAllLines();

            //full-draw all lines for initial render
            Console.Clear();
            RedrawLines();
            fullClearNext = false;
        }

        public static void BeginInputLoop()
        {
            while(true)
                InputLoop();
        }

        static void InputLoop()
        {
            //render footer
            PrintFooter();

            //set actual cursor position
            GetActiveCursor().ApplyPosition();

            //accept input
            Input.AcceptInput(Console.ReadKey());

            //redraw lines that have changed
            if(fullClearNext)
            {
                Console.Clear();
                RedrawLines();
                fullClearNext = false; //don't do it again until told
            }
            else
                RedrawLines();

            //render footer again
            PrintFooter();

            //apply user cursor position
            if(Input.GetInputTarget() == Input.InputTarget.Document)
                docCursor.Move(docCursor.dX, docCursor.dY);
            else if(Input.GetInputTarget() == Input.InputTarget.Command)
                cmdCursor.Move(cmdCursor.dX, cmdCursor.dY);
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
        public static Document GetCurrentDoc() { return docCursor.GetDocument(); }
        public static Cursor GetCommandCursor() { return cmdCursor; }

        public static void RefreshLine(int lineIndex)
        {
            refreshedLines.Add(lineIndex);
        }

        public static void RefreshAllLines()
        {
            for(int i = 0; i < GetCurrentDoc().GetMaxLine() + 1; i++)
                refreshedLines.Add(i);
            fullClearNext = true;
        }

        public static void RedrawLines()
        {
            //redraw lines that have changed
            foreach(int lineIndex in refreshedLines)
            {
                if(lineIndex <= GetCurrentDoc().GetMaxLine())
                    GetCurrentDoc().PrintLine(lineIndex);
                else
                    Printer.ClearLine(lineIndex - GetCurrentDoc().GetScrollY());
            }
            refreshedLines.Clear(); //clear for next time
        }

        public static void PrintFooter()
        {
            //generate footer string
            String defaultFooterContent = "maple (" + (docCursor.dX + 1) + ", " + (docCursor.dY + 1) + ") - " + GetCurrentDoc().GetFilePath();

            if(!CommandLine.HasOutput())
            {
                if(Input.GetInputTarget() == Input.InputTarget.Document) //render default footer
                    Printer.DrawFooter(defaultFooterContent, foregroundColor: Styler.accentColor, backgroundColor: ConsoleColor.Black);
                else if(Input.GetInputTarget() == Input.InputTarget.Command) //render input footer
                    Printer.DrawFooter("maple: " + CommandLine.GetText(), backgroundColor: Styler.cmdinColor);
            }
            else //render output footer
                Printer.DrawFooter("maple " + CommandLine.GetOutput(), foregroundColor: Styler.cmdoutColor, backgroundColor: ConsoleColor.Black);
        }

    }
}