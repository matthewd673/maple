using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

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

        public static bool Busy { get; private set; }

        /// <summary>
        /// Load initial cursor properties and perform first draw.
        /// </summary>
        /// <param name="filename"></param>
        public static void Initialize(String filename)
        {
            Log.Write("Performing editor initialization", "editor");

            //create cursor
            CmdCursor = new Cursor(0, 0);
            CmdCursor.ContentOffsetX = Footer.CommandPromptText.Length;
            CmdCursor.ContentOffsetY = Int32.MaxValue;

            //create doc cursor with document
            Log.Write("Creating document cursor", "editor");
            DocCursor = new DocumentCursor(filename, 0, 0);
            DocCursor.UpdateGutterOffset();
            DocCursor.Move(0, 0);
            DocCursor.ApplyPosition();

            Log.Write("Loading footer layout", "editor");
            Footer.LoadFooterLayout();

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
            Printer.ApplyBuffer();
            Printer.StartInputThread();
            while(true)
            {
                InputLoop();
            }
        }

        /// <summary>
        /// Accept input and render any updates as appropriate.
        /// </summary>
        static void InputLoop()
        {
            // set actual cursor position
            GetActiveCursor().ApplyPosition();

            // accept input
            while (Printer.KeyEventQueueLength == 0)
            {
                // auto-resize buffer, if enabled
                if (Printer.WindowBuffserSizeEventCount > 0 && Settings.Properties.AutoResize)
                {
                    RedrawWindow();
                    break;
                }

                // check for changes to file
                DateTime fileModifiedTime = File.GetLastWriteTime(CurrentDoc.Filepath);
                if (fileModifiedTime.ToFileTime() != Editor.CurrentDoc.LastModifiedTime)
                {
                    // file changed, apply update
                    if (Editor.CurrentDoc.LastModifiedTime != 0)
                    {
                        CommandLine.SetOutput("File has been modified externally, use \"load\" to reload", "document");
                        Printer.ApplyBuffer();
                    }
                    Editor.CurrentDoc.LastModifiedTime = fileModifiedTime.ToFileTime();
                }

                Thread.Sleep(10); // TODO: there's a better way to write this threading
            }
            lock (Printer.KeyEventQueue)
            {
                foreach (ConsoleKeyInfo k in Printer.KeyEventQueue)
                {
                    Input.AcceptInput(k);
                }
                Printer.KeyEventQueue.Clear();
                Printer.KeyEventQueueLength = 0;
            }

            // force line refresh each time if debugging tokens
            if(Settings.Properties.DebugTokens)
                RefreshLine(DocCursor.DY);

            // redraw lines that have changed
            if(fullClearNext)
            {
                Printer.Clear();
                Footer.RefreshOutputLine();
            }

            DrawLines();
            Footer.PrintFooter();
            Printer.ApplyBuffer();
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
                    if (lineIndex == CurrentDoc.GetMaxLine())
                    {
                        int lineScreenIndex = lineIndex - CurrentDoc.ScrollY;
                        if (lineScreenIndex < Printer.MaxScreenY - Footer.FooterHeight)
                        {
                            Printer.ClearLine(lineScreenIndex + 1);
                        }
                    }
                }
                else // TODO: is this doing anything?
                {
                    int lineScreenIndex = lineIndex - CurrentDoc.ScrollY;
                    if (lineScreenIndex <= Printer.MaxScreenY - Footer.FooterHeight)
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
            Footer.RefreshOutputLine();
            Printer.Clear();
            DrawLines();
        }
    }
}
