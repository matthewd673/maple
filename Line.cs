using System;
using System.Collections.Generic;

namespace maple
{

    class Line
    {

        public Token[] Tokens { get; private set; }= new Token[0];

        private String _lineContent = "";
        public String LineContent {
            get
            {
                return _lineContent;
            }
            set
            {
                Tokens = GenerateTokensFromString(value);

                String content = "";
                foreach(Token t in Tokens)
                    content += t.Text;

                _lineContent = content;
            }
        }

        public Line(String text)
        {
            LineContent = text;
        }

        public static Token[] GenerateTokensFromString(String text)
        {
            return Lexer.Tokenize(text);
        }

    }

}