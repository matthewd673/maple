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
            CharLiteral,
            Comment,
            Grouping,
            Operator,
        }

        TokenType tokenType;
        String text;
        ConsoleColor color = ConsoleColor.Gray;

        public Token(String text, TokenType tokenType)
        {
            this.text = text;
            this.tokenType = tokenType;
            ApplyColor();
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
            ApplyColor();
        }

        public void ApplyColor()
        {
            switch(tokenType)
            {
                case TokenType.NumberLiteral:
                    color = Styler.numberLiteralColor;
                    break;
                case TokenType.StringLiteral:
                    color = Styler.stringLiteralColor;
                    break;
                case TokenType.CharLiteral:
                    color = Styler.charLiteralColor;
                    break;
                case TokenType.Keyword:
                    color = Styler.keywordColor;
                    break;
                case TokenType.Comment:
                    color = Styler.commentColor;
                    break;
                case TokenType.Grouping:
                    color = Styler.groupingColor;
                    break;
                case TokenType.Operator:
                    color = Styler.operatorColor;
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