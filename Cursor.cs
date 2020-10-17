using System;

namespace maple
{
    class Cursor
    {
        //public int x, y;
        int docX, docY;
        int screenX, screenY;

        public static int minScreenX, minScreenY;
        public static int maxScreenX, maxScreenY;

        static int contentOffsetX = 0;
        static int contentOffsetY = 1;

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

        public void MoveLeft() { MoveCursor(docX - 1, docY); }
        public void MoveRight() { MoveCursor(docX + 1, docY); }
        public void MoveUp() { MoveCursor(docX, docY - 1); }
        public void MoveDown() { MoveCursor(docX, docY + 1); }

        public void SetDocPosition(int x, int y) { MoveCursor(docX, docY); }

        //public void UnsafeSetPosition(int x, int y) { UnsafeMoveCursor(x, y); }

        public int GetDocumentX() { return docX; }
        public int GetDocumentY() { return docY; }

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

        public void MoveCursor(int tX, int tY)
        {
            docX = tX;
            docY = tY;

            LockToDocConstraints();

            screenX = docX + contentOffsetX;
            screenY = docY + contentOffsetY;

            //keep screen x, y in safe range
            if(screenX < minScreenX)
                screenX = minScreenX;
            if(screenX > maxScreenX)
                screenX = maxScreenX;
            if(screenY < minScreenY)
                screenY = minScreenY;
            if(screenY > maxScreenY)
                screenY = maxScreenX;

            //move cursor pos
            Console.SetCursorPosition(screenX, screenY);
        }

        public void ForceMoveCursor(int tX, int tY)
        {
            Console.SetCursorPosition(tX, tY);
        }

    }
}