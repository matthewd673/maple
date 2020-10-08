using System;

namespace maple
{
    class Cursor
    {
        public int x, y;

        public static int minX = 0;
        public static int minY = 0;
        
        public Cursor(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void AcceptInput()
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            switch(keyInfo.Key)
            {
                case ConsoleKey.RightArrow:
                    x++;
                    ApplyCursorPosition();
                    break;
                case ConsoleKey.LeftArrow:
                    x--;
                    ApplyCursorPosition();
                    break;
                case ConsoleKey.UpArrow:
                    y--;
                    ApplyCursorPosition();
                    break;
                case ConsoleKey.DownArrow:
                    y++;
                    ApplyCursorPosition();
                    break;
            }

        } 
        
        public void ApplyCursorPosition()
        {
            //keep x and y within safe range
            if(x < minX)
                x = minX;
            if(y < minY)
                y = minY;

            //move cursor pos
            Console.SetCursorPosition(x, y);
        }

    }
}