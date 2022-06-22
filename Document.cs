using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace maple
{
    class Document
    {

        public string Filepath { get; private set; }
        List<Line> fileLines;

        public int ScrollYIncrement { get; private set; } = 0;
        int scrollXIncrement = 0;
        public int ScrollY { get; private set; } = 0;
        public int ScrollX { get; private set; } = 0;

        Point selectIn = new Point(-1, -1);
        Point selectOut = new Point(-1, -1);

        public int SelectInX { get { return selectIn.X; }}
        public int SelectInY { get { return selectIn.Y; }}
        public int SelectOutX { get { return selectOut.X; }}
        public int SelectOutY { get { return selectOut.Y; }}

        public int GutterWidth { get; private set; } = 0;
        int gutterPadding = 2;

        public bool NewlyCreated { get; private set; } = false;

        private History history;

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
                Log.Write("Loading syntax information", "document");
                if (Path.GetExtension(filepath).Length > 0)
                {
                    string fileExtension = Path.GetExtension(filepath).Remove(0, 1);
                    fileExtension = fileExtension.TrimEnd(); //remove trailing whitespace
                    Lexer.LoadSyntax(Path.Combine(Settings.Properties.SyntaxDirectory, fileExtension + ".xml"));
                }
                else
                    Log.Write("Loaded document is marked as internal", "document");

                //apply scroll properties
                CalculateScrollIncrement();
                ScrollY = 0;
                ScrollX = 0;

                history = new History();
            }

            LoadDocument(filepath);
        }

        /// <summary>
        /// Pre-process a filepath to substitute in file nicknames.
        /// <param name="filepath">The path to pre-process.</param>
        /// </summary>
        public static string ProcessFilepath(string filepath)
        {
            //if it doesn't exist, attempt to adjust
            if (!File.Exists(filepath))
            {
                Log.Write("Filepath '" + filepath + "' doesn't exist, attempting substitution", "document");

                //check for reserved filename
                switch (filepath)
                {
                    case "{themefile}":
                        return Path.Combine(Settings.Properties.ThemeDirectory, Settings.Properties.ThemeFile);
                    case "{propfile}":
                        return Settings.PropertiesFile;
                    case "{aliasfile}":
                        return Settings.AliasesFile;
                    case "{shortcutfile}":
                        return Settings.ShortcutsFile;
                    case "{syntaxfile}":
                        return Lexer.CurrentSyntaxFile;
                }

                //check for path substitution
                if (filepath.Contains("{mapledir}"))
                    return filepath.Replace("{mapledir}", Settings.MapleDirectory);
                if (filepath.Contains("{themedir}"))
                    return filepath.Replace("{themedir}", Settings.Properties.ThemeDirectory);
                if (filepath.Contains("{syntaxdir}"))
                    return filepath.Replace("{syntaxdir}", Settings.Properties.SyntaxDirectory);
            }
            return filepath; //nothing to change
        }

        /// <summary>
        /// Load the contents of a file into the Document (skips filepath pre-processing and loading of any auxiliary files).
        /// <param name="filepath">The filepath to load from.</param>
        /// </summary>
        private void LoadDocument(string filepath)
        {
            //clear any lines that may have existed from before
            fileLines.Clear();
            history.Clear();

            //load new document
            Filepath = filepath;
            Log.Write("Filpath: " + Filepath, "document");
            if(File.Exists(filepath))
            {
                Log.Write("Loading from '" + filepath + "'", "document");
                List<string> fileLinesText = File.ReadAllLines(filepath, Encoding.UTF8).ToList<String>();
                
                foreach(string s in fileLinesText)
                    fileLines.Add(new Line(s));

                Log.Write("Loaded " + fileLines.Count + " lines", "document");

                if(fileLines.Count == 0)
                    fileLines.Add(new Line(""));

                Log.Write("Initial file loaded from '" + filepath + "'", "document");
            }
            else //file does not exist
            {
                //create a file
                File.CreateText(filepath).Close();
                fileLines = new List<Line>() { new Line("") };
                CommandLine.SetOutput(String.Format("New file \"{0}\" was created", filepath.Trim()), "maple", renderFooter: false);
                Log.Write("Initial file doesn't exist, created '" + filepath + "'", "document", important: true);
                NewlyCreated = true;
            }

        }

        /// <summary>
        /// Save the contents of the Document to the given filepath.
        /// <param name="savePath">The filepath to save to.</param>
        /// </summary>
        public void SaveDocument(string savePath, Encoding encoding)
        {
            List<string> allLines = new List<string>();
            foreach(Line l in fileLines)
                allLines.Add(l.LineContent);
            File.WriteAllLines(savePath, allLines, encoding);
            Log.Write("Saved file to '" + savePath + "'", "document");
        }

        /// <summary>
        /// Print a single line of the Document.
        /// <param name="lineIndex">The index of the line (in the Document, not screen).</param>
        /// </summary>
        public void PrintLine(int lineIndex)
        {
            //don't, if out of range
            if(lineIndex < 0 ||
                lineIndex > fileLines.Count - 1 ||
                lineIndex - ScrollY < 0 ||
                lineIndex - ScrollY > Printer.MaxScreenY - Footer.FooterHeight)
            {
                return;
            }

            Line l = fileLines[lineIndex];
            
            //clear line on screen
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
            if(!Settings.Properties.DebugTokens) //ordinary printing:
            {
                int lineLen = 0;

                short selectColorAttribute = Printer.GetAttributeFromColor(ConsoleColor.Black, Styler.SelectionColor);
                foreach(Token t in l.Tokens)
                {
                    //store difference between previous and current line lengths
                    int oldLineLen = lineLen;
                    lineLen += t.Text.Length;

                    //if token comes before scroll x (hidden to left), skip
                    if (lineLen < ScrollX)
                    {
                        continue;
                    }
                    //if token comes after scroll x (hidden to right), skip it and all subsequent tokens
                    if (oldLineLen > ScrollX + Printer.MaxScreenX)
                    {
                        break;
                    }

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
                            Printer.PrintWord(preSelectSubstring, t.ColorAttribute);
                            Printer.PrintWord(inSelectSubstring, selectColorAttribute);
                            Printer.PrintWord(postSelectSubstring, t.ColorAttribute);
                        }
                        else //no precise selection to print
                        {
                            if (!fullySelected) //normal print
                                Printer.PrintWord(printText, t.ColorAttribute);
                            else //print fully selected
                                Printer.PrintWord(printText, selectColorAttribute);
                        }
                    }
                    else if (lineLen > ScrollX + Printer.MaxScreenX) //part of token is hidden to right, trim end
                    {
                        int hiddenCharCt = lineLen - (ScrollX + Printer.MaxScreenX);
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
                                Printer.PrintWord(preSelectSubstring, t.ColorAttribute);
                                Printer.PrintWord(inSelectSubstring, selectColorAttribute);
                                Printer.PrintWord(postSelectSubstring, t.ColorAttribute);
                            }
                            else //no precise selection to print
                            {
                                if (!fullySelected)
                                    Printer.PrintWord(printText, t.ColorAttribute);
                                else
                                    Printer.PrintWord(printText, selectColorAttribute);
                            }

                        }
                        else //can't be negative
                            hiddenCharCt = 0;
                    }
                }

                Printer.ClearRight();

                //print overflow indicator
                if (lineLen - ScrollX + GutterWidth >= Printer.MaxScreenX)
                    Printer.PrintManually(
                        Settings.Properties.OverflowIndicatorChar,
                        Printer.MaxScreenX,
                        lineIndex - ScrollY,
                        (short)(Printer.GetAttributeAtPosition(Printer.MaxScreenX, lineIndex - ScrollY) << 4 & 0x00F0) //set background to old foreground, and foreground to black
                        );
            }
            else //debug printing:
            {
                int totalLength = 0;
                Token hovered = null;
                foreach(Token t in l.Tokens)
                {
                    if(totalLength != -1)
                        totalLength += t.Text.Length;
                    if(totalLength > Editor.DocCursor.DX && lineIndex == Editor.DocCursor.DY)
                    {
                        totalLength = -1;
                        Printer.PrintWord(t.Text, foregroundColor: ConsoleColor.Black, backgroundColor: ConsoleColor.Yellow);
                        hovered = t;
                    }
                    else
                        Printer.PrintWord(t.Text, foregroundColor: t.Color);
                }

                if (hovered != null)
                {
                    CommandLine.SetOutput(String.Format("({0},{1}) {2}, '{3}'", Editor.DocCursor.DX, Editor.DocCursor.DY, hovered.TType, hovered.Text), "tdebug");
                }
            }
        }

        /// <summary>
        /// Generate the gutter string for a given line according to set preferences.
        /// </summary>
        /// <param name="lineIndex">The index of the line to generate for.</param>
        /// <returns>A String representing the gutter text to render.</returns>
        string BuildGutter(int lineIndex)
        {
            string gutterContent = (lineIndex + 1).ToString();
            while(gutterContent.Length < GutterWidth - gutterPadding)
            {
                gutterContent = Settings.Properties.GutterLeftPadChar + gutterContent;
            }
            while(gutterContent.Length < GutterWidth)
            {
                gutterContent += " ";
                if (gutterContent.Length == GutterWidth - 1)
                    gutterContent += Settings.Properties.GutterBarrier;
            }

            return gutterContent;
        }

        /// <summary>
        /// Calculate the width of the gutter.
        /// </summary>
        public int CalculateGutterWidth()
        {
            int oldGutterWidth = GutterWidth;
            GutterWidth = fileLines.Count.ToString().Length + gutterPadding;

            if (GutterWidth != oldGutterWidth)
                Editor.RefreshAllLines();

            return GutterWidth;
        }

        /// <summary>
        /// Safely scroll up by the Y scrolling increment.
        /// </summary>
        public void ScrollUp()
        {
            ScrollY -= ScrollYIncrement;
            if(ScrollY < 0)
                ScrollY = 0;
        }

        /// <summary>
        /// Safely scroll down by the Y scrolling increment.
        /// </summary>
        public void ScrollDown()
        {
            ScrollY += ScrollYIncrement;
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
            Log.WriteDebug("old scroll x: " + ScrollX, "document");
            ScrollX += scrollXIncrement;
            Log.WriteDebug("new scroll x: " + ScrollX + " (" + scrollXIncrement + ")", "document");
        }

        /// <summary>
        /// Set the X and Y scroll increments based on the current buffer dimensions.
        /// </summary>
        public void CalculateScrollIncrement()
        {
            if (Settings.Properties.ScrollYIncrement.Equals("half"))
            {
                ScrollYIncrement = (Printer.MaxScreenY - 1) / 2;
            }
            else if (Settings.Properties.ScrollYIncrement.Equals("full"))
            {
                ScrollYIncrement = (Printer.MaxScreenY - 1);
            }
            else
            {
                int parsedScrollY = 0;
                bool couldParse = Int32.TryParse(
                    Settings.Properties.ScrollYIncrement,
                    System.Globalization.NumberStyles.Integer,
                    null,
                    out parsedScrollY
                    );
                if (couldParse)
                {
                    ScrollYIncrement = parsedScrollY;
                }
                else
                {
                    Log.Write("Failed to parse ScrollYIncrement, invalid value", "document", important: true);
                }
            }

            if (Settings.Properties.ScrollXIncrement.Equals("half"))
            {
                scrollXIncrement = (Printer.MaxScreenX - 1) / 2;
            }
            else if (Settings.Properties.ScrollXIncrement.Equals("full"))
            {
                scrollXIncrement = (Printer.MaxScreenX - 1);
            }
            else
            {
                int parsedScrollX = 0;
                bool couldParse = Int32.TryParse(
                    Settings.Properties.ScrollXIncrement,
                    System.Globalization.NumberStyles.Integer,
                    null,
                    out parsedScrollX
                    );
                if (couldParse)
                {
                    scrollXIncrement = parsedScrollX;
                }
                else
                {
                    Log.Write("Failed to parse ScrollXIncrement, invalid value", "document", important: true);
                }
            }
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

        /// <summary>
        /// Set the content of a line.
        /// </summary>
        /// <param name="index">The index of the line to modify.</param>
        /// <param name="text">The new string content of the line.</param>
        public void SetLine(int index, String text)
        {
            if(index >= 0 && index < fileLines.Count)
                fileLines[index].LineContent = text;
        }

        /// <summary>
        /// Attempt to add a string at the given position in the Document.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate (line index).</param>
        /// <param name="text">The string to add.</param>
        /// <returns>Return true if the string was inserted (X and Y bounds were valid).</returns>
        public bool AddTextAtPosition(int x, int y, String text)
        {
            if(x < 0 || y < 0 || y > fileLines.Count)
                return false;
            
            String currentLine = GetLine(y);
            if (x > currentLine.Length)
                return false;
            
            currentLine = currentLine.Insert(x, text);

            //no change made
            if(GetLine(y) == currentLine)
                return false;

            SetLine(y, currentLine);

            return true;

        }

        /// <summary>
        /// Attempt to remove a single character at the given position in the Document.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate (line index).</param>
        /// <returns>Returns a string of the character that was removed. If the character was not removed, returns a 0-length string (X and Y bounds were invalid).</returns>
        public string RemoveTextAtPosition(int x, int y)
        {
            if(x < 0 || y < 0 || y > fileLines.Count)
                return "";

            String currentLine = GetLine(y);
            if (x >= currentLine.Length)
                return "";
            
            string removed = currentLine[x].ToString();
            currentLine = currentLine.Remove(x, 1);

            // no change made
            // if(GetLine(y) == currentLine)
            //     return "";
            
            SetLine(y, currentLine);

            return removed;
        }

        /// <summary>
        /// Get the text at the given position (from the X position to the end of the line).
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate (line index).</param>
        /// <returns>Returns the text at the position (or an empty string if bounds are invalid).</returns>
        public string GetTextAtPosition(int x, int y)
        {
            if(x < 0 || y < 0 || y > fileLines.Count)
                return "";

            String currentLine = GetLine(y);
            return currentLine.Remove(0, x);
        }

        /// <summary>
        /// Get the Token nearest to the given position.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate (line index).</param>
        /// <returns>Returns the Token at the position (or a Token with a null string and TokenType.None if the bounds are invalid or no token exists).</returns>
        public Token GetTokenAtPosition(int x, int y)
        {
            if (x < 0 || x > GetLine(y).Length || y < 0 || y > fileLines.Count)
                return new Token(null, TokenType.None);
            
            int totalLength = 0;
            foreach(Token t in fileLines[y].Tokens)
            {
                totalLength += t.Text.Length;
                if(totalLength > Editor.DocCursor.DX)
                    return t;
            }

            return new Token(null, TokenType.None);
        }

        /// <summary>
        /// Insert an empty line at the given index.
        /// </summary>
        /// <param name="index">The index to insert at.</param>
        /// <returns>Returns true if the line was inserted (the index was valid).</returns>
        public bool AddLine(int index)
        {
            if(index < 0 || index > fileLines.Count)
                return false;
            
            fileLines.Insert(index, new Line(""));
            CalculateGutterWidth();
            return true;
        }

        /// <summary>
        /// Remove the line at the given index.
        /// </summary>
        /// <param name="index">The index to remove at.</param>
        /// <returns>Returns true if the line was deleted (the index was valid).</returns>
        public bool RemoveLine(int index)
        {
            if(index < 0 || index > fileLines.Count - 1)
                return false;

            fileLines.RemoveAt(index);
            CalculateGutterWidth();
            return true;
        }

        /// <summary>
        /// Get the maximum valid line index (Count - 1).
        /// </summary>
        /// <returns>The maximum valid line index.</returns>
        public int GetMaxLine()
        {
            return fileLines.Count - 1;
        }

        /// <summary>
        /// Get the number of tokens on a given line.
        /// </summary>
        /// <param name="line">The index of the line.</param>
        /// <returns>The number of tokens on the line (or 0 if the index is invalid).</returns>
        public int GetLineTokenCount(int line)
        {
            if (line < fileLines.Count)
                return fileLines[line].Tokens.Count;
            else
                return 0;
        }

        /// <summary>
        /// Re-tokenize the entire Document.
        /// </summary>
        public void ForceReTokenize()
        {
            for (int i = 0; i < fileLines.Count; i++)
                fileLines[i].ForceTokenize();
        }

        /// <summary>
        /// Set the selection in point.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate (line index).</param>
        /// <returns>Returns true if the selection points were swapped to maintain ordering.</returns>
        public bool MarkSelectionIn(int x, int y)
        {
            selectIn = new Point(x, y);
            return ArrangeSelectionPoints();
        }

        /// <summary>
        /// Set the selection out point.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate (line index).</param>
        /// <returns>Returns true if the selection points were swapped to maintain ordering.</returns>
        public bool MarkSelectionOut(int x, int y)
        {
            if (selectIn.X == -1 || selectIn.Y == -1) return false;
            
            selectOut = new Point(x, y);
            return ArrangeSelectionPoints();
        }

        /// <summary>
        /// Reset the selection bounds.
        /// </summary>
        public void Deselect()
        {
            selectIn = new Point(-1, -1);
            selectOut = new Point(-1, -1);
        }

        /// <summary>
        /// Rearrange the selection points so that the starting point comes before the ending point.
        /// </summary>
        /// <returns>Returns true if the points were swapped, false otherwise.</returns>
        bool ArrangeSelectionPoints()
        {

            if (!HasSelection())
                return false;

            if (selectOut.Y < selectIn.Y) //flip start and end if end is on a previous line
            {
                Point tempIn = new Point(selectIn.X, selectIn.Y);
                selectIn = new Point(selectOut.X, selectOut.Y);
                selectOut = tempIn;
                return true;
            }
            else if (selectOut.Y == selectIn.Y && selectOut.X < selectIn.X) //flip start and end if end occurs first on same line
            {
                Point tempIn = new Point(selectIn.X, selectIn.Y);
                selectIn = new Point(selectOut.X, selectOut.Y);
                selectOut = tempIn;
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Check if the document has a complete selection (beginning and end).
        /// </summary>
        /// <returns>Returns true if the document has a starting and ending selection bound.</returns>
        public bool HasSelection()
        {
            return selectIn.X != -1 && selectIn.Y != -1 && selectOut.X != -1 && selectOut.Y != -1;
        }

        /// <summary>
        /// Check if the Document has a starting selection bound.
        /// </summary>
        /// <returns>Returns true if the Document has a starting selection bound.</returns>
        public bool HasSelectionStart()
        {
            return selectIn.X != -1 && selectIn.Y != -1;
        }

        /// <summary>
        /// Check if the Document has an ending selection bound.
        /// </summary>
        /// <returns>Returns true if the Document has an ending selection bound.</returns>
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

        private string GetBlockText(int aX, int aY, int bX, int bY)
        {
            String text = "";
            for (int i = aY; i <= bY; i++)
            {
                if (i == aY && i == bY)
                {
                    text += GetLine(i).Substring(aX, bX - aX);
                }
                else if (i == aY)
                {
                    text += GetLine(i).Substring(aX) + "\n";
                }
                else if (i == bY)
                {
                    text += GetLine(i).Substring(0, bX);
                }
                else
                {
                    text += GetLine(i) + "\n";
                }
            }

            return text;
        }

        private string RemoveBlockText(int aX, int aY, int bX, int bY)
        {
            string text = "";

            for (int i = aY; i <= bY; i++)
            {
                if (i == aY && i == bY)
                {
                    text += GetLine(i).Substring(aX, bX - aX);
                    SetLine(i, GetLine(i).Remove(aX, bX - aX));
                }
                else if (i == aY) // trim end
                {
                    text += GetLine(i).Substring(aX) + "\n";
                    SetLine(i, GetLine(i).Remove(aX));
                }
                else if (i == bY)
                {
                    string lineContent = GetLine(i).Remove(0, bX);
                    text += GetLine(i).Substring(0, bX);
                    SetLine(i - 1, GetLine(i - 1) + lineContent);
                    RemoveLine(i);
                }
                else
                {
                    text += GetLine(i) + "\n";
                    RemoveLine(i);
                    bY--;
                    i--;
                }
            }

            return text;
        }

        public Point AddBlockText(int x, int y, string text)
        {
            string[] lines = text.Split('\n');
            
            string followingText = GetLine(y).Substring(x);
            SetLine(y, GetLine(y).Remove(x));

            int outX = x;
            int outY = y;

            int i = y;
            foreach (string l in lines)
            {
                if (i == y && i == y + lines.Length - 1)
                {
                    outX = GetLine(i).Length + l.Length;
                    SetLine(i, GetLine(i) + l + followingText);
                }
                else if (i == y)
                {
                    SetLine(i, GetLine(i) + l);
                    AddLine(i + 1);
                }
                else if (i == y + lines.Length - 1)
                {
                    outX = l.Length;
                    outY = i;
                    SetLine(i, l + followingText + GetLine(i));
                }
                else
                {
                    SetLine(i, l);
                    AddLine(i + 1);
                }

                i++;
            }

            return new Point(outX, outY);
        }

        /// <summary>
        /// Get the text contained within the current selection bounds.
        /// </summary>
        /// <returns>A String containing the current selection text.</returns>
        public string GetSelectionText()
        {
            if (selectIn.Y == -1 || selectOut.Y == -1) //skip if no selection
                return "";

            return GetBlockText(SelectInX, SelectInY, SelectOutX, SelectOutY);
        }

        /// <summary>
        /// Delete the text contained within the current selection bounds.
        /// </summary>
        public void RemoveSelectionText()
        {
            if (HasSelection())
            {
                string removed = RemoveBlockText(SelectInX, SelectInY, SelectOutX, SelectOutY);
                LogHistoryEvent(new HistoryEvent(
                    HistoryEventType.RemoveSelection,
                    removed,
                    new Point(SelectInX, SelectInY),
                    new Point(Editor.DocCursor.DX, Editor.DocCursor.DY),
                    new Point[] { selectIn, selectOut }
                ));
            }
        }

        /// <summary>
        /// Deindent the current line or selection in the Document.
        /// </summary>
        public void Deindent()
        {
            string tabString = "";
            for (int i = 0; i < Settings.Properties.TabSpacesCount; i++)
                tabString += " ";
            
            if (HasSelection())
            {
                for (int i = SelectInY; i <= SelectOutY; i++)
                {
                    if (GetLine(i).StartsWith(tabString))
                    {
                        SetLine(
                            i,
                            GetLine(i).Remove(0, tabString.Length)
                        );
                        if (i == SelectInY)
                            MarkSelectionIn(SelectInX - tabString.Length, i);
                        if (i == SelectOutY)
                        {
                            MarkSelectionOut(SelectOutX - tabString.Length, i);
                            Editor.DocCursor.Move(Editor.DocCursor.DX - tabString.Length, Editor.DocCursor.DY);
                        }
                    }
                    Editor.RefreshLine(i);
                }
            }
            else
            {
                if (GetLine(Editor.DocCursor.DY).StartsWith(tabString))
                {
                    SetLine(
                            Editor.DocCursor.DY,
                            GetLine(Editor.DocCursor.DY).Remove(0, tabString.Length)
                        );
                    
                    Editor.DocCursor.Move(Editor.DocCursor.DX - tabString.Length, Editor.DocCursor.DY);
                    
                    Editor.RefreshLine(Editor.DocCursor.DY);
                }
            }
        }

        public void LogHistoryEvent(HistoryEvent historyEvent)
        {
            history.PushEvent(historyEvent);
        }

        public void Undo(bool redo = false)
        {
            HistoryEvent last;
            if (!redo)
            {
                if (!history.HasNext()) // nothing to undo
                {
                    CommandLine.SetOutput("No changes to undo", "undo");
                    return;
                }
                last = history.PopEvent();
            }
            else
            {
                if (!history.HasNextRedo())
                {
                    CommandLine.SetOutput("No changes to redo", "redo");
                    return;
                }
                last = history.PopRedoEvent();
            }

            if ((!redo && last.EventType == HistoryEventType.Add) || // did add, now remove
                (redo && last.EventType == HistoryEventType.Remove)) // undid remove, now remove
            {
                SetLine(last.DeltaPos.Y, 
                    GetLine(last.DeltaPos.Y).Remove(last.DeltaPos.X, last.TextDelta.Length));
                
                Editor.DocCursor.Move(last.CursorPos.X, last.CursorPos.Y);
                Editor.RefreshLine(last.DeltaPos.Y);
            }
            else if ((!redo && last.EventType == HistoryEventType.Remove) || // did remove, now add
                    (redo && last.EventType == HistoryEventType.Add)) // undid add, now add
            {
                SetLine(last.DeltaPos.Y,
                    GetLine(last.DeltaPos.Y).Insert(last.DeltaPos.X, last.TextDelta));
                
                Editor.DocCursor.Move(last.CursorPos.X, last.CursorPos.Y);
                Editor.RefreshLine(last.DeltaPos.Y);
            }
            // TODO: redo
            else if ((!redo && last.EventType == HistoryEventType.RemoveSelection) || // did remove selection, now add
                    (redo && last.EventType == HistoryEventType.AddSelection)) // undid add selection, now add
            {
                Point addOutPoint = AddBlockText(last.DeltaPos.X, last.DeltaPos.Y, last.TextDelta);
                
                if (!redo)
                {
                selectIn = last.SelectionPoints[0];
                selectOut = last.SelectionPoints[1];
                }

                Editor.DocCursor.Move(last.CursorPos.X, last.CursorPos.Y);
                Editor.RefreshAllLines();
            }
            else if ((!redo && last.EventType == HistoryEventType.AddSelection) || /// did add selection, now remove
                    (redo && last.EventType == HistoryEventType.RemoveSelection)) // undid remove selection, now remove
            {
                string[] textDeltaLines = last.TextDelta.Split('\n');
                int blockEndX = textDeltaLines[^1].Length;
                if (textDeltaLines.Length == 1)
                {
                    blockEndX += last.DeltaPos.X;
                }

                Log.WriteDebug(last.DeltaPos.X + ", " + last.DeltaPos.Y, "document/undo");

                RemoveBlockText(last.DeltaPos.X,
                                last.DeltaPos.Y,
                                blockEndX,
                                last.DeltaPos.Y + textDeltaLines.Length - 1
                                );

                if (redo) Deselect();
                
                Editor.DocCursor.Move(last.CursorPos.X, last.CursorPos.Y);
                Editor.RefreshAllLines();
            }
            else if (!redo && last.EventType == HistoryEventType.AddLine) // did add line, now remove
            {
                RemoveLine(last.DeltaPos.Y + 1);
                SetLine(last.DeltaPos.Y, GetLine(last.DeltaPos.Y) + last.TextDelta);
                
                Editor.DocCursor.Move(last.CursorPos.X, last.CursorPos.Y);
                Editor.RefreshAllLines();
            }
            else if (redo && last.EventType == HistoryEventType.RemoveLine)
            {
                RemoveLine(last.DeltaPos.Y + 1);
                SetLine(last.DeltaPos.Y, GetLine(last.DeltaPos.Y) + GetLine(last.DeltaPos.Y + 1));

                Editor.DocCursor.Move(last.CursorPos.X, last.CursorPos.Y);
                Editor.RefreshAllLines();
            }
            else if (!redo && last.EventType == HistoryEventType.RemoveLine) // did remove line, now add
            {
                AddLine(last.DeltaPos.Y + 1);
                SetLine(last.DeltaPos.Y + 1, GetLine(last.DeltaPos.Y).Substring(last.DeltaPos.X));
                SetLine(last.DeltaPos.Y, GetLine(last.DeltaPos.Y).Remove(last.DeltaPos.X));
                
                Editor.DocCursor.Move(last.CursorPos.X, last.CursorPos.Y);
                Editor.RefreshAllLines();
            }
            else if (redo && last.EventType == HistoryEventType.AddLine)
            {
                AddLine(last.DeltaPos.Y + 1);
                SetLine(last.DeltaPos.Y + 1, GetLine(last.DeltaPos.Y).Substring(last.DeltaPos.X));
                SetLine(last.DeltaPos.Y, GetLine(last.DeltaPos.Y).Remove(last.DeltaPos.X));
                
                Editor.DocCursor.Move(last.CursorPos.X, last.CursorPos.Y);
                Editor.RefreshAllLines();
            }
            else if ((!redo && last.EventType == HistoryEventType.IndentLine) || // did indent line, now deindent
                    (redo && last.EventType == HistoryEventType.DeindentLine))
            {
                selectIn = last.SelectionPoints[0];
                selectOut = last.SelectionPoints[1];
                Deindent();

                Editor.DocCursor.Move(last.CursorPos.X, last.CursorPos.Y);
                Editor.RefreshAllLines();
            }
            else if ((!redo && last.EventType == HistoryEventType.DeindentLine) || // did deindent line, now indent
                    (redo && last.EventType == HistoryEventType.IndentLine))
            {
                string tabString = "";
                for (int i = 0; i < Settings.Properties.TabSpacesCount; i++) tabString += " ";
                for (int i = last.SelectionPoints[0].Y; i <= last.SelectionPoints[1].Y; i++)
                {
                    SetLine(i, tabString + GetLine(i));
                }

                Editor.DocCursor.Move(last.CursorPos.X, last.CursorPos.Y);
                Editor.RefreshAllLines();
            }

            // if combined event, trigger last event automatically
            if (last.Combined)
            {
                Undo();
            }
        }
    }
}
