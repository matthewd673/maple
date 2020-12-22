using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace maple
{
    static class Styler
    {

        //basic colors
        public static ConsoleColor textColor = ConsoleColor.Gray;
        public static ConsoleColor accentColor = ConsoleColor.Yellow;
        public static ConsoleColor highlightColor = ConsoleColor.Yellow;
        public static ConsoleColor errorColor = ConsoleColor.Red;
        public static ConsoleColor cmdinColor = ConsoleColor.Yellow;
        public static ConsoleColor cmdoutColor = ConsoleColor.Cyan;
        public static ConsoleColor gutterColor = ConsoleColor.DarkGray;

        //syntax colors
        public static ConsoleColor numberLiteralColor = ConsoleColor.Magenta;
        public static ConsoleColor stringLiteralColor = ConsoleColor.Green;
        public static ConsoleColor charLiteralColor = ConsoleColor.Yellow;
        public static ConsoleColor variableColor = ConsoleColor.Gray;
        public static ConsoleColor keywordColor = ConsoleColor.Blue;
        public static ConsoleColor commentColor = ConsoleColor.DarkGray;
        public static ConsoleColor groupingColor = ConsoleColor.White;
        public static ConsoleColor operatorColor = ConsoleColor.Red;

        public static void LoadMapleTheme()
        {
            String mapleThemePath = Settings.themeDirectory + Settings.themeFile;
            if(File.Exists(mapleThemePath))
                AssignThemeColors(mapleThemePath);
            else
                Console.Title = mapleThemePath;
        }

        public static void AssignThemeColors(String themePath)
        {
            XmlDocument document = new XmlDocument();
            document.Load(themePath);

            XmlNodeList colors = document.GetElementsByTagName("color");
            foreach(XmlNode node in colors)
            {
                String category = "";
                String value = "";

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
                        textColor = color; break;
                    case "accent":
                        accentColor = color; break;
                    case "error":
                        errorColor = color; break;
                    case "commandinput":
                        cmdinColor = color; break;
                    case "commandoutput":
                        cmdoutColor = color; break;
                    case "gutter":
                        gutterColor = color; break;
                    case "numberliteral":
                        numberLiteralColor = color; break;
                    case "stringliteral":
                        stringLiteralColor = color; break;
                    case "characterliteral":
                        charLiteralColor = color; break;
                    case "variable":
                        variableColor = color; break;
                    case "keyword":
                        keywordColor = color; break;
                    case "comment":
                        commentColor = color; break;
                    case "grouping":
                        groupingColor = color; break;
                    case "operator":
                        operatorColor = color; break;
                }
            }
        }

        public static ConsoleColor ColorFromText(String name)
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