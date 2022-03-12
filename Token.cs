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
            Function,
            NumberLiteral,
            StringLiteral,
            CharLiteral,
            BooleanLiteral,
            Comment,
            Grouping,
            Operator,
            Url,
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
        public string ColorCode = "\u001b[95m"; //magenta if error

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
            ColorCode = "\u001b[" + ConsoleColorToCode(Color) + "m";
        }

        //TEMPORARY, INCOMPLETE
        static string ConsoleColorToCode(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return "30";
                case ConsoleColor.Red:
                    return "31";
                case ConsoleColor.Green:
                    return "32";
                case ConsoleColor.Yellow:
                    return "33";
                case ConsoleColor.Blue:
                    return "34";
                case ConsoleColor.Magenta:
                    return "35";
                case ConsoleColor.Cyan:
                    return "36";
                case ConsoleColor.White:
                    return "97";
                case ConsoleColor.Gray:
                    return "37";
                default:
                    return "37";
            }
        }
    }
}