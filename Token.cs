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
            Alphabetical,
            Keyword,
            NumberLiteral,
            StringLiteral,
            CharLiteral,
            BooleanLiteral,
            Comment,
            Grouping,
            Operator,
            Url,
            Function,
            //for cli lexer
            CliCommandValid,
            CliCommandInvalid,
            CliSwitch,
            CliString,
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

        public void ApplyColor()
        {
            switch(TType)
            {
                case TokenType.NumberLiteral:
                    Color = Styler.NumberLiteralColor;
                    break;
                case TokenType.StringLiteral:
                    Color = Styler.StringLiteralColor; break;
                case TokenType.CharLiteral:
                    Color = Styler.CharLiteralColor; break;
                case TokenType.BooleanLiteral:
                    Color = Styler.BooleanLiteralColor; break;
                case TokenType.Keyword:
                    Color = Styler.KeywordColor; break;
                case TokenType.Comment:
                    Color = Styler.CommentColor; break;
                case TokenType.Grouping:
                    Color = Styler.GroupingColor; break;
                case TokenType.Operator:
                    Color = Styler.OperatorColor; break;
                case TokenType.Url:
                    Color = Styler.UrlColor; break;
                case TokenType.Function:
                    Color = Styler.FunctionColor; break;
                case TokenType.CliCommandValid:
                    Color = Styler.CliCommandValidColor; break;
                case TokenType.CliCommandInvalid:
                    Color = Styler.CliCommandInvalidColor; break;
                case TokenType.CliSwitch:
                    Color = Styler.CliSwitchColor; break;
                case TokenType.CliString:
                    Color = Styler.CliStringColor; break;
                default:
                    Color = Styler.TextColor; break;
            }
        }
    }
}