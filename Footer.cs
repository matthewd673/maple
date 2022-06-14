using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace maple
{
    static class Footer
    {
        public static int FooterHeight { get; private set; }= 1;
        static bool refreshOutputNext = false;

        static string commandPrompt = "maple> ";

        public static string Separator { get; set; } = " ";
        static string formatString { get; set; }
        static List<Token> tokens = new();

        public static void SetFormatString(string formatString)
        {
            Footer.formatString = formatString;
            tokens = TokenizeFooter();
        }

        static string GetFooterTokenText(Token token)
        {
            switch (token.TType)
            {
                case TokenType.None:
                    return token.Text;
                case TokenType.FooterVanity:
                    return Settings.VanityFooter;
                case TokenType.FooterFilepath:
                    switch (token.Annotation)
                    {
                        case "{filepath}":
                            return Editor.CurrentDoc.Filepath.TrimEnd();
                        case "{filename}":
                            string[] split;
                            if (Editor.CurrentDoc.Filepath.IndexOf("\\") > Editor.CurrentDoc.Filepath.IndexOf("/"))
                            {
                                split = Editor.CurrentDoc.Filepath.Split("\\");
                            }
                            else
                            {
                                split = Editor.CurrentDoc.Filepath.Split("/");
                            }

                            return split[split.Length - 1].Trim();
                        default:
                            return "";
                    }
                case TokenType.FooterLnCol:
                    return String.Format("ln {0} col {1}", Editor.DocCursor.DY + 1, Editor.DocCursor.DX + 1);
                case TokenType.FooterSelection:
                    if (Editor.CurrentDoc.HasSelection()) //write selection bounds (if has selection)
                    {
                        return String.Format("{0},{1} - {2},{3}", (Editor.CurrentDoc.SelectInY + 1), (Editor.CurrentDoc.SelectInX + 1),
                            (Editor.CurrentDoc.SelectOutY + 1), (Editor.CurrentDoc.SelectOutX + 1));
                    }
                    else if (Editor.CurrentDoc.HasSelectionStart()) //write selection in as reminder
                    {
                        return String.Format("{0},{1} …", (Editor.CurrentDoc.SelectInY + 1), (Editor.CurrentDoc.SelectInX + 1));
                    }
                    return "";
                case TokenType.FooterIndicator: //TODO: this only works for readonly indicator
                    switch (token.Annotation)
                    {
                        case "{readonly}":
                            return Input.ReadOnly ? "[readonly]" : "";
                        default:
                            return "";
                    }
                default:
                    return "";
            }
        }

        static bool GetFooterTokenHasOutput(Token t)
        {
            return GetFooterTokenText(t).Length != 0;
        }

        static List<Token> TokenizeFooter()
        {
            List<Token> tokens = new();
            Regex pattern = new Regex("{[a-z-]+}", RegexOptions.IgnoreCase); // TODO: just stumbled upon this regex, it looks wrong

            string text = formatString;
            while (text.Length > 0)
            {
                Match nextMatch = pattern.Match(text);
                // stop if there are no more matches
                if (!nextMatch.Success)
                {
                    tokens.Add(new Token(text, TokenType.None));
                    break;
                }
                
                // next token matches, add
                if (nextMatch.Index == 0)
                {
                    Token newToken = new Token("", Token.StringToTokenType(nextMatch.Value));
                    newToken.Annotation = nextMatch.Value;
                    
                    text = text.Remove(nextMatch.Index, nextMatch.Value.Length);
                    tokens.Add(newToken);
                }
                else
                {
                    tokens.Add(new Token(text, TokenType.None));
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
            Printer.ClearLine(Printer.MaxScreenY);
            if (Input.CurrentTarget == Input.InputTarget.Document)
            {
                // Get content from footer tokens
                Printer.MoveCursor(0, Printer.MaxScreenY); //to manually set initial X
                for (int i = 0; i < tokens.Count; i++)
                {
                    Token t = tokens[i];
                    if (t.TType == TokenType.FooterSeparator)
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
                        }
                        if (i < tokens.Count - 1)
                        {
                            for (int k = i + 1; k < tokens.Count; k++)
                            {
                                if (tokens[k].TType == TokenType.FooterSeparator)
                                {
                                    break;
                                }
                                if (GetFooterTokenHasOutput(tokens[k]))
                                {
                                    contentNext = true;
                                    break;
                                }
                            }
                        }

                        if (contentPrevious && contentNext)
                        {
                            t.Text = Separator;
                            Printer.WriteToFooter(Separator, attribute: t.ColorAttribute);
                        }
                        else
                        {
                            t.Text = "";
                        }
                    }
                    else
                    {
                        Printer.WriteToFooter(GetFooterTokenText(t), attribute: t.ColorAttribute);
                    }
                }

                Printer.ClearRight();
                // Printer.ApplyBuffer();
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
                    List<Token> cliTokens = Lexer.TokenizeCommandLine(CommandLine.InputText);
                    Printer.MoveCursor(commandPrompt.Length, Printer.MaxScreenY);
                    
                    for (int i = 0; i < cliTokens.Count; i++)
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

            // Printer.ApplyBuffer();
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
                Printer.ClearLine(Printer.MaxScreenY - 1);
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