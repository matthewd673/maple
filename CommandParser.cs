using System;
using System.Collections.Generic;

namespace maple
{
    static class CommandParser
    {

        public struct CommandInfo
        {
            public String PrimaryCommand { get; set; }
            public List<String> Args { get; set; }
            public List<String> Switches { get; set; }

            public CommandInfo(String primaryCommand, List<String> args, List<String> switches)
            {
                PrimaryCommand = primaryCommand;
                Args = args;
                Switches = switches;
            }
        }

        public static CommandInfo Parse(String input, String defaultPrimaryCommand = "", bool combineArgs = false)
        {
            String[] commands = input.Split(" ");

            bool setPrimaryCommand = false;
            if(defaultPrimaryCommand != "")
                setPrimaryCommand = true;

            bool inQuoteBlock = false;
            String primaryCommand = defaultPrimaryCommand;
            List<String> commandArgs = new List<String>();
            List<String> commandSwitches = new List<String>();

            foreach(String s in commands)
            {
                //first string is primary command
                if(!setPrimaryCommand)
                {
                    primaryCommand = s;
                    setPrimaryCommand = true;
                    continue;
                }

                //switches start with at least one '-'
                if(s.StartsWith("-"))
                {
                    commandSwitches.Add(s);
                    continue;
                }

                //nothing has been triggered, its an argument
                if(!inQuoteBlock && !combineArgs)
                    commandArgs.Add(s); //append to list if not in quote block
                else
                {
                    if(commandArgs.Count > 0)
                        commandArgs[commandArgs.Count - 1] += " " + s; //append to last item if in quote block
                    else
                        commandArgs.Add(s);
                }

                //determine if it starts / ends a quote block
                char[] commandChars = s.ToCharArray();
                foreach(char c in commandChars)
                {
                    if(c == '\"') //toggle for each quote found
                        inQuoteBlock = !inQuoteBlock;
                }

            }

            return new CommandInfo(primaryCommand, commandArgs, commandSwitches);

        }
    }
}