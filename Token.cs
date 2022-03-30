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
            HexLiteral,
            Comment,
            Grouping,
            Operator,
            Url,
            Function,
            SpecialChar,
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
            set
            {
                _ttype = value;
                Color = Styler.GetColor(value);
            }
        }
        public String Text { get; private set; }
        public ConsoleColor Color { get; private set; } = ConsoleColor.Gray;

        public Token(String text, TokenType tokenType)
        {
            this.Text = text;
            this.TType = tokenType;
        }

        public static TokenType StringToTokenType(string name)
        {
            switch (name)
            {
                case "numberliteral":
                    return Token.TokenType.NumberLiteral;
                case "alphabetical":
                    return Token.TokenType.Alphabetical;
                case "break":
                    return Token.TokenType.Break;
                case "grouping":
                    return Token.TokenType.Grouping;
                case "stringliteral":
                    return Token.TokenType.StringLiteral;
                case "characterliteral":
                    return Token.TokenType.CharLiteral;
                case "booleanliteral":
                    return Token.TokenType.BooleanLiteral;
                case "hexliteral":
                    return Token.TokenType.HexLiteral;
                case "comment":
                    return Token.TokenType.Comment;
                case "operator":
                    return Token.TokenType.Operator;
                case "url":
                    return Token.TokenType.Url;
                case "function":
                    return Token.TokenType.Function;
                case "keyword":
                    return Token.TokenType.Keyword;
                case "specialchar":
                    return Token.TokenType.SpecialChar;
                //command line
                case "clicommandvalid":
                    return Token.TokenType.CliCommandValid;
                case "clicommandinvalid":
                    return Token.TokenType.CliCommandInvalid;
                case "cliswitch":
                    return Token.TokenType.CliSwitch;
                case "clistring":
                    return Token.TokenType.CliString;
                default:
                    return Token.TokenType.None;
            }
        }
    }
}