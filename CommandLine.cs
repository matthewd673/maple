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

            inputText = "";
            Input.ToggleInputTarget();
        }

        static void HelpCommand()
        {
            SetOutput("save | close");
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

    }
}