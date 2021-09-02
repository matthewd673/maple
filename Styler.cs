using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace maple
{
    static class Styler
    {

        //basic colors
        public static ConsoleColor TextColor { get; private set; } = ConsoleColor.Gray;
        public static ConsoleColor AccentColor { get; private set; } = ConsoleColor.Yellow;
        public static ConsoleColor HighlightColor { get; private set; } = ConsoleColor.Yellow;
        public static ConsoleColor ErrorColor { get; private set; } = ConsoleColor.Red;
        public static ConsoleColor CmdInColor { get; private set; } = ConsoleColor.Yellow;
        public static ConsoleColor CmdOutColor { get; private set; } = ConsoleColor.Cyan;
        public static ConsoleColor GutterColor { get; private set; } = ConsoleColor.DarkGray;
        public static ConsoleColor SelectionColor { get; private set; } = ConsoleColor.Blue;

        //syntax colors
        public static ConsoleColor NumberLiteralColor { get; private set; } = ConsoleColor.Magenta;
        public static ConsoleColor StringLiteralColor { get; private set; } = ConsoleColor.Green;
        public static ConsoleColor CharLiteralColor { get; private set; } = ConsoleColor.DarkGreen;
        public static ConsoleColor BooleanLiteralColor { get; private set; } = ConsoleColor.Blue;
        public static ConsoleColor VariableColor { get; private set; } = ConsoleColor.Gray;
        public static ConsoleColor KeywordColor { get; private set; } = ConsoleColor.Yellow;
        public static ConsoleColor CommentColor { get; private set; } = ConsoleColor.DarkGray;
        public static ConsoleColor GroupingColor { get; private set; } = ConsoleColor.White;
        public static ConsoleColor OperatorColor { get; private set; } = ConsoleColor.Red;

        //text customizations
        public static string VanityFooter { get; private set; } = "maple";

        public static void LoadMapleTheme()
        {
            string mapleThemePath = Settings.ThemeDirectory + Settings.ThemeFile;
            if (File.Exists(mapleThemePath))
            {
                AssignThemeColors(mapleThemePath);
                AssignCustomText(mapleThemePath);
            }
        }

        public static void AssignThemeColors(string themePath)
        {
            XmlDocument document = new XmlDocument();
            document.Load(themePath);

            XmlNodeList colors = document.GetElementsByTagName("color");
            foreach(XmlNode node in colors)
            {
                string category = "";
                string value = "";

                foreach(XmlAttribute a in node.Attributes)
                {
                    if(a.Name.ToLower() != "category")
                        return;
                    
                    category = a.Value.ToLower();
                }
                value = node.InnerText.ToLower();

                ConsoleColor color = ColorFromText(value);
                switch(category)
                {
                    case "text":
                        TextColor = color; break;
                    case "accent":
                        AccentColor = color; break;
                    case "error":
                        ErrorColor = color; break;
                    case "commandinput":
                        CmdInColor = color; break;
                    case "commandoutput":
                        CmdOutColor = color; break;
                    case "gutter":
                        GutterColor = color; break;
                    case "selection":
                        SelectionColor = color; break;
                    case "numberliteral":
                        NumberLiteralColor = color; break;
                    case "stringliteral":
                        StringLiteralColor = color; break;
                    case "characterliteral":
                        CharLiteralColor = color; break;
                    case "booleanliteral":
                        BooleanLiteralColor = color; break;
                    case "variable":
                        VariableColor = color; break;
                    case "keyword":
                        KeywordColor = color; break;
                    case "comment":
                        CommentColor = color; break;
                    case "grouping":
                        GroupingColor = color; break;
                    case "operator":
                        OperatorColor = color; break;
                }
            }
        }

        public static void AssignCustomText(string themePath)
        {
            XmlDocument document = new XmlDocument();
            document.Load(themePath);

            XmlNodeList texts = document.GetElementsByTagName("string");
            foreach (XmlNode node in texts)
            {
                String category = "";
                String value = "";

                foreach (XmlAttribute a in node.Attributes)
                {
                    if (a.Name.ToLower() != "category")
                        return;

                    category = a.Value.ToLower();
                }
                value = node.InnerText.ToLower();

                switch (category)
                {
                    case "vanityfooter":
                        VanityFooter = value; break;
                }
            }
        }

        public static ConsoleColor ColorFromText(string name)
        {
            switch(name)
            {
                case "black":
                    return ConsoleColor.Black;
                case "darkblue":
                    return ConsoleColor.DarkBlue;
                case "darkgreen":
                    return ConsoleColor.DarkGreen;
                case "darkcyan":
                    return ConsoleColor.DarkCyan;
                case "darkred":
                    return ConsoleColor.DarkRed;
                case "darkmagenta":
                    return ConsoleColor.DarkMagenta;
                case "darkyellow":
                    return ConsoleColor.DarkYellow;
                case "darkgray":
                    return ConsoleColor.DarkGray;
                case "gray":
                    return ConsoleColor.Gray;
                case "blue":
                    return ConsoleColor.Blue;
                case "green":
                    return ConsoleColor.Green;
                case "cyan":
                    return ConsoleColor.Cyan;
                case "red":
                    return ConsoleColor.Red;
                case "magenta":
                    return ConsoleColor.Magenta;
                case "yellow":
                    return ConsoleColor.Yellow;
                case "white":
                    return ConsoleColor.White;
                default:
                    return ConsoleColor.Gray;
            }
        }

    }
}