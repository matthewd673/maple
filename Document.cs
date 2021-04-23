using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace maple
{
    class Document
    {

        string filepath;
        List<Line> fileLines;

        int scrollYIncrement = 0;
        int scrollXIncrement = 0;
        int scrollY = 0;
        int scrollX = 0;

        Point selectIn = new Point(-1, -1);
        Point selectOut = new Point(-1, -1);

        public int gutterWidth = 0;
        int gutterPadding = 2;

        /// <summary>
        /// <c>Document</c> represents a text file which is either user-facing or for internal use.
        /// </summary>
        /// <param name="filepath">The path of the file to load.</param>
        /// <param name="internalDocument">Indicates if the document is internal, and can therefore operate with limited functionality.</param>
        public Document(String filepath, bool internalDocument = false)
        {
            fileLines = new List<Line>();

            //calculate user-facing document properties (if not interal)
            if (!internalDocument)
            {
                //load theme file if one exists
                String fileExtension = Path.GetExtension(filepath).Remove(0, 1);
                fileExtension = fileExtension.TrimEnd(); //remove trailing whitespace
                if (File.Exists(Settings.syntaxDirectory + fileExtension + ".xml")) //load lexer settings if they are available for this filetype
                    Lexer.LoadSyntax(Settings.syntaxDirectory + fileExtension + ".xml");

                //apply new properties (if not internal)
                CalculateScrollIncrement();
                scrollY = 0;
                scrollX = 0;
            }

            LoadDocument(filepath);

        }

        public void LoadDocument(String filepath)
        {
            //clear any lines that may have existed from before
            fileLines.Clear();

            //load new document
            this.filepath = filepath;
            if(File.Exists(filepath))
            {
                List<String> fileLinesText = File.ReadAllLines(filepath).ToList<String>();
                
                foreach(String s in fileLinesText)
                {
                    //List<Token> lineTokens = Line.GenerateTokensFromString(s);
                    fileLines.Add(new Line(s));
                }

                if(fileLines.Count == 0)
                    fileLines.Add(new Line(""));
            }
            else
            {
                File.Create(filepath).Close();
                fileLines = new List<Line>() { new Line("") };
            }

        }

        public void SaveDocument(String savePath)
        {
            List<String> allLines = new List<String>();
            foreach(Line l in fileLines)
                allLines.Add(l.GetContent());
            File.WriteAllLines(savePath, allLines);
        }

        public String GetFilePath()
        {
            return filepath;
        }

        public void PrintFileLines()
        {
            for(int i = scrollY; i < Cursor.maxScreenY + scrollY; i++)
                PrintLine(i);
        }

        public void PrintLine(int lineIndex)
        {
            //don't, if out of range
            if(lineIndex < 0 || lineIndex > fileLines.Count - 1 || lineIndex - scrollY < 0 || lineIndex - scrollY > Cursor.maxScreenY - 1)
                return;

            Line l = fileLines[lineIndex];
            
            //clear line on screen
            Printer.ClearLine(lineIndex - scrollY);
            Printer.MoveCursor(0, lineIndex - scrollY);

            //build gutter content & print gutter
            String gutterContent = BuildGutter(lineIndex);
            Printer.PrintWord(gutterContent, foregroundColor: Styler.gutterColor);

            bool lineContainsSelection = LineContainsSelection(lineIndex);
            //find start and end relative to line
            int lineSelectInX = selectIn.x;
            if (selectIn.y < lineIndex) //selection starts on previous line...
                lineSelectInX = 0; //...so it starts at 0 on this line
            int lineSelectOutX = selectOut.x;
            if (selectOut.y > lineIndex) //selection ends on following line...
                lineSelectOutX = l.GetContent().Length; //...so it ends at the end of this line

            bool fullySelected = false;
            if (lineContainsSelection && lineSelectInX == 0 && lineSelectOutX >= l.GetContent().Length - 1)
                fullySelected = true;

            //print all tokens in line
            if(!Settings.debugTokens) //ordinary printing:
            {
                int lineLen = 0;

                foreach(Token t in l.GetTokens())
                {
                    //store difference between previous and current line lengths
                    int oldLineLen = lineLen;
                    lineLen += t.GetText().Length;

                    //if token comes before scroll x (hidden to left), skip
                    if (lineLen < scrollX)
                        continue;
                    //if token comes after scroll x (hidden to right), skip it and all subsequent tokens
                    if (oldLineLen > scrollX + Cursor.maxScreenX)
                        break;

                    int tSelectStart = -1;
                    int tSelectEnd = -1;
                    bool tokenHasSelect = false;
                    //calculate start and end of highlight if:
                    //line has highlight, highlight begins before end of word, and highlight ends before beginning of word
                    if (lineContainsSelection && !fullySelected && lineSelectInX < lineLen && lineSelectOutX > oldLineLen)
                    {
                        tSelectStart = lineSelectInX - oldLineLen;
                        tSelectEnd = lineSelectOutX - oldLineLen;

                        //constrain to bounds of token string
                        if (tSelectStart < 0)
                            tSelectStart = 0;
                        if (tSelectEnd >= t.GetText().Length)
                            tSelectEnd = t.GetText().Length;

                        tokenHasSelect = true;

                    }

                    string printText = t.GetText();

                    //trim the parts of the token that are hidden by horizontal scroll
                    if (lineLen > scrollX) //part of token is hidden to left, trim beginning
                    {
                        int hiddenCharCt = scrollX - oldLineLen;
                        if (hiddenCharCt > 0)
                        {
                            printText = printText.Remove(0, hiddenCharCt); //trim off hidden
                            //if token is selected, trim selection bounds as well
                            if (tokenHasSelect)
                            {
                                tSelectStart -= hiddenCharCt;
                                tSelectEnd -= hiddenCharCt;
                                //clamp
                                if (tSelectStart < 0)
                                    tSelectStart = 0;
                                if (tSelectEnd < 0)
                                    tSelectEnd = 0;
                            }
                        }
                        else //hiddenCharCt can't be negative, for highlighter's sake
                            hiddenCharCt = 0;

                        if (tokenHasSelect) //print selected part with separate styles
                        {
                            int selectLength = tSelectEnd - tSelectStart;
                            string preSelectSubstring = printText.Substring(0, tSelectStart);
                            string inSelectSubstring = printText.Substring(tSelectStart, selectLength);
                            string postSelectSubstring = printText.Substring(tSelectEnd);
                            Printer.PrintWord(preSelectSubstring, foregroundColor: t.GetColor());
                            Printer.PrintWord(inSelectSubstring, foregroundColor: ConsoleColor.Black, backgroundColor: Styler.selectionColor);
                            Printer.PrintWord(postSelectSubstring, foregroundColor: t.GetColor());
                        }
                        else //no precise selection to print
                        {
                            if (!fullySelected) //normal print
                                Printer.PrintWord(printText, foregroundColor: t.GetColor());
                            else //print fully selected
                                Printer.PrintWord(printText, foregroundColor: ConsoleColor.Black, backgroundColor: Styler.selectionColor);
                        }
                    }
                    else if (lineLen > scrollX + Cursor.maxScreenX) //part of token is hidden to right, trim end
                    {
                        int hiddenCharCt = lineLen - (scrollX + Cursor.maxScreenX);
                        if (hiddenCharCt > 0)
                        {
                            printText = printText.Remove(printText.Length - 1 - hiddenCharCt, printText.Length);

                            if (tSelectStart > printText.Length)
                                tSelectStart = printText.Length;
                            if (tSelectEnd > printText.Length)
                                tSelectEnd = printText.Length;
                            if (tSelectStart == tSelectEnd)
                                tokenHasSelect = false;

                            if (tokenHasSelect)
                            {
                                int selectLength = tSelectEnd - tSelectStart;
                                string preSelectSubstring = printText.Substring(0, tSelectStart);
                                string inSelectSubstring = printText.Substring(tSelectStart, selectLength);
                                string postSelectSubstring = printText.Substring(tSelectEnd);
                                Printer.PrintWord(preSelectSubstring, foregroundColor: t.GetColor());
                                Printer.PrintWord(inSelectSubstring, foregroundColor: ConsoleColor.Black, backgroundColor: Styler.selectionColor);
                                Printer.PrintWord(postSelectSubstring, foregroundColor: t.GetColor());
                            }
                            else //no precise selection to print
                            {
                                if (!fullySelected)
                                    Printer.PrintWord(printText, foregroundColor: t.GetColor());
                                else
                                    Printer.PrintWord(printText, foregroundColor: ConsoleColor.Black, backgroundColor: Styler.selectionColor);
                            }
                        }
                        else //can't be negative
                            hiddenCharCt = 0;
                    }
                }
            }
            else //debug printing:
            {
                int totalLength = 0;
                foreach(Token t in l.GetTokens())
                {
                    if(totalLength != -1)
                        totalLength += t.GetText().Length;
                    if(totalLength > Editor.GetDocCursor().dX && lineIndex == Editor.GetDocCursor().dY)
                    {
                        totalLength = -1;
                        Printer.PrintWord(t.GetText(), foregroundColor: ConsoleColor.Black, backgroundColor: ConsoleColor.Yellow);
                    }
                    else
                        Printer.PrintWord(t.GetText(), foregroundColor: t.GetColor());
                }
            }
        }

        /// <summary>
        /// Generate the gutter content for a given line according to set preferences.
        /// </summary>
        /// <param name="lineIndex">The index of the line to generate for.</param>
        /// <returns>A String representing the gutter text to render.</returns>
        String BuildGutter(int lineIndex)
        {
            String gutterContent = (lineIndex + 1).ToString();
            while(gutterContent.Length < gutterWidth - gutterPadding)
                gutterContent = "0" + gutterContent;
            while(gutterContent.Length < gutterWidth)
                gutterContent += " ";
            return gutterContent;
        }

        /// <summary>
        /// Safely scroll up by the Y scrolling increment.
        /// </summary>
        public void ScrollUp()
        {
            scrollY -= scrollYIncrement;
            if(scrollY < 0)
                scrollY = 0;
        }

        /// <summary>
        /// Safely scroll down by the Y scrolling increment.
        /// </summary>
        public void ScrollDown()
        {
            scrollY += scrollYIncrement;
        }

        /// <summary>
        /// Safely scroll left by the X scrolling increment.
        /// </summary>
        public void ScrollLeft()
        {
            scrollX -= scrollXIncrement;
            if(scrollX < 0)
                scrollX = 0;
        }

        /// <summary>
        /// Safely scroll right by the X scrolling increment.
        /// </summary>
        public void ScrollRight()
        {
            scrollX += scrollXIncrement;
        }

        public int GetScrollY() { return scrollY; }

        public int GetScrollX() { return scrollX; }

        /// <summary>
        /// Set the X and Y scroll increments based on the current buffer dimensions.
        /// </summary>
        public void CalculateScrollIncrement()
        {
            scrollYIncrement = (Cursor.maxScreenY - 1) / 2;
            scrollXIncrement = (Cursor.maxScreenX - 1) / 2;
        }

        public int CalculateGutterWidth()
        {
            int oldGutterWidth = gutterWidth;
            gutterWidth = fileLines.Count.ToString().Length + gutterPadding;

            if(gutterWidth != oldGutterWidth)
                Editor.RefreshAllLines();

            return gutterWidth;
        }

        /// <summary>
        /// Get the text of a given line in the document.
        /// </summary>
        /// <param name="index">The index of the line in question.</param>
        /// <returns>A String containing the line text.</returns>
        public String GetLine(int index)
        {
            if(index >= 0 && index < fileLines.Count)
                return fileLines[index].GetContent();
            else
                return "";
        }

        /// <summary>
        /// Get the text of all lines in the document.
        /// </summary>
        /// <returns>A List of Strings containing the text of all lines.</returns>
        public List<String> GetAllLines()
        {
            List<String> lines = new List<String>();
            foreach(Line l in fileLines)
                lines.Add(l.GetContent());
            return lines;
        }

        public void SetLine(int index, String text)
        {
            if(index >= 0 && index < fileLines.Count)
                fileLines[index].SetContent(text);
        }

        public bool AddTextAtPosition(int x, int y, String text)
        {
            if(x < 0 || y < 0 || y > fileLines.Count)
                return false;
            
            String currentLine = GetLine(y);
            currentLine = AddText(currentLine, x, text);

            //no change made
            if(GetLine(y) == currentLine)
                return false;

            SetLine(y, currentLine);

            return true;

        }

        public bool RemoveTextAtPosition(int x, int y)
        {
            if(x < 0 || y < 0 || y > fileLines.Count)
                return false;

            String currentLine = GetLine(y);
            currentLine = RemoveText(currentLine, x);

            //no change made
            if(GetLine(y) == currentLine)
                return false;
            
            SetLine(y, currentLine);

            return true;
        }

        public bool AddLine(int index)
        {
            if(index < 0 || index > fileLines.Count)
                return false;
            
            fileLines.Insert(index, new Line(""));
            CalculateGutterWidth();
            return true;
        }
        
        public bool RemoveLine(int index)
        {
            if(index < 0 || index > fileLines.Count - 1)
                return false;

            fileLines.RemoveAt(index);
            CalculateGutterWidth();
            return true;
        }

        public static String AddText(String source, int pos, String text)
        {
            if(pos >= 0 && pos <= source.Length)
                return source.Insert(pos, text);
            else
                return source;
        }

        public static String RemoveText(String source, int pos)
        {
            if(pos >= 0 && pos <= source.Length - 1)
                return source.Remove(pos, 1);
            else
                return source;
        }

        public int GetMaxLine()
        {
            return fileLines.Count - 1;
        }

        public int GetLineLength(int line)
        {
            if(line < fileLines.Count)
                return fileLines[line].GetContent().Length;
            else
                return 0;
        }

        public void MarkSelectionIn(int x, int y)
        {
            selectIn = new Point(x, y);
            ArrangeSelectionPoints();
        }

        public void MarkSelectionOut(int x, int y)
        {
            selectOut = new Point(x, y);
            ArrangeSelectionPoints();
        }

        public void ArrangeSelectionPoints()
        {

            if (!HasSelection())
                return;

            if (selectOut.y < selectIn.y) //flip start and end if end is on a previous line
            {
                Point tempIn = new Point(selectIn.x, selectIn.y);
                selectIn = new Point(selectOut.x, selectOut.y);
                selectOut = tempIn;
            }
            else if (selectOut.y == selectIn.y && selectOut.x < selectIn.x) //flip start and end if end occurs first on same line
            {
                Point tempIn = new Point(selectIn.x, selectIn.y);
                selectIn = new Point(selectOut.x, selectOut.y);
                selectOut = tempIn;
            }

            //selecting a 0-width range causes errors (and why would you want to anyway?)
            if (selectIn.x == selectOut.x && selectIn.y == selectOut.y)
            {
                selectIn = new Point(-1, -1);
                selectOut = new Point(-1, -1);
            }

        }
        
        /// <summary>
        /// Check if the document has a complete selection (beginning and end).
        /// </summary>
        /// <returns>Returns true if the document has a starting and ending selection bound.</returns>
        public bool HasSelection()
        {
            return selectIn.x != -1 && selectIn.y != -1 && selectOut.x != -1 && selectOut.y != -1;
        }

        public bool HasSelectionStart()
        {
            return selectIn.x != -1 && selectIn.y != -1;
        }

        public bool HasSelectionEnd()
        {
            return selectOut.x != -1 && selectIn.y != -1;
        }

        /// <summary>
        /// Check if a given line contains selected text.
        /// </summary>
        /// <param name="lineIndex">The line in question.</param>
        /// <returns>Returns true if the line contains selected text.</returns>
        bool LineContainsSelection(int lineIndex)
        {
            return selectIn.y <= lineIndex && selectOut.y >= lineIndex;
        }

        /// <summary>
        /// Check if the current document selection spans multiple lines.
        /// </summary>
        /// <returns>Returns true if the current selection starts and ends on different lines.</returns>
        bool IsMultilineSelection()
        {
            return selectIn.y != selectOut.y;
        }
        
        public int GetSelectionInX() { return selectIn.x; }
        public int GetSelectionInY() { return selectIn.y; }
        public int GetSelectionOutX() { return selectOut.x; }
        public int GetSelectionOutY() { return selectOut.y; }

        /// <summary>
        /// Get the text contained within the current selection bounds.
        /// </summary>
        /// <returns>A String containing the current selection text.</returns>
        public string GetSelectionText()
        {

            Console.Title = selectIn.x + " , " + selectOut.x;

            if (selectIn.y == -1 || selectOut.y == -1) //skip if no selection
                return "";

            if (selectIn.y == selectOut.y) //just return substring if on same line
                return GetLine(selectIn.y).Substring(selectIn.x, selectOut.x);

            //multiple lines
            String text = "";
            for (int y = selectIn.y; y <= selectOut.y; y++)
            {
                if (y == selectIn.y)
                    text += GetLine(y).Substring(selectIn.x) + "\n";
                else if (y == selectOut.y)
                    text += GetLine(y).Substring(0, selectOut.x);
                else
                    text += GetLine(y) + "\n";
            }

            //debug!
            File.WriteAllText("DEBUGLONGSELECT.txt", text);

            return text;
        }

    }
}
