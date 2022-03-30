using System;

namespace maple
{
    static class Footer
    {
        static int dynamicFooterStartX = 0;

        /// <summary>
        /// Print the footer (prints command line input if user is accessing cli).
        /// </summary>
        public static void PrintFooter()
        {
            DocumentCursor c = Editor.DocCursor;

            //generate footer string
            if (Input.CurrentTarget == Input.InputTarget.Document)
            {
                // Draw footer content
                //draw piece by piece
                Printer.ClearLine(Cursor.MaxScreenY);
                string vanityString = String.Format("{0} ", Styler.VanityFooter);
                string filepathString = String.Format("{0} ", c.Doc.Filepath.TrimEnd());
                Printer.WriteToFooter(vanityString, 0, Styler.AccentColor, ConsoleColor.Black); //write vanity prefix
                Printer.WriteToFooter(filepathString, -1, Styler.TextColor, ConsoleColor.Black); //write doc name
                dynamicFooterStartX = vanityString.Length + filepathString.Length;

                Printer.WriteToFooter(String.Format("ln {0} col {1} ", (c.DY + 1), (c.DX + 1)), dynamicFooterStartX, Styler.AccentColor, ConsoleColor.Black); //writer cursor position
                if (c.Doc.HasSelection()) //write selection bounds (if has selection)
                {
                    Printer.WriteToFooter(String.Format("{0},{1} - {2},{3} ", (c.Doc.SelectInY + 1), (c.Doc.SelectInX + 1),
                        (c.Doc.SelectOutY + 1), (c.Doc.SelectOutX + 1)),
                        -1, Styler.SelectionColor, ConsoleColor.Black);
                }
                else if (c.Doc.HasSelectionStart()) //write selection in as reminder
                {
                    Printer.WriteToFooter(String.Format("{0},{1} ...", (c.Doc.SelectInY + 1), (c.Doc.SelectInX + 1)),
                        -1, Styler.SelectionColor, ConsoleColor.Black);
                }

                if (Input.ReadOnly)
                {
                    Printer.WriteToFooter("[readonly] ", -1, Styler.AccentColor);
                }
                
                Printer.ClearRight();
                Printer.ApplyBuffer();
            }
            else if (Input.CurrentTarget == Input.InputTarget.Command)
            {
                Printer.ClearLine(Cursor.MaxScreenY);
                Printer.WriteToFooter("maple: ", x: 0, foregroundColor: Styler.AccentColor);
                if (Settings.CliNoHighlight)
                {
                    Printer.WriteToFooter(CommandLine.InputText, x: Styler.VanityFooter.Length + 2, Styler.CliInputDefaultColor);
                }
                else
                {
                    Token[] cliTokens = Lexer.TokenizeCommandLine(CommandLine.InputText);
                    Printer.MoveCursor(Styler.VanityFooter.Length + 2, Cursor.MaxScreenY);
                    
                    for (int i = 0; i < cliTokens.Length; i++)
                    {
                        Printer.WriteToFooter(cliTokens[i].Text, foregroundColor: cliTokens[i].Color);
                    }
                }
            }

            // Draw output text
            if (CommandLine.HasOutput)
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

                if (Settings.ColorOutputBackground)
                {
                    Printer.DrawFooter(CommandLine.OutputText, foregroundColor: ConsoleColor.Black, backgroundColor: outputColor, yOffset: 1);
                }
                else
                {
                    Printer.DrawFooter(CommandLine.OutputText, foregroundColor: outputColor, backgroundColor: ConsoleColor.Black, yOffset: 1);
                }
            }

            Printer.ApplyBuffer();
        }
    }
}