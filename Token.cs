using System;

namespace maple
{
    class Token
    {

        String text;
        ConsoleColor color;
        bool isWord;

        public Token(String text, ConsoleColor color, bool isWord = true)
        {
            this.text = text;
            this.color = color;
            this.isWord = isWord;
        }

        public String GetText()
        {
            if(!isWord)
                return text;
            else
                return text;
        }
        
        public ConsoleColor GetColor()
        {
            return color;
        }

    }
}