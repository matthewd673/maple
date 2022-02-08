using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace maple
{
    class Document
    {

        public string Filepath { get; private set; }
        List<Line> fileLines;

        int scrollYIncrement = 0;
        int scrollXIncrement = 0;
        public int ScrollY { get; private set; } = 0;
        public int ScrollX { get; private set; } = 0;

        Point selectIn = new Point(-1, -1);
        Point selectOut = new Point(-1, -1);

        public int GutterWidth { get; private set; } = 0;
        int gutterPadding = 2;

        /// <summary>
        /// <c>Document</c> represents a text file which is either user-facing or for internal use.
        /// </summary>
        /// <param name="filepath">The path of the file to load.</param>
        /// <param name="internalDocument">Indicates if the document is internal, and can therefore operate with limited functionality.</param>
        public Document(string filepath, bool internalDocument = false)
        {

            filepath = ProcessFilepath(filepath);
            fileLines = new List<Line>();

            //calculate user-facing document properties (if not interal)
            if (!internalDocument)
            {
                //load theme file if one exists (and if it has a filepath at all)
                if (Path.GetExtension(filepath).Length > 0)
                {
                    string fileExtension = Path.GetExtension(filepath).Remove(0, 1);
                    fileExtension = fileExtension.TrimEnd(); //remove trailing whitespace
                    if (File.Exists(Settings.SyntaxDirectory + fileExtension + ".xml")) //load lexer settings if they are available for this filetype
                        Lexer.LoadSyntax(Settings.SyntaxDirectory + fileExtension + ".xml");
                }

                //apply scroll properties
                CalculateScrollIncrement();
                ScrollY = 0;
                ScrollX = 0;
            }

            LoadDocument(filepath);

        }

        public string ProcessFilepath(string filepath)
        {
            //if it doesn't exist, attempt to adjust
            if (!File.Exists(filepath))
            {
                //check for reserved filename
                switch (filepath)
                {
                    case "{themefile}":
                        return Settings.ThemeDirectory + Settings.ThemeFile;
                    case "{propfile}":
                        return Settings.settingsFile;
                }

                //check for path substitution
                if (filepath.Contains("{mapledir}"))
                    return filepath.Replace("{mapledir}", Settings.mapleDirectory);
                if (filepath.Contains("{themedir}"))
                    return filepath.Replace("{themedir}", Settings.ThemeDirectory);
                if (filepath.Contains("{syntaxdir}"))
                    return filepath.Replace("{syntaxdir}", Settings.SyntaxDirectory);
            }
            return filepath; //nothing to change
        }

        public void LoadDocument(string filepath)
        {
            //clear any lines that may have existed from before
            fileLines.Clear();

            //load new document
            this.Filepath = filepath;
            if(File.Exists(filepath))
            {
                List<string> fileLinesText = File.ReadAllLines(filepath).ToList<String>();
                
                foreach(string s in fileLinesText)
                    fileLines.Add(new Line(s));

                if(fileLines.Count == 0)
                    fileLines.Add(new Line(""));
            }
            else //file does not exist
            {
                //create a file
                File.Create(filepath).Close();
                fileLines = new List<Line>() { new Line("") };
                CommandLine.OutputText = "New file \"" + filepath.Trim() + "\" was created";
            }

        }

        public void SaveDocument(string savePath)
        {
            List<string> allLines = new List<string>();
            foreach(Line l in fileLines)
                allLines.Add(l.LineContent);
            File.WriteAllLines(savePath, allLines);
        }

        public void PrintFileLines()
        {
            for(int i = ScrollY; i < Cursor.MaxScreenY + ScrollY; i++)
                PrintLine(i);
        }

        public void PrintLine(int lineIndex)
        {
            //don't, if out of range
            if(lineIndex < 0 || lineIndex > fileLines.Count - 1 || lineIndex - ScrollY < 0 || lineIndex - ScrollY > Cursor.MaxScreenY - 1)
                return;

            Line l = fileLines[lineIndex];
            
            //clear line on screen
            Printer.ClearLine(lineIndex - ScrollY);
            Printer.MoveCursor(0, lineIndex - ScrollY);

            //build gutter content & print gutter
            String gutterContent = BuildGutter(lineIndex);
            Printer.PrintWord(gutterContent, foregroundColor: Styler.GutterColor);

            bool lineContainsSelection = LineContainsSelection(lineIndex);
            //find start and end relative to line
            int lineSelectInX = selectIn.X;
            if (selectIn.Y < lineIndex) //selection starts on previous line...
                lineSelectInX = 0; //...so it starts at 0 on this line
            int lineSelectOutX = selectOut.X;
            if (selectOut.Y > lineIndex) //selection ends on following line...
                lineSelectOutX = l.LineContent.Length; //...so it ends at the end of this line

            bool fullySelected = false;
            if (lineContainsSelection && lineSelectInX == 0 && lineSelectOutX >= l.LineContent.Length - 1)
                fullySelected = true;

            //print all tokens in line
            if(!Settings.DebugTokens) //ordinary printing:
            {
                int lineLen = 0;

                foreach(Token t in l.Tokens)
                {
                    //store difference between previous and current line lengths
                    int oldLineLen = lineLen;
                    lineLen += t.Text.Length;

                    //if token comes before scroll x (hidden to left), skip
                    if (lineLen < ScrollX)
                        continue;
                    //if token comes after scroll x (hidden to right), skip it and all subsequent tokens
                    if (oldLineLen > ScrollX + Cursor.MaxScreenX)
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
                        if (tSelectEnd >= t.Text.Length)
                            tSelectEnd = t.Text.Length;

                        tokenHasSelect = true;

                    }

                    string printText = t.Text;

                    //trim the parts of the token that are hidden by horizontal scroll
                    if (lineLen > ScrollX) //part of token is hidden to left, trim beginning
                    {
                        int hiddenCharCt = ScrollX - oldLineLen;
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
                            Printer.PrintWord(preSelectSubstring, foregroundColor: t.Color);
                            Printer.PrintWord(inSelectSubstring, foregroundColor: ConsoleColor.Black, backgroundColor: Styler.SelectionColor);
                            Printer.PrintWord(postSelectSubstring, foregroundColor: t.Color);
                        }
                        else //no precise selection to print
                        {
                            if (!fullySelected) //normal print
                                Printer.PrintWord(printText, foregroundColor: t.Color);
                            else //print fully selected
                                Printer.PrintWord(printText, foregroundColor: ConsoleColor.Black, backgroundColor: Styler.SelectionColor);
                        }
                    }
                    else if (lineLen > ScrollX + Cursor.MaxScreenX) //part of token is hidden to right, trim end
                    {
                        int hiddenCharCt = lineLen - (ScrollX + Cursor.MaxScreenX);
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
                                Printer.PrintWord(preSelectSubstring, foregroundColor: t.Color);
                                Printer.PrintWord(inSelectSubstring, foregroundColor: ConsoleColor.Black, backgroundColor: Styler.SelectionColor);
                                Printer.PrintWord(postSelectSubstring, foregroundColor: t.Color);
                            }
                            else //no precise selection to print
                            {
                                if (!fullySelected)
                                    Printer.PrintWord(printText, foregroundColor: t.Color);
                                else
                                    Printer.PrintWord(printText, foregroundColor: ConsoleColor.Black, backgroundColor: Styler.SelectionColor);
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
                foreach(Token t in l.Tokens)
                {
                    if(totalLength != -1)
                        totalLength += t.Text.Length;
                    if(totalLength > Editor.DocCursor.DX && lineIndex == Editor.DocCursor.DY)
                    {
                        totalLength = -1;
                        Printer.PrintWord(t.Text, foregroundColor: ConsoleColor.Black, backgroundColor: ConsoleColor.Yellow);
                        Console.Title = t.Text + ": " + t.TType;
                    }
                    else
                        Printer.PrintWord(t.Text, foregroundColor: t.Color);
                }
            }
        }

        /// <summary>
        /// Generate the gutter content for a given line according to set preferences.
        /// </summary>
        /// <param name="lineIndex">The index of the line to generate for.</param>
        /// <returns>A String representing the gutter text to render.</returns>
        string BuildGutter(int lineIndex)
        {
            string gutterContent = (lineIndex + 1).ToString();
            while(gutterContent.Length < GutterWidth - gutterPadding)
                gutterContent = "0" + gutterContent;
            while(gutterContent.Length < GutterWidth)
                gutterContent += " ";

            return gutterContent;
        }

        /// <summary>
        /// Safely scroll up by the Y scrolling increment.
        /// </summary>
        public void ScrollUp()
        {
            ScrollY -= scrollYIncrement;
            if(ScrollY < 0)
                ScrollY = 0;
        }

        /// <summary>
        /// Safely scroll down by the Y scrolling increment.
        /// </summary>
        public void ScrollDown()
        {
            ScrollY += scrollYIncrement;
        }

        /// <summary>
        /// Safely scroll left by the X scrolling increment.
        /// </summary>
        public void ScrollLeft()
        {
            ScrollX -= scrollXIncrement;
            if(ScrollX < 0)
                ScrollX = 0;
        }

        /// <summary>
        /// Safely scroll right by the X scrolling increment.
        /// </summary>
        public void ScrollRight()
        {
            ScrollX += scrollXIncrement;
        }

        /// <summary>
        /// Set the X and Y scroll increments based on the current buffer dimensions.
        /// </summary>
        public void CalculateScrollIncrement()
        {
            scrollYIncrement = (Cursor.MaxScreenY - 1) / 2;
            scrollXIncrement = (Cursor.MaxScreenX - 1) / 2;
        }

        public int CalculateGutterWidth()
        {
            int oldGutterWidth = GutterWidth;
            GutterWidth = fileLines.Count.ToString().Length + gutterPadding;

            if (GutterWidth != oldGutterWidth)
                Editor.RefreshAllLines();

            return GutterWidth;
        }

        /// <summary>
        /// Get the text of a given line in the document.
        /// </summary>
        /// <param name="index">The index of the line in question.</param>
        /// <returns>A String containing the line text.</returns>
        public String GetLine(int index)
        {
            if(index >= 0 && index < fileLines.Count)
                return fileLines[index].LineContent;
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
                lines.Add(l.LineContent);
            return lines;
        }

        public void SetLine(int index, String text)
        {
            if(index >= 0 && index < fileLines.Count)
                fileLines[index].LineContent = text;
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

        public string GetTextAtPosition(int x, int y)
        {
            if(x < 0 || y < 0 || y > fileLines.Count)
                return "";

            String currentLine = GetLine(y);
            return currentLine.Remove(0, x);
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
                return fileLines[line].LineContent.Length;
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

            if (selectOut.Y < selectIn.Y) //flip start and end if end is on a previous line
            {
                Point tempIn = new Point(selectIn.X, selectIn.Y);
                selectIn = new Point(selectOut.X, selectOut.Y);
                selectOut = tempIn;
            }
            else if (selectOut.Y == selectIn.Y && selectOut.X < selectIn.X) //flip start and end if end occurs first on same line
            {
                Point tempIn = new Point(selectIn.X, selectIn.Y);
                selectIn = new Point(selectOut.X, selectOut.Y);
                selectOut = tempIn;
            }

            //selecting a 0-width range causes errors (and why would you want to anyway?)
            if (selectIn.X == selectOut.X && selectIn.Y == selectOut.Y)
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
            return selectIn.X != -1 && selectIn.Y != -1 && selectOut.X != -1 && selectOut.Y != -1;
        }

        public bool HasSelectionStart()
        {
            return selectIn.X != -1 && selectIn.Y != -1;
        }

        public bool HasSelectionEnd()
        {
            return selectOut.X != -1 && selectIn.Y != -1;
        }

        /// <summary>
        /// Check if a given line contains selected text.
        /// </summary>
        /// <param name="lineIndex">The line in question.</param>
        /// <returns>Returns true if the line contains selected text.</returns>
        bool LineContainsSelection(int lineIndex)
        {
            return selectIn.Y <= lineIndex && selectOut.Y >= lineIndex;
        }

        /// <summary>
        /// Check if the current document selection spans multiple lines.
        /// </summary>
        /// <returns>Returns true if the current selection starts and ends on different lines.</returns>
        bool IsMultilineSelection()
        {
            return selectIn.Y != selectOut.Y;
        }
        
        public int GetSelectionInX() { return selectIn.X; }
        public int GetSelectionInY() { return selectIn.Y; }
        public int GetSelectionOutX() { return selectOut.X; }
        public int GetSelectionOutY() { return selectOut.Y; }

        /// <summary>
        /// Get the text contained within the current selection bounds.
        /// </summary>
        /// <returns>A String containing the current selection text.</returns>
        public string GetSelectionText()
        {

            Console.Title = selectIn.X + " , " + selectOut.X;

            if (selectIn.Y == -1 || selectOut.Y == -1) //skip if no selection
                return "";

            if (selectIn.Y == selectOut.Y) //just return substring if on same line
                return GetLine(selectIn.Y).Substring(selectIn.X, selectOut.X);

            //multiple lines
            String text = "";
            for (int y = selectIn.Y; y <= selectOut.Y; y++)
            {
                if (y == selectIn.Y)
                    text += GetLine(y).Substring(selectIn.X) + "\n";
                else if (y == selectOut.Y)
                    text += GetLine(y).Substring(0, selectOut.X);
                else
                    text += GetLine(y) + "\n";
            }

            //debug!
            File.WriteAllText("DEBUGLONGSELECT.txt", text);

            return text;
        }

    }
}
