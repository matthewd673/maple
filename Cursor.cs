using System;

namespace maple
{
    class Cursor
    {
        public int dX, dY;
        public int sX, sY;

        public static int minScreenX, minScreenY;
        public static int maxScreenX, maxScreenY;

        public int contentOffsetX = 0;
        public int contentOffsetY = 0;

        /*
        public static int offsetX = 0;
        public static int offsetY = 1;
        */

        public Cursor(int x, int y)
        {
            this.dX = x;
            this.dY = y;

            CalculateCursorBounds();
        }
        
        public void CalculateCursorBounds()
        {
            maxScreenX = Console.BufferWidth - 1;
            maxScreenY = Console.BufferHeight - 1;
        }

        public void MoveLeft()
        {
            if(dX > 0) //can move back
                MoveCursor(dX - 1, dY);
            else
            {
                if(dY > 0)
                {
                    MoveUp();
                    MoveCursor(Program.GetDocument().GetLineLength(dY), dY);
                }
            }
        }
        public void MoveRight()
        {
            if(dX < Program.GetDocument().GetLineLength(dY)) //can move forward
                MoveCursor(dX + 1, dY);
            else
            {
                if(dY < Program.GetDocument().GetMaxLine())
                {
                    MoveDown();
                    MoveCursor(0, dY);
                }
            }
        }
        public void MoveUp()
        {
            if(sY == 0 && Program.GetDocument().GetScrollY() > 0)
            {
                Program.GetDocument().ScrollUp();
                Console.Clear();
                Program.GetDocument().PrintFileLines();
            }
            else
                MoveCursor(dX, dY - 1);
        }

        public void MoveDown()
        {
            if(sY == maxScreenY - 1)
            {
                Program.GetDocument().ScrollDown();
                Console.Clear();
                Program.GetDocument().PrintFileLines();
            }
            else
                MoveCursor(dX, dY + 1);
        }

        public void SetDocPosition(int x, int y)
        {
            dX = x;
            dY = y;
            MoveCursor(dX, dY);
        }

        public void LockToDocConstraints()
        {
            if(dY < 0)
                dY = 0;
            if(dY > Program.GetDocument().GetMaxLine())
                dY = Program.GetDocument().GetMaxLine();

            if(dX < 0)
                dX = 0;
            if(dX > Program.GetDocument().GetLineLength(dY))
                dX = Program.GetDocument().GetLineLength(dY);
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

        public void MoveCursor(int tX, int tY)
        {
            dX = tX;
            dY = tY;

            LockToDocConstraints();

            sX = dX + contentOffsetX;
            sY = dY + contentOffsetY - Program.GetDocument().GetScrollY();

            LockToScreenConstraints();

            //move cursor pos
            Console.SetCursorPosition(sX, sY);
        }

        public void MoveCursor()
        {
            MoveCursor(dX, dY);
        }

        public void ForceDocumentPosition(int x, int y)
        {
            dX = x;
            dY = y;

            sX = dX + contentOffsetX;
            sY = dY + contentOffsetY - Program.GetDocument().GetScrollY();

            LockToScreenConstraints();

            Console.SetCursorPosition(sX, sY);
        }

        public void ForceMoveCursor(int tX, int tY)
        {
            Console.SetCursorPosition(tX, tY);
        }

    }
}