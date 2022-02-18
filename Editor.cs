using System;
using System.Collections.Generic;

namespace maple
{
    static class Editor
    {
        public static Cursor CmdCursor { get; private set; }
        public static DocumentCursor DocCursor { get; private set; }
        static List<int> refreshedLines = new List<int>();
        static bool fullClearNext = false;

        public static void Initialize(String filename)
        {
            Log.Write("Performing editor initialization", "editor");
            //create cursor
            CmdCursor = new Cursor(0, 0);

            CmdCursor.ContentOffsetX = Styler.VanityFooter.Length + 2;
            CmdCursor.ContentOffsetY = Cursor.MaxScreenY;

            //create doc cursor with document
            Log.Write("Creating document cursor", "editor");
            DocCursor = new DocumentCursor(filename, 0, 0);
            DocCursor.CalculateGutterWidth();
            DocCursor.Move(0, 0);
            DocCursor.ApplyPosition();

            //render initial footer
            Printer.DrawFooter("maple", foregroundColor: Styler.AccentColor, backgroundColor: ConsoleColor.Black);
            DocCursor.Move(DocCursor.DX, DocCursor.DY); //reset to original position

            //prep for initial full-draw
            Log.Write("Preparing for initial full-draw", "editor");
            RefreshAllLines();

            //full-draw all lines for initial render
            Log.Write("Full-drawing", "editor");
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

            //force line refresh each time if debugging tokens
            if(Settings.DebugTokens)
                RefreshLine(DocCursor.DY);

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
            if(Input.CurrentTarget == Input.InputTarget.Document)
                DocCursor.Move(DocCursor.DX, DocCursor.DY);
            else if(Input.CurrentTarget == Input.InputTarget.Command)
                CmdCursor.Move(CmdCursor.DX, CmdCursor.DY);
        }

        public static Cursor GetActiveCursor()
        {
            if(Input.CurrentTarget == Input.InputTarget.Document)
                return DocCursor;
            else if(Input.CurrentTarget == Input.InputTarget.Command)
                return CmdCursor;
            
            return DocCursor;
        }

        public static Document GetCurrentDoc() { return DocCursor.Doc; }

        public static void RefreshLine(int lineIndex)
        {
            refreshedLines.Add(lineIndex);
        }

        /// <summary>
        /// Mark all document lines to be refreshed when the next redraw is called.
        /// </summary>
        public static void RefreshAllLines()
        {
            for(int i = 0; i < GetCurrentDoc().GetMaxLine() + 1; i++)
                refreshedLines.Add(i);
            fullClearNext = true;
        }

        /// <summary>
        /// Redraw all lines (within window bounds) that are marked for refresh.
        /// </summary>
        public static void RedrawLines()
        {
            //redraw lines that have changed
            foreach(int lineIndex in refreshedLines)
            {
                if(lineIndex <= GetCurrentDoc().GetMaxLine())
                    GetCurrentDoc().PrintLine(lineIndex);
                else
                    Printer.ClearLine(lineIndex - GetCurrentDoc().ScrollY);
            }
            refreshedLines.Clear(); //clear for next time
        }

        public static void PrintFooter()
        {
            //generate footer string
            if(!CommandLine.HasOutput) //print normal footer if there isn't any command output
            {
                if (Input.CurrentTarget == Input.InputTarget.Document) //render default footer
                {
                    //draw piece by piece
                    Printer.ClearFooter(ConsoleColor.Black);
                    Printer.WriteToFooter(String.Format("{0} ", Styler.VanityFooter), 0, Styler.AccentColor, ConsoleColor.Black); //write vanity prefix
                    Printer.WriteToFooter(String.Format("{0} ", GetCurrentDoc().Filepath.TrimEnd()), -1, Styler.TextColor, ConsoleColor.Black); //write doc name
                    Printer.WriteToFooter(String.Format("ln {0} col {1} ", (DocCursor.DY + 1), (DocCursor.DX + 1)), -1, Styler.AccentColor, ConsoleColor.Black); //writer cursor position
                    if (GetCurrentDoc().HasSelection()) //write selection bounds (if has selection)
                        Printer.WriteToFooter(String.Format("{0},{1} - {2},{3} ", (GetCurrentDoc().GetSelectionInX() + 1), (GetCurrentDoc().GetSelectionInY() + 1),
                            (GetCurrentDoc().GetSelectionOutX() + 1), (GetCurrentDoc().GetSelectionOutY() + 1)),
                            -1, Styler.SelectionColor, ConsoleColor.Black);
                    else if (GetCurrentDoc().HasSelectionStart()) //write selection in as reminder
                        Printer.WriteToFooter(String.Format("{0},{1} ...", (GetCurrentDoc().GetSelectionInX() + 1), (GetCurrentDoc().GetSelectionInY() + 1)),
                            -1, Styler.SelectionColor, ConsoleColor.Black);
                    //Printer.DrawFooter(defaultFooterContent, foregroundColor: Styler.accentColor, backgroundColor: ConsoleColor.Black);
                }
                else if (Input.CurrentTarget == Input.InputTarget.Command) //render input footer
                    Printer.DrawFooter(String.Format("maple: {0}", CommandLine.InputText), foregroundColor: Styler.CmdInColor, backgroundColor: ConsoleColor.Black);
            }
            else //render output footer
                Printer.DrawFooter(CommandLine.OutputText, foregroundColor: Styler.CmdOutColor, backgroundColor: ConsoleColor.Black);
        }
    }
}
