using System;
using System.Collections.Generic;

namespace maple
{
    static class Editor
    {
        public static Cursor CmdCursor { get; private set; }
        public static DocumentCursor DocCursor { get; private set; }
        public static Document CurrentDoc { get { return DocCursor.Doc; }}
        static List<int> refreshedLines = new List<int>();
        static bool fullClearNext = false;
        
        public static string ClipboardContents { get; set; } = "";

        /// <summary>
        /// Load initial cursor properties and perform first draw.
        /// </summary>
        /// <param name="filename"></param>
        public static void Initialize(String filename)
        {
            Log.Write("Performing editor initialization", "editor");
            //create cursor
            CmdCursor = new Cursor(0, 0);
            CmdCursor.ContentOffsetX = 7; //hardcoded for "maple: "
            CmdCursor.ContentOffsetY = Int32.MaxValue;

            //create doc cursor with document
            Log.Write("Creating document cursor", "editor");
            DocCursor = new DocumentCursor(filename, 0, 0);
            DocCursor.UpdateGutterOffset();
            DocCursor.Move(0, 0);
            DocCursor.ApplyPosition();

            Footer.SetFormatString("{vanity}{-}{filepath}{-}{lncol}{-}{selection}{-}{readonly}");

            //load footer lexer
            Log.Write("Building command line input lexer rules", "editor");
            Lexer.LoadCommandLineSyntax();

            //prep for initial full-draw
            Log.Write("Preparing for initial full-draw", "editor");
            RefreshAllLines();

            //full-draw all lines for initial render
            Log.Write("Full-drawing", "editor");
            Printer.Clear();
            DrawLines();
            Printer.ApplyBuffer();
            fullClearNext = false;
        }

        /// <summary>
        /// Enter the input loop.
        /// </summary>
        public static void BeginInputLoop()
        {
            Footer.PrintFooter();
            while(true)
                InputLoop();
        }

        /// <summary>
        /// Accept input and render any updates as appropriate.
        /// </summary>
        static void InputLoop()
        {
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
                Printer.Clear();
                Footer.RefreshOutputLine();
                DrawLines();
            }
            else
                DrawLines();

            //render footer
            Footer.PrintFooter();
        }

        /// <summary>
        /// Returns the currently active cursor (depending on if input is toggled to Document or cli).
        /// </summary>
        /// <returns>The active cursor (this is a DocumentCursor, if the Document is currently active).</returns>
        public static Cursor GetActiveCursor()
        {
            if(Input.CurrentTarget == Input.InputTarget.Document)
                return DocCursor;
            else if(Input.CurrentTarget == Input.InputTarget.Command)
                return CmdCursor;
            
            return DocCursor;
        }

        /// <summary>
        /// Mark a line to be updated on next draw (Document line index).
        /// </summary>
        /// <param name="lineIndex">The index of the line to update.</param>
        public static void RefreshLine(int lineIndex)
        {
            refreshedLines.Add(lineIndex);
        }

        /// <summary>
        /// Mark all document lines to be refreshed when the next redraw is called.
        /// </summary>
        public static void RefreshAllLines()
        {
            for(int i = 0; i <= CurrentDoc.GetMaxLine(); i++)
                refreshedLines.Add(i);
            fullClearNext = true;
        }

        /// <summary>
        /// Redraw all lines (within window bounds) that are marked for refresh.
        /// </summary>
        public static void DrawLines()
        {
            //redraw lines that have changed
            foreach(int lineIndex in refreshedLines)
            {
                if(lineIndex <= CurrentDoc.GetMaxLine())
                {
                    CurrentDoc.PrintLine(lineIndex);
                }
                else
                {
                    int lineScreenIndex = lineIndex - CurrentDoc.ScrollY;
                    if (lineScreenIndex <= Cursor.MaxScreenY - Footer.FooterHeight)
                    {
                        Printer.ClearLine(lineScreenIndex);
                    }
                }
            }
            
            refreshedLines.Clear(); //clear for next time
            fullClearNext = false; //don't full clear again unless told
        }

        /// <summary>
        /// Re-initialize the Printer and fully redraw window contents.
        /// Very expensive, use only when something has changed / gone wrong with the buffer.
        /// </summary>
        public static void RedrawWindow()
        {
            Printer.Resize();
            DocCursor.Doc.CalculateScrollIncrement();
            DocCursor.Move(DocCursor.DX, DocCursor.DY);
            DocCursor.LockToScreenConstraints();
            Editor.RefreshAllLines();
            Printer.Clear();
            DrawLines();
        }
    }
}
