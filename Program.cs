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

            Printer.DrawHeader("maple", backgroundColor: ConsoleColor.Yellow);

            //load file
            if(args.Length > 0)
            {
                document = new Document(args[0]);
                document.PrintFileLines();
                docCursor.MoveCursor(docCursor.GetDocX(), docCursor.GetDocY());
            }

            while(true)
            {
                Input.AcceptInput(Console.ReadKey());

                Console.Clear();
                Printer.DrawHeader("maple (" + docCursor.GetDocX() + ", " + docCursor.GetDocY() + ")", backgroundColor: ConsoleColor.Yellow);
                document.PrintFileLines();

                //render footer
                if(!CommandLine.HasOutput())
                {
                    if(Input.GetInputTarget() == Input.InputTarget.Document)
                        Printer.DrawFooter("[esc]", foregroundColor: ConsoleColor.Yellow, backgroundColor: ConsoleColor.Black);
                    else if(Input.GetInputTarget() == Input.InputTarget.Command)
                        Printer.DrawFooter(": " + CommandLine.GetText(), backgroundColor: ConsoleColor.Yellow);
                }
                else
                {
                    Printer.DrawFooter("[esc]: " + CommandLine.GetOutput(), foregroundColor: ConsoleColor.Yellow, backgroundColor: ConsoleColor.Black);
                }

                //reset to user cursor position
                if(Input.GetInputTarget() == Input.InputTarget.Document)
                    docCursor.MoveCursor(docCursor.GetDocX(), docCursor.GetDocY());
                else if(Input.GetInputTarget() == Input.InputTarget.Command)
                    cmdCursor.ForceDocumentPosition(cmdCursor.GetDocX(), cmdCursor.GetDocY());
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

        public static void Close()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("maple session ({0})", GetDocument().GetFilePath());
            Console.ForegroundColor = ConsoleColor.Gray;
            Environment.Exit(0);
        }

    }
}
