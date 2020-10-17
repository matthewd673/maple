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
                    ToggleInputTarget();
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

        static void AcceptCommandInput(ConsoleKeyInfo keyInfo)
        {
            switch(keyInfo.Key)
            {
                //MOVEMENT
                case ConsoleKey.LeftArrow:
                    Program.GetCursor().ForceDocumentPosition(Program.GetCursor().GetDocumentX() - 1, 0);
                    break;
                case ConsoleKey.RightArrow:
                    Program.GetCursor().ForceDocumentPosition(Program.GetCursor().GetDocumentX() + 1, 0);
                    break;

                //COMMANDS
                case ConsoleKey.Enter:
                    CommandLine.ExecuteCommand();
                    break;
                
                //TYPING
                default:                    
                    String typed = keyInfo.KeyChar.ToString();

                    //continue only if the typed character can be displayed
                    Regex r = new Regex("\\P{Cc}");
                    if(!r.Match(typed).Success)
                        break;

                    //bool addedText = Program.GetDocument().AddTextAtPosition(Program.GetCursor().GetDocumentX(), Program.GetCursor().GetDocumentY(), typed);
                    
                    bool addedText = CommandLine.AddText(Program.GetCursor().GetDocumentX(), typed);

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
                currentTarget = InputTarget.Command;
            else if(currentTarget == InputTarget.Command)
                currentTarget = InputTarget.Document;
        }

    }
}