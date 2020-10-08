using System;

namespace maple
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Printer.printLine("maple", ConsoleColor.Yellow);
            Printer.drawHeader("maple", ConsoleColor.Yellow);

            Cursor c = new Cursor(0, 0);

            while(true)
            {
                c.AcceptInput();
            }
        }

        static void PrepareWindow()
        {
            Console.Title = "maple";
        }

    }
}
