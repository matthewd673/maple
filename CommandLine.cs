using System;

namespace maple
{
    class CommandLine
    {

        static String inputText = "";
        static String outputText = "";

        static bool hasOutput = false;

        public static bool AddText(int pos, String text)
        {
            if(pos < 0 || pos > inputText.Length)
                return false;
                
            inputText = inputText.Insert(pos, text);
            return true;
        }

        public static bool RemoveText(int pos)
        {
            if(pos < 0 || pos > inputText.Length - 1)
                return false;
            
            inputText = inputText.Remove(pos, 1);
            return true;
        }

        public static bool IsSafeCursorX(int x)
        {
            return (x >= 0 && x < inputText.Length);
        }

        public static String GetText() { return inputText; }

        public static bool HasOutput() { return hasOutput; }

        public static String GetOutput() { return outputText; }

        static void SetOutput(String text)
        {
            outputText = text;
            hasOutput = true;
        }

        public static void ClearOutput()
        {
            outputText = "";
            hasOutput = false;
        }

        public static void ClearInput()
        {
            inputText = "";
            Program.GetCursor().ForceDocumentPosition(0, 0);
        }

        public static void ExecuteInput()
        {

            if(hasOutput) //output is being displayed
            {
                ClearOutput();
            }

            if(inputText == "help")
                HelpCommand();

            if(inputText == "save")
                SaveCommand();

            if(inputText == "close")
                CloseCommand();

            if(inputText == "mtheme")
                MthemeCommand();

            inputText = "";
            Input.ToggleInputTarget();
        }

        static void HelpCommand()
        {
            SetOutput("save | close | mtheme");
        }

        static void SaveCommand()
        {
            Program.GetDocument().SaveDocument();
            SetOutput("Working file saved to " + Program.GetDocument().GetFilePath());
        }

        static void CloseCommand()
        {
            Program.Close();
        }

        static void MthemeCommand()
        {
            Styler.LoadMapleTheme();

            //refresh all lines
            Console.Clear();
            Program.GetDocument().PrintFileLines();

            SetOutput("Refreshed maple theme");
            Program.PrintFooter(); //since console was just cleared

        }

    }
}