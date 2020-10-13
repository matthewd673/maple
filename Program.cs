using System;

namespace maple
{
    class Program
    {

        static Cursor cursor;
        static Document document;

        public static String userText = "";

        static void Main(string[] args)
        {
            PrepareWindow();
            
            //create cursor
            cursor = new Cursor(0, 0);

            Printer.DrawHeader("maple", backgroundColor: ConsoleColor.Yellow);

            //load file
            if(args.Length > 0)
            {
                document = new Document(args[0]);
                document.PrintFileLines();
                cursor.MoveCursor(cursor.GetDocumentX(), cursor.GetDocumentY());
            }

            while(true)
            {
                Input.AcceptInput(Console.ReadKey());

                Console.Clear();
                Printer.DrawHeader("maple (" + cursor.GetDocumentX() + ", " + cursor.GetDocumentY() + ")", backgroundColor: ConsoleColor.Yellow);
                //Printer.PrintLine(userText, ConsoleColor.Blue);
                document.PrintFileLines();

                //reset to user cursor position
                cursor.MoveCursor(cursor.GetDocumentX(), cursor.GetDocumentY());
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

        public static Document GetDocument()
        {
            return document;
        }

    }
}
