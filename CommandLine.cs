﻿using System;
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
            "syntax", "alias", "url", "find", "deindent", "count", "copy", "paste",
            "cut", "selectline", "shortcut"
            };

        public static string InputText { get; set; } = "";

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

            Log.WriteDebug("refresh: " + (Cursor.MaxScreenY - 1 + Editor.CurrentDoc.ScrollY), "commandline");
            Editor.RefreshLine(Cursor.MaxScreenY - 1 + Editor.CurrentDoc.ScrollY);
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

            if (InputText.Equals("")) //skip empty commands
            {
                Input.ToggleInputTarget();
                return;
            }

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
                case "deindent":
                    DeindentCommand();
                    break;
                case "count":
                    CountCommand(commandArgs, commandSwitches);
                    break;
                case "copy":
                    CopyCommand();
                    break;
                case "paste":
                    PasteCommand();
                    break;
                case "cut":
                    CutCommand();
                    break;
                case "selectline":
                    SelectLineCommand();
                    break;
                case "shortcut":
                    ShortcutCommand(commandArgs, commandSwitches);
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
            string defaultHelpString = "'help [command]', 'help all' for command list, or 'help wiki' to open wiki";
            string wikiUrl = "https://github.com/matthewd673/maple/wiki";
            
            if (args.Count < 1)
            {
                SetOutput(defaultHelpString, "help");
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
                    SetOutput(defaultHelpString, "help");
                    break;
                case "all":
                    string output = "";
                    for (int i = 0; i < CommandMasterList.Length; i++)
                    {
                        output += CommandMasterList[i];
                        if (i < CommandMasterList.Length - 1)
                            output += ", ";
                    }
                    SetOutput(output, "help");
                    break;
                case "wiki":
                    Process.Start("explorer", wikiUrl);
                    Log.Write("Launching GitHub wiki: " + wikiUrl, "commandline/help");
                    SetOutput(String.Format("Navigating to {0}", wikiUrl), "help", OutputType.Success);
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
                case "deindent":
                    SetOutput("deindent: deintent the current line or selection", "help");
                    break;
                case "count":
                    SetOutput("count: get stats on the document", "count");
                    break;
                case "copy":
                    SetOutput("copy: copy the current selection or line to the internal clipboard", "help");
                    break;
                case "paste":
                    SetOutput("paste: paste the contents of the internal clipboard", "help");
                    break;
                case "cut":
                    SetOutput("cut: cut the current selection or line", "help");
                    break;
                case "selectline":
                    SetOutput("selectline: select the current line", "help");
                    break;
                case "shortcut":
                    SetOutput("shortcut [key]: display the command that is executed when the given shortcut key is pressed", "help");
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
            if (Settings.SaveOnClose) //call save command first
                SaveCommand(new List<string>(), new List<string>());
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
            Editor.CurrentDoc.MarkSelectionIn(Editor.DocCursor.DX, Editor.DocCursor.DY);
            if (Editor.CurrentDoc.HasSelection()) //only refresh if there is a complete selection
                Editor.RefreshAllLines();
        }

        static void SelectOutCommand()
        {
            Editor.CurrentDoc.MarkSelectionOut(Editor.DocCursor.DX, Editor.DocCursor.DY);
            if (Editor.CurrentDoc.HasSelection()) //only refresh if there is a complete selection
                Editor.RefreshAllLines();
        }

        static void DeselectCommand()
        {
            bool hadSelection = Editor.CurrentDoc.HasSelection();
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
        }

        static void SyntaxCommand(List<string> args, List<string> switches)
        {
            if (args.Count < 1) //default to giving info about current syntax
            {
                SetOutput(String.Format("Current syntax file: {0}", Lexer.CurrentSyntaxFile), "syntax");
                return;
            }

            Lexer.LoadSyntax(Settings.SyntaxDirectory + args[0] + ".xml");

            Editor.CurrentDoc.ForceReTokenize();
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
            Token hoveredToken = Editor.CurrentDoc.GetTokenAtPosition(Editor.DocCursor.DX, Editor.DocCursor.DY);
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
                search = Editor.CurrentDoc.GetTokenAtPosition(Editor.DocCursor.DX, Editor.DocCursor.DY).Text;
                Log.WriteDebug("Finding --here: '" + search + "'", "commandline/find");
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
            Input.UpdateMaxCursorX(Editor.DocCursor);

            lastFindIndex = findIndex;

        }

        static void DeindentCommand()
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
                SetOutput("Provide a statistic to count (--lines, --chars)", "count", OutputType.Error);
        }

        static void CopyCommand()
        {
            if (Editor.CurrentDoc.HasSelection())
                Editor.ClipboardContents = Editor.CurrentDoc.GetSelectionText();
            else
            {
                Editor.ClipboardContents = Editor.CurrentDoc.GetLine(Editor.DocCursor.DY);
                Editor.ClipboardContents += "\n"; //add newline at end, it feels more natural
            }
            Log.WriteDebug("Clipboard: '" + Editor.ClipboardContents + "'", "commandline/copy");
        }

        static void PasteCommand()
        {
            if (Editor.CurrentDoc.HasSelection())
                Input.DeleteSelectionText(Editor.DocCursor);
            
            string[] clipboardLines = Editor.ClipboardContents.Split("\n");
            string beforePastePoint = Editor.CurrentDoc.GetLine(Editor.DocCursor.DY).Substring(0, Editor.DocCursor.DX);
            string afterPastePoint = Editor.CurrentDoc.GetLine(Editor.DocCursor.DY).Substring(Editor.DocCursor.DX);

            Editor.CurrentDoc.SetLine(Editor.DocCursor.DY, beforePastePoint + clipboardLines[0]);
            Editor.DocCursor.DX += clipboardLines[0].Length;
            Editor.RefreshLine(Editor.DocCursor.DY);

            int startingLine = Editor.DocCursor.DY;
            for (int i = 1; i < clipboardLines.Length; i++)
            {
                Editor.DocCursor.DX = 0;
                Editor.DocCursor.DY++;
                Editor.CurrentDoc.AddLine(Editor.DocCursor.DY);
                Editor.CurrentDoc.SetLine(Editor.DocCursor.DY, clipboardLines[i]);
            }

            if (startingLine != Editor.DocCursor.DY) //if multiple lines were pasted...
            {
                Editor.RefreshAllLines();
            }

            Editor.CurrentDoc.AddTextAtPosition(Editor.DocCursor.DX, Editor.DocCursor.DY, afterPastePoint);
        }

        static void CutCommand()
        {
            CopyCommand();

            if (Editor.CurrentDoc.HasSelection())
                Input.DeleteSelectionText(Editor.DocCursor);
            else
            {
                Editor.CurrentDoc.RemoveLine(Editor.DocCursor.DY);
                Editor.RefreshAllLines();
            }
        }

        static void SelectLineCommand()
        {
            if (Editor.CurrentDoc.HasSelection())
                Editor.RefreshAllLines();
            
            Editor.CurrentDoc.MarkSelectionIn(0, Editor.DocCursor.DY);
            Editor.CurrentDoc.MarkSelectionOut(Editor.CurrentDoc.GetLine(Editor.DocCursor.DY).Length, Editor.DocCursor.DY);

            Editor.RefreshLine(Editor.DocCursor.DY);
        }

        static void ShortcutCommand(List<string> args, List<string> switches)
        {
            if (args.Count < 1 || args[0].Length != 1)
            {
                SetOutput("No shortcut key provided", "shortcut", oType: OutputType.Error);
                return;
            }

            args[0] = args[0].ToUpper(); //stylistic
            ConsoleKey key = Settings.CharToConsoleKey(args[0].ToCharArray()[0]);
            if (!Settings.Shortcuts.ContainsKey(key))
            {
                SetOutput(String.Format("Ctrl+{0} is not a shortcut", args[0]), "shortcut");
                return;
            }

            SetOutput(String.Format(
                "Ctrl+{0} {1} '{2}'",
                args[0],
                Settings.Shortcuts[key].Execute ? "executes" : "prefills",
                Settings.Shortcuts[key].Command),
                "shortcut");
        }

        static void UnknownCommand()
        {
            SetOutput("Unknown command, try 'help all' or update the alias file", "error", oType: OutputType.Error);
        }

    }
}
