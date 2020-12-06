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

        static InputTarget currentTarget = InputTarget.Document;

        public static void AcceptInput(ConsoleKeyInfo keyInfo)
        {

            if(currentTarget == InputTarget.Document)
                AcceptDocumentInput(keyInfo);
            else if(currentTarget == InputTarget.Command)
                AcceptCommandInput(keyInfo);

        }

        static void AcceptDocumentInput(ConsoleKeyInfo keyInfo)
        {

            DocumentCursor docCursor = Editor.GetDocCursor();
            Document doc = docCursor.GetDocument();

            switch(keyInfo.Key)
            {
                //MOVEMENT
                case ConsoleKey.UpArrow:
                    docCursor.MoveUp();
                    break;
                case ConsoleKey.DownArrow:
                    docCursor.MoveDown();
                    break;
                case ConsoleKey.LeftArrow:
                    docCursor.MoveLeft();
                    break;
                case ConsoleKey.RightArrow:
                    docCursor.MoveRight();
                    break;
                
                //LINE MANIPULATION
                case ConsoleKey.Backspace:
                    if(docCursor.dX > 0) //not at the beginning of the line
                    {
                        bool backspaceTriggered = doc.RemoveTextAtPosition(
                            docCursor.dX - 1,
                            docCursor.dY);

                        if(backspaceTriggered)
                            docCursor.MoveLeft();
                    }
                    else //at beginning of line, append current line to previous
                    {
                        
                        bool backspaceScrolledUp = false;
                        if(docCursor.dY > 0)
                        {
                            String currentLineContent = doc.GetLine(docCursor.dY); //get remaining content on current line
                            int previousLineMaxX = doc.GetLineLength(docCursor.dY - 1); //get max position on preceding line
                            String combinedLineContent = doc.GetLine(docCursor.dY - 1) + currentLineContent; //combine content
                            doc.SetLine(docCursor.dY - 1, combinedLineContent); //set previous line to combined content
                            doc.RemoveLine(docCursor.dY); //remove current line

                            docCursor.SetDocPosition(docCursor.dX, docCursor.dY - 1);

                            //scroll up if necessary
                            if(docCursor.sY == 0)
                            {
                                doc.ScrollUp();
                                backspaceScrolledUp = true;
                            }

                            docCursor.SetDocPosition(previousLineMaxX, docCursor.dY); //move cursor to preceding line
                        }
                        //update all lines below
                        if(!backspaceScrolledUp)
                        {
                            for(int i = docCursor.dY; i <= doc.GetMaxLine() + 1; i++) //+1 so that the old line is cleared
                                Editor.RefreshLine(i);
                        }
                        else
                            Editor.RefreshAllLines();
                    }

                    Editor.RefreshLine(docCursor.dY);
                    break;
                case ConsoleKey.Delete:
                    if(docCursor.dX == doc.GetLineLength(docCursor.dY)) //deleting at end of line
                    {
                        if(docCursor.dY < doc.GetMaxLine()) //there is a following line to append
                        {
                            String followingLineText = doc.GetLine(docCursor.dY + 1); //get following line content
                            doc.SetLine(docCursor.dY, doc.GetLine(docCursor.dY) + followingLineText); //append to current
                            doc.RemoveLine(docCursor.dY + 1); //remove next line
                            for(int i = docCursor.dY; i < doc.GetMaxLine() + 1; i++)
                                Editor.RefreshLine(i);
                        }
                    }
                    else
                    {
                        doc.RemoveTextAtPosition(docCursor.dX, docCursor.dY); //remove next character
                        Editor.RefreshLine(docCursor.dY); //update line
                    }
                    break;
                case ConsoleKey.Enter:
                    doc.AddLine(docCursor.dY + 1); //add new line

                    String followingTextLine = doc.GetLine(docCursor.dY);
                    String followingText = followingTextLine.Substring(docCursor.dX); //get text following cursor (on current line)

                    doc.AddTextAtPosition(0, docCursor.dY + 1, followingText); //add following text to new line

                    if(docCursor.dX < followingTextLine.Length)
                        doc.SetLine(docCursor.dY, followingTextLine.Remove(docCursor.dX)); //remove following text on current line
                    
                    //scroll down if necessary
                    bool enterScrolledDown = false;
                    if(docCursor.sY >= Cursor.maxScreenY - 1)
                    {
                        doc.ScrollDown();
                        enterScrolledDown = true;
                    }

                    docCursor.SetDocPosition(0, docCursor.dY + 1); //move cursor to beginning of new line
                    Editor.RefreshLine(docCursor.sY);

                    //update all lines below
                    if(!enterScrolledDown)
                    {
                        for(int i = docCursor.dY - 1; i <= doc.GetMaxLine(); i++)
                            Editor.RefreshLine(i);
                    }
                    else
                        Editor.RefreshAllLines();
                    break;
                case ConsoleKey.Escape:
                    ToggleInputTarget();
                    break;
                case ConsoleKey.Tab:
                    bool tabText = doc.AddTextAtPosition(docCursor.dX, docCursor.dY, "    ");
                    
                    if(tabText)
                    {
                        for(int i = 0; i < 4; i++)
                            docCursor.MoveRight();
                        Editor.RefreshLine(docCursor.dY);
                    }
                    break;
                //TYPING
                default:                    
                    String typed = keyInfo.KeyChar.ToString();

                    //continue only if the typed character can be displayed
                    Regex r = new Regex("\\P{Cc}");
                    if(!r.Match(typed).Success)
                        break;

                    bool addedText = doc.AddTextAtPosition(docCursor.dX, docCursor.dY, typed);
                    
                    if(addedText)
                    {
                        docCursor.MoveRight();
                        Editor.RefreshLine(docCursor.dY);
                    }

                    break;
            }
        }

        static void AcceptCommandInput(ConsoleKeyInfo keyInfo)
        {

            Cursor cmdCursor = Editor.GetCommandCursor();

            switch(keyInfo.Key)
            {
                //MOVEMENT
                case ConsoleKey.LeftArrow:
                    int newLeftX = cmdCursor.dX - 1;
                    if(CommandLine.IsSafeCursorX(newLeftX))
                        cmdCursor.Move(newLeftX, 0);
                    break;
                case ConsoleKey.RightArrow:
                    int newRightX = cmdCursor.dX + 1;
                    if(CommandLine.IsSafeCursorX(newRightX))
                        cmdCursor.Move(newRightX, 0);
                    break;

                //LINE MANIPULATION
                case ConsoleKey.Backspace:
                    bool backspaceTriggered = CommandLine.RemoveText(cmdCursor.dX - 1);
                    
                    if(backspaceTriggered)
                        cmdCursor.Move(cmdCursor.dX - 1, 0);
                    break;
                case ConsoleKey.Delete:
                    CommandLine.RemoveText(cmdCursor.dX);
                    break;

                //COMMANDS
                case ConsoleKey.Enter:
                    CommandLine.ExecuteInput();
                    break;
                case ConsoleKey.Escape:
                    CommandLine.ClearInput();
                    ToggleInputTarget();
                    break;
                
                //TYPING
                default:                    
                    String typed = keyInfo.KeyChar.ToString();

                    //continue only if the typed character can be displayed
                    Regex r = new Regex("\\P{Cc}");
                    if(!r.Match(typed).Success)
                        break;
                    
                    bool addedText = CommandLine.AddText(Editor.GetCommandCursor().dX, typed);

                    if(addedText)
                        Editor.GetCommandCursor().dX++;

                    break;
            }
        }

        public static InputTarget GetInputTarget()
        {
            return currentTarget;
        }

        public static void ToggleInputTarget()
        {
            if(currentTarget == InputTarget.Document)
            {
                //check if there is command output to be cleared
                if(CommandLine.GetOutput() != "" && !Settings.quickCli)
                    CommandLine.ClearOutput(); //there is output, clear it
                else
                {
                    CommandLine.ClearOutput();
                    currentTarget = InputTarget.Command; //there is no output, toggle
                    Editor.GetCommandCursor().Move(0, 0); //reset cursor position
                }
            }
            else if(currentTarget == InputTarget.Command)
                currentTarget = InputTarget.Document;
        }

    }
}