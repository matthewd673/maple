using System;
using System.Text.RegularExpressions;

namespace maple
{
    static class Input
    {
        public static void AcceptInput(ConsoleKeyInfo keyInfo)
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
                    bool backspaceTriggered = Program.GetDocument().RemoveTextAtPosition(
                        Program.GetCursor().GetDocumentX() - 1,
                        Program.GetCursor().GetDocumentY());

                    if(backspaceTriggered)
                        Program.GetCursor().MoveLeft();
                    break;
                case ConsoleKey.Delete:
                    Program.GetDocument().RemoveTextAtPosition(Program.GetCursor().GetDocumentX(), Program.GetCursor().GetDocumentY());
                    break;
                case ConsoleKey.Enter:
                    Program.GetDocument().AddLine(Program.GetCursor().GetDocumentY());
                    Program.GetCursor().MoveDown();
                    break;
                case ConsoleKey.Escape:
                    Commands.ToggleInputTarget();
                    break;

                //TYPING
                default:                    
                    String typed = keyInfo.KeyChar.ToString();

                    //continue only if the typed character can be displayed
                    Regex r = new Regex("\\P{Cc}");
                    if(!r.Match(typed).Success)
                        break;

                    bool addedText = Program.GetDocument().AddTextAtPosition(Program.GetCursor().GetDocumentX(), Program.GetCursor().GetDocumentY(), typed);
                    
                    if(addedText)
                        Program.GetCursor().MoveRight();

                    break;
            }

        }
    }
}