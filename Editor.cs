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
        
        static int dynamicFooterStartX = 0;

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

            //load footer lexer
            Log.Write("Building command line input lexer rules", "editor");
            Lexer.LoadCommandLineSyntax();
            //render initial footer
            Printer.DrawFooter("maple", foregroundColor: Styler.AccentColor, backgroundColor: ConsoleColor.Black);
            DocCursor.Move(DocCursor.DX, DocCursor.DY); //reset to original position

            //prep for initial full-draw
            Log.Write("Preparing for initial full-draw", "editor");
            RefreshAllLines();

            //full-draw all lines for initial render
            Log.Write("Full-drawing", "editor");
            Printer.Clear();
            RedrawLines();
            fullClearNext = false;
        }

        public static void BeginInputLoop()
        {
            PrintFooter();
            while(true)
                InputLoop();
        }

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
                RedrawLines();
                // FullRefreshFooter(); //since the console was cleared
            }
            else
                RedrawLines();

            //render footer
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
        public static void RedrawLines()
        {
            //redraw lines that have changed
            foreach(int lineIndex in refreshedLines)
            {
                if(lineIndex <= CurrentDoc.GetMaxLine())
                    CurrentDoc.PrintLine(lineIndex);
                else
                    Printer.ClearLine(lineIndex - CurrentDoc.ScrollY);
            }

            Printer.ApplyBuffer();
            
            refreshedLines.Clear(); //clear for next time
            fullClearNext = false; //don't full clear again unless told
        }

        public static void PrintFooter()
        {
            //generate footer string
            if (Input.CurrentTarget == Input.InputTarget.Document)
            {
                if (!CommandLine.HasOutput)
                {
                    //draw piece by piece
                    Printer.ClearFooter();
                    string vanityString = String.Format("{0} ", Styler.VanityFooter);
                    string filepathString = String.Format("{0} ", CurrentDoc.Filepath.TrimEnd());
                    Printer.WriteToFooter(vanityString, 0, Styler.AccentColor, ConsoleColor.Black); //write vanity prefix
                    Printer.WriteToFooter(filepathString, -1, Styler.TextColor, ConsoleColor.Black); //write doc name
                    dynamicFooterStartX = vanityString.Length + filepathString.Length;

                    Printer.WriteToFooter(String.Format("ln {0} col {1} ", (DocCursor.DY + 1), (DocCursor.DX + 1)), dynamicFooterStartX, Styler.AccentColor, ConsoleColor.Black); //writer cursor position
                    if (CurrentDoc.HasSelection()) //write selection bounds (if has selection)
                        Printer.WriteToFooter(String.Format("{0},{1} - {2},{3} ", (CurrentDoc.SelectInY + 1), (CurrentDoc.SelectInX + 1),
                            (CurrentDoc.SelectOutY + 1), (CurrentDoc.SelectOutX + 1)),
                            -1, Styler.SelectionColor, ConsoleColor.Black);
                    else if (CurrentDoc.HasSelectionStart()) //write selection in as reminder
                        Printer.WriteToFooter(String.Format("{0},{1} ...", (CurrentDoc.SelectInY + 1), (CurrentDoc.SelectInX + 1)),
                            -1, Styler.SelectionColor, ConsoleColor.Black);

                    if (Input.ReadOnly)
                        Printer.WriteToFooter("[readonly] ", -1, Styler.AccentColor);
                    
                    Printer.ClearRight();
                    Printer.ApplyBuffer();
                }
                else
                {
                    ConsoleColor outputColor = Styler.CliOutputInfoColor;
                    switch (CommandLine.OType)
                    {
                        case CommandLine.OutputType.Error:
                            outputColor = Styler.CliOutputErrorColor;
                            break;
                        case CommandLine.OutputType.Success:
                            outputColor = Styler.CliOutputSuccessColor;
                            break;
                    }
                    Printer.DrawFooter(CommandLine.OutputText, foregroundColor: outputColor, backgroundColor: ConsoleColor.Black);
                }
            }
            else if (Input.CurrentTarget == Input.InputTarget.Command)
            {
                Printer.ClearFooter();
                Printer.WriteToFooter("maple: ", x: 0, foregroundColor: Styler.AccentColor);
                if (Settings.CliNoHighlight)
                    Printer.WriteToFooter(CommandLine.InputText, x: Styler.VanityFooter.Length + 2, Styler.CliInputDefaultColor);
                else
                {
                    Token[] cliTokens = Lexer.TokenizeCommandLine(CommandLine.InputText);
                    Printer.MoveCursor(Styler.VanityFooter.Length + 2, Cursor.MaxScreenY);
                    
                    for (int i = 0; i < cliTokens.Length; i++)
                        Printer.WriteToFooter(cliTokens[i].Text, foregroundColor: cliTokens[i].Color);                        
                }
            }

            Printer.ApplyBuffer();
        }
    }
}
