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

        public Document(String filepath)
        {
            fileLines = new List<Line>();
            LoadDocument(filepath);
        }

        public void LoadDocument(String filepath)
        {
            this.filepath = filepath;
            if(File.Exists(filepath))
            {
                List<String> fileLinesText = File.ReadAllLines(filepath).ToList<String>();
                
                foreach(String s in fileLinesText)
                {
                    List<Token> lineTokens = Line.GenerateTokensFromString(s);
                    fileLines.Add(new Line(lineTokens));
                }

                if(fileLines.Count == 0)
                    fileLines.Add(new Line(new List<Token>()));
            }
            else
            {
                Printer.PrintLine("Creating new files is not supported at this time", ConsoleColor.Blue);
                //File.Create(filepath);
                //fileLines = new List<String>() { "" };
            }
        }

        public void SaveDocument()
        {
            //File.WriteAllLines(filepath, fileLines);
        }

        public String GetFilePath()
        {
            return filepath;
        }

        public void PrintFileLines()
        {
            //set initial cursor position
            foreach(Line l in fileLines)
            {
                foreach(Token t in l.GetTokens())
                {
                    Printer.PrintWord(t.GetText() + " ", foregroundColor: t.GetColor());
                }
                //Printer.PrintLine(s);
                Console.WriteLine();
            }
        }

        public void PrintLine(int lineIndex)
        {
            //don't, if out of range
            if(lineIndex < 0 || lineIndex > fileLines.Count - 1)
                return;

            Line l = fileLines[lineIndex];
            
            //clear line on screen
            Printer.ClearLine(lineIndex);
            Printer.MoveCursor(0, lineIndex);
            //print all tokens in line
            foreach(Token t in l.GetTokens())
            {
                Printer.PrintWord(t.GetText() + " ", foregroundColor: t.GetColor());
            }
        }

        public String GetLine(int index)
        {
            if(index >= 0 && index < fileLines.Count)
                return fileLines[index].GetContent();
            else
                return "";
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
            
            fileLines.Insert(index, new Line(new List<Token>()));
            return true;
        }
        
        public bool RemoveLine(int index)
        {
            if(index < 0 || index > fileLines.Count - 1)
                return false;

            fileLines.RemoveAt(index);
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