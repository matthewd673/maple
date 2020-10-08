using System;
using System.IO;

namespace maple
{
    class FileManager
    {

        String filepath;
        String[] fileLines;

        public FileManager(String filepath)
        {
            LoadFile(filepath);
        }

        public void LoadFile(String filepath)
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

    }
}