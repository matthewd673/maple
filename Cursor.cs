using System;

namespace maple
{
    class Cursor
    {
        public int DX { get; set; }
        public int DY { get; set; }

        public int SX { get; set; }
        public int SY { get; set; }

        public const int MinScreenX = 0;
        public const int MinScreenY = 0;
        
        public static int MaxScreenX { get; set; }
        public static int MaxScreenY { get; set; }

        public int ContentOffsetX { get; set; } = 0;
        public int ContentOffsetY { get; set; } = 0;

        public Cursor(int dX, int dY)
        {
            DX = dX;
            DY = dY;

            CalculateCursorBounds();
        }
        
        public static void CalculateCursorBounds()
        {
            MaxScreenX = Console.BufferWidth - 1;
            MaxScreenY = Console.BufferHeight - 1;
        }

        public void LockToScreenConstraints()
        {
            //keep screen x, y in safe range
            if(SX < MinScreenX)
                SX = MinScreenX;
            if(SX > MaxScreenX)
                SX = MaxScreenX;
            if(SY < MinScreenY)
                SY = MinScreenY;
            if(SY > MaxScreenY)
                SY = MaxScreenX;
        }

        public void Move(int tX, int tY, bool constrainToScreen = true)
        {
            DX = tX;
            DY = tY;

            SX = DX + ContentOffsetX;
            SY = DY + ContentOffsetY;

            if(constrainToScreen)
                LockToScreenConstraints();

            Console.SetCursorPosition(SX, SY);
        }

        public void ApplyPosition()
        {
            Console.SetCursorPosition(SX, SY);
        }

    }
}