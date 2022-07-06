using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;

namespace maple
{
    public class Alias
    {
        [XmlAttribute(AttributeName = "command")]
        public string Command { get; set; }
        [XmlText]
        public string AliasString { get; set; }
    }

    public class Shortcut
    {
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }
        [XmlText]
        public string Command { get; set; }
        [XmlAttribute(AttributeName = "execute")]
        public bool Execute { get; set; }
    }

    public class Properties
    {
        public bool DebugTokens { get; set; } = false;
        public bool NoHighlight { get; set; } = false;
        public bool NoTokenize { get; set; } = false;
        public bool CliNoHighlight { get; set; } = false;
        public bool RelativePath { get; set; } = false;
        public bool NavigatePastTabs { get; set; } = true;
        public bool DeleteEntireTabs { get; set; } = true;
        public bool ReadOnly
        {
            get { return Input.ReadOnly; }
            set { Input.ReadOnly = value; }
        }
        public bool EnableLogging { get; set; } = true;
        public bool SummarizeLog { get; set; } = false;
        public bool SaveOnClose { get; set; } = false;
        public bool PreserveIndentOnEnter { get; set; } = true;
        public bool ShiftSelect { get; set; } = true;
        public bool ShiftDeindent { get; set; } = true;
        public bool ArrowsDeselect { get; set; } = true;
        public bool ClearOutputOnToggle { get; set; } = true;
        public bool ColorOutputBackground { get; set; } = true;
        public bool HighlightTrailingWhitespace { get; set; } = false;
        public bool Autocomplete { get; set; } = true;
        public bool AutoResize { get; set; } = true;

        private string _themeDirectory = Path.Combine(Settings.MapleDirectory, "themes");
        public string ThemeDirectory
        {
            get { return _themeDirectory; }
            set
            {
                _themeDirectory = value.Replace("{mapledir}", Settings.MapleDirectory);
            }
        }
        public string ThemeFile { get; set; } = "maple.xml";
        private string _syntaxDirectory = Path.Combine(Settings.MapleDirectory, "syntax");
        public string SyntaxDirectory
        {
            get { return _syntaxDirectory; }
            set
            {
                _syntaxDirectory = value.Replace("{mapledir}", Settings.MapleDirectory);
            }
        }
        private string _footerLayoutFile = Path.Combine(Settings.MapleDirectory, "properties", "footer.xml");
        public string FooterLayoutFile
        {
            get { return _footerLayoutFile; }
            set
            {
                _footerLayoutFile = value.Replace("{mapledir}", Settings.MapleDirectory);
            }
        }
        public int TabSpacesCount { get; set; } = 4;
        public string ScrollYIncrement { get; set; } = "half";
        public string ScrollXIncrement { get; set; } = "half";
        public int HistoryMaxSize { get; set; } = 5000; // arbitrary

        public string FooterFormat { get; set; } = "maple {filepath}{-}{lncol}"; // super simple default
        public string FooterSeparator { get; set; } = " ";
        public string GutterLeftPad { get; set; } = "0";
        public char GutterLeftPadChar { get { return GutterLeftPad[0]; }}

        public string GutterBarrier { get; set; } = " ";
        public char GutterBarrierChar { get { return GutterBarrier[0]; }}
        public string OverflowIndicator { get; set; } = "…";
        public char OverflowIndicatorChar { get { return OverflowIndicator[0]; }}
        public string DefaultEncoding { get; set; } = "utf8";
        public List<char> AutocompleteOpeningChars { get; set; } = new List<char>();
        public List<char> AutocompleteEndingChars { get; set; } = new List<char>();

        [XmlIgnore]
        public Dictionary<string, string> AliasesTable { get; set; }= new();
        public List<Alias> Aliases { get; set; } = new();

        [XmlIgnore]
        public Dictionary<ConsoleKey, Shortcut> ShortcutsTable { get; set; } = new();
        public List<Shortcut> Shortcuts { get; set; } = new();
    }

    public class TokenColor
    {
        [XmlAttribute(AttributeName = "type")]
        public string TokenType { get; set; }
        [XmlText]
        public string Color { get; set; }
    }

    public class Theme
    {
        public ConsoleColor TextColor { get; set; } = ConsoleColor.Gray;
        public string Text
        {
            get { return TextColor.ToString(); }
            set { TextColor = Settings.StringToConsoleColor(value); }
        }
        public ConsoleColor AccentColor { get; set; } = ConsoleColor.Yellow;
        public string Accent
        {
            get { return AccentColor.ToString(); }
            set { AccentColor = Settings.StringToConsoleColor(value); }
        }
        public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
        public string Error
        {
            get { return ErrorColor.ToString(); }
            set { ErrorColor = Settings.StringToConsoleColor(value); }
        }
        public ConsoleColor GutterColor { get; set; } = ConsoleColor.DarkGray;
        public string Gutter
        {
            get { return GutterColor.ToString(); }
            set { GutterColor = Settings.StringToConsoleColor(value); }
        }
        public ConsoleColor SelectionColor { get; set; } = ConsoleColor.Blue;
        public string Selection
        {
            get { return SelectionColor.ToString(); }
            set { SelectionColor = Settings.StringToConsoleColor(value); }
        }

        // cli colors
        public ConsoleColor CliInputDefaultColor { get; set; } = ConsoleColor.Yellow;
        public string CliInputDefault
        {
            get { return CliInputDefaultColor.ToString(); }
            set { CliInputDefaultColor = Settings.StringToConsoleColor(value); }
        }
        public ConsoleColor CliOutputInfoColor { get; set; } = ConsoleColor.Cyan;
        public string CliOutputInfo
        {
            get { return CliOutputInfoColor.ToString(); }
            set { CliOutputInfoColor = Settings.StringToConsoleColor(value); }
        }
        public ConsoleColor CliOutputErrorColor { get; set; } = ConsoleColor.Red;
        public string CliOutputError
        {
            get { return CliOutputErrorColor.ToString(); }
            set { CliOutputErrorColor = Settings.StringToConsoleColor(value); }
        }
        public ConsoleColor CliOutputSuccessColor { get; set; } = ConsoleColor.Green;
        public string CliOutputSuccess
        {
            get { return CliOutputSuccessColor.ToString(); }
            set { CliOutputSuccessColor = Settings.StringToConsoleColor(value); }
        }
        public ConsoleColor CliPromptColor { get; set; } = ConsoleColor.Yellow;
        public string CliPrompt
        {
            get { return CliPromptColor.ToString(); }
            set { CliPromptColor = Settings.StringToConsoleColor(value); }
        }

        // token colors
        [XmlIgnore]
        public Dictionary<TokenType, ConsoleColor> TokenColorTable { get; set; } = new();

        [XmlArray(ElementName = "TokenColors")]
        [XmlArrayItem(ElementName = "Token")]
        public List<TokenColor> Tokens { get; set; } = new List<TokenColor>();
    }

    static class Settings
    {
        // directories, these aren't user-defined
        public static string MapleDirectory { get; private set; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string PropertiesFile { get; } = Path.Combine(MapleDirectory, "properties", "properties.xml");
        public static string ThemeFile { get { return Path.Combine(Properties.ThemeDirectory, Properties.ThemeFile); } }

        public static Dictionary<string, ConsoleKey> StringToConsoleKeyTable { get; } = new()
        {
            { "A", ConsoleKey.A },
            { "a", ConsoleKey.A },
            { "B", ConsoleKey.B },
            { "b", ConsoleKey.B },
            { "C", ConsoleKey.C },
            { "c", ConsoleKey.C },
            { "D", ConsoleKey.D },
            { "d", ConsoleKey.D },
            { "E", ConsoleKey.E },
            { "e", ConsoleKey.E },
            { "F", ConsoleKey.F },
            { "f", ConsoleKey.F },
            { "G", ConsoleKey.G },
            { "g", ConsoleKey.G },
            { "H", ConsoleKey.H },
            { "h", ConsoleKey.H },
            { "I", ConsoleKey.I },
            { "i", ConsoleKey.I },
            { "J", ConsoleKey.J },
            { "j", ConsoleKey.J },
            { "K", ConsoleKey.K },
            { "k", ConsoleKey.K },
            { "L", ConsoleKey.L },
            { "l", ConsoleKey.L },
            { "M", ConsoleKey.M },
            { "m", ConsoleKey.M },
            { "N", ConsoleKey.N },
            { "n", ConsoleKey.N },
            { "O", ConsoleKey.O },
            { "o", ConsoleKey.O },
            { "P", ConsoleKey.P },
            { "p", ConsoleKey.P },
            { "Q", ConsoleKey.Q },
            { "q", ConsoleKey.Q },
            { "R", ConsoleKey.R },
            { "r", ConsoleKey.R },
            { "S", ConsoleKey.S },
            { "s", ConsoleKey.S },
            { "T", ConsoleKey.T },
            { "t", ConsoleKey.T },
            { "U", ConsoleKey.U },
            { "u", ConsoleKey.U },
            { "V", ConsoleKey.V },
            { "v", ConsoleKey.V },
            { "W", ConsoleKey.W },
            { "w", ConsoleKey.W },
            { "X", ConsoleKey.X },
            { "x", ConsoleKey.X },
            { "Y", ConsoleKey.Y },
            { "y", ConsoleKey.Y },
            { "Z", ConsoleKey.Z },
            { "z", ConsoleKey.Z }
        };
        public static Dictionary<string, ConsoleColor> StringToConsoleColorTable { get; } = new()
        {
            { "black", ConsoleColor.Black },
            { "darkblue", ConsoleColor.DarkBlue },
            { "darkgreen", ConsoleColor.DarkGreen },
            { "darkcyan", ConsoleColor.DarkCyan },
            { "darkred", ConsoleColor.DarkRed },
            { "darkmagenta", ConsoleColor.DarkMagenta },
            { "darkyellow", ConsoleColor.DarkYellow },
            { "darkgray", ConsoleColor.DarkGray },
            { "gray", ConsoleColor.Gray },
            { "blue", ConsoleColor.Blue },
            { "green", ConsoleColor.Green },
            { "cyan", ConsoleColor.Cyan },
            { "red", ConsoleColor.Red },
            { "magenta", ConsoleColor.Magenta },
            { "yellow", ConsoleColor.Yellow },
            { "white", ConsoleColor.White },

            // Windows Terminal-style, why not?
            { "purple", ConsoleColor.Magenta },
            { "darkpurple", ConsoleColor.DarkMagenta },
        };
        public static Dictionary<String, TokenType> StringToTokenTypeTable { get; } = new()
        {
            { "misc", TokenType.Misc },
            { "break", TokenType.Break },
            { "whitespace", TokenType.Whitespace },
            { "alphabetical", TokenType.Alphabetical },
            { "keyword", TokenType.Keyword },
            { "numberliteral", TokenType.NumberLiteral },
            { "stringliteral", TokenType.StringLiteral },
            { "characterliteral", TokenType.CharLiteral },
            { "booleanliteral", TokenType.BooleanLiteral },
            { "hexliteral", TokenType.HexLiteral },
            { "comment", TokenType.Comment },
            { "grouping", TokenType.Grouping },
            { "operator", TokenType.Operator },
            { "url", TokenType.Url },
            { "function", TokenType.Function },
            { "specialchar", TokenType.SpecialChar },
            
            { "clicommandvalid", TokenType.CliCommandValid },
            { "clicommandinvalid", TokenType.CliCommandInvalid },
            { "cliswitch", TokenType.CliSwitch },
            { "clistring", TokenType.CliString },

            { "footerseparator", TokenType.FooterSeparator },
            { "footerfilepath", TokenType.FooterFilepath },
            { "footerlncol", TokenType.FooterLnCol },
            { "footerselection", TokenType.FooterSelection },
            { "footerindicator", TokenType.FooterIndicator },

            { "trailingwhitespace", TokenType.TrailingWhitespace },
        };

        public static Properties Properties { get; private set; } = new Properties();
        public static Theme Theme { get; private set; } = new Theme();

        public static TSettings ReadSettingsFile<TSettings>(string filename)
        {
            Log.Write(String.Format("Loading XML from \"{0}\"", filename), "settings");
            XmlSerializer serializer = new XmlSerializer(typeof(TSettings));

            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

            FileStream stream = new FileStream(filename, FileMode.Open);
            try
            {
                TSettings settings = (TSettings)serializer.Deserialize(stream);
                stream.Close();
                return settings;
            }
            catch (Exception e)
            {
                Log.Write(e.Message, "settings", important: true);
                CommandLine.SetOutput("Encountered an error while deserializing", "settings", oType: CommandLine.OutputType.Error);
                stream.Close();
                return default(TSettings);
            }
        }

        private static void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            Log.Write(String.Format("Encountered an unknown node while deserializing (\"{0}\" line {1})", e.Name, e.LineNumber), "settings", important: true);
            CommandLine.SetOutput("Encountered an unknown node while deserializing", "settings", oType: CommandLine.OutputType.Error);
        }

        private static void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            Log.Write(String.Format("Encountered an unknown attribute while deserializing (\"{0}\" line {1})", e.Attr, e.LineNumber), "settings", important: true);
            CommandLine.SetOutput("Encountered an unknown attribute while deserializing", "settings", oType: CommandLine.OutputType.Error);
        }

        public static void LoadSettings()
        {
            Properties loadedProperties = ReadSettingsFile<Properties>(PropertiesFile);
            if (loadedProperties != null) Properties = loadedProperties;
            PopulateAliasesTable(Properties);
            PopulateShortcutsTable(Properties);
            
            Theme loadedTheme = ReadSettingsFile<Theme>(ThemeFile);
            if (loadedTheme != null) Theme = loadedTheme;
            PopulateTokenColorsTable(Theme);
        }

        // TODO: remove
        public static ConsoleColor StringToConsoleColor(string s)
        {
            try
            {
                return StringToConsoleColorTable[s.ToLower()];
            }
            catch (Exception)
            {
                return ConsoleColor.Gray;
            }
        }

        private static void PopulateTokenColorsTable(Theme theme)
        {
            foreach (TokenColor t in theme.Tokens)
            {
                theme.TokenColorTable.Add(StringToTokenTypeTable[t.TokenType.ToLower()], StringToConsoleColorTable[t.Color.ToLower()]);
            }
        }

        private static void PopulateAliasesTable(Properties properties)
        {
            foreach (Alias a in properties.Aliases)
            {
                properties.AliasesTable.Add(a.AliasString, a.Command);
            }
        }

        private static void PopulateShortcutsTable(Properties properties)
        {
            foreach (Shortcut s in properties.Shortcuts)
            {
                properties.ShortcutsTable.Add(StringToConsoleKeyTable[s.Key], s);
            }
        }
    }
}
