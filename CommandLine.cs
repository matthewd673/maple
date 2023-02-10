using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace maple
{

    public enum OutputType
    {
        Info,
        Error,
        Success,
        Prompt,
    }

    class OutputPrompt
    {
        public delegate void InstantActionDelegate();
        public delegate void PromptResponseDelegate(string text);

        public Dictionary<ConsoleKey, InstantActionDelegate> InstantActionTable { get; set; }
        public PromptResponseDelegate ResponseDelegate { get; set; }

        public ConsoleKey DefaultInstantAction { get; set; }

        public OutputPrompt(Dictionary<ConsoleKey, InstantActionDelegate> instantActionTable, PromptResponseDelegate responseDelegate, ConsoleKey defaultInstantAction = ConsoleKey.NoName)
        {
            InstantActionTable = instantActionTable;
            ResponseDelegate = responseDelegate;
            DefaultInstantAction = defaultInstantAction;
        }
    }

    static class CommandLine
    {

        public delegate void CommandDelegate(List<string> args, List<string> switches);

        public struct Command
        {
            public string Name { get; set; }
            public string HelpText { get; set; }
            public CommandDelegate Function { get; set; }

            public Command(string name, string helpText, CommandDelegate function)
            {
                Name = name;
                HelpText = helpText;
                Function = function;
            }
        }

        public static Dictionary<string, Command> Commands { get; } = new() {
            { "help", new Command("help", "\"help [command]\", \"help all\", or \"help wiki\"", HelpCommand) },
            { "save", new Command("save", "save [filename]: save document, or save a copy to filename", SaveCommand) },
            { "load", new Command("load", "load [filename]: reload document, or load document at filename", LoadCommand) },
            { "new", new Command("new", "new <filename>: create a new document at filename", NewCommand) },
            { "close", new Command("close", "close: close maple without saving", CloseCommand) },
            { "cls", new Command("cls", "cls: clear the command output", ClearCommand) },
            { "top", new Command("top", "top: jump to the top of the document", TopCommand) },
            { "bot", new Command("bot", "bot: jump to the bottom of the document", BotCommand) },
            { "redraw", new Command("redraw", "redraw: force a complete redraw of the window", RedrawCommand) },
            { "goto", new Command("goto", "goto <line>: jump to the specified line", GotoCommand) },
            { "selectin", new Command("selectin", "selectin: open selection", SelectInCommand) },
            { "selectout", new Command("selectout", "selectout: close selection", SelectOutCommand) },
            { "deselect", new Command("deselect", "deselect: deselect the current selection", DeselectCommand) },
            { "readonly", new Command("readonly", "readonly: toggle readonly mode", ReadonlyCommand) },
            { "syntax", new Command("syntax", "syntax [extension]: render the current file with the specified syntax rules, or view the current syntax file", SyntaxCommand) },
            { "alias", new Command("alias", "alias <command>: view all aliases for a given command", AliasCommand) },
            { "url", new Command("url", "url: if the cursor is currently hovered on a url, open it in the browser", UrlCommand) },
            { "find", new Command("find", "find <query> [switches]: find the next occurrence of the search phrase, starting from the top", FindCommand) },
            { "deindent", new Command("deindent", "deindent: deindent the current line or selection", DeindentCommand) },
            { "count", new Command("count", "count <statistic>: count document '--lines' or '--chars'", CountCommand) },
            { "copy", new Command("copy", "copy: copy the current selection or line to the internal clipboard", CopyCommand) },
            { "paste", new Command("paste", "paste: paste the contents of the internal clipboard", PasteCommand) },
            { "cut", new Command("cut", "cut: cut the current selection or line", CutCommand) },
            { "selectline", new Command("selectline", "selectline: select the current line", SelectLineCommand) },
            { "selectall", new Command("selectall", "selectall: select the entire document", SelectAllCommand) },
            { "shortcut", new Command("shortcut", "shortcut <key>: display the shortcut command for the given key", ShortcutCommand) },
            { "undo", new Command("undo", "undo: undo the last edit to the document", UndoCommand) },
            { "redo", new Command("redo", "redo: redo the last edit in the undo history", RedoCommand) },
            { "comment", new Command("comment", "comment: toggle the comment status of the current line or selection", CommentCommand) },
            { "pos", new Command("pos", "pos: get the line and column position of the cursor", PositionCommand)},
            { "snippet", new Command("snippet", "snippet [name]: replace the current token with a snippet, or insert a snippet by name", SnippetCommand) },
        };

        public static string InputText { get; set; } = "";

        public static string OutputText { get; private set; }
        public static OutputType OType = OutputType.Info;
        public static OutputPrompt OPrompt { get; private set; }
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

        public static void SetOutput(string text, string speaker, OutputPrompt oPrompt = null, OutputType oType = OutputType.Info, bool renderFooter = true)
        {
            OutputText = String.Format("[{0}]: {1}", speaker, text);
            OType = oType;
            OPrompt = oPrompt;
            if (oPrompt != null) OType = OutputType.Prompt;
            HasOutput = true;

            Footer.RefreshOutputLine();
            if (renderFooter)
            {
                Footer.PrintFooter();
            }
        }

        public static void ClearOutput()
        {
            OutputText = "";
            HasOutput = false;
            OType = OutputType.Info;
            OPrompt = null;

            //reprint bottom editor line
            Footer.RefreshOutputLine();
            Editor.RefreshLine(Printer.MaxScreenY - 1 + Editor.CurrentDoc.ScrollY);
        }

        public static void ClearInput()
        {
            InputText = "";
            Editor.CmdCursor.Move(0, 0);
        }

        public static void ExecuteInput()
        {
            if(HasOutput) // output is being displayed, reset for the future
                ClearOutput();

            if (InputText.Equals("")) // skip empty commands
            {
                Input.ToggleInputTarget();
                return;
            }

            CommandHistoryIndex = -1; // reset command history index
            if (CommandHistory.Count == 0 || !CommandHistory[^1].Equals(InputText))
                CommandHistory.Insert(0, InputText);

            CommandParser.CommandInfo commandInfo = CommandParser.Parse(InputText);

            string primaryCommand = commandInfo.PrimaryCommand;
            List<string> commandArgs = commandInfo.Args;
            List<string> commandSwitches = commandInfo.Switches;

            // swap out alias with actual primary command
            if (Settings.Properties.AliasesTable.ContainsKey(primaryCommand) && !Commands.ContainsKey(primaryCommand))
                primaryCommand = Settings.Properties.AliasesTable[primaryCommand];

            if (Commands.ContainsKey(primaryCommand))
            {
                Commands[primaryCommand].Function(commandArgs, commandSwitches);
            }
            else
            {
                UnknownCommand();
            }

            //empty input field, but only toggle back if not a prompt
            ClearInput();

            if (OType != OutputType.Prompt)
                Input.ToggleInputTarget();
        }

        static void HelpCommand(List<string> args, List<string> switches)
        {
            string defaultHelpString = "\"help [command]\", \"help all\" for command list, or \"help wiki\" to open wiki";
            string wikiUrl = "https://github.com/matthewd673/maple/wiki";

            if (args.Count < 1)
            {
                SetOutput(defaultHelpString, "help");
                return;
            }

            // print command help text
            if (Commands.ContainsKey(args[0]))
            {
                SetOutput(Commands[args[0]].HelpText, "help");
            }
            // command wasn't found, it may be a special case
            else
            {
                switch (args[0])
                {
                    case "all":
                        string output = "";
                        foreach (string k in Commands.Keys)
                        {
                            output += Commands[k].Name + ", ";
                        }
                        output = output.Remove(output.Length - 3);
                        SetOutput(output, "help");
                        break;
                    case "wiki":
                        Process.Start("explorer", wikiUrl);
                        Log.Write("Launching GitHub wiki: " + wikiUrl, "commandline/help");
                        SetOutput(String.Format("Navigating to {0}", wikiUrl), "help", oType: OutputType.Success);
                        break;
                    default:
                        if (Settings.Properties.AliasesTable.ContainsKey(args[0]))
                        {
                            SetOutput(String.Format("\"{0}\" is an alias for \"{1}\"", args[0], Settings.Properties.AliasesTable[args[0]]), "help");
                        }
                        else
                        {
                            UnknownCommand();
                        }
                        break;
                }
            }
        }

        static void SaveCommand(List<string> args, List<string> switches)
        {
            string savePath = Editor.CurrentDoc.Filepath;
            if(args.Count > 0)
                savePath = args[0];
            savePath = savePath.Trim('\"');

            if (savePath.Equals(""))
            {
                SetOutput("Enter a name or path for the file", "save",
                    oPrompt: new OutputPrompt(
                        null,
                        SaveCommandResponse
                    ),
                    oType: OutputType.Prompt);
                return;
            }

            Encoding encoding = Encoding.UTF8;
            if (Lexer.Properties.DefaultEncoding.Equals("utf8"))
            {
                encoding = Encoding.UTF8;
            }
            else if (Lexer.Properties.DefaultEncoding.Equals("ascii"))
            {
                encoding = Encoding.ASCII;
            }

            if (switches.Contains("--utf8"))
            {
                encoding = Encoding.UTF8;
            }
            else if (switches.Contains("--ascii"))
            {
                encoding = Encoding.ASCII;
            }

            Editor.CurrentDoc.SaveDocument(savePath, encoding);
            Editor.CurrentDoc.LastModifiedTime = File.GetLastWriteTime(Editor.CurrentDoc.Filepath).ToFileTime();

            string existingPath = Editor.CurrentDoc.Filepath;

            if(savePath != existingPath)
                SetOutput(String.Format("Copy of file saved to \"{0}\"", savePath.Trim()), "save");
            else
                SetOutput(String.Format("File saved to \"{0}\"", savePath.Trim()), "save");
        }

        static void SaveCommandResponse(string filename)
        {
            SaveCommand(new List<string>() { filename }, new List<string>()); // just send it right back
        }

        static void LoadCommand(List<string> args, List<string> switches)
        {
            if (args.Count < 1)
            {
                SetOutput("No filepath provided", "load", oType: OutputType.Error);
                return;
            }

            string filepath = args[0];
            //trim quotes if it was in a block
            if (filepath.StartsWith("\""))
                filepath = filepath.Trim('\"');

            if (filepath.Equals(""))
                filepath = Editor.CurrentDoc.Filepath;

            //initialize new editor
            if (File.Exists(Document.ProcessFilepath(filepath)))
            {
                Editor.Initialize(filepath);
            }
            else
                SetOutput(String.Format("File \"{0}\" doesn't exist, use \"new\" to create a new file", filepath), "load", oType: OutputType.Error);
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

            // initialize new editor (TODO: this seems overkill)
            // file exists, do nothing
            if (File.Exists(Document.ProcessFilepath(filepath)))
            {
                SetOutput(String.Format("File \"{0}\" already exists, use \"load\" to load an existing file", filepath), "new", oType: OutputType.Error);
            }
            // file doesn't exist but directory does, create
            else if (Path.GetDirectoryName(Document.ProcessFilepath(filepath)).Equals("") ||
                     Directory.Exists(Path.GetDirectoryName(Document.ProcessFilepath(filepath))))
            {
                Editor.Initialize(filepath);
                SetOutput(String.Format("Created a new file at \"{0}\"", filepath), "new", oType: OutputType.Success);
            }
            // directory doesn't exist, do nothing
            else
            {
                SetOutput(String.Format("Directory \"{0}\" does not exist", Path.GetDirectoryName(Document.ProcessFilepath(filepath))), "new", oType: OutputType.Error);
            }

        }

        static void CloseCommand(List<string> args, List<string> switches)
        {
            SetOutput(
                "Are you sure you want to close?",
                "close",
                new OutputPrompt(
                    new Dictionary<ConsoleKey, OutputPrompt.InstantActionDelegate>()
                    {
                        { ConsoleKey.Y, CloseCommandActionYes },
                        { ConsoleKey.N, CloseCommandActionNo },
                    },
                    null,
                    defaultInstantAction: ConsoleKey.Y
                    ),
                oType: OutputType.Prompt
                );

            if (Settings.Properties.SaveOnClose) // call save command first
                SaveCommand(new List<string>(), new List<string>());
        }

        static void CloseCommandActionYes()
        {
            if (Editor.CurrentDoc.Dirty)
            {
                SetOutput(
                    "This file has unsaved changes, close without saving?",
                    "close",
                    new OutputPrompt(
                        new Dictionary<ConsoleKey, OutputPrompt.InstantActionDelegate>()
                        {
                            { ConsoleKey.N, CloseCommandUnsavedActionNo },
                            { ConsoleKey.Y, CloseCommandUnsavedActionYes },
                        },
                        null,
                        defaultInstantAction: ConsoleKey.N
                        ),
                    oType: OutputType.Prompt
                    );
                return;
            }
            Program.Close();
        }

        static void CloseCommandUnsavedActionYes()
        {
            Program.Close();
        }

        static void CloseCommandUnsavedActionNo()
        {
            ClearOutput();
        }

        static void CloseCommandActionNo()
        {
            ClearOutput();
            Input.ToggleInputTarget();
        }

        static void ClearCommand(List<string> args, List<string> switches)
        {
            ClearOutput();
        }

        static void TopCommand(List<string> args, List<string> switches)
        {
            Editor.DocCursor.Move(Editor.DocCursor.DX, 0);
        }

        static void BotCommand(List<string> args, List<string> switches)
        {
            Editor.DocCursor.Move(Editor.DocCursor.DX, Editor.DocCursor.Doc.GetMaxLine());
        }

        static void RedrawCommand(List<string> args, List<string> switches)
        {
            Editor.RedrawWindow();
        }

        static void SelectInCommand(List<string> args, List<string> switches)
        {
            Editor.CurrentDoc.MarkSelectionIn(Editor.DocCursor.DX, Editor.DocCursor.DY);
            if (Editor.CurrentDoc.HasSelection) //only refresh if there is a complete selection
                Editor.RefreshAllLines();
        }

        static void SelectOutCommand(List<string> args, List<string> switches)
        {
            Editor.CurrentDoc.MarkSelectionOut(Editor.DocCursor.DX, Editor.DocCursor.DY);
            if (Editor.CurrentDoc.HasSelection) //only refresh if there is a complete selection
                Editor.RefreshAllLines();
        }

        static void DeselectCommand(List<string> args, List<string> switches)
        {
            bool hadSelection = Editor.CurrentDoc.HasSelection;
            Editor.CurrentDoc.Deselect();
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

            if (int.TryParse(args[0], out int l))
            {
                if (l > Editor.DocCursor.Doc.GetMaxLine() + 1 || l < 0)
                    SetOutput(String.Format("Invalid line number, must be >= 1 and <= {0}", (Editor.DocCursor.Doc.GetMaxLine() + 1)), "goto", oType: OutputType.Error);
                else
                    Editor.DocCursor.Move(0, l - 1);
            }
            else
                SetOutput("Invalid line number, must be an integer", "goto", oType: OutputType.Error);
        }

        static void ReadonlyCommand(List<string> args, List<string> switches)
        {
            Input.ReadOnly = !Input.ReadOnly;
        }

        static void SyntaxCommand(List<string> args, List<string> switches)
        {
            if (args.Count < 1) //default to giving info about current syntax
            {
                SetOutput(String.Format("Current syntax file: \"{0}\"", Lexer.CurrentSyntaxFile), "syntax");
                return;
            }

            Lexer.LoadSyntax(Settings.Properties.SyntaxDirectory + args[0] + ".xml");

            Editor.CurrentDoc.ForceReTokenize();
            // Printer.Clear();
            Editor.RefreshAllLines();
            // Editor.DrawLines();
        }

        static void AliasCommand(List<string> args, List<string> switches)
        {
            if (args.Count < 1)
            {
                SetOutput("No command provided", "alias", oType: OutputType.Error);
                return;
            }

            //it may be an alias, though that isn't what this command is really for
            if (Settings.Properties.AliasesTable.ContainsKey(args[0]))
            {
                SetOutput(String.Format("\"{0}\" is an alias for \"{1}\"", args[0], Settings.Properties.AliasesTable[args[0]]), "alias");
                return;
            }

            //it must be a command, not an alias
            if (!Settings.Properties.AliasesTable.ContainsValue(args[0]))
            {
                SetOutput(String.Format("\"{0}\" does not have any aliases", args[0]), "alias");
                return;
            }

            List<string> commandAliases = new List<string>();
            foreach (string k in Settings.Properties.AliasesTable.Keys)
            {
                if (Settings.Properties.AliasesTable[k].Equals(args[0]))
                    commandAliases.Add(k);
            }

            string output = String.Format("\"{0}\" has {1} ", args[0], commandAliases.Count);
            if (commandAliases.Count == 1)
                output += "alias: ";
            else
                output += "aliases: ";

            for (int i = 0; i < commandAliases.Count; i++)
            {
                output += "\"" + commandAliases[i] + "\"";
                if (i < commandAliases.Count - 1)
                    output += ", ";
            }

            SetOutput(output, "alias");
        }

        static void UrlCommand(List<string> args, List<string> switches)
        {
            Token hoveredToken = Editor.CurrentDoc.GetTokenAtPosition(Editor.DocCursor.DX, Editor.DocCursor.DY);
            if (hoveredToken.TType != TokenType.Url)
            {
                SetOutput("Selected token isn't a valid URL", "url", oType: OutputType.Error);
                Log.Write("Attempted to navigate to " + hoveredToken.Text, "commandline/url");
                return;
            }

            try
            {
                Process.Start("explorer", hoveredToken.Text);
                SetOutput("Navigating to " + hoveredToken.Text, "url", oType: OutputType.Success);
            }
            catch (Exception e)
            {
                SetOutput("Failed to launch browser process", "url", oType: OutputType.Error);
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
                search = Editor.CurrentDoc.GetTokenAtPosition(Editor.DocCursor.DX, Editor.DocCursor.DY).Text;
                Log.WriteDebug("Finding --here: '" + search + "'", "commandline/find");
                SetOutput(String.Format("Finding \"{0}\"", search), "find");
            }

            bool updateLastSearch = true;
            if (search.Equals("")) //default to the last search
            {
                if (lastFindSearch.Equals("")) //there is no last search
                {
                    if (Editor.CurrentDoc.HasSelection)
                    {
                        search = Editor.CurrentDoc.GetSelectionText().Split('\n')[0];
                        // Editor.CurrentDoc.Deselect();
                        Editor.RefreshAllLines();
                        SetOutput(String.Format("Finding \"{0}\"", search), "find");
                    }
                    else
                    {
                        SetOutput("No query provided", "find", oType: OutputType.Error);
                        return;
                    }
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
            foreach (string l in Editor.CurrentDoc.GetAllLines())
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
                string template = "There are {0} occurrences of \"{1}\"";
                if (indexes.Count == 1)
                    template = "There is {0} occurrence of \"{1}\"";
                SetOutput(String.Format(template, indexes.Count, search), "find");
                return;
            }

            //no results
            if (indexes.Count == 0)
            {
                SetOutput(String.Format("There are 0 occurrences of \"{0}\"", search), "find");
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
                lastFindIndex = firstAfterCursor - 1;
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
            Input.UpdateMaxCursorX(Editor.DocCursor);

            lastFindIndex = findIndex;

        }

        static void DeindentCommand(List<string> args, List<string> switches)
        {
            Editor.CurrentDoc.Deindent();
        }

        static void CountCommand(List<string> args, List<string> switches)
        {
            string output = "Document contains ";
            int i = 0;
            foreach (string s in switches)
            {
                if (s.Equals("-l") || s.Equals("--lines"))
                    output += String.Format("{0} lines", Editor.CurrentDoc.GetMaxLine() + 1);
                if (s.Equals("-c") || s.Equals("--chars"))
                {
                    int charCt = 0;
                    for (int k = 0; k <= Editor.CurrentDoc.GetMaxLine(); k++)
                        charCt += Editor.CurrentDoc.GetLine(k).Length;
                    output += String.Format("{0} characters", charCt);
                }

                if (i < switches.Count - 1)
                    output += ", ";
                i++;
            }

            if (i > 0)
                SetOutput(output, "count");
            else
                SetOutput("Provide a statistic to count (--lines, --chars)", "count", oType: OutputType.Error);
        }

        static void CopyCommand(List<string> args, List<string> switches)
        {
            if (Editor.CurrentDoc.HasSelection)
                Editor.ClipboardContents = Editor.CurrentDoc.GetSelectionText();
            else
            {
                Editor.ClipboardContents = Editor.CurrentDoc.GetLine(Editor.DocCursor.DY);
                Editor.ClipboardContents += "\n"; // add newline at end, it feels more natural
            }
        }

        static void PasteCommand(List<string> args, List<string> switches)
        {
            bool deletedSelection = false;
            if (Editor.CurrentDoc.HasSelection)
            {
                Input.DeleteSelectionText(Editor.DocCursor); // TODO: undo doesn't work correctly
                deletedSelection = true;
            }

            Log.WriteDebug(Editor.ClipboardContents, "commandline/paste");

            Editor.CurrentDoc.AddBlockText(Editor.DocCursor.DX, Editor.DocCursor.DY, Editor.ClipboardContents);
            int clipboardLinesLength = Editor.ClipboardContents.Length;

            string[] clipboardLinesSplit = Editor.ClipboardContents.Split('\n');
            int newCursorX = clipboardLinesSplit[^1].Length;
            int newCursorY = Editor.DocCursor.DY + clipboardLinesSplit.Length - 1;
            if (newCursorY == Editor.DocCursor.DY)
            {
                newCursorX += Editor.DocCursor.DX;
            }

            Editor.CurrentDoc.LogHistoryEvent(new HistoryEvent(
                HistoryEventType.AddSelection,
                Editor.ClipboardContents,
                new Point(Editor.DocCursor.DX, Editor.DocCursor.DY),
                new Point(Editor.DocCursor.DX, Editor.DocCursor.DY),
                combined: deletedSelection
            ));

            Editor.DocCursor.Move(newCursorX, newCursorY);

            Editor.RefreshAllLines();
        }

        static void CutCommand(List<string> args, List<string> switches)
        {
            CopyCommand(args, switches);

            if (Editor.CurrentDoc.HasSelection)
            {
                Input.DeleteSelectionText(Editor.DocCursor);
            }
            else
            {
                Editor.CurrentDoc.LogHistoryEvent(new HistoryEvent(
                    HistoryEventType.RemoveSelection,
                    Editor.CurrentDoc.GetLine(Editor.DocCursor.DY) + "\n",
                    new Point(0, Editor.DocCursor.DY),
                    new Point(Editor.DocCursor.DX, Editor.DocCursor.DY),
                    new Point[] {
                        new Point(0, Editor.DocCursor.DY),
                        new Point(0, Editor.DocCursor.DY + 1)
                        }
                ));

                Editor.CurrentDoc.RemoveLine(Editor.DocCursor.DY);
                if (Editor.DocCursor.DY > 0)
                {
                    Editor.DocCursor.Move(Editor.CurrentDoc.GetLine(Editor.DocCursor.DY - 1).Length, Editor.DocCursor.DY - 1);
                }
                else
                {
                    Editor.DocCursor.Move(0, 0);
                }
                Editor.RefreshAllLines();
            }
        }

        static void SelectLineCommand(List<string> args, List<string> switches)
        {
            if (Editor.CurrentDoc.HasSelection)
                Editor.RefreshAllLines();

            Editor.CurrentDoc.MarkSelectionIn(0, Editor.DocCursor.DY);
            Editor.CurrentDoc.MarkSelectionOut(Editor.CurrentDoc.GetLine(Editor.DocCursor.DY).Length, Editor.DocCursor.DY);

            Editor.RefreshLine(Editor.DocCursor.DY);
        }

        static void SelectAllCommand(List<string> args, List<string> switches)
        {
            Editor.CurrentDoc.MarkSelectionIn(0, 0);
            Editor.CurrentDoc.MarkSelectionOut(
                Editor.CurrentDoc.GetLine(Editor.CurrentDoc.GetMaxLine()).Length,
                Editor.CurrentDoc.GetMaxLine()
                );

            Editor.RefreshAllLines();
        }

        static void ShortcutCommand(List<string> args, List<string> switches)
        {
            if (args.Count < 1 || args[0].Length != 1)
            {
                SetOutput("No shortcut key provided", "shortcut", oType: OutputType.Error);
                return;
            }

            args[0] = args[0].ToUpper(); // stylistic
            ConsoleKey key = Settings.StringToConsoleKeyTable[args[0]];
            if (!Settings.Properties.ShortcutsTable.ContainsKey(key))
            {
                SetOutput(String.Format("Ctrl+{0} is not a shortcut", args[0]), "shortcut");
                return;
            }

            SetOutput(String.Format(
                "Ctrl+{0} {1} \"{2}\"",
                args[0],
                (Settings.Properties.ShortcutsTable[key].Execute) ? "executes" : "prefills",
                Settings.Properties.ShortcutsTable[key].Command),
                "shortcut");
        }

        static void UndoCommand(List<string> args, List<string> switches)
        {
            Editor.CurrentDoc.Undo();
        }

        static void RedoCommand(List<string> args, List<string> switches)
        {
            Editor.CurrentDoc.Undo(redo: true);
        }

        static void CommentCommand(List<string> args, List<string> switches)
        {
            if (Lexer.Properties.CommentPrefix.Equals(""))
            {
                SetOutput("No comment prefix is defined for this filetype.", "comment", oType: OutputType.Error);
                return;
            }

            // comment entire selection
            if (Editor.CurrentDoc.HasSelection)
            {
                bool commenting = true;

                for (int i = Editor.CurrentDoc.SelectInY; i <= Editor.CurrentDoc.SelectOutY; i++)
                {
                    string line = Editor.CurrentDoc.GetLine(i);
                    string indentation = "";

                    while (line.StartsWith(Settings.TabString))
                    {
                        indentation += Settings.TabString;
                        line = line.Remove(0, Settings.Properties.TabSpacesCount);
                    }

                    if (i == Editor.CurrentDoc.SelectInY)
                    {
                        // first line is commented, so we'll be uncommenting
                        if (line.StartsWith(Lexer.Properties.CommentPrefix))
                            commenting = false;
                    }

                    if (commenting)
                    {
                        Editor.CurrentDoc.SetLine(
                            i,
                            indentation + Lexer.Properties.CommentPrefix + line
                        );
                        Editor.CurrentDoc.LogHistoryEvent(new HistoryEvent(
                            HistoryEventType.AddSelection,
                            Lexer.Properties.CommentPrefix,
                            new Point(indentation.Length, i),
                            new Point(Editor.DocCursor.DX, Editor.DocCursor.DY),
                            combined: i != Editor.CurrentDoc.SelectInY
                        ));
                    }
                    else
                    {
                        line = line.Remove(0, Lexer.Properties.CommentPrefix.Length);
                        Editor.CurrentDoc.SetLine(
                            i,
                            indentation + line
                        );
                        Editor.CurrentDoc.LogHistoryEvent(new HistoryEvent(
                            HistoryEventType.RemoveSelection,
                            Lexer.Properties.CommentPrefix,
                            new Point(indentation.Length, i),
                            new Point(Editor.DocCursor.DX, Editor.DocCursor.DY),
                            combined: i != Editor.CurrentDoc.SelectInY
                        ));
                    }

                    Editor.RefreshLine(i);
                }
            }
            // comment one line
            else
            {
                string line = Editor.CurrentDoc.GetLine(Editor.DocCursor.DY);

                // get line indentation prefix
                string indentation = "";
                while (line.StartsWith(Settings.TabString))
                {
                    indentation += Settings.TabString;
                    line = line.Remove(0, Settings.Properties.TabSpacesCount);
                }

                // is not commented - comment
                if (!line.StartsWith(Lexer.Properties.CommentPrefix))
                {
                    Editor.CurrentDoc.SetLine(
                        Editor.DocCursor.DY,
                        indentation + Lexer.Properties.CommentPrefix + line
                    );
                    Editor.CurrentDoc.LogHistoryEvent(new HistoryEvent(
                        HistoryEventType.AddSelection,
                        Lexer.Properties.CommentPrefix,
                        new Point(indentation.Length, Editor.DocCursor.DY),
                        new Point(Editor.DocCursor.DX, Editor.DocCursor.DY)
                    ));
                }
                // is commented - uncomment
                else
                {
                    line = line.Remove(0, Lexer.Properties.CommentPrefix.Length);
                    Editor.CurrentDoc.SetLine(
                        Editor.DocCursor.DY,
                        indentation + line
                    );
                    Editor.CurrentDoc.LogHistoryEvent(new HistoryEvent(
                        HistoryEventType.RemoveSelection,
                        Lexer.Properties.CommentPrefix,
                        new Point(indentation.Length, Editor.DocCursor.DY),
                        new Point(Editor.DocCursor.DX, Editor.DocCursor.DY)
                    ));
                }

                Editor.RefreshLine(Editor.DocCursor.DY);
            }
        }

        static void PositionCommand(List<string> args, List<string> switches)
        {
            SetOutput(
                String.Format("Line {0} Column {1}", Editor.DocCursor.DY + 1, Editor.DocCursor.DX + 1),
                "pos"
            );
        }

        static void SnippetCommand(List<string> args, List<string> switches)
        {
            // check for switches
            foreach (string s in switches)
            {
                if (s.Equals("-ct") || s.Equals("--count"))
                {
                    int count = Settings.Snippets.SnippetsList.Count;
                    SetOutput(String.Format("{0} snippet{1} loaded", count, count == 1 ? "" : "s"), "snippet");
                    return;
                }
            }

            // grab snippet name from current token
            // unless its specified as an argument
            Token hoverToken = Editor.CurrentDoc.GetTokenAtPosition(Editor.DocCursor);
            // try previous token also
            if (hoverToken == null)
            {
                hoverToken = Editor.CurrentDoc.GetTokenAtPosition(Editor.DocCursor.DX - 1, Editor.DocCursor.DY);
            }

            string name = hoverToken.Text;
            bool tokenSnip = true;
            if (args.Count > 0)
            {
                name = args[0];
                tokenSnip = false;
            }

            Log.Write(String.Format("Inserting snippet '{0}' (by {1})", name, tokenSnip ? "token" : "argument"), "commandline");

            // not valid
            if (!Settings.Snippets.SnippetsTable.ContainsKey(name))
            {
                Log.Write("Failed to insert snippet - not defined", "commandline");
                SetOutput(String.Format("Snippet '{0}' is not defined", name), "snippet", oType: OutputType.Error);
                return;
            }

            // snippet name derived from current token
            // therefore, delete that token before adding
            if (tokenSnip)
            {
                Point[] tokenBounds = Editor.CurrentDoc.GetBoundsOfTokenAtPosition(Editor.DocCursor);
                Log.WriteDebug(String.Format("Snip replace bounds: {0} - {1}", tokenBounds[0], tokenBounds[1]), "commandline");
                Editor.CurrentDoc.RemoveBlockText(
                    tokenBounds[0],
                    tokenBounds[1]
                );

                Editor.CurrentDoc.LogHistoryEvent(new HistoryEvent(
                    HistoryEventType.RemoveSelection,
                    name,
                    tokenBounds[0],
                    new Point(Editor.DocCursor.DX, Editor.DocCursor.DY),
                    selectionPoints: tokenBounds,
                    combined: false
                ));

                Editor.DocCursor.Move(tokenBounds[0], applyPosition: false);
            }

            string text = Settings.Snippets.SnippetsTable[name].Text;
            Point newPoint = Editor.CurrentDoc.AddBlockText(Editor.DocCursor.DX, Editor.DocCursor.DY, text);

            Editor.CurrentDoc.LogHistoryEvent(new HistoryEvent(
                HistoryEventType.AddSelection,
                text,
                new Point(Editor.DocCursor.DX, Editor.DocCursor.DY),
                new Point(newPoint.X, newPoint.Y),
                selectionPoints: new Point[] {
                    new Point(Editor.DocCursor.DX, Editor.DocCursor.DY),
                    new Point(Editor.DocCursor.DX, Editor.DocCursor.DY),
                },
                combined: true
            ));

            Editor.RefreshAllLines();
            Editor.DocCursor.UpdateGutterOffset();
            Editor.DocCursor.Move(newPoint.X, newPoint.Y);
        }

        static void UnknownCommand()
        {
            SetOutput("Unknown command, try \"help all\" or update the alias file", "error", oType: OutputType.Error);
        }
    }
}
