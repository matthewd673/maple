using System;

namespace maple
{
    class Cursor
    {
        public int dX, dY;
        public int sX, sY;

        public const int minScreenX = 0;
        public const int minScreenY = 0;
        
        public static int maxScreenX, maxScreenY;

        public int contentOffsetX = 0;
        public int contentOffsetY = 0;

        public Cursor(int dX, int dY)
        {
            this.dX = dX;
            this.dY = dY;

            CalculateCursorBounds();
        }
        
        public static void CalculateCursorBounds()
        {
            maxScreenX = Console.BufferWidth - 1;
            maxScreenY = Console.BufferHeight - 1;
        }

        public void LockToScreenConstraints()
        {
            //keep screen x, y in safe range
            if(sX < minScreenX)
                sX = minScreenX;
            if(sX > maxScreenX)
                sX = maxScreenX;
            if(sY < minScreenY)
                sY = minScreenY;
            if(sY > maxScreenY)
                sY = maxScreenX;
        }

        public void Move(int tX, int tY, bool constrainToScreen = true)
        {
            dX = tX;
            dY = tY;

            sX = dX + contentOffsetX;
            sY = dY + contentOffsetY;

            if(constrainToScreen)
                LockToScreenConstraints();

            Console.SetCursorPosition(sX, sY);
        }

        public void ApplyPosition()
        {
            Console.SetCursorPosition(sX, sY);
        }

    }
}