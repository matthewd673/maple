using System;

namespace maple
{
    public static class Printer
    {

        static ConsoleColor defaultForegroundColor = ConsoleColor.Gray;
        static ConsoleColor defaultBackgroundColor = ConsoleColor.Black;

        public static void printLine(String message, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            ResetColors();
        }

        public static void drawHeader(String message, ConsoleColor backgroundColor = ConsoleColor.Gray, ConsoleColor foregroundColor = ConsoleColor.Black)
        {
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