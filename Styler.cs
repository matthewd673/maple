using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace maple
{
    static class Styler
    {
        //basic colors
        // public static ConsoleColor TextColor { get; private set; } = ConsoleColor.Gray; //default color
        // public static ConsoleColor AccentColor { get; private set; } = ConsoleColor.Yellow;
        // public static ConsoleColor HighlightColor { get; private set; } = ConsoleColor.Yellow;
        // public static ConsoleColor ErrorColor { get; private set; } = ConsoleColor.Red;
        // public static ConsoleColor GutterColor { get; private set; } = ConsoleColor.DarkGray;
        // public static ConsoleColor SelectionColor { get; private set; } = ConsoleColor.Blue;

        // //cli colors
        // public static ConsoleColor CliInputDefaultColor { get; private set; } = ConsoleColor.Yellow;
        // public static ConsoleColor CliOutputInfoColor { get; private set; } = ConsoleColor.Cyan;
        // public static ConsoleColor CliOutputErrorColor { get; private set; } = ConsoleColor.Red;
        // public static ConsoleColor CliOutputSuccessColor { get; private set; } = ConsoleColor.Green;
        // public static ConsoleColor CliPromptColor { get; private set; } = ConsoleColor.Yellow;

        static Dictionary<TokenType, ConsoleColor> tokenColors = new();

        public static void LoadMapleTheme()
        {
            string mapleThemePath = Settings.Properties.ThemeDirectory + Settings.Properties.ThemeFile;
            if (File.Exists(mapleThemePath))
            {
                AssignThemeColors(mapleThemePath);
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
                CommandLine.SetOutput("Encountered an exception while loading theme XML", "maple", oType: CommandLine.OutputType.Error);
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

                ConsoleColor color = Settings.StringToConsoleColor(value);
                TokenType categoryTokenType = Token.StringToTokenType(category);

                if (categoryTokenType != TokenType.None) //its a token type
                {
                    tokenColors.Add(categoryTokenType, color);
                }
                // else //its a different type of category
                // {
                //     switch(category)
                //     {
                //         case "text":
                //             TextColor = color; break;
                //         case "accent":
                //             AccentColor = color; break;
                //         case "error":
                //             ErrorColor = color; break;
                //         case "gutter":
                //             GutterColor = color; break;
                //         case "selection":
                //             SelectionColor = color; break;
                //         case "cliinputdefault":
                //             CliInputDefaultColor = color; break;
                //         case "clioutputinfo":
                //             CliOutputInfoColor = color; break;
                //         case "clioutputerror":
                //             CliOutputErrorColor = color; break;
                //         case "clioutputsuccess":
                //             CliOutputSuccessColor = color; break;
                //         case "cliprompt":
                //             CliPromptColor = color; break;
                //         default:
                //             Log.Write("Encountered unknown theme category '" + category + "'", "styler", important: true);
                //             break;
                //     }
                // }
            }
        }

        public static ConsoleColor GetColor(TokenType tokenType)
        {
            if (!Settings.Properties.NoHighlight && tokenColors.ContainsKey(tokenType))
                return tokenColors[tokenType];
            return Settings.Theme.TextColor;
        }

        

    }
}