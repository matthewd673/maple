using System;
using System.Collections.Generic;
using System.IO;

namespace maple
{
    static class Styler
    {

        public static ConsoleColor textColor = ConsoleColor.Gray;
        public static ConsoleColor accentColor = ConsoleColor.Red;
        public static ConsoleColor highlightColor = ConsoleColor.Yellow;
        public static ConsoleColor errorColor = ConsoleColor.Red;

        static List<String> terms = new List<String>();

        public static void LoadMapleTheme()
        {
            String mapleThemePath = "themes/maple.txt";
            if(File.Exists(mapleThemePath))
            {
                Document themeDoc = new Document(mapleThemePath);
                List<String> lines = themeDoc.GetAllLines();
                AssignThemeColors(lines);
            }
        }

        public static void AssignThemeColors(List<String> lines)
        {
            foreach(String l in lines)
            {
                String[] keyVal = l.Split(":");
                switch(keyVal[0])
                {
                    case "text":
                        textColor = ColorFromText(keyVal[1]);
                        break;
                    case "accent":
                        accentColor = ColorFromText(keyVal[1]);
                        break;
                    case "highlight":
                        highlightColor = ColorFromText(keyVal[1]);
                        break;
                    case "error":
                        errorColor = ColorFromText(keyVal[1]);
                        break;
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
                case "darkwhite":
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

        public static void LoadTheme(List<String> termList)
        {
            terms = termList;
        }

        public static void LoadTheme(String themePath)
        {
            Document themeDoc = new Document(themePath);
            List<String> lines = themeDoc.GetAllLines();
            terms = lines;
        }

        public static bool IsTerm(String word)
        {
            if(terms.Contains(word))
                return true;
            return false;
        }

    }
}