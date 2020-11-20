using System;

namespace maple
{
    class DocumentCursor : Cursor
    {

        Document doc;

        public DocumentCursor(String filepath, int dX, int dY) : base(dX, dY)
        {
            doc = new Document(filepath, internalDocument: false);
        }

        public Document GetDocument() { return doc; }

        public void MoveLeft()
        {
            if(dX > 0) //can move back
                Move(dX - 1, dY);
            else
            {
                if(dY > 0)
                {
                    MoveUp();
                    Move(doc.GetLineLength(dY), dY);
                }
            }
        }
        public void MoveRight()
        {
            if(dX < doc.GetLineLength(dY)) //can move forward
                Move(dX + 1, dY);
            else
            {
                if(dY < doc.GetMaxLine())
                {
                    MoveDown();
                    Move(0, dY);
                }
            }
        }
        public void MoveUp()
        {
            if(sY == 0 && doc.GetScrollY() > 0)
            {
                doc.ScrollUp();
                Console.Clear();
                doc.PrintFileLines();
                //base.Move(dX, dY);
            }
            else
                Move(dX, dY - 1);
        }
        public void MoveDown()
        {
            if(sY == maxScreenY - 1)
            {
                doc.ScrollDown();
                Console.Clear();
                doc.PrintFileLines();
                //base.Move(dX, dY);
            }
            else
                Move(dX, dY + 1);
        }

        public void SetDocPosition(int tX, int tY)
        {
            dX = tX;
            dY = tY;
            Move(dX, dY);
        }

        public void LockToDocConstraints()
        {
            if(dY < 0)
                dY = 0;
            if(dY > doc.GetMaxLine())
                dY = doc.GetMaxLine();

            if(dX < 0)
                dX = 0;
            if(dX > doc.GetLineLength(dY))
                dX = doc.GetLineLength(dY);
        }

        public void Move(int tX, int tY, bool constrainToDoc = true, bool constrainToScreen = true)
        {
            dX = tX;
            dY = tY;

            if(constrainToDoc)
                LockToDocConstraints();

            sX = dX + contentOffsetX;
            sY = dY + contentOffsetY - doc.GetScrollY();

            if(constrainToScreen)
                LockToScreenConstraints();

            Console.SetCursorPosition(sX, sY);
        }

        public void CalculateGutterWidth()
        {
            //get gutter width from doc
            int gutterWidth = doc.CalculateGutterWidth();
            contentOffsetX = gutterWidth;

            //update position of cursor
            ApplyPosition();
        }

    }
}