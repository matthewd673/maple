using System;

namespace maple
{
    class Token
    {

        String text;
        ConsoleColor color;

        public Token(String text, ConsoleColor color)
        {
            this.text = text;
            this.color = color;
        }

        public String GetText()
        {
            return text;
        }
        
        public ConsoleColor GetColor()
        {
            return color;
        }

    }
}