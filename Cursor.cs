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
        

        private static int _oldMaxScreenX = 0;
        public static int MaxScreenX
        {
            get
            {
                int newMaxScreenX = Console.WindowWidth - 1;
                if (newMaxScreenX != _oldMaxScreenX)
                    Printer.Resize();
                _oldMaxScreenX = newMaxScreenX;
                return newMaxScreenX;
            }
        }
        private static int _oldMaxScreenY = 0;
        public static int MaxScreenY
        {
            get
            {
                int newMaxScreenY = Console.WindowHeight - 1;
                if (newMaxScreenY != _oldMaxScreenY)
                    Printer.Resize();
                _oldMaxScreenY = newMaxScreenY;
                return newMaxScreenY;
            }
        }

        public int ContentOffsetX { get; set; } = 0;
        public int ContentOffsetY { get; set; } = 0;

        public Cursor(int dX, int dY)
        {
            DX = dX;
            DY = dY;

            // CalculateCursorBounds();
        }
        
        // public static void CalculateCursorBounds()
        // {
        //     MaxScreenX = Console.BufferWidth - 1;
        //     MaxScreenY = Console.BufferHeight - 1;
        // }

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
                SY = MaxScreenY;
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