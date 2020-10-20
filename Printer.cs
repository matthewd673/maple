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
            printerCursor.ForceMoveCursor(0, offsetTop);
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(content);
            ResetColors();
        }

        public static void DrawFooter(String content, ConsoleColor foregroundColor = ConsoleColor.Black, ConsoleColor backgroundColor = ConsoleColor.Gray, int offsetBottom = 0)
        {
            ClearLine(Cursor.maxScreenY - offsetBottom);
            printerCursor.ForceMoveCursor(0, Cursor.maxScreenY - offsetBottom);
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.Write(content);
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