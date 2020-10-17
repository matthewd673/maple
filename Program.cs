using System;

namespace maple
{
    class Program
    {

        static Cursor docCursor;
        static Cursor cmdCursor;
        static Document document;

        public static String userText = "";

        static void Main(string[] args)
        {
            PrepareWindow();
            
            //create cursor
            docCursor = new Cursor(0, 0);
            cmdCursor = new Cursor(0, 0);

            cmdCursor.contentOffsetX = 2;
            cmdCursor.contentOffsetY = Cursor.maxScreenY;
            Console.Title = cmdCursor.contentOffsetX + " " + cmdCursor.contentOffsetY;

            Printer.DrawHeader("maple", backgroundColor: ConsoleColor.Yellow);

            //load file
            if(args.Length > 0)
            {
                document = new Document(args[0]);
                document.PrintFileLines();
                docCursor.MoveCursor(docCursor.GetDocumentX(), docCursor.GetDocumentY());
            }

            while(true)
            {
                Input.AcceptInput(Console.ReadKey());

                Console.Clear();
                Printer.DrawHeader("maple (" + docCursor.GetDocumentX() + ", " + docCursor.GetDocumentY() + ")", backgroundColor: ConsoleColor.Yellow);
                //Printer.PrintLine(userText, ConsoleColor.Blue);
                document.PrintFileLines();

                if(Input.GetInputTarget() == Input.InputTarget.Document)
                    Printer.DrawFooter("(esc) : \"save\"", foregroundColor: ConsoleColor.Yellow, backgroundColor: ConsoleColor.Black);
                else if(Input.GetInputTarget() == Input.InputTarget.Command)
                    Printer.DrawFooter(": " + CommandLine.GetText(), backgroundColor: ConsoleColor.Yellow);

                //reset to user cursor position
                if(Input.GetInputTarget() == Input.InputTarget.Document)
                    docCursor.MoveCursor(docCursor.GetDocumentX(), docCursor.GetDocumentY());
                else if(Input.GetInputTarget() == Input.InputTarget.Command)
                    cmdCursor.ForceDocumentPosition(cmdCursor.GetDocumentX(), cmdCursor.GetDocumentY());
            }
        }

        static void PrepareWindow()
        {
            Console.Clear();
            Console.Title = "maple";
        }

        public static Cursor GetCursor()
        {
            if(Input.GetInputTarget() == Input.InputTarget.Document)
                return docCursor;
            else if(Input.GetInputTarget() == Input.InputTarget.Command)
                return cmdCursor;
            
            return docCursor;
        }

        public static Document GetDocument()
        {
            return document;
        }

    }
}
