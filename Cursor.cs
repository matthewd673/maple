using System;

namespace maple
{
    class Cursor
    {
        //public int x, y;
        int docX, docY;
        int screenX, screenY;

        static int minScreenX, minScreenY;
        static int maxScreenX, maxScreenY;

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

        public void MoveCursor(int tX, int tY)
        {
            docX = tX;
            docY = tY;

            screenX = docX;
            screenY = docY + 1;

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