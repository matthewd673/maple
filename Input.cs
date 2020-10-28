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
            switch(keyInfo.Key)
            {
                //MOVEMENT
                case ConsoleKey.UpArrow:
                    Program.GetCursor().MoveUp();
                    break;
                case ConsoleKey.DownArrow:
                    Program.GetCursor().MoveDown();
                    break;
                case ConsoleKey.LeftArrow:
                    Program.GetCursor().MoveLeft();
                    break;
                case ConsoleKey.RightArrow:
                    Program.GetCursor().MoveRight();
                    break;
                
                //LINE MANIPULATION
                case ConsoleKey.Backspace:
                    if(Program.GetCursor().dX > 0) //not at the beginning of the line
                    {
                        bool backspaceTriggered = Program.GetDocument().RemoveTextAtPosition(
                            Program.GetCursor().dX - 1,
                            Program.GetCursor().dY);

                        if(backspaceTriggered)
                            Program.GetCursor().MoveLeft();
                    }
                    else //at beginning of line, append current line to previous
                    {
                        
                        bool backspaceScrolledUp = false;
                        if(Program.GetCursor().dY > 0)
                        {
                            String currentLineContent = Program.GetDocument().GetLine(Program.GetCursor().dY); //get remaining content on current line
                            int previousLineMaxX = Program.GetDocument().GetLineLength(Program.GetCursor().dY - 1); //get max position on preceding line
                            String combinedLineContent = Program.GetDocument().GetLine(Program.GetCursor().dY - 1) + currentLineContent; //combine content
                            Program.GetDocument().SetLine(Program.GetCursor().dY - 1, combinedLineContent); //set previous line to combined content
                            Program.GetDocument().RemoveLine(Program.GetCursor().dY); //remove current line

                            Program.GetCursor().SetDocPosition(Program.GetCursor().dX, Program.GetCursor().dY - 1);

                            //scroll up if necessary
                            if(Program.GetCursor().sY == 0)
                            {
                                Program.GetDocument().ScrollUp();
                                backspaceScrolledUp = true;
                            }

                            Program.GetCursor().SetDocPosition(previousLineMaxX, Program.GetCursor().dY); //move cursor to preceding line
                        }
                        //update all lines below
                        if(!backspaceScrolledUp)
                        {
                            for(int i = Program.GetCursor().dY; i <= Program.GetDocument().GetMaxLine() + 1; i++) //+1 so that the old line is cleared
                                Program.RefreshLine(i);
                        }
                        else
                            Program.RefreshAllLines();
                    }

                    Program.RefreshLine(Program.GetCursor().dY);
                    break;
                case ConsoleKey.Delete:
                    if(Program.GetCursor().dX == Program.GetDocument().GetLineLength(Program.GetCursor().dY)) //deleting at end of line
                    {
                        if(Program.GetCursor().dY < Program.GetDocument().GetMaxLine()) //there is a following line to append
                        {
                            String followingLineText = Program.GetDocument().GetLine(Program.GetCursor().dY + 1); //get following line content
                            Program.GetDocument().SetLine(Program.GetCursor().dY, Program.GetDocument().GetLine(Program.GetCursor().dY) + followingLineText); //append to current
                            Program.GetDocument().RemoveLine(Program.GetCursor().dY + 1); //remove next line
                            for(int i = Program.GetCursor().dY; i < Program.GetDocument().GetMaxLine() + 1; i++)
                                Program.RefreshLine(i);
                        }
                    }
                    else
                    {
                        Program.GetDocument().RemoveTextAtPosition(Program.GetCursor().dX, Program.GetCursor().dY); //remove next character
                        Program.RefreshLine(Program.GetCursor().dY); //update line
                    }
                    break;
                case ConsoleKey.Enter:
                    Program.GetDocument().AddLine(Program.GetCursor().dY + 1); //add new line

                    String followingTextLine = Program.GetDocument().GetLine(Program.GetCursor().dY);
                    String followingText = followingTextLine.Substring(Program.GetCursor().dX); //get text following cursor (on current line)

                    Program.GetDocument().AddTextAtPosition(0, Program.GetCursor().dY + 1, followingText); //add following text to new line

                    if(Program.GetCursor().dX < followingTextLine.Length)
                        Program.GetDocument().SetLine(Program.GetCursor().dY, followingTextLine.Remove(Program.GetCursor().dX)); //remove following text on current line
                    
                    //scroll down if necessary
                    bool enterScrolledDown = false;
                    if(Program.GetCursor().sY >= Cursor.maxScreenY - 1)
                    {
                        Program.GetDocument().ScrollDown();
                        enterScrolledDown = true;
                    }

                    Program.GetCursor().SetDocPosition(0, Program.GetCursor().dY + 1); //move cursor to beginning of new line
                    Program.RefreshLine(Program.GetCursor().sY);

                    //update all lines below
                    if(!enterScrolledDown)
                    {
                        for(int i = Program.GetCursor().dY - 1; i <= Program.GetDocument().GetMaxLine(); i++)
                            Program.RefreshLine(i);
                    }
                    else
                        Program.RefreshAllLines();
                    break;
                case ConsoleKey.Escape:
                    ToggleInputTarget();
                    break;

                //TYPING
                default:                    
                    String typed = keyInfo.KeyChar.ToString();

                    //continue only if the typed character can be displayed
                    Regex r = new Regex("\\P{Cc}");
                    if(!r.Match(typed).Success)
                        break;

                    bool addedText = Program.GetDocument().AddTextAtPosition(Program.GetCursor().dX, Program.GetCursor().dY, typed);
                    
                    if(addedText)
                    {
                        Program.GetCursor().MoveRight();
                        Program.RefreshLine(Program.GetCursor().dY);
                    }

                    break;
            }
        }

        static void AcceptCommandInput(ConsoleKeyInfo keyInfo)
        {
            switch(keyInfo.Key)
            {
                //MOVEMENT
                case ConsoleKey.LeftArrow:
                    int newLeftX = Program.GetCursor().dX - 1;
                    if(CommandLine.IsSafeCursorX(newLeftX))
                        Program.GetCursor().ForceDocumentPosition(newLeftX, 0);
                    break;
                case ConsoleKey.RightArrow:
                    int newRightX = Program.GetCursor().dX + 1;
                    if(CommandLine.IsSafeCursorX(newRightX))
                        Program.GetCursor().ForceDocumentPosition(newRightX, 0);
                    break;

                //LINE MANIPULATION
                case ConsoleKey.Backspace:
                    bool backspaceTriggered = CommandLine.RemoveText(Program.GetCursor().dX - 1);
                    
                    if(backspaceTriggered)
                        Program.GetCursor().ForceDocumentPosition(Program.GetCursor().dX - 1, 0);
                    break;
                case ConsoleKey.Delete:
                    CommandLine.RemoveText(Program.GetCursor().dX);
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

                    //bool addedText = Program.GetDocument().AddTextAtPosition(Program.GetCursor().GetDocumentX(), Program.GetCursor().GetDocumentY(), typed);
                    
                    bool addedText = CommandLine.AddText(Program.GetCursor().dX, typed);

                    if(addedText)
                        Program.GetCursor().MoveRight();

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
                if(CommandLine.GetOutput() != "")
                    CommandLine.ClearOutput(); //there is output, clear it
                else
                {
                    currentTarget = InputTarget.Command; //there is no output, toggle
                    Program.GetCursor().ForceDocumentPosition(0, 0); //reset cursor position
                }
            }
            else if(currentTarget == InputTarget.Command)
                currentTarget = InputTarget.Document;
        }

    }
}