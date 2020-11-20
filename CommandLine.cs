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
            Editor.GetCommandCursor().Move(0, 0);
        }

        public static void ExecuteInput()
        {
            if(hasOutput) //output is being displayed, reset for the future
                ClearOutput();

            CommandParser.CommandInfo commandInfo = CommandParser.Parse(inputText);

            String primaryCommand = commandInfo.primaryCommand;
            List<String> commandArgs = commandInfo.args;
            List<String> commandOps = commandInfo.ops;

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
            SetOutput("close | save | load", "help");
        }

        static void SaveCommand(List<String> args, List<String> ops)
        {

            String savePath = Editor.GetDocCursor().GetDocument().GetFilePath();
            if(args.Count > 0)
                savePath = args[0];
            savePath = savePath.Trim('\"');

            Editor.GetDocCursor().GetDocument().SaveDocument(savePath);

            String existingPath = Editor.GetDocCursor().GetDocument().GetFilePath();

            if(savePath != existingPath)
                SetOutput("Copy of file saved to " + savePath, "save");
            else
                SetOutput("File saved to " + savePath, "save");
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

            //initialize new editor
            Editor.Initialize(filepath);
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
