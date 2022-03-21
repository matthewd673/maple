using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace maple
{
    static class CommandLine
    {

        public enum OutputType
        {
            Info,
            Error,
            Success,
        }

        public static string[] CommandMasterList { get; } = new string[] {
            "help", "save", "load", "new", "close", "cls", "top", "bot",
            "redraw", "goto", "selectin", "selectout", "deselect", "readonly",
            "syntax", "alias", "url", "find"
            };

        public static string InputText { get; private set; } = "";

        private static string _outputText = "";
        public static string OutputText
        {
            get { return _outputText; }
            set { _outputText = value; HasOutput = true; }
        }
        public static OutputType OType = OutputType.Info;

        public static bool HasOutput { get; private set; } = false;

        public static List<string> CommandHistory { get; private set; } = new List<string>();
        public static int CommandHistoryIndex { get; set; } = -1;

        private static string lastFindSearch = "";
        private static int lastFindIndex = -1;
        private static bool lastFindLast = false;
        private static bool lastFindFirst = false;
        private static bool lastFindUpwards = false;
        private static bool lastFindCaseSensitive = false;

        public static bool AddText(int pos, string text)
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

        public static void SetOutput(string text, string speaker, OutputType oType = OutputType.Info)
        {
            OutputText = String.Format("[{0}]: {1}", speaker, text);
            OType = oType;
        }

        public static void ClearOutput()
        {
            OutputText = "";
            HasOutput = false;
            OType = OutputType.Info;
            Editor.PrintFooter();
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

            CommandHistoryIndex = -1; //reset command history index
            if (CommandHistory.Count == 0 || !CommandHistory[CommandHistory.Count - 1].Equals(InputText))
                CommandHistory.Insert(0, InputText);

            CommandParser.CommandInfo commandInfo = CommandParser.Parse(InputText);

            string primaryCommand = commandInfo.PrimaryCommand;
            List<string> commandArgs = commandInfo.Args;
            List<string> commandSwitches = commandInfo.Switches;

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
                case "new":
                    NewCommand(commandArgs, commandSwitches);
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
                case "alias":
                    AliasCommand(commandArgs, commandSwitches);
                    break;
                case "url":
                    UrlCommand();
                    break;
                case "find":
                    FindCommand(commandArgs, commandSwitches);
                    break;
                default:
                    UnknownCommand();
                    break;
            }

            //empty input field and toggle back to editor
            InputText = "";
            Input.ToggleInputTarget();
        }

        static void HelpCommand(List<string> args, List<string> switches)
        {
            if (args.Count < 1)
            {
                SetOutput("'help [command]' or 'help all'", "help");
                return;
            }

            if (Settings.Aliases.ContainsKey(args[0]))
            {
                SetOutput(String.Format("'{0}' is an alias for '{1}'", args[0], Settings.Aliases[args[0]]), "help");
                return;
            }

            switch (args[0])
            {
                case "help":
                    SetOutput("'help [command]' or 'help all'", "help");
                    break;
                case "all":
                    SetOutput("save, load, new, close, cls, top, bot, redraw, goto, find, selectin, selectout, readonly, syntax, alias, url", "help");
                    break;
                case "save":
                    SetOutput("save [optional filename]: save document to filename", "help");
                    break;
                case "load":
                    SetOutput("load [filename]: load document at filename", "help");
                    break;
                case "new":
                    SetOutput("new [filename]: create a new document at filename", "help");
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
                case "alias":
                    SetOutput("alias [command]: view all aliases for a given command", "help");
                    break;
                case "url":
                    SetOutput("url: if the cursor is currently hovered on a url, open it in the browser", "help");
                    break;
                case "find":
                    SetOutput("find [query] [switches]: find the next occurrence of the search phrase, starting from the top", "help");
                    break;
                default:
                    UnknownCommand();
                    break;
            }
        }

        static void SaveCommand(List<string> args, List<string> switches)
        {
            string savePath = Editor.DocCursor.Doc.Filepath;
            if(args.Count > 0)
                savePath = args[0];
            savePath = savePath.Trim('\"');

            Editor.DocCursor.Doc.SaveDocument(savePath);

            string existingPath = Editor.DocCursor.Doc.Filepath;

            if(savePath != existingPath)
                SetOutput(String.Format("Copy of file saved to {0}", savePath), "save");
            else
                SetOutput(String.Format("File saved to {0}", savePath), "save");
        }

        static void LoadCommand(List<string> args, List<string> switches)
        {
            if(args.Count < 1)
            {
                SetOutput("No filepath provided", "load", oType: OutputType.Error);
                return;
            }

            string filepath = args[0];
            //trim quotes if it was in a block
            if(filepath.StartsWith("\""))
                filepath = filepath.Trim('\"');

            //initialize new editor
            if (File.Exists(Document.ProcessFilepath(filepath)))
                Editor.Initialize(filepath);
            else
                SetOutput(String.Format("File '{0}' doesn't exist, use 'new' to create a new file", filepath), "load", OutputType.Error);
        }

        static void NewCommand(List<string> args, List<string> switches)
        {
            if (args.Count < 1)
            {
                SetOutput("No filepath provided", "new", oType: OutputType.Error);
                return;
            }

            string filepath = args[0];
            //trim quotes if it was in a block
            if(filepath.StartsWith("\""))
                filepath = filepath.Trim('\"');

            //initialize new editor
            if (File.Exists(Document.ProcessFilepath(filepath)))
                SetOutput(String.Format("File '{0}' already exists, use 'load' to load an existing file", filepath), "new", OutputType.Error);
            else
            {
                Editor.Initialize(filepath);
                SetOutput(String.Format("Created a new file at '{0}'", filepath), "new", OutputType.Success);
            }
        
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
            // Cursor.CalculateCursorBounds();
            Editor.DocCursor.Doc.CalculateScrollIncrement();
            Editor.DocCursor.Move(Editor.DocCursor.DX, Editor.DocCursor.DY);
            Editor.DocCursor.LockToScreenConstraints();
            Editor.RefreshAllLines();
            Printer.Clear();
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

        static void GotoCommand(List<string> args, List<string> switches)
        {
            if (args.Count < 1)
            {
                SetOutput("No line number provided", "goto", oType: OutputType.Error);
                return;
            }

            int l = 0;
            if (int.TryParse(args[0], out l))
            {
                if (l > Editor.DocCursor.Doc.GetMaxLine() + 1 || l < 0)
                    SetOutput(String.Format("Invalid line number, must be >= 1 and <= {0}", (Editor.DocCursor.Doc.GetMaxLine() + 1)), "goto", oType: OutputType.Error);
                else
                    Editor.DocCursor.Move(0, l - 1);
            }
            else
                SetOutput("Invalid line number, must be an integer", "goto", oType: OutputType.Error);
        }

        static void ReadonlyCommand()
        {
            Input.ReadOnly = !Input.ReadOnly;
            if (Input.ReadOnly)
                SetOutput("Editor is now readonly, use 'readonly' command to toggle", "readonly");
            else
                SetOutput("Editor is no longer readonly", "readonly");
        }

        static void SyntaxCommand(List<string> args, List<string> switches)
        {
            if (args.Count < 1)
            {
                SetOutput("No file extension provided", "syntax", oType: OutputType.Error);
                return;
            }

            Lexer.LoadSyntax(Settings.SyntaxDirectory + args[0] + ".xml");

            Editor.GetCurrentDoc().ForceReTokenize();
            Printer.Clear();
            Editor.RefreshAllLines();
            Editor.RedrawLines();
        }

        static void AliasCommand(List<string> args, List<string> switches)
        {
            if (args.Count < 1)
            {
                SetOutput("No command provided", "alias", oType: OutputType.Error);
                return;
            }

            //it may be an alias, though that isn't what this command is really for
            if (Settings.Aliases.ContainsKey(args[0]))
            {
                SetOutput(String.Format("'{0}' is an alias for '{1}'", args[0], Settings.Aliases[args[0]]), "alias");
                return;
            }

            //it must be a command, not an alias
            if (!Settings.Aliases.ContainsValue(args[0]))
            {
                SetOutput(String.Format("'{0}' does not have any aliases", args[0]), "alias");
                return;
            }

            List<string> commandAliases = new List<string>();
            foreach (string k in Settings.Aliases.Keys)
            {
                if (Settings.Aliases[k].Equals(args[0]))
                    commandAliases.Add(k);
            }

            string output = String.Format("{0} has {1} ", args[0], commandAliases.Count);
            if (commandAliases.Count == 1)
                output += "alias: ";
            else
                output += "aliases: ";
            
            for (int i = 0; i < commandAliases.Count; i++)
            {
                output += "'" + commandAliases[i] + "'";
                if (i < commandAliases.Count - 1)
                    output += ", ";
            }

            SetOutput(output, "alias");
        }

        static void UrlCommand()
        {
            Token hoveredToken = Editor.GetCurrentDoc().GetTokenAtPosition(Editor.DocCursor.DX, Editor.DocCursor.DY);
            if (hoveredToken.TType != Token.TokenType.Url)
            {
                SetOutput("Selected token isn't a valid URL", "url", OutputType.Error);
                Log.Write("Attempted to navigate to '" + hoveredToken.Text + "'", "commandline/url");
                return;
            }

            string url = hoveredToken.Text;
            try
            {
                Process.Start("explorer", hoveredToken.Text);
                SetOutput("Navigating to " + hoveredToken.Text, "url", OutputType.Success);
            }
            catch (Exception e)
            {
                SetOutput("Failed to launch browser process", "url", OutputType.Error);
                Log.Write("URL command failed: " + e.Message, "commandline/url", important: true);
                Log.Write("...was attempting to navigate to " + hoveredToken.Text, "commandline/url");
            }
        }

        static void FindCommand(List<string> args, List<string> switches)
        {
            string search = "";
            foreach (string a in args)
                search += a + " ";
            search = search.Trim();

            bool findLast = (switches.Contains("--last") || switches.Contains("-l"));
            bool findFirst = (switches.Contains("--first") || switches.Contains("-f"));
            bool findUpwards = (switches.Contains("--up") || switches.Contains("-u"));
            bool findCount = (switches.Contains("--count") || switches.Contains("-ct"));
            bool findCaseSensitive = (switches.Contains("--case") || switches.Contains("-c"));
            
            bool forceFindHere = (switches.Contains("--here") || switches.Contains("-h"));
            if (forceFindHere) {
                search = Editor.GetCurrentDoc().GetTokenAtPosition(Editor.DocCursor.DX, Editor.DocCursor.DY).Text;
                SetOutput(String.Format("Finding '{0}'", search), "find");
            }

            bool updateLastSearch = true;
            if (search.Equals("")) //default to the last search
            {
                if (lastFindSearch.Equals("")) //there is no last search
                {
                    SetOutput("No query provided", "find", OutputType.Error);
                    return;
                }
                else
                {
                    updateLastSearch = false;
                    search = lastFindSearch;
                    if (!findLast) findLast = lastFindLast;
                    if (!findFirst) findFirst = lastFindFirst;
                    if (!findUpwards) findUpwards = lastFindUpwards;
                    if (!findCaseSensitive) findCaseSensitive = lastFindCaseSensitive;
                }
            }


            if (updateLastSearch)
            {
                lastFindSearch = search;
                lastFindIndex = -1;
                lastFindLast = findLast;
                lastFindFirst = findFirst;
                lastFindUpwards = findUpwards;
                lastFindCaseSensitive = findCaseSensitive;
            }

            //search all lines of document and find all indexes
            List<Point> indexes = new List<Point>();
            int i = 0;
            int firstAfterCursor = -1;
            foreach (string l in Editor.GetCurrentDoc().GetAllLines())
            {
                i++;
                int lastIndex = -1;

                string caseSearch = search;
                string caseL = l;
                if (!findCaseSensitive)
                {
                    caseSearch = caseSearch.ToLower();
                    caseL = caseL.ToLower();
                }

                while (true)
                {

                    int nextIndex = caseL.IndexOf(caseSearch, (lastIndex == -1)? 0 : lastIndex );
                    if (nextIndex == lastIndex)
                        break;

                    // Log.Write(nextIndex + " on line " + i, "commandline/find");
                    indexes.Add(new Point(nextIndex, i - 1));

                    if (firstAfterCursor == -1 && 
                        (i - 1 >= Editor.DocCursor.DY ||
                            (i - 1 == Editor.DocCursor.DY && nextIndex >= Editor.DocCursor.DX)
                        ))
                        firstAfterCursor = indexes.Count - 1;

                    lastIndex = nextIndex;
                }
            }

            //count and then stop, if selected
            if (findCount)
            {
                string template = "There are {0} occurrences of '{1}'";
                if (indexes.Count == 1)
                    template = "There is {0} occurrence of '{1}'";
                SetOutput(String.Format(template, indexes.Count, search), "find");
                return;
            }

            //no results
            if (indexes.Count == 0)
            {
                SetOutput(String.Format("There are 0 occurrences of '{0}'", search), "find");
                return;
            }

            bool firstSearch = (lastFindIndex == -1);

            if (lastFindIndex == -1 && findLast)
            {
                lastFindIndex = indexes.Count - 2;
                if (findUpwards)
                    lastFindIndex += 2;
            }
            else if (lastFindIndex == -1 && findFirst)
            {
                lastFindIndex = -1;
                if (findUpwards)
                    lastFindIndex = 1;
            }
            else if (lastFindIndex == -1 && !findLast && !findFirst)
            {
                lastFindIndex = firstAfterCursor;
                if (findUpwards)
                    lastFindIndex++;
            }

            int findIndex = lastFindIndex;
            
            if (findUpwards)
            {
                findIndex--;
                if (findIndex == 0) //about to wrap
                    SetOutput("This is the first occurrence", "find");
                //wrap
                if (findIndex < 0)
                    findIndex = indexes.Count - 1;
            }
            else
            {
                findIndex++;
                if (findIndex == indexes.Count - 1) //about to wrap
                    SetOutput("This is the final occurrence", "find");
                //wrap
                if (findIndex >= indexes.Count)
                    findIndex = 0;
            }

            if (indexes.Count == 1)
                SetOutput("This is the only occurrence", "find");

            Log.WriteDebug("Find index: " + findIndex, "commandline/find");

            Editor.DocCursor.Move(indexes[findIndex].X, indexes[findIndex].Y);

            lastFindIndex = findIndex;

        }

        static void UnknownCommand()
        {
            SetOutput("Unknown command, try 'help all' or update the alias file", "error", oType: OutputType.Error);
        }

    }
}
