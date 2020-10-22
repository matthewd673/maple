using System;
using System.Collections.Generic;

namespace maple
{

    class Line
    {

        List<Token> tokens = new List<Token>();
        String lineContent = "";

        public Line(List<Token> tokens)
        {
            this.tokens = tokens;
            UpdateContentFromTokens();
        }

        public static List<Token> GenerateTokensFromString(String[] strings)
        {
            List<Token> tokens = new List<Token>();
            foreach(String s in strings)
                tokens.Add(new Token(s, ConsoleColor.Gray));
            return tokens;
        }

        public static List<Token> GenerateTokensFromString(String text)
        {
            List<Token> tokens = new List<Token>();
            String[] words = text.Split(" ");
            return GenerateTokensFromString(words);
        }

        public List<Token> GetTokens()
        {
            return tokens;
        }

        public void SetTokens(List<Token> tokens)
        {
            this.tokens = tokens;
            UpdateContentFromTokens();
        }

        public String GetContent()
        {
            return lineContent;
        }

        public void UpdateContentFromTokens()
        {
            String content = "";
            for(int i = 0; i < tokens.Count; i++)
            {
                if(i < tokens.Count - 1)
                    content += tokens[i].GetText() + " ";
                else
                    content += tokens[i].GetText();
            }

            lineContent = content;
        }

        public void SetContent(String newContent)
        {
            tokens = GenerateTokensFromString(newContent);
            UpdateContentFromTokens();
        }

    }

}