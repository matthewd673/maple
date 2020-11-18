using System;
using System.Collections.Generic;

namespace maple
{
    static class ArugmentParser
    {

        public struct Argument
        {
            public string name;
            public string argument;

            public Argument(string name, string argument)
            {
                this.name = name;
                this.argument = argument;
            }
        }

        static void Parse(String[] args)
        {
            List<Argument> argList = new List<Argument>();

            Argument openArg = new Argument("", "");

            foreach(String s in args)
            {
                if(openArg.name == "")
                    openArg.name = s;
                else
                {
                    
                }
            }
        }

    }
}