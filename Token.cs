using System;

namespace maple
{
    public class Token
    {

        public enum TokenType
        {
            None,
            Misc,
            Break,
            Variable,
            Keyword,
            Function,
            NumberLiteral,
            StringLiteral,
            Comment,
            Grouping,
        }

        TokenType tokenType;
        String text;
        ConsoleColor color = ConsoleColor.Gray;

        public Token(String text, TokenType tokenType)
        {
            this.text = text;
            this.tokenType = tokenType;
            SetColor();
        }

        public String GetText()
        {
            return text;
        }

        public void Append(String s)
        {
            text += s;
        }

        public void SetType(TokenType tokenType)
        {
            this.tokenType = tokenType;
        }

        public void SetColor()
        {
            switch(tokenType)
            {
                case TokenType.NumberLiteral:
                    color = Styler.numberLiteralColor;
                    break;
                case TokenType.StringLiteral:
                    color = Styler.stringLiteralColor;
                    break;
                case TokenType.Keyword:
                    color = Styler.keywordColor;
                    break;
                case TokenType.Comment:
                    color = Styler.commentColor;
                    break;
                default:
                    color = Styler.textColor;
                    break;
            }
        }

        public ConsoleColor GetColor()
        {
            return color;
        }

        public TokenType GetTokenType()
        {
            return tokenType;
        }

    }
}