using System;

namespace maple
{
    public static class Printer
    {

        static ConsoleColor defaultForegroundColor = ConsoleColor.Gray;
        static ConsoleColor defaultBackgroundColor = ConsoleColor.Black;

        static Cursor printerCursor = new Cursor(0, 0);

        public static void PrintLine(String message, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine(message);
            ResetColors();
        }

        public static void DrawHeader(String message, ConsoleColor foregroundColor = ConsoleColor.Black, ConsoleColor backgroundColor = ConsoleColor.Gray)
        {
            printerCursor.SetPosition(0, 0);
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(message);
            ResetColors();
        }

        public static void ResetColors()
        {
            Console.ForegroundColor = defaultForegroundColor;
            Console.BackgroundColor = defaultBackgroundColor;
        }
    }
}