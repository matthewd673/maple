using System;

namespace maple
{
    class CommandLine
    {

        static String commandText = "";

        public static bool AddText(int pos, String text)
        {
            if(pos < 0 || pos > commandText.Length)
                return false;
                
            commandText = commandText.Insert(pos, text);
            return true;
        }

        public static String GetText()
        {
            return commandText;
        }

        public static void ExecuteCommand()
        {
            if(commandText == "save")
                Program.GetDocument().SaveDocument();

            commandText = "";
            Input.ToggleInputTarget();
        }

    }
}