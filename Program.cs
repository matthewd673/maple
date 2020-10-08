using System;

namespace maple
{
    class Program
    {

        static Cursor cursor;
        static FileManager fileManager;

        public static String userText = "";

        static void Main(string[] args)
        {
            PrepareWindow();
            Printer.DrawHeader("maple", backgroundColor: ConsoleColor.Yellow);

            //load file
            /*
            if(args.Length > 0)
            {
                fileManager = new FileManager(args[0]);
                fileManager.PrintFileLines();
            }
            */

            //create cursor
            cursor = new Cursor(0, 0);

            while(true)
            {
                InputManager.AcceptInput(Console.ReadKey());

                Console.Clear();
                Printer.DrawHeader("maple", backgroundColor: ConsoleColor.Yellow);
                Printer.PrintLine(userText, ConsoleColor.Blue);
            }
        }

        static void PrepareWindow()
        {
            Console.Clear();
            Console.Title = "maple";
        }

        public static Cursor GetCursor()
        {
            return cursor;
        }

        public static FileManager GetFileManager()
        {
            return fileManager;
        }

    }
}
