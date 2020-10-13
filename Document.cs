using System;
using System.IO;

namespace maple
{
    class Document
    {

        String filepath;
        String[] fileLines;

        public Document(String filepath)
        {
            LoadDocument(filepath);
        }

        public void LoadDocument(String filepath)
        {
            this.filepath = filepath;
            fileLines = File.ReadAllLines(filepath);
        }

        public void PrintFileLines()
        {
            foreach(String s in fileLines)
            {
                Printer.PrintLine(s);
            }
        }

        public String GetLine(int index)
        {
            if(index >= 0 && index < fileLines.Length)
                return fileLines[index];
            else
                return "";
        }

        public void SetLine(int index, String text)
        {
            if(index >= 0 && index < fileLines.Length)
                fileLines[index] = text;
        }

        public bool AddTextAtPosition(int x, int y, String text)
        {
            if(x < 0 || y < 0 || y > fileLines.Length)
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
            if(x < 0 || y < 0 || y > fileLines.Length)
                return false;

            String currentLine = GetLine(y);
            currentLine = RemoveText(currentLine, x);

            //no change made
            if(GetLine(y) == currentLine)
                return false;
            
            SetLine(y, currentLine);

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

        public int GetLineLength(int line)
        {
            if(line < fileLines.Length)
                return fileLines[line].Length;
            else
                return 0;
        }

    }
}