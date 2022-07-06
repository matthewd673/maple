using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace maple
{
    public abstract class FooterBlock
    {
        [XmlIgnore]
        public short ColorAttribute { get; private set; }

        private string _color = "Gray";
        public string Color
        {
            get { return _color; }
            set
            {
                _color = value;
                ColorAttribute = Printer.GetAttributeFromColor(
                    Settings.StringToConsoleColor(value)
                );
            }
        }
        
        public abstract string GetText();
    }

    public class FooterBlockText : FooterBlock
    {
        public string Content { get; set; }

        public override string GetText()
        {
            return Content;
        }
    }

    public class FooterBlockSeparator : FooterBlock
    {
        public override string GetText()
        {
            return "-";
        }
    }

    public class FooterBlockFilepath : FooterBlock
    {
        public override string GetText()
        {
            return Editor.CurrentDoc.Filepath.Trim();
        }
    }

    public class FooterBlockLnCol : FooterBlock
    {
        public override string GetText()
        {
            return String.Format("ln {0} col {1}", Editor.DocCursor.DY + 1, Editor.DocCursor.DX + 1);
        }
    }

    public class FooterBlockSelection : FooterBlock
    {
        public override string GetText()
        {
            if (Editor.CurrentDoc.HasSelection())
            {
                return String.Format("{0},{1} - {2},{3}",
                    Editor.CurrentDoc.SelectInY + 1,
                    Editor.CurrentDoc.SelectInX + 1,
                    Editor.CurrentDoc.SelectOutY + 1,
                    Editor.CurrentDoc.SelectOutX + 1);
            }
            else if (Editor.CurrentDoc.HasSelectionStart())
            {
                return String.Format("{0},{1} …",
                    Editor.CurrentDoc.SelectInY + 1,
                    Editor.CurrentDoc.SelectInX + 1);
            }
            
            return "";
        }
    }

    public class FooterBlockReadOnly : FooterBlock
    {
        public override string GetText()
        {
            return (Input.ReadOnly) ? "[readonly]" : "";
        }
    }

    public class FooterLayout
    {
        [XmlArrayItem(ElementName = "Text", Type = typeof(FooterBlockText)),
         XmlArrayItem(ElementName = "Separator", Type = typeof(FooterBlockSeparator)),
         XmlArrayItem(ElementName = "Filepath", Type = typeof(FooterBlockFilepath)),
         XmlArrayItem(ElementName = "LnCol", Type = typeof(FooterBlockLnCol)),
         XmlArrayItem(ElementName = "Selection", Type = typeof(FooterBlockSelection)),
         XmlArrayItem(ElementName = "ReadOnly", Type = typeof(FooterBlockReadOnly))]
        public List<FooterBlock> BlockGroup { get; set; } = new();
    }

    static class Footer
    {
        public static int FooterHeight { get; private set; }= 1;
        static bool refreshOutputNext = false;

        public static string CommandPromptText { get; } = "maple $ ";

        static FooterLayout layout;

        public static void LoadFooterLayout()
        {
            FooterLayout loaded = Settings.ReadSettingsFile<FooterLayout>(Settings.Properties.FooterLayoutFile);
            if (loaded != null) layout = loaded;
        }

        /// <summary>
        /// Print the footer (prints command line input if user is accessing cli).
        /// </summary>
        public static void PrintFooter()
        {
            Printer.ClearLine(Printer.MaxScreenY);
            // DRAW DOCUMENT FOOTER
            if (Input.CurrentTarget == Input.InputTarget.Document)
            {
                // Get content from footer tokens
                Printer.MoveCursor(0, Printer.MaxScreenY); // to manually set initial X
                
                for (int i = 0; i < layout.BlockGroup.Count; i++)
                {
                    FooterBlock b = layout.BlockGroup[i];
                    string text = b.GetText();

                    // separators are special, check if they should display
                    if (b.GetType() == typeof(FooterBlockSeparator))
                    {
                        // never render a separator if it is first or last
                        if (i == 0 || i == layout.BlockGroup.Count - 1)
                        {
                            text = "";
                        }
                        else
                        {
                            // search backwards and only render if there is a non-empty block
                            // stop searching if a separator is seen
                            int lookI = i - 1;
                            text = "";
                            while (lookI >= 0)
                            {
                                if (layout.BlockGroup[lookI].GetType() == typeof(FooterBlockSeparator))
                                    break;
                                if (layout.BlockGroup[lookI].GetText().Length != 0)
                                {
                                    text = b.GetText();
                                    break;
                                }
                                lookI--;
                            }
                        }
                    }

                    Printer.WriteToFooter(text, attribute: b.ColorAttribute);
                }
                Printer.ClearRight();
            }
            // DRAW COMMAND FOOTER
            else if (Input.CurrentTarget == Input.InputTarget.Command)
            {
                Printer.WriteToFooter(CommandPromptText, x: 0, foregroundColor: Settings.Theme.AccentColor);
                if (Settings.Properties.CliNoHighlight)
                {
                    Printer.WriteToFooter(CommandLine.InputText, x: CommandPromptText.Length, Settings.Theme.CliInputDefaultColor);
                }
                else
                {
                    List<Token> cliTokens = Lexer.TokenizeCommandLine(CommandLine.InputText);
                    Printer.MoveCursor(CommandPromptText.Length, Printer.MaxScreenY);
                    
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
                ConsoleColor outputColor = Settings.Theme.CliOutputInfoColor;
                switch (CommandLine.OType)
                {
                    case CommandLine.OutputType.Error:
                        outputColor = Settings.Theme.CliOutputErrorColor;
                        break;
                    case CommandLine.OutputType.Success:
                        outputColor = Settings.Theme.CliOutputSuccessColor;
                        break;
                }

                if (Settings.Properties.ColorOutputBackground)
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