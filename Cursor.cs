using System;

namespace maple
{
    class Cursor
    {
        int docX, docY;
        int screenX, screenY;

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
            this.docX = x;
            this.docY = y;

            CalculateCursorBounds();
        }
        
        public void CalculateCursorBounds()
        {
            maxScreenX = Console.BufferWidth - 1;
            maxScreenY = Console.BufferHeight - 1;
        }

        public void MoveLeft()
        {
            if(docX > 0) //can move back
                MoveCursor(docX - 1, docY);
            else
            {
                if(docY > 0)
                {
                    MoveUp();
                    MoveCursor(Program.GetDocument().GetLineLength(docY), docY);
                }
            }
        }
        public void MoveRight()
        {
            if(docX < Program.GetDocument().GetLineLength(docY)) //can move forward
                MoveCursor(docX + 1, docY);
            else
            {
                if(docY < Program.GetDocument().GetMaxLine())
                {
                    MoveDown();
                    MoveCursor(0, docY);
                }
            }
        }
        public void MoveUp() { MoveCursor(docX, docY - 1); }
        public void MoveDown() { MoveCursor(docX, docY + 1); }

        public void SetDocPosition(int x, int y)
        {
            docX = x;
            docY = y;
            MoveCursor(docX, docY);
        }

        //public void UnsafeSetPosition(int x, int y) { UnsafeMoveCursor(x, y); }

        public int GetDocX() { return docX; }
        public int GetDocY() { return docY; }

        public void LockToDocConstraints()
        {
            if(docY < 0)
                docY = 0;
            if(docY > Program.GetDocument().GetMaxLine())
                docY = Program.GetDocument().GetMaxLine();

            if(docX < 0)
                docX = 0;
            if(docX > Program.GetDocument().GetLineLength(docY))
                docX = Program.GetDocument().GetLineLength(docY);
        }

        public void LockToScreenConstraints()
        {
            //keep screen x, y in safe range
            if(screenX < minScreenX)
                screenX = minScreenX;
            if(screenX > maxScreenX)
                screenX = maxScreenX;
            if(screenY < minScreenY)
                screenY = minScreenY;
            if(screenY > maxScreenY)
                screenY = maxScreenX;
        }

        public void MoveCursor(int tX, int tY)
        {
            docX = tX;
            docY = tY;

            LockToDocConstraints();

            screenX = docX + contentOffsetX;
            screenY = docY + contentOffsetY;

            LockToScreenConstraints();

            //move cursor pos
            Console.SetCursorPosition(screenX, screenY);
        }

        public void MoveCursor()
        {
            MoveCursor(docX, docY);
        }

        public void ForceDocumentPosition(int x, int y)
        {
            docX = x;
            docY = y;

            screenX = docX + contentOffsetX;
            screenY = docY + contentOffsetY;

            LockToScreenConstraints();

            Console.SetCursorPosition(screenX, screenY);
        }

        public void ForceMoveCursor(int tX, int tY)
        {
            Console.SetCursorPosition(tX, tY);
        }

    }
}