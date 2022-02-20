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
            BooleanLiteral,
            Comment,
            Grouping,
            Operator,
            Error, //only used by cli lexer
        }

        private TokenType _ttype;
        public TokenType TType
        {
            get { return _ttype; }
            set { _ttype = value; ApplyColor(); }
        }
        public String Text { get; private set; }
        public ConsoleColor Color { get; private set; } = ConsoleColor.Gray;

        public Token(String text, TokenType tokenType)
        {
            this.Text = text;
            this.TType = tokenType;
        }

        public void Append(String s)
        {
            Text += s;
        }

        public void ApplyColor()
        {
            switch(TType)
            {
                case TokenType.NumberLiteral:
                    Color = Styler.NumberLiteralColor;
                    break;
                case TokenType.StringLiteral:
                    Color = Styler.StringLiteralColor;
                    break;
                case TokenType.CharLiteral:
                    Color = Styler.CharLiteralColor;
                    break;
                case TokenType.BooleanLiteral:
                    Color = Styler.BooleanLiteralColor;
                    break;
                case TokenType.Keyword:
                    Color = Styler.KeywordColor;
                    break;
                case TokenType.Comment:
                    Color = Styler.CommentColor;
                    break;
                case TokenType.Grouping:
                    Color = Styler.GroupingColor;
                    break;
                case TokenType.Operator:
                    Color = Styler.OperatorColor;
                    break;
                case TokenType.Error:
                    Color = Styler.ErrorColor;
                    break;
                default:
                    Color = Styler.TextColor;
                    break;
            }
        }
    }
}