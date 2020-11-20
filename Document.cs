using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace maple
{
    class Document
    {

        String filepath;
        List<Line> fileLines;

        int scrollIncrement = 0;
        int scrollY = 0;

        int gutterWidth = 0;
        int gutterPadding = 2;

        public Document(String filepath, bool internalDocument = false)
        {
            fileLines = new List<Line>();
            LoadDocument(filepath);

            //calculate external document properties (if not interal)
            if (!internalDocument)
            {
                //load theme file if one exists
                String fileExtension = Path.GetExtension(filepath).Remove(0, 1);
                if(File.Exists("themes/" + fileExtension + ".txt"))
                    Styler.LoadTheme("themes/" + fileExtension + ".txt");

                //apply new properties (if not internal)
                CalculateScrollIncrement();
                scrollY = 0;
            }

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

            //print all tokens in line
            foreach(Token t in l.GetTokens())
                Printer.PrintWord(t.GetText() + " ", foregroundColor: t.GetColor());
        }

        String BuildGutter(int lineIndex)
        {
            String gutterContent = (lineIndex + 1).ToString();
            while(gutterContent.Length < gutterWidth - gutterPadding)
                gutterContent = "0" + gutterContent;
            while(gutterContent.Length < gutterWidth)
                gutterContent += " ";
            return gutterContent;
        }

        public void ScrollUp()
        {
            scrollY -= scrollIncrement;
            if(scrollY < 0)
                scrollY = 0;
        }

        public void ScrollDown()
        {
            scrollY += scrollIncrement;
        }

        public int GetScrollY() { return scrollY; }

        public void CalculateScrollIncrement()
        {
            scrollIncrement = (Cursor.maxScreenY - 1) / 2;
        }

        public int CalculateGutterWidth()
        {
            int oldGutterWidth = gutterWidth;
            gutterWidth = fileLines.Count.ToString().Length + gutterPadding;

            if(gutterWidth != oldGutterWidth)
                Editor.RefreshAllLines();

            return gutterWidth;
        }

        public String GetLine(int index)
        {
            if(index >= 0 && index < fileLines.Count)
                return fileLines[index].GetContent();
            else
                return "";
        }

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

    }
}
