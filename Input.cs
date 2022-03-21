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

        public static bool ReadOnly { get; set; } = false;

        public static void AcceptInput(ConsoleKeyInfo keyInfo)
        {
            //build tab string if necessary
            if (tabString.Length == 0)
            {
                for (int i = 0; i < Settings.TabSpacesCount; i++)
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

            switch(keyInfo.Key)
            {
                //MOVEMENT
                case ConsoleKey.UpArrow:
                    if (doc.HasSelection()) break;
                    docCursor.MoveUp();
                    docCursor.Move(maxCursorX, docCursor.DY); //attempt to move to max x position
                    //for debugtokens
                    if (Settings.DebugTokens && docCursor.DY + 1 <= doc.GetMaxLine())
                        Editor.RefreshLine(docCursor.DY + 1);
                    break;
                case ConsoleKey.DownArrow:
                    if (doc.HasSelection()) break;
                    docCursor.MoveDown();
                    docCursor.Move(maxCursorX, docCursor.DY); //attempt to move to max x position
                    //for debugtokens
                    if (Settings.DebugTokens && docCursor.DY - 1 >= 0)
                        Editor.RefreshLine(docCursor.DY - 1);
                    break;
                case ConsoleKey.LeftArrow:
                    if (doc.HasSelection()) break;
                    docCursor.MoveLeft();
                    maxCursorX = docCursor.DX; //update max x position
                    break;
                case ConsoleKey.RightArrow:
                    if (doc.HasSelection()) break;
                    if (Settings.NavigatePastTabs && docCursor.Doc.GetTextAtPosition(docCursor.DX, docCursor.DY).StartsWith(tabString)) //can skip, do so
                    {
                        for (int i = 0; i < Settings.TabSpacesCount; i++)
                            docCursor.MoveRight();
                    }
                    else //can't skip, move normally
                        docCursor.MoveRight();
                    maxCursorX = docCursor.DX; //update max x position
                    break;
                
                //LINE MANIPULATION
                case ConsoleKey.Backspace:
                    if (ReadOnly) break;

                    if (doc.HasSelection())
                    {
                        DeleteSelectionText(docCursor);
                        break;
                    }

                    int charsDeleted = 0;
                    if(docCursor.DX > 0) //not at the beginning of the line
                    {
                        if (Settings.DeleteEntireTabs //fancy tab delete
                            && docCursor.DX >= Settings.TabSpacesCount
                            && docCursor.Doc.GetTextAtPosition(docCursor.DX - Settings.TabSpacesCount, docCursor.DY).StartsWith(tabString))
                            {
                                for(int i = 0; i < Settings.TabSpacesCount; i++)
                                {
                                    bool backspaceTriggered = doc.RemoveTextAtPosition(docCursor.DX - 1, docCursor.DY);
                                    if (backspaceTriggered)
                                    {
                                        docCursor.MoveLeft();
                                        charsDeleted++;
                                    }
                                }
                            }
                            else
                            {
                                bool backspaceTriggered = doc.RemoveTextAtPosition(
                                    docCursor.DX - 1,
                                    docCursor.DY);

                                if(backspaceTriggered)
                                {
                                    docCursor.MoveLeft();
                                    charsDeleted++;
                                }
                            }
                    }
                    else //at beginning of line, append current line to previous
                    {
                        bool backspaceScrolledUp = false;
                        if(docCursor.DY > 0)
                        {
                            string currentLineContent = doc.GetLine(docCursor.DY); //get remaining content on current line
                            int previousLineMaxX = doc.GetLineLength(docCursor.DY - 1); //get max position on preceding line
                            string combinedLineContent = doc.GetLine(docCursor.DY - 1) + currentLineContent; //combine content
                            doc.SetLine(docCursor.DY - 1, combinedLineContent); //set previous line to combined content
                            doc.RemoveLine(docCursor.DY); //remove current line
                            docCursor.CalculateGutterWidth();

                            docCursor.Move(docCursor.DX, docCursor.DY - 1);

                            //scroll up if necessary
                            if(docCursor.SY == 0)
                            {
                                doc.ScrollUp();
                                backspaceScrolledUp = true;
                            }

                            docCursor.Move(previousLineMaxX, docCursor.DY); //move cursor to preceding line
                        }
                        //update all lines below
                        if(!backspaceScrolledUp)
                        {
                            for(int i = docCursor.DY; i <= doc.GetMaxLine() + 1; i++) //+1 so that the old line is cleared
                                Editor.RefreshLine(i);
                        }
                        else
                            Editor.RefreshAllLines();
                    }

                    Editor.RefreshLine(docCursor.DY);

                    maxCursorX = docCursor.DX; //update max x position
                    break;
                case ConsoleKey.Delete:
                    if (ReadOnly) break;

                    if (doc.HasSelection())
                    {
                        DeleteSelectionText(docCursor);
                        break;
                    }

                    if(docCursor.DX == doc.GetLineLength(docCursor.DY)) //deleting at end of line
                    {
                        if(docCursor.DY < doc.GetMaxLine()) //there is a following line to append
                        {
                            string followingLineText = doc.GetLine(docCursor.DY + 1); //get following line content
                            doc.SetLine(docCursor.DY, doc.GetLine(docCursor.DY) + followingLineText); //append to current
                            doc.RemoveLine(docCursor.DY + 1); //remove next line
                            docCursor.CalculateGutterWidth();
                            for(int i = docCursor.DY; i < doc.GetMaxLine() + 1; i++)
                                Editor.RefreshLine(i);
                        }
                    }
                    else //basic delete
                    {
                        if (Settings.DeleteEntireTabs && docCursor.Doc.GetTextAtPosition(docCursor.DX, docCursor.DY).StartsWith(tabString))
                        {
                            for (int i = 0; i < Settings.TabSpacesCount; i++)
                                doc.RemoveTextAtPosition(docCursor.DX, docCursor.DY);
                        }
                        else
                            doc.RemoveTextAtPosition(docCursor.DX, docCursor.DY); //remove next character
                        Editor.RefreshLine(docCursor.DY); //update line
                    }
                    break;
                case ConsoleKey.Enter:
                    if (ReadOnly) break;

                    //don't break after clearing selection since we still want a newline
                    if (doc.HasSelection())
                        DeleteSelectionText(docCursor);

                    doc.AddLine(docCursor.DY + 1); //add new line
                    docCursor.CalculateGutterWidth(); //update gutter position

                    string followingTextLine = doc.GetLine(docCursor.DY);
                    string followingText = followingTextLine.Substring(docCursor.DX); //get text following cursor (on current line)

                    doc.AddTextAtPosition(0, docCursor.DY + 1, followingText); //add following text to new line

                    if(docCursor.DX < followingTextLine.Length)
                        doc.SetLine(docCursor.DY, followingTextLine.Remove(docCursor.DX)); //remove following text on current line
                    
                    //scroll down if necessary
                    bool enterScrolledDown = false;
                    if(docCursor.SY >= Cursor.MaxScreenY - 1)
                    {
                        doc.ScrollDown();
                        enterScrolledDown = true;
                    }

                    docCursor.Move(0, docCursor.DY + 1); //move cursor to beginning of new line
                    Editor.RefreshLine(docCursor.SY);

                    //update all lines below
                    if(!enterScrolledDown)
                    {
                        for(int i = docCursor.DY - 1; i <= doc.GetMaxLine(); i++)
                            Editor.RefreshLine(i);
                    }
                    else
                        Editor.RefreshAllLines();

                    maxCursorX = docCursor.DX; //update max x position
                    break;
                case ConsoleKey.Escape:
                    ToggleInputTarget();
                    break;
                case ConsoleKey.Tab:
                    if (ReadOnly) break;

                    //if selected, indent all
                    if (doc.HasSelection())
                    {
                        for (int i = doc.SelectInY; i <= doc.SelectOutY; i++)
                            doc.AddTextAtPosition(0, i, tabString);

                        //rerender all
                        Printer.Clear();
                        Editor.RefreshAllLines();
                        Editor.RedrawLines();
                        break;
                    }

                    bool tabTextAdded = doc.AddTextAtPosition(docCursor.DX, docCursor.DY, tabString); //attempt to add tab text
                    
                    if(tabTextAdded)
                    {
                        for(int i = 0; i < Settings.TabSpacesCount; i++) //move cursor forward as appropriate
                            docCursor.MoveRight();
                        Editor.RefreshLine(docCursor.DY);
                    }

                    maxCursorX = docCursor.DX; //update max x position
                    break;
                case ConsoleKey.Home:
                    if (doc.HasSelection())
                        break;
                    docCursor.Move(0, docCursor.DY); //move to beginning of line
                    maxCursorX = docCursor.DX; //update max x position
                    break;
                case ConsoleKey.End:
                    if (doc.HasSelection())
                        break;
                    docCursor.Move(doc.GetLineLength(docCursor.DY), docCursor.DY); //move to end of line
                    maxCursorX = docCursor.DX; //update max x position
                    break;
                case ConsoleKey.PageUp:
                    if (doc.HasSelection())
                        break;
                    docCursor.Move(docCursor.DX, docCursor.DY - doc.ScrollYIncrement);
                    break;
                case ConsoleKey.PageDown:
                    if (doc.HasSelection())
                        break;
                    docCursor.Move(docCursor.DX, docCursor.DY + doc.ScrollYIncrement);
                    break;
                //TYPING
                default:
                    if (ReadOnly) break;

                    //clear selection before typing
                    if (doc.HasSelection())
                        DeleteSelectionText(docCursor);

                    String typed = keyInfo.KeyChar.ToString();
                    //continue only if the typed character can be displayed
                    Regex r = new Regex("\\P{Cc}");
                    if(!r.Match(typed).Success)
                        break;

                    int oldTokenCount = doc.GetLineTokenCount(docCursor.DY); //track old token count to determine if redraw is necessary
                    bool addedText = doc.AddTextAtPosition(docCursor.DX, docCursor.DY, typed);
                    if(addedText)
                    {
                        docCursor.MoveRight();
                        Editor.RefreshLine(docCursor.DY);
                    }
                    maxCursorX = docCursor.DX; //update max x position
                    break;
            }
        }

        static void AcceptCommandInput(ConsoleKeyInfo keyInfo)
        {

            Cursor cmdCursor = Editor.CmdCursor;

            switch(keyInfo.Key)
            {
                //MOVEMENT
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

                //HISTORY
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

                //LINE MANIPULATION
                case ConsoleKey.Backspace:
                    bool backspaceTriggered = CommandLine.RemoveText(cmdCursor.DX - 1);
                    
                    if(backspaceTriggered)
                        cmdCursor.Move(cmdCursor.DX - 1, 0);

                    CommandLine.CommandHistoryIndex = -1; //break out of command history
                    break;
                case ConsoleKey.Delete:
                    CommandLine.RemoveText(cmdCursor.DX);
                    break;

                //COMMANDS
                case ConsoleKey.Enter:
                    CommandLine.ExecuteInput();
                    break;
                case ConsoleKey.Escape:
                    CommandLine.ClearInput();
                    CommandLine.CommandHistoryIndex = -1;
                    ToggleInputTarget();
                    break;
                
                //TYPING
                default:                    
                    String typed = keyInfo.KeyChar.ToString();

                    //continue only if the typed character can be displayed
                    Regex r = new Regex("\\P{Cc}");
                    if(!r.Match(typed).Success)
                        break;
                    
                    bool addedText = CommandLine.AddText(Editor.CmdCursor.DX, typed);

                    if(addedText)
                        Editor.CmdCursor.DX++;

                    CommandLine.CommandHistoryIndex = -1; //break out of command history

                    break;
            }
        }

        public static void ToggleInputTarget()
        {
            Editor.PrintFooter();
            if(CurrentTarget == InputTarget.Document)
            {
                //check if there is command output to be cleared
                if(!CommandLine.OutputText.Equals("") && !Settings.QuickCli)
                    CommandLine.ClearOutput(); //there is output, clear it
                else
                {
                    CommandLine.ClearOutput();
                    CurrentTarget = InputTarget.Command; //there is no output, toggle
                    Editor.CmdCursor.Move(0, 0); //reset cursor position
                }
            }
            else if(CurrentTarget == InputTarget.Command)
                CurrentTarget = InputTarget.Document;
        }

        //this function is pretty messy, definitely refactor later
        static void DeleteSelectionText(DocumentCursor docCursor)
        {
            Document doc = docCursor.Doc;

            if (doc.HasSelection())
            {
                if (doc.SelectOutY > doc.SelectInY) //multi-line selection
                {
                    for (int i = doc.SelectInY; i <= doc.SelectOutY; i++) {
                        if (i == doc.SelectInY) { //trim end
                            doc.SetLine(i,
                                doc.GetLine(i).Remove(doc.SelectInX, doc.GetLineLength(i) - doc.SelectInX)
                                );                            
                        }
                        else if (i == doc.SelectOutY) { //trim start
                            string lastLineContent = doc.GetLine(i).Remove(0, doc.SelectOutX);
                            doc.SetLine(i - 1,
                                        doc.GetLine(i - 1) + lastLineContent
                                        );
                        }
                        else { //remove
                            doc.RemoveLine(i);
                            i--;
                            doc.MarkSelectionOut(doc.SelectOutX, doc.SelectOutY - 1); //move select out down
                        }
                    }
                }
                else //one line
                {
                    //remove from line
                    doc.SetLine(
                        doc.SelectInY,
                        doc.GetLine(doc.SelectInY).Remove(
                            doc.SelectInX, doc.SelectOutX - doc.SelectInX
                        )
                    );
                }

                //reset cursor, clear selection and rerender all
                docCursor.Move(doc.SelectInX, doc.SelectInY);
                doc.Deselect();
                Printer.Clear();
                Editor.RefreshAllLines();
                Editor.RedrawLines();
            }
        }

    }
}