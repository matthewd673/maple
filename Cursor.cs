using System;

namespace maple
{
    public class Cursor
    {
        public int DX { get; set; }
        public int DY { get; set; }

        public int SX { get; set; }
        public int SY { get; set; }

        public int ContentOffsetX { get; set; } = 0;
        public int ContentOffsetY { get; set; } = 0;

        public Cursor(int dX, int dY)
        {
            DX = dX;
            DY = dY;
        }

        /// <summary>
        /// Force the screen X and Y coordinates to fall within the window constraints
        /// </summary>
        public void LockToScreenConstraints()
        {
            //keep screen x, y in safe range
            if(SX < Printer.MinScreenX)
                SX = Printer.MinScreenX;
            if(SX > Printer.MaxScreenX)
                SX = Printer.MaxScreenX;
            if(SY < Printer.MinScreenY)
                SY = Printer.MinScreenY;
            if(SY > Printer.MaxScreenY)
                SY = Printer.MaxScreenY;
        }

        /// <summary>
        /// Safely move the Cursor to the given coordinates.
        /// </summary>
        /// <param name="tX">The new X coordinate (Document).</param>
        /// <param name="tY">The new Y coordinate (Document).</param>
        /// <param name="applyPosition">Determines if the Console's cursor should be moved to the given position.</param>
        /// <param name="constrainToScreen">Lock the Cursor to the screen constraints after moving.</param>
        public void Move(int tX, int tY, bool constrainToScreen = true, bool applyPosition = true)
        {
            DX = tX;
            DY = tY;

            SX = DX + ContentOffsetX;
            SY = DY + ContentOffsetY;

            if (constrainToScreen)
            {
                LockToScreenConstraints();
            }

            if (applyPosition)
            {
                ApplyPosition();
            }
        }

        public void Move(Point target, bool constrainToScreen = true, bool applyPosition = true)
        {
            Move(target.X, target.Y, constrainToScreen, applyPosition);
        }

        /// <summary>
        /// Force the Console's cursor to move to the Cursor's screen coordinates.
        /// </summary>
        public void ApplyPosition()
        {
            Console.SetCursorPosition(SX, SY);
        }

    }
}