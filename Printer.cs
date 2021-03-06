using System;

namespace maple
{
    public static class Printer
    {

        static ConsoleColor defaultForegroundColor = ConsoleColor.Gray;
        static ConsoleColor defaultBackgroundColor = ConsoleColor.Black;

        static Cursor printerCursor = new Cursor(0, 0);

        static String clearString = " ";
        public static void Resize()
        {
            clearString = new string(' ', Console.WindowWidth);
        }

        public static void PrintWord(String word, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(word);
            ResetColors();
        }

        public static void MoveCursor(int x, int y)
        {
            printerCursor.Move(x, y);
        }

        public static void PrintLine(String message, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine(message);
            ResetColors();
        }

        public static void DrawHeader(String content, ConsoleColor foregroundColor = ConsoleColor.Black, ConsoleColor backgroundColor = ConsoleColor.Gray, int offsetTop = 0)
        {
            ClearLine(offsetTop);
            printerCursor.Move(0, offsetTop);
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(content);
            ResetColors();
        }

        public static void DrawFooter(String content, ConsoleColor foregroundColor = ConsoleColor.Black, ConsoleColor backgroundColor = ConsoleColor.Gray)
        {
            ClearFooter();
            WriteToFooter(content, 0, foregroundColor, backgroundColor);
        }

        public static void ClearFooter(ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            ClearLine(Cursor.maxScreenY, backgroundColor);
            ResetColors();
        }

        public static void WriteToFooter(String text, int x = 0, ConsoleColor foregroundColor = ConsoleColor.Black, ConsoleColor backgroundColor = ConsoleColor.Gray)
        {
            if (x != -1) //manual cursor position
                printerCursor.Move(x, Cursor.maxScreenY);
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.Write(text);
            ResetColors();
        }

        public static void ResetColors()
        {
            Console.ForegroundColor = defaultForegroundColor;
            Console.BackgroundColor = defaultBackgroundColor;
        }

        public static void ClearLine(int line, ConsoleColor clearColor = ConsoleColor.Black)
        {
            //don't clear if out of range
            if(line < 0 || line > Cursor.maxScreenY)
                return;

            //overwrite entire line
            Console.BackgroundColor = clearColor;
            Console.SetCursorPosition(0, line);
            Console.Write(clearString);
            ResetColors();
        }
    }
}