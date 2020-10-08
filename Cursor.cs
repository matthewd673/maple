using System;

namespace maple
{
    class Cursor
    {
        public int x, y;

        public static int minX = 0;
        public static int minY = 0;
        public static int maxX = 0;
        public static int maxY = 0;
        
        public Cursor(int x, int y)
        {
            this.x = x;
            this.y = y;

            CalculateCursorBounds();
        }

        public void AcceptInput()
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            /*
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
            */

        } 
        
        public void CalculateCursorBounds()
        {
            maxX = Console.BufferWidth - 1;
            maxY = Console.BufferHeight - 1;
        }

        public void MoveLeft() { MoveCursor(x - 1, y); }
        public void MoveRight() { MoveCursor(x + 1, y); }
        public void MoveUp() { MoveCursor(x, y - 1); }
        public void MoveDown() { MoveCursor(x, y + 1); }

        public void SetPosition(int x, int y) { MoveCursor(x, y); }

        void MoveCursor(int tX, int tY)
        {
            x = tX;
            y = tY;

            //keep x and y within safe range
            if(x < minX)
                x = minX;
            if(y < minY)
                y = minY;
            if(x > maxX)
                x = maxX;
            if(y > maxY)
                y = maxY;

            //move cursor pos
            Console.SetCursorPosition(x, y);
        }

    }
}