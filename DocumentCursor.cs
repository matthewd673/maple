using System;

namespace maple
{
    class DocumentCursor : Cursor
    {
        
        public Document Doc { get; private set; }

        public DocumentCursor(String filepath, int dX, int dY) : base(dX, dY)
        {
            Doc = new Document(filepath, internalDocument: false);
        }

        public void MoveLeft()
        {
            if(SX == 0 && Doc.ScrollX > 0)
            {
                Doc.ScrollLeft();
                Console.Clear();
                Doc.PrintFileLines();
            }
            else
            {
                if(DX > 0) //can move back
                    Move(DX - 1, DY);
                else
                {
                    if(DY > 0)
                    {
                        MoveUp();
                        Move(Doc.GetLineLength(DY), DY);
                    }
                }
            }
        }
        public void MoveRight()
        {
            if(SY == MaxScreenX - 1)
            {
                Doc.ScrollRight();
                Console.Clear();
                Doc.PrintFileLines();
            }

            if(DX < Doc.GetLineLength(DY)) //can move forward
                Move(DX + 1, DY);
            else
            {
                if(DY < Doc.GetMaxLine())
                {
                    MoveDown();
                    Move(0, DY);
                }
            }
        }
        public void MoveUp()
        {
            if(SY == 0 && Doc.ScrollY > 0)
            {
                Doc.ScrollUp();
                Console.Clear();
                Doc.PrintFileLines();
            }
            else
                Move(DX, DY - 1);
        }
        public void MoveDown()
        {
            if(SY == MaxScreenY - 1)
            {
                Doc.ScrollDown();
                Console.Clear();
                Doc.PrintFileLines();
            }
            else
                Move(DX, DY + 1);
        }

        public void LockToDocConstraints()
        {
            if(DY < 0)
                DY = 0;
            if(DY > Doc.GetMaxLine())
                DY = Doc.GetMaxLine();

            if(DX < 0)
                DX = 0;
            if(DX > Doc.GetLineLength(DY))
                DX = Doc.GetLineLength(DY);
        }

        public void Move(int tX, int tY, bool constrainToDoc = true, bool constrainToScreen = true)
        {
            DX = tX;
            DY = tY;

            if(constrainToDoc)
                LockToDocConstraints();

            //scroll if set position is outside current viewport
            bool hasScrolled = false;
            while (DY - Doc.ScrollY > MaxScreenY)
            {
                Doc.ScrollDown();
                hasScrolled = true;
            }
            while (DY - Doc.ScrollY < MinScreenY)
            {
                Doc.ScrollUp();
                hasScrolled = true;
            }
            while (DX - Doc.ScrollX + Doc.GutterWidth > MaxScreenX)
            {
                Doc.ScrollRight();
                hasScrolled = true;
            }
            while (DX - Doc.ScrollX < MinScreenX)
            {
                Doc.ScrollLeft();
                hasScrolled = true;
            }
            //refresh all lines if scroll was performed
            if(hasScrolled)
                Editor.RefreshAllLines();

            SX = DX + ContentOffsetX - Doc.ScrollX;
            SY = DY + ContentOffsetY - Doc.ScrollY;

            if(constrainToScreen)
                LockToScreenConstraints();

            Console.SetCursorPosition(SX, SY);
        }

        public void CalculateGutterWidth()
        {
            //get gutter width from doc
            int gutterWidth = Doc.CalculateGutterWidth();
            ContentOffsetX = gutterWidth;

            //update position of cursor
            ApplyPosition();
        }

    }
}