using System;
using System.Collections.Generic;

namespace maple
{

    class Line
    {

        Token[] tokens = new Token[0];
        String lineContent = "";

        public Line(String text)
        {
            tokens = GenerateTokensFromString(text);
            UpdateContentFromTokens();
        }

        public static Token[] GenerateTokensFromString(String text)
        {
            return Lexer.Tokenize(text);
        }

        public Token[] GetTokens() { return tokens; }

        public String GetContent() { return lineContent; }

        public void UpdateContentFromTokens()
        {
            String content = "";
            foreach(Token t in tokens)
            {
                content += t.GetText();
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