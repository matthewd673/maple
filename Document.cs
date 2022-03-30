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
                    Lexer.LoadSyntax(Path.Combine(Settings.SyntaxDirectory, fileExtension + ".xml"));
                }
                else
                    Log.Write("Loaded document is marked as internal", "document");

                //apply scroll properties
                CalculateScrollIncrement();
                ScrollY = 0;
                ScrollX = 0;
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
                        return Path.Combine(Settings.ThemeDirectory, Settings.ThemeFile);
                    case "{propfile}":
                        return Settings.SettingsFile;
                    case "{aliasfile}":
                        return Settings.AliasesFile;
                    case "{shortcutfile}":
                        return Settings.ShortcutsFile;
                }

                //check for path substitution
                if (filepath.Contains("{mapledir}"))
                    return filepath.Replace("{mapledir}", Settings.MapleDirectory);
                if (filepath.Contains("{themedir}"))
                    return filepath.Replace("{themedir}", Settings.ThemeDirectory);
                if (filepath.Contains("{syntaxdir}"))
                    return filepath.Replace("{syntaxdir}", Settings.SyntaxDirectory);
            }
            return filepath; //nothing to change
        }

        /// <summary>
        /// Load the contents of a file into the Document (skips filepath pre-processing and loading of any auxiliary files).
        /// <param name="filepath">The filepath to load from.</param>
        /// </summary>
        public void LoadDocument(string filepath)
        {
            //clear any lines that may have existed from before
            fileLines.Clear();

            //load new document
            this.Filepath = filepath;
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
                CommandLine.OutputText = String.Format("New file \"{0}\" was created", filepath.Trim());
                Log.Write("Initial file doesn't exist, created '" + filepath + "'", "document", important: true);
                NewlyCreated = true;
            }

        }

        /// <summary>
        /// Save the contents of the Document to the given filepath.
        /// <param name="savePath">The filepath to save to.</param>
        /// </summary>
        public void SaveDocument(string savePath)
        {
            List<string> allLines = new List<string>();
            foreach(Line l in fileLines)
                allLines.Add(l.LineContent);
            File.WriteAllLines(savePath, allLines, Encoding.UTF8);
            Log.Write("Saved file to '" + savePath + "'", "document");
        }

        /// <summary>
        /// Print all files currently within the bounds of the screen.
        /// </summary>
        public void PrintFileLines()
        {
            for(int i = ScrollY; i < Cursor.MaxScreenY + ScrollY; i++)
                PrintLine(i);
        }

        /// <summary>
        /// Print a single line of the Document.
        /// <param name="lineIndex">The index of the line (in the Document, not screen).</param>
        /// </summary>
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
                    {
                        continue;
                    }
                    //if token comes after scroll x (hidden to right), skip it and all subsequent tokens
                    if (oldLineLen > ScrollX + Cursor.MaxScreenX)
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

                //print overflow indicator
                if (lineLen - ScrollX + GutterWidth >= Cursor.MaxScreenX)
                    Printer.PrintManually(
                        Styler.OverflowIndicator,
                        Cursor.MaxScreenX,
                        lineIndex - ScrollY,
                        (short)(Printer.GetAttributeAtPosition(Cursor.MaxScreenX, lineIndex - ScrollY) << 4 & 0x00F0) //set background to old foreground, and foreground to black
                        );

                Printer.ApplyBuffer();

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
                gutterContent = Styler.GutterLeftPad + gutterContent;
            while(gutterContent.Length < GutterWidth)
            {
                gutterContent += " ";
                if (gutterContent.Length == GutterWidth - 1)
                    gutterContent += Styler.GutterBarrier;
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
            ScrollX += scrollXIncrement;
        }

        /// <summary>
        /// Set the X and Y scroll increments based on the current buffer dimensions.
        /// </summary>
        public void CalculateScrollIncrement()
        {
            if (Settings.ScrollYIncrement == -1) //"half"
                ScrollYIncrement = (Cursor.MaxScreenY - 1) / 2;
            else if (Settings.ScrollYIncrement == -2) //"full"
                ScrollYIncrement = (Cursor.MaxScreenY - 1);
            else
                ScrollYIncrement = Settings.ScrollYIncrement;

            if (Settings.ScrollXIncrement == -1) //"half"
                scrollXIncrement = (Cursor.MaxScreenX - 1) / 2;
            else if (Settings.ScrollXIncrement == -2) //"full"
                scrollXIncrement = (Cursor.MaxScreenX - 1);
            else
                scrollXIncrement = Settings.ScrollXIncrement;
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
        /// <returns>Returns true if the character was deleted (X and Y bounds were valid).</returns>
        public bool RemoveTextAtPosition(int x, int y)
        {
            if(x < 0 || y < 0 || y > fileLines.Count)
                return false;

            String currentLine = GetLine(y);
            if (x >= currentLine.Length)
                return false;
            
            currentLine = currentLine.Remove(x, 1);

            //no change made
            if(GetLine(y) == currentLine)
                return false;
            
            SetLine(y, currentLine);

            return true;
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
                return new Token(null, Token.TokenType.None);
            
            int totalLength = 0;
            foreach(Token t in fileLines[y].Tokens)
            {
                totalLength += t.Text.Length;
                if(totalLength > Editor.DocCursor.DX)
                    return t;
            }

            return new Token(null, Token.TokenType.None);
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
                return fileLines[line].Tokens.Length;
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

            //selecting a 0-width range causes errors (and why would you want to anyway?)
            // if (selectIn.X == selectOut.X && selectIn.Y == selectOut.Y)
            // {
            //     selectIn = new Point(-1, -1);
            //     selectOut = new Point(-1, -1);
            // }

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

        /// <summary>
        /// Check if the current document selection spans multiple lines.
        /// </summary>
        /// <returns>Returns true if the current selection starts and ends on different lines.</returns>
        bool IsMultilineSelection()
        {
            return selectIn.Y != selectOut.Y;
        }

        /// <summary>
        /// Get the text contained within the current selection bounds.
        /// </summary>
        /// <returns>A String containing the current selection text.</returns>
        public string GetSelectionText()
        {
            if (selectIn.Y == -1 || selectOut.Y == -1) //skip if no selection
                return "";

            Log.WriteDebug("Selection bounds: (" + selectIn.X + ", " + selectIn.Y + "), (" + selectOut.X + ", " + selectOut.Y + ")", "document");

            if (selectIn.Y == selectOut.Y) //just return substring if on same line
            {
                string lineContent = GetLine(selectIn.Y).Substring(selectIn.X, selectOut.X - selectIn.X);
                Log.WriteDebug("Selection text: '" + lineContent + "'", "document");
                return lineContent;
            }

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

            return text;
        }

        /// <summary>
        /// Delete the text contained within the current selection bounds.
        /// </summary>
        public void DeleteSelectionText()
        {
            if (HasSelection())
            {
                if (SelectOutY > SelectInY) //multi-line selection
                {
                    for (int i = SelectInY; i <= SelectOutY; i++) {
                        if (i == SelectInY) { //trim end
                            SetLine(i,
                                GetLine(i).Remove(SelectInX, GetLine(i).Length - SelectInX)
                                );                            
                        }
                        else if (i == SelectOutY) { //trim start
                            string lastLineContent = GetLine(i).Remove(0, SelectOutX);
                            SetLine(i - 1,
                                GetLine(i - 1) + lastLineContent
                                );
                        }
                        else { //remove
                            RemoveLine(i);
                            i--;
                            MarkSelectionOut(SelectOutX, SelectOutY - 1); //move select out down
                        }
                    }
                }
                else //one line
                {
                    //remove from line
                    SetLine(
                        SelectInY,
                        GetLine(SelectInY).Remove(
                            SelectInX, SelectOutX - SelectInX
                        )
                    );
                }
            }
        }

        /// <summary>
        /// Deindent the current line or selection in the Document.
        /// </summary>
        public void Deindent()
        {
            string tabString = "";
            for (int i = 0; i < Settings.TabSpacesCount; i++)
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
                            MarkSelectionOut(SelectInX - tabString.Length, i);

                        if (i == SelectOutY)
                        {
                            MarkSelectionOut(SelectOutX - tabString.Length, SelectOutY);
                            Editor.DocCursor.DX -= tabString.Length;
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
                    
                    Editor.DocCursor.DX -= tabString.Length;
                    
                    Editor.RefreshLine(Editor.DocCursor.DY);
                }
            }
        }

    }
}
