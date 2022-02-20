using System;
using System.Collections.Generic;

namespace maple
{
    static class CommandLine
    {

        public static string[] CommandMasterList { get; } = new string[] {
            "help", "save", "load", "close", "cls", "top", "bot", "redraw",
            "goto", "selectin", "selectout", "deselect", "readonly", "syntax"
            };

        public static String InputText { get; private set; } = "";

        private static String _outputText = "";
        public static String OutputText
        {
            get { return _outputText; }
            set { _outputText = value; HasOutput = true; }
        }

        public static bool HasOutput { get; private set; } = false;

        public static bool AddText(int pos, String text)
        {
            if(pos < 0 || pos > InputText.Length)
                return false;
                
            InputText = InputText.Insert(pos, text);
            return true;
        }

        public static bool RemoveText(int pos)
        {
            if(pos < 0 || pos > InputText.Length - 1)
                return false;
            
            InputText = InputText.Remove(pos, 1);
            return true;
        }

        public static bool IsSafeCursorX(int x)
        {
            return (x >= 0 && x < InputText.Length);
        }

        public static void SetOutput(String text, String speaker)
        {
            OutputText = String.Format("[{0}]: {1}", speaker, text);
        }

        public static void ClearOutput()
        {
            OutputText = "";
            HasOutput = false;
        }

        public static void ClearInput()
        {
            InputText = "";
            Editor.CmdCursor.Move(0, 0);
        }

        public static void ExecuteInput()
        {
            if(HasOutput) //output is being displayed, reset for the future
                ClearOutput();

            CommandParser.CommandInfo commandInfo = CommandParser.Parse(InputText);

            String primaryCommand = commandInfo.PrimaryCommand;
            List<String> commandArgs = commandInfo.Args;
            List<String> commandSwitches = commandInfo.Switches;

            //swap out alias with actual primary command
            if (Settings.Aliases.ContainsKey(primaryCommand))
                primaryCommand = Settings.Aliases[primaryCommand];

            switch(primaryCommand)
            {
                case "help":
                    HelpCommand(commandArgs, commandSwitches);
                    break;
                case "save":
                    SaveCommand(commandArgs, commandSwitches);
                    break;
                case "load":
                    LoadCommand(commandArgs, commandSwitches);
                    break;
                case "close":
                    CloseCommand();
                    break;
                case "cls":
                    ClearCommand();
                    break;
                case "top":
                    TopCommand();
                    break;
                case "bot":
                    BotCommand();
                    break;
                case "redraw":
                    RedrawCommand();
                    break;
                case "goto":
                    GotoCommand(commandArgs, commandSwitches);
                    break;
                case "selectin":
                    SelectInCommand();
                    break;
                case "selectout":
                    SelectOutCommand();
                    break;
                case "deselect":
                    DeselectCommand();
                    break;
                case "readonly":
                    ReadonlyCommand();
                    break;
                case "syntax":
                    SyntaxCommand(commandArgs, commandSwitches);
                    break;
                default:
                    UnknownCommand();
                    break;
            }

            //empty input field and toggle back to editor
            InputText = "";
            Input.ToggleInputTarget();
        }

        static void HelpCommand(List<String> args, List<String> switches)
        {
            if (args.Count < 1)
            {
                SetOutput("'help [command]' or 'help all'", "help");
                return;
            }

            if (Settings.Aliases.ContainsKey(args[0]))
            {
                SetOutput(String.Format("{0}: alias for '{1}'", args[0], Settings.Aliases[args[0]]), "help");
                return;
            }

            switch (args[0])
            {
                case "help":
                    SetOutput("'help [command]' or 'help all'", "help");
                    break;
                case "all":
                    SetOutput("save, load, close, cls, top, bot, redraw, goto, selectin, selectout, readonly, syntax", "help");
                    break;
                case "save":
                    SetOutput("save [optional filename]: save document to filename", "help");
                    break;
                case "load":
                    SetOutput("load [filename]: load document at filename", "help");
                    break;
                case "close":
                    SetOutput("close: close maple without saving", "help");
                    break;
                case "cls":
                    SetOutput("cls: clear the last command output", "help");
                    break;
                case "top":
                    SetOutput("top: jump to the top of the document", "help");
                    break;
                case "bot":
                    SetOutput("bot: jump to the bottom of the document", "help");
                    break;
                case "redraw":
                    SetOutput("redraw: redraw the editor, usually fixes any rendering errors", "help");
                    break;
                case "goto":
                    SetOutput("goto [line]: jump to the specified line", "help");
                    break;
                case "selectin":
                    SetOutput("selectin: start selection", "help");
                    break;
                case "selectout":
                    SetOutput("selectout: end selection", "help");
                    break;
                case "deselect":
                    SetOutput("deselect: clear selection region bounds", "help");
                    break;
                case "readonly":
                    SetOutput("readonly: toggle readonly mode", "help");
                    break;
                case "syntax":
                    SetOutput("syntax [extension]: render the current file with the syntax rules defined for [extension] files", "help");
                    break;
                default:
                    UnknownCommand();
                    break;
            }
        }

