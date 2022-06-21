using System;

namespace maple
{

    public enum TokenType
    {
        // MISC
        None,

        // DOCUMENT SYNTAX
        Misc,
        Break,
        Whitespace,
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

        // CLI INPUT
        CliCommandValid,
        CliCommandInvalid,
        CliSwitch,
        CliString,

        // FOOTER
        FooterSeparator,
        FooterFilepath,
        FooterLnCol,
        FooterSelection,
        FooterIndicator,

        // SPECIAL
        TrailingWhitespace,
    }

    public class Token
    {
        private TokenType _ttype;
        public TokenType TType
        {
            get { return _ttype; }
            set
            {
                _ttype = value;
                Color = Styler.GetColor(value);
                ColorAttribute = Printer.GetAttributeFromColor(Color);
                // TODO: this is a mess
                if (_ttype == TokenType.TrailingWhitespace)
                {
                    ColorAttribute = (short)(ColorAttribute << 4);
                }
            }
        }
        public String Text { get; set; }
        public ConsoleColor Color { get; private set; } = ConsoleColor.Gray;
        public short ColorAttribute { get; private set; } = 0x0007;
        public string Annotation { get; set; } = "";

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
                    return TokenType.NumberLiteral;
                case "alphabetical":
                    return TokenType.Alphabetical;
                case "break":
                    return TokenType.Break;
                case "whitespace":
                    return TokenType.Whitespace;
                case "trailingwhitespace":
                    return TokenType.TrailingWhitespace;
                case "grouping":
                    return TokenType.Grouping;
                case "stringliteral":
                    return TokenType.StringLiteral;
                case "characterliteral":
                    return TokenType.CharLiteral;
                case "booleanliteral":
                    return TokenType.BooleanLiteral;
                case "hexliteral":
                    return TokenType.HexLiteral;
                case "comment":
                    return TokenType.Comment;
                case "operator":
                    return TokenType.Operator;
                case "url":
                    return TokenType.Url;
                case "function":
                    return TokenType.Function;
                case "keyword":
                    return TokenType.Keyword;
                case "specialchar":
                    return TokenType.SpecialChar;
                //command line
                case "clicommandvalid":
                    return TokenType.CliCommandValid;
                case "clicommandinvalid":
                    return TokenType.CliCommandInvalid;
                case "cliswitch":
                    return TokenType.CliSwitch;
                case "clistring":
                    return TokenType.CliString;
                //footer
                case "{-}":
                case "footerseparator":
                    return TokenType.FooterSeparator;
                case "{filepath}":
                case "{filename}":
                case "footerfilepath":
                    return TokenType.FooterFilepath;
                case "{lncol}":
                case "footerlncol":
                    return TokenType.FooterLnCol;
                case "{selection}":
                case "footerselection":
                    return TokenType.FooterSelection;
                case "{readonly}":
                case "footerindicator":
                    return TokenType.FooterIndicator;
                default:
                    return TokenType.None;
            }
        }
    }
}