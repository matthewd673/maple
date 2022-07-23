using System;
using System.Text.RegularExpressions;

namespace maple
{
    static class Input
    {
        public enum InputTarget
        {
            Document,
            Command
        }

        public static InputTarget CurrentTarget { get; private set; } = InputTarget.Document;

        static int maxCursorX = 0;

        static string tabString = "";
        static bool quickSelectOutPoint = true;

        public static bool ReadOnly { get; set; } = false;

        public static void AcceptInput(ConsoleKeyInfo keyInfo)
        {
            // build tab string if necessary
            // TODO: move this somewhere else
            if (tabString.Length == 0)
            {
                for (int i = 0; i < Settings.Properties.TabSpacesCount; i++)
                    tabString += " ";
            }

            if(CurrentTarget == InputTarget.Document)
                AcceptDocumentInput(keyInfo);
            else if(CurrentTarget == InputTarget.Command)
                AcceptCommandInput(keyInfo);
        }

        static void AcceptDocumentInput(ConsoleKeyInfo keyInfo)
        {
            DocumentCursor docCursor = Editor.DocCursor;
            Document doc = docCursor.Doc;

            // allow shortcuts to be triggered while alt-key is pressed
            // when alt-tabbing, alt key will become pressed and never register
            // as unpressed
            if (keyInfo.Modifiers == ConsoleModifiers.Control ||
                keyInfo.Modifiers == (ConsoleModifiers.Control | ConsoleModifiers.Alt))
            {
                HandleShortcut(keyInfo);
                return;
            }

            switch(keyInfo.Key)
            {
                // MOVEMENT
                case ConsoleKey.UpArrow:
                    // create / move selection area
                    if (Settings.Properties.ShiftSelect && keyInfo.Modifiers == ConsoleModifiers.Shift)
                    {
                        if (!docCursor.Doc.HasSelectionStart())
                        {
                            docCursor.Doc.MarkSelectionIn(docCursor.DX, docCursor.DY);
                            docCursor.Doc.MarkSelectionOut(docCursor.DX, docCursor.DY);
                            quickSelectOutPoint = false;
                        }
                        int oldDY = docCursor.DY;
                        HandleUp(docCursor);

                        if (quickSelectOutPoint)
                        {
                            quickSelectOutPoint = !docCursor.Doc.MarkSelectionOut(docCursor.DX, docCursor.DY);
                        }
                        else
                        {
                            quickSelectOutPoint = docCursor.Doc.MarkSelectionIn(docCursor.DX, docCursor.DY);
                        }

                        Editor.RefreshLine(docCursor.DY);
                        if (oldDY != docCursor.DY)
                            Editor.RefreshLine(oldDY);
                    }

                    if (docCursor.Doc.HasSelection())
                    {
                        if (Settings.Properties.ArrowsDeselect && keyInfo.Modifiers != ConsoleModifiers.Shift)
                        {
                            docCursor.Doc.Deselect();
                            Editor.RefreshAllLines();
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    HandleUp(docCursor);
                    break;
                case ConsoleKey.DownArrow:
                    // create / move selection area
                    if (Settings.Properties.ShiftSelect && keyInfo.Modifiers == ConsoleModifiers.Shift)
                    {
                        if (!docCursor.Doc.HasSelectionStart())
                        {
                            docCursor.Doc.MarkSelectionIn(docCursor.DX, docCursor.DY);
                            docCursor.Doc.MarkSelectionOut(docCursor.DX, docCursor.DY);
                            quickSelectOutPoint = true;
                        }
                        int oldDY = docCursor.DY;
                        HandleDown(docCursor);
                        
                        if (quickSelectOutPoint)
                        {
                            quickSelectOutPoint = !docCursor.Doc.MarkSelectionOut(docCursor.DX, docCursor.DY);
                        }
                        else
                        {
                            quickSelectOutPoint = docCursor.Doc.MarkSelectionIn(docCursor.DX, docCursor.DY);
                        }

                        Editor.RefreshLine(docCursor.DY);
                        if (oldDY != docCursor.DY)
                            Editor.RefreshLine(oldDY);
                    }

                    if (docCursor.Doc.HasSelection())
                    {
                        if (Settings.Properties.ArrowsDeselect && keyInfo.Modifiers != ConsoleModifiers.Shift)
                        {
                            docCursor.Doc.Deselect();
                            Editor.RefreshAllLines();
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    HandleDown(docCursor);
                    break;
                case ConsoleKey.LeftArrow:
                    // create / move selection area
                    if (Settings.Properties.ShiftSelect && keyInfo.Modifiers == ConsoleModifiers.Shift)
                    {
                        if (!docCursor.Doc.HasSelectionStart())
                        {
                            docCursor.Doc.MarkSelectionIn(docCursor.DX, docCursor.DY);
                            docCursor.Doc.MarkSelectionOut(docCursor.DX, docCursor.DY);
                            quickSelectOutPoint = false;
                        }
                        int oldDY = docCursor.DY;
                        HandleLeft(docCursor);

                        if (quickSelectOutPoint)
                        {
                            quickSelectOutPoint = !docCursor.Doc.MarkSelectionOut(docCursor.DX, docCursor.DY);
                        }
                        else
                        {
                            quickSelectOutPoint = docCursor.Doc.MarkSelectionIn(docCursor.DX, docCursor.DY);
                        }

                        Editor.RefreshLine(docCursor.DY);
                        if (oldDY != docCursor.DY)
                            Editor.RefreshLine(oldDY);
                    }

                    if (docCursor.Doc.HasSelection())
                    {
                        if (Settings.Properties.ArrowsDeselect && keyInfo.Modifiers != ConsoleModifiers.Shift)
                        {
                            docCursor.Doc.Deselect();
                            Editor.RefreshAllLines();
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    HandleLeft(docCursor);
                    break;
                case ConsoleKey.RightArrow:
                    // create / move selection area
                    if (Settings.Properties.ShiftSelect && keyInfo.Modifiers == ConsoleModifiers.Shift)
                    {
                        if (!docCursor.Doc.HasSelectionStart())
                        {
                            docCursor.Doc.MarkSelectionIn(docCursor.DX, docCursor.DY);
                            docCursor.Doc.MarkSelectionOut(docCursor.DX, docCursor.DY);
                            quickSelectOutPoint = true;
                        }
                        int oldDY = docCursor.DY;
                        HandleRight(docCursor);
                        
                        if (quickSelectOutPoint)
                        {
                            quickSelectOutPoint = !docCursor.Doc.MarkSelectionOut(docCursor.DX, docCursor.DY);
                        }
                        else
                        {
                            quickSelectOutPoint = docCursor.Doc.MarkSelectionIn(docCursor.DX, docCursor.DY);
                        }

                        Editor.RefreshLine(docCursor.DY);
                        if (oldDY != docCursor.DY)
                            Editor.RefreshLine(oldDY);
                    }
                    
                    if (docCursor.Doc.HasSelection())
                    {
                        if (Settings.Properties.ArrowsDeselect && keyInfo.Modifiers != ConsoleModifiers.Shift)
                        {
                            docCursor.Doc.Deselect();
                            Editor.RefreshAllLines();
                        }
                        else
                        {
                            break;
                        }
                    }

                    HandleRight(docCursor);
                    break;
                
                // LINE MANIPULATION
                case ConsoleKey.Backspace:
                    if (ReadOnly)
                        break;
                    
                    HandleBackspace(docCursor);
                    break;
                case ConsoleKey.Delete:
                    if (ReadOnly)
                        break;

                    HandleDelete(docCursor);
                    break;
                case ConsoleKey.Enter:
                    if (ReadOnly)
                        break;

                    HandleEnter(docCursor);
                    break;
                case ConsoleKey.Tab:
                    if (ReadOnly)
                        break;
                    
                    HandleTab(docCursor, keyInfo);
                    break;

                // NAVIGATION
                case ConsoleKey.Escape:
                    HandleEscape(docCursor);
                    break;
                case ConsoleKey.Home:
                    if (doc.HasSelection())
                        break;
                    
                    HandleHome(docCursor);
                    break;
                case ConsoleKey.End:
                    if (doc.HasSelection())
                        break;
                    
                    HandleEnd(docCursor);
                    break;
                case ConsoleKey.PageDown:
                    if (doc.HasSelection())
                        break;
                    
                    HandlePageDown(docCursor);
                    break;
                case ConsoleKey.PageUp:
                    if (doc.HasSelection())
                        break;

                    HandlePageUp(docCursor);
                    break;
                
                // TYPING
                default:
                    if (ReadOnly) break;
                    HandleTyping(docCursor, keyInfo);
                    break;
            }
        }

        static void AcceptCommandInput(ConsoleKeyInfo keyInfo)
        {

            Cursor cmdCursor = Editor.CmdCursor;

            // allow shortcuts to be triggered while alt-key is pressed
            // when alt-tabbing, alt key will become pressed and never register
            // as unpressed
            if (keyInfo.Modifiers == ConsoleModifiers.Control ||
                keyInfo.Modifiers == (ConsoleModifiers.Control | ConsoleModifiers.Alt))
            {
                HandleShortcut(keyInfo);
                return;
            }

            switch(keyInfo.Key)
            {
                // MOVEMENT
                case ConsoleKey.LeftArrow:
                    int newLeftX = cmdCursor.DX - 1;
                    if(CommandLine.IsSafeCursorX(newLeftX))
                        cmdCursor.Move(newLeftX, 0);
                    break;
                case ConsoleKey.RightArrow:
                    int newRightX = cmdCursor.DX + 1;
                    if(CommandLine.IsSafeCursorX(newRightX))
                        cmdCursor.Move(newRightX, 0);
                    break;

                // HISTORY
                case ConsoleKey.UpArrow:
                    if (CommandLine.CommandHistoryIndex >= CommandLine.CommandHistory.Count - 1)
                        break;
                    CommandLine.CommandHistoryIndex++;
                    CommandLine.ClearInput();
                    CommandLine.AddText(0, CommandLine.CommandHistory[CommandLine.CommandHistoryIndex]);
                    cmdCursor.Move(CommandLine.InputText.Length, 0);
                    break;
                case ConsoleKey.DownArrow:
                    if (CommandLine.CommandHistoryIndex <= 0)
                    {
                        CommandLine.ClearInput();
                        CommandLine.CommandHistoryIndex = -1;
                        break;
                    }
                    CommandLine.CommandHistoryIndex--;
                    CommandLine.ClearInput();
                    CommandLine.AddText(0, CommandLine.CommandHistory[CommandLine.CommandHistoryIndex]);
                    cmdCursor.Move(CommandLine.InputText.Length, 0);
                    break;

                // LINE MANIPULATION
                case ConsoleKey.Backspace:
                    bool backspaceTriggered = CommandLine.RemoveText(cmdCursor.DX - 1);
                    
                    if(backspaceTriggered)
                        cmdCursor.Move(cmdCursor.DX - 1, 0);

                    CommandLine.CommandHistoryIndex = -1; //break out of command history
                    break;
                case ConsoleKey.Delete:
                    CommandLine.RemoveText(cmdCursor.DX);
                    break;

                // COMMANDS
                case ConsoleKey.Enter:
                    CommandLine.ExecuteInput();
                    break;
                case ConsoleKey.Escape:
                    CommandLine.ClearInput();
                    CommandLine.CommandHistoryIndex = -1;
                    ToggleInputTarget();
                    break;
                
                // TYPING
                default:                    
                    String typed = keyInfo.KeyChar.ToString();

                    // continue only if the typed character can be displayed
                    Regex r = new Regex("\\P{Cc}");
                    if(!r.Match(typed).Success)
                        break;
                    
                    bool addedText = CommandLine.AddText(Editor.CmdCursor.DX, typed);

                    if(addedText)
                    {
                        Editor.CmdCursor.Move(Editor.CmdCursor.DX + 1, 0);
                    }

                    CommandLine.CommandHistoryIndex = -1; //break out of command history
                    break;
            }
        }

        static void HandleUp(DocumentCursor c)
        {
            c.MoveUp(applyPosition: false);
            c.Move(maxCursorX, c.DY); //attempt to move to max x position

            //for debugtokens
            if (Settings.Properties.DebugTokens && c.DY + 1 <= c.Doc.GetMaxLine())
                Editor.RefreshLine(c.DY + 1);
        }

        static void HandleDown(DocumentCursor c)
        {
            c.MoveDown(applyPosition: false);
            c.Move(maxCursorX, c.DY); //attempt to move to max x position

            //for debugtokens
            if (Settings.Properties.DebugTokens && c.DY - 1 >= 0)
                Editor.RefreshLine(c.DY - 1);
        }

        public static void UpdateMaxCursorX(DocumentCursor c)
        {
            maxCursorX = c.DX;
        }

        static void HandleLeft(DocumentCursor c)
        {
            c.MoveLeft(applyPosition: false);
            c.ApplyPosition();
            UpdateMaxCursorX(c); //update max x position
        }

        static void HandleRight(DocumentCursor c)
        {
            if (Settings.Properties.NavigatePastTabs && c.Doc.GetTextAtPosition(c.DX, c.DY).StartsWith(tabString)) //can skip, do so
            {
                c.Move(c.DX + tabString.Length, c.DY, applyPosition: false);
            }
            else //can't skip, move normally
            {
                c.MoveRight(applyPosition: false);
            }
            
            c.ApplyPosition();
            UpdateMaxCursorX(c); //update max x position
        }

        static void HandleBackspace(DocumentCursor c)
        {
            if (c.Doc.HasSelection())
            {
                DeleteSelectionText(c);
                // undo event is logged in Document.RemoveSelectionText
                return;
            }

            Point initialCursorPos = new Point(c.DX, c.DY);
            int charsDeleted = 0;
            if(c.DX > 0) // not at the beginning of the line
            {
                if (Settings.Properties.DeleteEntireTabs // fancy tab delete
                    && c.DX >= Settings.Properties.TabSpacesCount
                    && c.Doc.GetTextAtPosition(c.DX - Settings.Properties.TabSpacesCount, c.DY).StartsWith(tabString))
                    {
                        string deletedChars = "";
                        for(int i = 0; i < Settings.Properties.TabSpacesCount; i++)
                        {
                            string backspaceChar = c.Doc.RemoveTextAtPosition(c.DX - 1, c.DY);
                            if (backspaceChar.Length != 0) // success!
                            {
                                deletedChars = deletedChars + backspaceChar;
                                c.MoveLeft(applyPosition: false);
                                charsDeleted++;
                            }
                        }

                        c.ApplyPosition();

                        // log history event
                        c.Doc.LogHistoryEvent(new HistoryEvent(
                            HistoryEventType.Remove,
                            deletedChars,
                            new Point(c.DX, c.DY),
                            initialCursorPos
                        ));
                    }
                    else // normal delete
                    {
                        string backspaceChar = c.Doc.RemoveTextAtPosition(
                            c.DX - 1,
                            c.DY);

                        if(backspaceChar.Length != 0) // success!
                        {
                            c.MoveLeft();
                            charsDeleted++;

                            // log history event
                            c.Doc.LogHistoryEvent(new HistoryEvent(
                                HistoryEventType.Remove,
                                backspaceChar,
                                new Point(c.DX, c.DY),
                                initialCursorPos
                            ));
                        }
                    }
            }
            else // at beginning of line, append current line to previous
            {
                if(c.DY > 0)
                {
                    string currentLineContent = c.Doc.GetLine(c.DY); // get remaining content on current line
                    int previousLineMaxX = c.Doc.GetLine(c.DY - 1).Length; // get max position on preceding line
                    string combinedLineContent = c.Doc.GetLine(c.DY - 1) + currentLineContent; // combine content
                    c.Doc.SetLine(c.DY - 1, combinedLineContent); // set previous line to combined content
                    c.Doc.RemoveLine(c.DY); // remove current line
                    c.UpdateGutterOffset();

                    c.Move(c.DX, c.DY - 1);

                    // scroll up if necessary
                    if(c.SY == 0)
                    {
                        c.Doc.ScrollUp();
                    }

                    // update
                    c.Move(previousLineMaxX, c.DY);
                    Editor.RefreshAllLines();

                    c.Doc.LogHistoryEvent(new HistoryEvent(
                        HistoryEventType.RemoveLine,
                        "",
                        new Point(c.DX, c.DY),
                        initialCursorPos
                    ));
                }
            }

            Editor.RefreshLine(c.DY);

            maxCursorX = c.DX; // update max x position
        }

        static void HandleDelete(DocumentCursor c)
        {
            if (c.Doc.HasSelection())
            {
                DeleteSelectionText(c);
                // undo event is logged in Document.RemoveSelectionText
                return;
            }

            Point initialCursorPos = new Point(c.DX, c.DY);
            if(c.DX == c.Doc.GetLine(c.DY).Length) // deleting at end of line
            {
                if(c.DY < c.Doc.GetMaxLine()) // there is a following line to append
                {
                    string followingLineText = c.Doc.GetLine(c.DY + 1); // get following line content
                    c.Doc.SetLine(c.DY, c.Doc.GetLine(c.DY) + followingLineText); // append to current
                    c.Doc.RemoveLine(c.DY + 1); // remove next line
                    
                    // update
                    c.UpdateGutterOffset();
                    Editor.RefreshAllLines();

                    c.Doc.LogHistoryEvent(new HistoryEvent(
                        HistoryEventType.RemoveLine,
                        "",
                        new Point(c.DX, c.DY),
                        initialCursorPos
                    ));
                }
            }
            else // basic delete
            {
                string deletedChars = "";
                if (Settings.Properties.DeleteEntireTabs && c.Doc.GetTextAtPosition(c.DX, c.DY).StartsWith(tabString))
                {
                    for (int i = 0; i < Settings.Properties.TabSpacesCount; i++)
                        deletedChars = deletedChars + c.Doc.RemoveTextAtPosition(c.DX, c.DY);
                }
                else
                    deletedChars = c.Doc.RemoveTextAtPosition(c.DX, c.DY); // remove next character
                c.Doc.LogHistoryEvent(new HistoryEvent(
                    HistoryEventType.Remove,
                    deletedChars,
                    new Point(c.DX, c.DY),
                    initialCursorPos
                ));
                Editor.RefreshLine(c.DY); // update line
            }
        }

        static void HandleEnter(DocumentCursor c)
        {
            //don't break after clearing selection since we still want a newline
            bool deletedSelection = false;
            if (c.Doc.HasSelection())
            {
                DeleteSelectionText(c);
                deletedSelection = true;
                // undo event is logged in Document.RemoveSelectionText
            }

            Point initialCursorPos = new Point(c.DX, c.DY);

            //insert tab string at beginning of new line
            string newLineTabString = "";
            if (Settings.Properties.PreserveIndentOnEnter)
            {
                string lineTabSearchString = c.Doc.GetLine(c.DY);
                while (lineTabSearchString.StartsWith(tabString))
                {
                    newLineTabString += tabString;
                    lineTabSearchString = lineTabSearchString.Remove(0, tabString.Length);
                }
            }

            c.Doc.AddLine(c.DY + 1); // add new line
            c.UpdateGutterOffset(); // update gutter position

            string followingTextLine = c.Doc.GetLine(c.DY);
            string followingText = followingTextLine.Substring(c.DX); // get text following cursor (on current line)

            // remove tab string from beginning of following line
            if (Settings.Properties.PreserveIndentOnEnter)
            {
                while (followingText.StartsWith(tabString))
                {
                    followingText = followingText.Remove(0, tabString.Length);
                }
            }

            c.Doc.AddTextAtPosition(0, c.DY + 1, newLineTabString + followingText); // add following text to new line

            if(c.DX < followingTextLine.Length)
                c.Doc.SetLine(c.DY, followingTextLine.Remove(c.DX)); // remove following text on current line
            
            // scroll down if necessary
            if(c.SY >= Printer.MaxScreenY - Footer.FooterHeight)
            {
                c.Doc.ScrollDown();
            }

            // update
            c.Move(newLineTabString.Length, c.DY + 1);
            Editor.RefreshAllLines();
            maxCursorX = c.DX;

            c.Doc.LogHistoryEvent(new HistoryEvent(
                HistoryEventType.AddLine,
                c.Doc.GetLine(c.DY),
                initialCursorPos,
                initialCursorPos,
                combined: deletedSelection
            ));
        }

        static void HandleEscape(DocumentCursor c)
        {
            CommandLine.ClearInput();
            CommandLine.CommandHistoryIndex = -1;
            ToggleInputTarget();
        }

        static void HandleTab(DocumentCursor c, ConsoleKeyInfo keyInfo)
        {
            Point initialCursorPos = new Point(c.DX, c.DY);
            if (Settings.Properties.ShiftDeindent && keyInfo.Modifiers == ConsoleModifiers.Shift)
            {
                c.Doc.Deindent();
                c.Doc.LogHistoryEvent(new HistoryEvent(
                    HistoryEventType.DeindentLine,
                    "",
                    new Point(c.DX, c.DY),
                    initialCursorPos,
                    new Point[] {
                        new Point(c.Doc.SelectInX, c.Doc.SelectInY),
                        new Point(c.Doc.SelectOutX, c.Doc.SelectOutY)
                    }
                ));

                return;
            }

            //if selected, indent all
            if (c.Doc.HasSelection())
            {
                for (int i = c.Doc.SelectInY; i <= c.Doc.SelectOutY; i++)
                {
                    c.Doc.AddTextAtPosition(0, i, tabString);
                }

                c.Doc.MarkSelectionIn(c.Doc.SelectInX + tabString.Length, c.Doc.SelectInY);
                c.Doc.MarkSelectionOut(c.Doc.SelectOutX + tabString.Length, c.Doc.SelectOutY);
                Editor.DocCursor.Move(Editor.DocCursor.DX + tabString.Length, Editor.DocCursor.DY);

                //rerender all
                Editor.RefreshAllLines();

                // TODO: add undo support
                c.Doc.LogHistoryEvent(new HistoryEvent(
                    HistoryEventType.IndentLine,
                    "",
                    new Point(c.DX, c.DY),
                    initialCursorPos,
                    new Point[] {
                        new Point(c.Doc.SelectInX, c.Doc.SelectInY),
                        new Point(c.Doc.SelectOutX, c.Doc.SelectOutY)
                    }
                ));

                return;
            }

            bool tabTextAdded = c.Doc.AddTextAtPosition(c.DX, c.DY, tabString); //attempt to add tab text
            
            if(tabTextAdded)
            {
                c.Doc.LogHistoryEvent(new HistoryEvent(
                    HistoryEventType.Add,
                    tabString,
                    new Point(c.DX, c.DY),
                    initialCursorPos
                ));
                c.Move(c.DX + tabString.Length, c.DY);
                Editor.RefreshLine(c.DY);
            }

            maxCursorX = c.DX; //update max x position
        }

        static void HandleHome(DocumentCursor c)
        {
            string tabSearchString = c.Doc.GetLine(c.DY);
            int tabSpaceCount = 0;
            while (tabSearchString.StartsWith(tabString))
            {
                tabSpaceCount += tabString.Length;
                tabSearchString = tabSearchString.Remove(0, tabString.Length);
            }
            
            if (c.DX <= tabSpaceCount) //if document is already at tab string pos, go all the way to 0
                tabSpaceCount = 0;
            
            c.Move(tabSpaceCount, c.DY);
            maxCursorX = c.DX;
        }

        static void HandleEnd(DocumentCursor c)
        {
            c.Move(c.Doc.GetLine(c.DY).Length, c.DY); //move to end of line
            maxCursorX = c.DX; //update max x position
        }

        static void HandlePageUp(DocumentCursor c)
        {
            c.Move(c.DX, c.DY - c.Doc.ScrollYIncrement);
        }

        static void HandlePageDown(DocumentCursor c)
        {
            c.Move(c.DX, c.DY + c.Doc.ScrollYIncrement);
        }

        static void HandleTyping(DocumentCursor c, ConsoleKeyInfo keyInfo)
        {
            String typed = keyInfo.KeyChar.ToString();
            // continue only if the typed character can be displayed
            Regex r = new Regex("\\P{Cc}");
            if(!r.Match(typed).Success)
                return;

            Point initialCursorPos = new Point(c.DX, c.DY);

            // clear selection before typing
            bool deletedSelection = false;
            if (c.Doc.HasSelection())
            {
                DeleteSelectionText(c);
                // undo event is logged in Document.RemoveSelectionText
                deletedSelection = true;
            }

            int oldTokenCount = c.Doc.GetLineTokenCount(c.DY); // track old token count to determine if redraw is necessary
            bool addedText = c.Doc.AddTextAtPosition(c.DX, c.DY, typed);
            if(addedText)
            {
                c.Doc.LogHistoryEvent(new HistoryEvent(
                    HistoryEventType.Add,
                    typed,
                    new Point(c.DX, c.DY),
                    initialCursorPos,
                    combined: deletedSelection
                ));
                c.MoveRight();
                Editor.RefreshLine(c.DY);
            }
            // attempt autocomplete, if enabled
            if ((Settings.Properties.Autocomplete && Settings.Properties.AutocompleteOnlyAtEOL && c.DX == c.Doc.GetLine(c.DY).Length) ||
                (Settings.Properties.Autocomplete && !Settings.Properties.AutocompleteOnlyAtEOL))
            {
                if (Lexer.Properties.AutocompleteTable.ContainsKey(keyInfo.KeyChar))
                {
                    string autocompleteText = Lexer.Properties.AutocompleteTable[keyInfo.KeyChar].ToString();
                    c.Doc.AddTextAtPosition(c.DX, c.DY, autocompleteText);
                    // autocomplete events are logged in history separately
                    c.Doc.LogHistoryEvent(new HistoryEvent(
                        HistoryEventType.Add,
                        autocompleteText,
                        new Point(c.DX, c.DY),
                        initialCursorPos,
                        combined: true
                    ));
                }
            }
            maxCursorX = c.DX; // update max x position
        }

        static void HandleShortcut(ConsoleKeyInfo keyInfo)
        {
            // skip if shortcut doesn't exist
            if (!Settings.Properties.ShortcutsTable.ContainsKey(keyInfo.Key))
                return;

            Shortcut shortcutInfo = Settings.Properties.ShortcutsTable[keyInfo.Key];

            InputTarget previousInputTarget = CurrentTarget;
            CurrentTarget = InputTarget.Command; // simulate user entering cli
            CommandLine.InputText = shortcutInfo.Command;
            Editor.CmdCursor.Move(CommandLine.InputText.Length, 0);

            if (shortcutInfo.Execute && previousInputTarget == InputTarget.Document)
            {
                CommandLine.ExecuteInput();
            }
        }

        public static void ToggleInputTarget()
        {
            Footer.PrintFooter();
            if(CurrentTarget == InputTarget.Document)
            {
                if (Settings.Properties.ClearOutputOnToggle)
                {
                    CommandLine.ClearOutput();
                }
                CurrentTarget = InputTarget.Command; //there is no output, toggle
                Editor.CmdCursor.Move(0, 0); //reset cursor position
            }
            else if(CurrentTarget == InputTarget.Command)
            {
                CurrentTarget = InputTarget.Document;
            }
        }

        /// <summary>
        /// Delete the text contained within the cursor's Document's selection bounds, move the cursor, and refresh the Editor appropriately.
        /// </summary>
        /// <param name="docCursor">The DocumentCursor to move. Text will be deleted from the cursor's Document.</param>
        public static void DeleteSelectionText(DocumentCursor docCursor)
        {
            docCursor.Doc.RemoveSelectionText();

            //reset cursor, clear selection and rerender all
            docCursor.Move(docCursor.Doc.SelectInX, docCursor.Doc.SelectInY);
            docCursor.Doc.Deselect();
            Editor.RefreshAllLines();
        }

    }
}
