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

        /// <summary>
        /// Safely move the Cursor left by 1 character (Document coordinates).
        /// </summary>
        public void MoveLeft()
        {
            if(SX == 0 && Doc.ScrollX > 0)
            {
                Doc.ScrollLeft();
                Printer.Clear();
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
                        Move(Doc.GetLine(DY).Length, DY);
                    }
                }
            }
        }
        /// <summary>
        /// Safely move the Cursor right by 1 character (Document coordinates).
        /// </summary>
        public void MoveRight()
        {
            if(SY == MaxScreenX - 1)
            {
                Doc.ScrollRight();
                Printer.Clear();
                Doc.PrintFileLines();
            }

            if(DX < Doc.GetLine(DY).Length) //can move forward
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
        /// <summary>
        /// Safely move the Cursor up 1 line (Document coordinates).
        /// </summary>
        public void MoveUp()
        {
            if(SY == 0 && Doc.ScrollY > 0)
            {
                Doc.ScrollUp();
                Printer.Clear();
                Doc.PrintFileLines();
            }
            else
                Move(DX, DY - 1);
        }
        /// <summary>
        /// Safely move the Cursor down 1 line (Document coordinates).
        /// </summary>
        public void MoveDown()
        {
            if(SY == MaxScreenY - 1)
            {
                Doc.ScrollDown();
                Printer.Clear();
                Doc.PrintFileLines();
            }
            else
                Move(DX, DY + 1);
        }

        /// <summary>
        /// Force the document X and Y coordinates to fall within the Document's constraints.
        /// </summary>
        public void LockToDocConstraints()
        {
            if(DY < 0)
                DY = 0;
            if(DY > Doc.GetMaxLine())
                DY = Doc.GetMaxLine();

            if(DX < 0)
                DX = 0;
            if(DX > Doc.GetLine(DY).Length)
                DX = Doc.GetLine(DY).Length;
        }

        /// <summary>
        /// Safely move the Cursor to the given coordinates.
        /// </summary>
        /// <param name="tX">The new X coordinate (Document).</param>
        /// <param name="tY">The new Y coordinate (Document).</param>
        /// <param name="constrainToDoc">Lock the Cursor to the Document constraints after moving.</param>
        /// <param name="constrainToScreen">Lock the Cursor to the screen constraints after moving.</param>
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

        /// <summary>
        /// Update the Cursor's X offset according to the Document gutter.
        /// </summary>
        public void UpdateGutterOffset()
        {
            //get gutter width from doc
            int gutterWidth = Doc.CalculateGutterWidth();
            ContentOffsetX = gutterWidth;

            //update position of cursor
            ApplyPosition();
        }

    }
}