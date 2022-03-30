using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace maple
{
    static class Styler
    {

        //basic colors
        public static ConsoleColor TextColor { get; private set; } = ConsoleColor.Gray; //default color
        public static ConsoleColor AccentColor { get; private set; } = ConsoleColor.Yellow;
        public static ConsoleColor HighlightColor { get; private set; } = ConsoleColor.Yellow;
        public static ConsoleColor ErrorColor { get; private set; } = ConsoleColor.Red;
        public static ConsoleColor GutterColor { get; private set; } = ConsoleColor.DarkGray;
        public static ConsoleColor SelectionColor { get; private set; } = ConsoleColor.Blue;

        //cli colors
        public static ConsoleColor CliInputDefaultColor { get; private set; } = ConsoleColor.Yellow;
        public static ConsoleColor CliOutputInfoColor { get; private set; } = ConsoleColor.Cyan;
        public static ConsoleColor CliOutputErrorColor { get; private set; } = ConsoleColor.Red;
        public static ConsoleColor CliOutputSuccessColor { get; private set; } = ConsoleColor.Green;
        public static ConsoleColor CliPromptColor { get; private set; } = ConsoleColor.Yellow;
        public static ConsoleColor CliCommandValidColor { get; private set; } = ConsoleColor.Yellow;
        public static ConsoleColor CliCommandInvalidColor { get; private set; } = ConsoleColor.Red;
        public static ConsoleColor CliSwitchColor { get; private set; } = ConsoleColor.DarkGray;
        public static ConsoleColor CliStringColor { get; private set; } = ConsoleColor.Green;

        static Dictionary<Token.TokenType, ConsoleColor> tokenColors = new();

        //text customizations
        public static string VanityFooter { get; private set; } = "maple";
        public static char GutterLeftPad { get; private set; } = '0';
        public static char GutterBarrier { get; private set; } = ' ';
        public static char OverflowIndicator { get; private set; } = '…';

        public static void LoadMapleTheme()
        {
            string mapleThemePath = Settings.ThemeDirectory + Settings.ThemeFile;
            if (File.Exists(mapleThemePath))
            {
                AssignThemeColors(mapleThemePath);
                AssignCustomText(mapleThemePath);
            }
            else
                Log.Write("Theme file doesn't exist at '" + mapleThemePath + "'", "styler", important: true);
        }

        public static void AssignThemeColors(string themePath)
        {
            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;

            try
            {
                document.Load(themePath);
            }
            catch (Exception e)
            {
                CommandLine.SetOutput("Encountered an exception while loading theme XML", "internal", oType: CommandLine.OutputType.Error);
                Log.Write("Encountered exception while loading theme XML: " + e.Message, "styler", important: true);
                return;
            }

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

                ConsoleColor color = StringToColor(value);
                Token.TokenType categoryTokenType = Token.StringToTokenType(category);

                if (categoryTokenType != Token.TokenType.None) //its a token type
                {
                    tokenColors.Add(categoryTokenType, color);
                }
                else //its a different type of category
                {
                    switch(category)
                    {
                        case "text":
                            TextColor = color; break;
                        case "accent":
                            AccentColor = color; break;
                        case "error":
                            ErrorColor = color; break;
                        case "gutter":
                            GutterColor = color; break;
                        case "selection":
                            SelectionColor = color; break;
                        case "cliinputdefault":
                            CliInputDefaultColor = color; break;
                        case "clioutputinfo":
                            CliOutputInfoColor = color; break;
                        case "clioutputerror":
                            CliOutputErrorColor = color; break;
                        case "clioutputsuccess":
                            CliOutputSuccessColor = color; break;
                        case "cliprompt":
                            CliPromptColor = color; break;
                        case "clicommandvalid":
                            CliCommandValidColor = color; break;
                        case "clicommandinvalid":
                            CliCommandInvalidColor = color; break;
                        case "cliswitch":
                            CliSwitchColor = color; break;
                        case "clistring":
                            CliStringColor = color; break;
                        default:
                            Log.Write("Encountered unknown theme category '" + category + "'", "styler", important: true);
                            break;
                    }
                }
            }
        }

        public static void AssignCustomText(string themePath)
        {
            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;

            try
            {
                document.Load(themePath);
            }
            catch (Exception e)
            {
                CommandLine.SetOutput("Encountered an exception while loading custom text XML", "internal", oType: CommandLine.OutputType.Error);
                Log.Write("Encountered exception while loading custom text XML: " + e.Message, "styler", important: true);
                return;
            }

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
                    case "gutterleftpad":
                        if (value.Length > 1 || value.Length == 0)
                            Log.Write("GutterLeftPad value must be 1 character", "styler", important: true);
                        else
                            GutterLeftPad = value.ToCharArray()[0];
                        break;
                    case "gutterbarrier":
                        if (value.Length > 1 || value.Length == 0)
                            Log.Write("GutterBarrier value must be 1 character", "styler", important: true);
                        else
                            GutterBarrier = value.ToCharArray()[0];
                        break;
                    case "overflowindicator":
                        if (value.Length > 1 || value.Length == 0)
                            Log.Write("OverflowIndicator value must be 1 character", "styler", important: true);
                        else
                            OverflowIndicator = value.ToCharArray()[0];
                        break;
                    default:
                        Log.Write("Encountered unknown text category '" + category + "'", "styler", important: true);
                        break;
                }
            }
        }

        public static ConsoleColor GetColor(Token.TokenType tokenType)
        {
            if (tokenColors.ContainsKey(tokenType))
                return tokenColors[tokenType];
            return TextColor;
        }

        public static ConsoleColor StringToColor(string name)
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
                    Log.Write("Encountered unknown ConsoleColor '" + name + "'", "styler", important: true);
                    return ConsoleColor.Gray;
            }
        }

    }
}