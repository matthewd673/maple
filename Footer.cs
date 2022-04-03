using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace maple
{
    static class Footer
    {
        static int dynamicFooterStartX = 0;

        public static int FooterHeight { get; private set; }= 1;
        static bool refreshOutputNext = false;

        static string commandPrompt = "maple> ";

        public static string Separator { get; set; } = " - ";
        static string formatString { get; set; }
        static List<FooterToken> tokens = new();

        public enum FooterTokenType
        {
            None,
            Separator,
            Vanity,
            Filepath,
            LnCol,
            Selection,
            ReadonlyIndicator,
        }

        struct FooterToken
        {
            public FooterTokenType TType { get; set; }
            public string Text { get; set; }
        }

        public static void SetFormatString(string formatString)
        {
            Footer.formatString = formatString;
            tokens = TokenizeFooter();
        }

        static string GetFooterTokenText(FooterTokenType ttype)
        {
            switch (ttype)
            {
                case FooterTokenType.None:
                    return "";
                case FooterTokenType.Vanity:
                    return Styler.VanityFooter;
                case FooterTokenType.Filepath:
                    return Editor.CurrentDoc.Filepath.TrimEnd();
                case FooterTokenType.LnCol:
                    return String.Format("ln {0} col {1}", Editor.DocCursor.DY + 1, Editor.DocCursor.DX + 1);
                case FooterTokenType.Selection:
                    if (Editor.CurrentDoc.HasSelection()) //write selection bounds (if has selection)
                    {
                        return String.Format("{0},{1} - {2},{3} ", (Editor.CurrentDoc.SelectInY + 1), (Editor.CurrentDoc.SelectInX + 1),
                            (Editor.CurrentDoc.SelectOutY + 1), (Editor.CurrentDoc.SelectOutX + 1));
                    }
                    else if (Editor.CurrentDoc.HasSelectionStart()) //write selection in as reminder
                    {
                        return String.Format("{0},{1} …", (Editor.CurrentDoc.SelectInY + 1), (Editor.CurrentDoc.SelectInX + 1));
                    }
                    return "";
                case FooterTokenType.ReadonlyIndicator:
                    return Input.ReadOnly ? "[readonly]" : "";
                default:
                    return "";
            }
        }

        static bool GetFooterTokenHasOutput(FooterToken t)
        {
            switch (t.TType)
            {
                case FooterTokenType.Separator:
                    return !t.Equals("");
                case FooterTokenType.Selection:
                    return Editor.CurrentDoc.HasSelectionStart();
                case FooterTokenType.ReadonlyIndicator:
                    return Input.ReadOnly;
                default:
                    return true;
            }
        }

        static FooterToken FooterTokenFromString(string text)
        {
            FooterTokenType ttype = FooterTokenType.None;
            switch (text)
            {
                case "{-}":
                    ttype = FooterTokenType.Separator;
                    break;
                case "{vanity}":
                    ttype = FooterTokenType.Vanity;
                    break;
                case "{filepath}":
                    ttype = FooterTokenType.Filepath;
                    break;
                case "{lncol}":
                    ttype = FooterTokenType.LnCol;
                    break;
                case "{selection}":
                    ttype = FooterTokenType.Selection;
                    break;
                case "{readonly}":
                    ttype = FooterTokenType.ReadonlyIndicator;
                    break;
            }

            string outText = text;
            if (ttype != FooterTokenType.None && ttype != FooterTokenType.Separator)
            {
                text = GetFooterTokenText(ttype);
            }

            return new FooterToken()
            {
                TType = ttype,
                Text = text
            };
        }

        static List<FooterToken> TokenizeFooter()
        {
            List<FooterToken> tokens = new();
            Regex pattern = new Regex("{[a-z-]+}", RegexOptions.IgnoreCase);

            string text = formatString;
            while (text.Length > 0)
            {
                Match nextMatch = pattern.Match(text);
                // stop if there are no more matches
                if (!nextMatch.Success)
                {
                    tokens.Add(new FooterToken()
                                {
                                    TType = FooterTokenType.None,
                                    Text = text
                                });
                    break;
                }
                
                // next token matches, add
                if (nextMatch.Index == 0)
                {
                    Log.WriteDebug("matched!", "footer");
                    tokens.Add(FooterTokenFromString(nextMatch.Groups[0].Value));
                    text = text.Remove(nextMatch.Index, nextMatch.Value.Length);
                }
                else
                {
                    tokens.Add(new FooterToken()
                                {
                                    TType = FooterTokenType.None,
                                    Text = text
                                });
                    text = text.Remove(nextMatch.Index, nextMatch.Value.Length);
                }
            }

            return tokens;
        }

        /// <summary>
        /// Print the footer (prints command line input if user is accessing cli).
        /// </summary>
        public static void PrintFooter()
        {
            Printer.ClearLine(Cursor.MaxScreenY);
            if (Input.CurrentTarget == Input.InputTarget.Document)
            {
                // Get content from footer tokens
                Printer.MoveCursor(0, Cursor.MaxScreenY); //to manually set initial X
                for (int i = 0; i < tokens.Count; i++)
                {
                    FooterToken t = tokens[i];
                    if (t.TType != FooterTokenType.Separator)
                    {
                        Printer.WriteToFooter(GetFooterTokenText(t.TType));
                    }
                    else
                    {
                        bool contentPrevious = false;
                        bool contentNext = false;
                        if (i > 0)
                        {
                            for (int k = i - 1; k >= 0; k--)
                            {
                                if (GetFooterTokenHasOutput(tokens[k]))
                                {
                                    contentPrevious = true;
                                    break;
                                }
                            }
                            // if (!GetFooterTokenHasOutput(tokens[i - 1].TType))
                            // {
                            //     continue;
                            // }
                        }
                        if (i < tokens.Count - 1)
                        {
                            for (int k = i + 1; k < tokens.Count - 1; k++)
                            {
                                if (t.TType == FooterTokenType.Separator)
                                {
                                    t.Text = "";
                                }
                                if (GetFooterTokenHasOutput(tokens[k]))
                                {
                                    contentNext = true;
                                    break;
                                }
                            }
                            // if (!GetFooterTokenHasOutput(tokens[i + 1].TType))
                            // {
                            //     continue;
                            // }
                        }

                        if (contentPrevious && contentNext)
                        {
                            t.Text = Separator;
                            Printer.WriteToFooter(Separator);
                        }
                        else
                        {
                            t.Text = "";
                        }
                    }
                }

                Log.WriteDebug("footer token count: " + tokens.Count, "footer");

                Printer.ClearRight();
                Printer.ApplyBuffer();
            }
            else if (Input.CurrentTarget == Input.InputTarget.Command)
            {
                Printer.WriteToFooter(commandPrompt, x: 0, foregroundColor: Styler.AccentColor);
                if (Settings.CliNoHighlight)
                {
                    Printer.WriteToFooter(CommandLine.InputText, x: commandPrompt.Length, Styler.CliInputDefaultColor);
                }
                else
                {
                    Token[] cliTokens = Lexer.TokenizeCommandLine(CommandLine.InputText);
                    Printer.MoveCursor(commandPrompt.Length, Cursor.MaxScreenY);
                    
                    for (int i = 0; i < cliTokens.Length; i++)
                    {
                        Printer.WriteToFooter(cliTokens[i].Text, foregroundColor: cliTokens[i].Color);
                    }
                }
            }

            if (refreshOutputNext)
            {
                PrintOutputLine();
                refreshOutputNext = false;
            }

            Printer.ApplyBuffer();
        }

        public static void RefreshOutputLine()
        {
            FooterHeight = (CommandLine.HasOutput) ? 2 : 1;
            refreshOutputNext = true;
        }

        public static void PrintOutputLine()
        {
            // Draw output text
            if (CommandLine.HasOutput)
            {
                Printer.ClearLine(Cursor.MaxScreenY - 1);
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
        }

    }
}