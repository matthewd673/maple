using System;
using System.Collections.Generic;

namespace maple
{

    class Line
    {

        public List<Token> Tokens { get; private set; } = new List<Token>();

        public int IndentLevel { get; set; } = 0;
        public int IndentIndex { get { return IndentLevel * Settings.Properties.TabSpacesCount; } }
        public bool BlockCommented { get; set; } = false;

        private String _lineContent = "";
        public String LineContent {
            get
            {
                return _lineContent;
            }
            set
            {
                _lineContent = value;
                IndentLevel = FindIndentLevel();
                ForceTokenize();
            }
        }

        public Line(String text)
        {
            LineContent = text;
        }

        public void ForceTokenize()
        {
            Tokens = Lexer.Tokenize(_lineContent);
        }

        private int FindIndentLevel()
        {
            int indentLevel = 0;
            string tabLineContent = LineContent;
            while (tabLineContent.StartsWith(Settings.TabString))
            {
                indentLevel++;
                tabLineContent = tabLineContent.Remove(0, Settings.TabString.Length);
            }

            return indentLevel;
        }
    }

}