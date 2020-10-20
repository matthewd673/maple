using System;

namespace maple
{
    class Program
    {

        static Cursor docCursor;
        static Cursor cmdCursor;
        static Document document;

        public static String userText = "";

        static bool redrawNext;

        static void Main(string[] args)
        {
            PrepareWindow();
            
            //create cursor
            docCursor = new Cursor(0, 0);
            cmdCursor = new Cursor(0, 0);

            cmdCursor.contentOffsetX = 7;
            cmdCursor.contentOffsetY = Cursor.maxScreenY;

            //Printer.DrawHeader("maple", backgroundColor: ConsoleColor.Yellow);

            //load file
            if(args.Length > 0)
            {
                document = new Document(args[0]);
                document.PrintFileLines();
                docCursor.MoveCursor(docCursor.GetDocX(), docCursor.GetDocY());
            }

            //render initial footer
            Printer.DrawFooter("maple", foregroundColor: ConsoleColor.Yellow, backgroundColor: ConsoleColor.Black);
            //reset to initial position
            docCursor.MoveCursor(docCursor.GetDocX(), docCursor.GetDocY());

            while(true)
            {
                //render initial footer
                RenderFooter();

                //set actual cursor position
                docCursor.MoveCursor();

                //accept input
                Input.AcceptInput(Console.ReadKey());
                
                if(redrawNext)
                {
                    Console.Clear();
                    document.PrintFileLines();
                }

                //render footer
                RenderFooter();

                //reset to user cursor position
                if(Input.GetInputTarget() == Input.InputTarget.Document)
                    docCursor.MoveCursor(docCursor.GetDocX(), docCursor.GetDocY());
                else if(Input.GetInputTarget() == Input.InputTarget.Command)
                    cmdCursor.ForceDocumentPosition(cmdCursor.GetDocX(), cmdCursor.GetDocY());

                
            //reset redraw flag
            redrawNext = false;
            }

        }

        static void PrepareWindow()
        {
            Console.Clear();
            Console.Title = "maple";
            Printer.Resize();
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

        public static void TriggerContentRedraw()
        {
            redrawNext = true;
        }

        public static void RenderFooter()
        {
            //generate footer string
            String defaultFooterContent = "maple (" + docCursor.GetDocX() + ", " + docCursor.GetDocY() + ") - " + document.GetFilePath();

            if(!CommandLine.HasOutput())
            {
                if(Input.GetInputTarget() == Input.InputTarget.Document) //render default footer
                    Printer.DrawFooter(defaultFooterContent, foregroundColor: ConsoleColor.Yellow, backgroundColor: ConsoleColor.Black);
                else if(Input.GetInputTarget() == Input.InputTarget.Command) //render input footer
                    Printer.DrawFooter("maple: " + CommandLine.GetText(), backgroundColor: ConsoleColor.Yellow);
            }
            else //render output footer
                Printer.DrawFooter("maple \"" + CommandLine.GetOutput() + "\"", foregroundColor: ConsoleColor.Yellow, backgroundColor: ConsoleColor.Black);
        }

    }
}
