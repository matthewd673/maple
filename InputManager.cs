using System;

namespace maple
{
    static class InputManager
    {
        public static void AcceptInput(ConsoleKeyInfo keyInfo)
        {

            switch(keyInfo.Key)
            {
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
                default:
                    //Console.Clear();
                    //Program.GetFileManager().PrintFileLines();
                    //Printer.printLine(keyInfo.KeyChar.ToString(), ConsoleColor.Blue);
                    //Program.userText += keyInfo.KeyChar.ToString();
                    Program.userText += keyInfo.KeyChar.ToString();
                    //Program.GetCursor().MoveRight();
                    break;
            }

        }
    }
}