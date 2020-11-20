using System;
using System.Collections.Generic;

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

        static void SetOutput(String text, String speaker)
        {
            outputText = "[" + speaker + "]: " + text;
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
            Program.GetCommandCursor().Move(0, 0);
        }

        public static void ExecuteInput()
        {
            if(hasOutput) //output is being displayed, reset for the future
                ClearOutput();

            String[] commands = inputText.Split(" ");

            bool setPrimaryCommand = false;
            bool inQuoteBlock = false;
            String primaryCommand = "";
            List<String> commandArgs = new List<String>();
            List<String> commandOps = new List<String>();

            foreach(String s in commands)
            {
                //first string is primary command
                if(!setPrimaryCommand)
                {
                    primaryCommand = s;
                    setPrimaryCommand = true;
                    continue;
                }

                //options start with at least one '-'
                if(s.StartsWith("-"))
                {
                    commandOps.Add(s);
                    continue;
                }

                //nothing has been triggered, its an argument
                if(!inQuoteBlock)
                    commandArgs.Add(s); //append to list if not in quote block
                else
                    commandArgs[commandOps.Count - 1] += " " + s; //append to last item if in quote block

                //determine if it starts / ends a quote block
                char[] commandChars = s.ToCharArray();
                foreach(char c in commandChars)
                {
                    if(c == '\"') //toggle for each quote found
                        inQuoteBlock = !inQuoteBlock;
                }

            }

            switch(primaryCommand)
            {
                case "help":
                    HelpCommand();
                    break;
                case "save":
                    SaveCommand(commandArgs, commandOps);
                    break;
                case "load":
                    LoadCommand(commandArgs, commandOps);
                    break;
                case "close":
                    CloseCommand();
                    break;
                default:
                    UnknownCommand();
                    break;
            }

            //empty input field and toggle back to editor
            inputText = "";
            Input.ToggleInputTarget();
        }

        static void HelpCommand()
        {
            SetOutput("save | close", "help");
        }

        static void SaveCommand(List<String> args, List<String> ops)
        {
            Program.GetDocCursor().GetDocument().SaveDocument();
            SetOutput("Working file saved to " + Program.GetDocCursor().GetDocument().GetFilePath(), "save");
        }

        static void LoadCommand(List<String> args, List<String> ops)
        {
            if(args.Count < 1)
            {
                SetOutput("No filepath provided", "load");
                return;
            }

            String filepath = args[0];
            //trim quotes if it was in a block
            if(filepath.StartsWith("\""))
                filepath = filepath.Trim('\"');

            
            Program.LoadExternalDocument(filepath);
        }

        static void CloseCommand()
        {
            Program.Close();
        }

        static void UnknownCommand()
        {
            SetOutput("Unknown command, run 'help'", "error");
        }

    }
}
