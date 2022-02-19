using System;
using System.Collections.Generic;

namespace maple
{

    class Line
    {

        public Token[] Tokens { get; private set; } = new Token[0];

        private String _lineContent = "";
        public String LineContent {
            get
            {
                return _lineContent;
            }
            set
            {
                Tokens = GenerateTokensFromString(value);

                _lineContent = value;
            }
        }

        public Line(String text)
        {
            LineContent = text;
        }

        public void ForceTokenize()
        {
            Tokens = GenerateTokensFromString(_lineContent);
        }

        public static Token[] GenerateTokensFromString(String text)
        {
            return Lexer.Tokenize(text);
        }

    }

}