        static void SaveCommand(List<String> args, List<String> switches)
        {
            String savePath = Editor.DocCursor.Doc.Filepath;
            if(args.Count > 0)
                savePath = args[0];
            savePath = savePath.Trim('\"');

            Editor.DocCursor.Doc.SaveDocument(savePath);

            String existingPath = Editor.DocCursor.Doc.Filepath;

            if(savePath != existingPath)
                SetOutput(String.Format("Copy of file saved to {0}", savePath), "save");
            else
                SetOutput(String.Format("File saved to {0}", savePath), "save");
        }

        static void LoadCommand(List<String> args, List<String> switches)
        {
            if(args.Count < 1)
            {
                SetOutput("No filepath provided", "load");
                return;
            }

            String filepath = args[0];
            //trim quotes if it was in a block
            if(filepath.StartsWith("\""))
                filepath = filepath.Trim('\"');

            //initialize new editor
            Editor.Initialize(filepath);
        }

        static void CloseCommand()
        {
            Program.Close();
        }

        static void ClearCommand()
        {
            ClearOutput();
        }

        static void TopCommand()
        {
            Editor.DocCursor.Move(Editor.DocCursor.DX, 0);
        }

        static void BotCommand()
        {
            Editor.DocCursor.Move(Editor.DocCursor.DX, Editor.DocCursor.Doc.GetMaxLine());
        }

        static void RedrawCommand()
        {
            Printer.Resize();
            Cursor.CalculateCursorBounds();
            Editor.DocCursor.Doc.CalculateScrollIncrement();
            Editor.DocCursor.Move(Editor.DocCursor.DX, Editor.DocCursor.DY);
            Editor.DocCursor.LockToScreenConstraints();
            Editor.RefreshAllLines();
            Console.Clear();
            Editor.RedrawLines();
        }

        static void SelectInCommand()
        {
            Editor.GetCurrentDoc().MarkSelectionIn(Editor.DocCursor.DX, Editor.DocCursor.DY);
            if (Editor.GetCurrentDoc().HasSelection()) //only refresh if there is a complete selection
                Editor.RefreshAllLines();
        }

        static void SelectOutCommand()
        {
            Editor.GetCurrentDoc().MarkSelectionOut(Editor.DocCursor.DX, Editor.DocCursor.DY);
            if (Editor.GetCurrentDoc().HasSelection()) //only refresh if there is a complete selection
                Editor.RefreshAllLines();
        }

        static void DeselectCommand()
        {
            bool hadSelection = Editor.GetCurrentDoc().HasSelection();
            Editor.GetCurrentDoc().Deselect();
            if (hadSelection)
                Editor.RefreshAllLines();
        }

        static void GotoCommand(List<String> args, List<String> switches)
        {
            if (args.Count < 1)
            {
                SetOutput("No line number provided", "goto");
                return;
            }

            int l = 0;
            if (int.TryParse(args[0], out l))
            {
                if (l > Editor.DocCursor.Doc.GetMaxLine() + 1 || l < 0)
                    SetOutput(String.Format("Invalid line number, must be >= 1 and <= {0}", (Editor.DocCursor.Doc.GetMaxLine() + 1)), "goto");
                else
                    Editor.DocCursor.Move(0, l - 1);
            }
            else
                SetOutput("Invalid line number, must be an integer", "goto");
        }

        static void ReadonlyCommand()
        {
            Input.ReadOnly = !Input.ReadOnly;
            if (Input.ReadOnly)
                SetOutput("Editor is now readonly, use 'readonly' command to toggle", "readonly");
            else
                SetOutput("Editor is no longer readonly", "readonly");
        }

        static void SyntaxCommand(List<String> args, List<String> switches)
        {
            if (args.Count < 1)
            {
                SetOutput("No file extension provided", "syntax");
                return;
            }

            Lexer.LoadSyntax(Settings.SyntaxDirectory + args[0] + ".xml");

            Editor.GetCurrentDoc().ForceReTokenize();
            Console.Clear();
            Editor.RefreshAllLines();
            Editor.RedrawLines();
        }

        static void UnknownCommand()
        {
            SetOutput("Unknown command, try 'help all' or update the alias file", "error");
        }

    }
}
