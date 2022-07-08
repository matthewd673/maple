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
        MultilineCommentOpen,
        MultilineCommentClose,

        // CLI INPUT
        CliCommandValid,
        CliCommandInvalid,
        CliSwitch,
        CliString,

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
                if (Settings.Theme.TokenColorTable.ContainsKey(value))
                    Color = Settings.Theme.TokenColorTable[value];
                else
                {
                    // Log.Write("Theme missing TokenType: " + value, "token", important: true);
                    Color = ConsoleColor.Gray;
                }
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
    }
}