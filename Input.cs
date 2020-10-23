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
                    if(Program.GetCursor().GetDocX() > 0) //not at the beginning of the line
                    {
                        bool backspaceTriggered = Program.GetDocument().RemoveTextAtPosition(
                            Program.GetCursor().GetDocX() - 1,
                            Program.GetCursor().GetDocY());

                        if(backspaceTriggered)
                            Program.GetCursor().MoveLeft();
                    }
                    else //at beginning of line, append current line to previous
                    {
                        if(Program.GetCursor().GetDocY() > 0)
                        {
                            String currentLineContent = Program.GetDocument().GetLine(Program.GetCursor().GetDocY()); //get remaining content on current line
                            int previousLineMaxX = Program.GetDocument().GetLineLength(Program.GetCursor().GetDocY() - 1); //get max position on preceding line
                            Program.GetDocument().AddTextAtPosition(previousLineMaxX, Program.GetCursor().GetDocY() - 1, currentLineContent); //add remaining text to previous line
                            Program.GetDocument().RemoveLine(Program.GetCursor().GetDocY()); //remove current line
                            Program.GetCursor().SetDocPosition(previousLineMaxX, Program.GetCursor().GetDocY() - 1); //move cursor to preceding line
                        }
                        //update all lines below
                        for(int i = Program.GetCursor().GetDocY(); i <= Program.GetDocument().GetMaxLine() + 1; i++) //+1 so that the old line is cleared
                            Program.RefreshLine(i);
                    }

                    Program.RefreshLine(Program.GetCursor().GetDocY());
                    break;
                case ConsoleKey.Delete:
                    Program.GetDocument().RemoveTextAtPosition(Program.GetCursor().GetDocX(), Program.GetCursor().GetDocY());
                    Program.RefreshLine(Program.GetCursor().GetDocY());
                    break;
                case ConsoleKey.Enter:
                    Program.GetDocument().AddLine(Program.GetCursor().GetDocY() + 1); //add new line

                    String followingTextLine = Program.GetDocument().GetLine(Program.GetCursor().GetDocY());
                    String followingText = followingTextLine.Substring(Program.GetCursor().GetDocX()); //get text following cursor (on current line)

                    Program.GetDocument().AddTextAtPosition(0, Program.GetCursor().GetDocY() + 1, followingText); //add following text to new line

                    if(Program.GetCursor().GetDocX() < followingTextLine.Length)
                        Program.GetDocument().SetLine(Program.GetCursor().GetDocY(), followingTextLine.Remove(Program.GetCursor().GetDocX())); //remove following text on current line
                    
                    Program.GetCursor().SetDocPosition(0, Program.GetCursor().GetDocY() + 1); //move cursor to beginning of new line
                    Program.RefreshLine(Program.GetCursor().GetDocY());

                    //update all lines below
                        for(int i = Program.GetCursor().GetDocY() - 1; i <= Program.GetDocument().GetMaxLine(); i++)
                            Program.RefreshLine(i);
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

                    bool addedText = Program.GetDocument().AddTextAtPosition(Program.GetCursor().GetDocX(), Program.GetCursor().GetDocY(), typed);
                    
                    if(addedText)
                    {
                        Program.GetCursor().MoveRight();
                        Program.RefreshLine(Program.GetCursor().GetDocY());
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
                    int newLeftX = Program.GetCursor().GetDocX() - 1;
                    if(CommandLine.IsSafeCursorX(newLeftX))
                        Program.GetCursor().ForceDocumentPosition(newLeftX, 0);
                    break;
                case ConsoleKey.RightArrow:
                    int newRightX = Program.GetCursor().GetDocX() + 1;
                    if(CommandLine.IsSafeCursorX(newRightX))
                        Program.GetCursor().ForceDocumentPosition(newRightX, 0);
                    break;

                //LINE MANIPULATION
                case ConsoleKey.Backspace:
                    bool backspaceTriggered = CommandLine.RemoveText(Program.GetCursor().GetDocX() - 1);
                    
                    if(backspaceTriggered)
                        Program.GetCursor().ForceDocumentPosition(Program.GetCursor().GetDocX() - 1, 0);
                    break;
                case ConsoleKey.Delete:
                    CommandLine.RemoveText(Program.GetCursor().GetDocX());
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
                    
                    bool addedText = CommandLine.AddText(Program.GetCursor().GetDocX(), typed);

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