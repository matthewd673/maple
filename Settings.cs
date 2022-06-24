using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.Text;

namespace maple
{


    [Serializable]
    [XmlType("TokenColor")]
    public struct TokenColor
    {
        public string TokenType { get; set; }
        public string Color { get; set; }
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
            get { return Accent.ToString(); }
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

        //cli colors
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
    }

    static class Settings
    {
        // directories, these aren't user-defined
        public static string MapleDirectory { get; private set; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string PropertiesFile { get; } = Path.Combine(MapleDirectory, "properties", "properties.xml");
        public static string AliasesFile { get; } = Path.Combine(MapleDirectory, "properties", "aliases.xml");
        public static string ShortcutsFile { get; } = Path.Combine(MapleDirectory, "properties", "shortcuts.xml");
        public static string ThemeFile { get { return Path.Combine(Properties.ThemeDirectory, Properties.ThemeFile); } }

        private static Dictionary<string, ConsoleKey> stringToConsoleKeyTable = new()
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
        private static Dictionary<string, ConsoleColor> stringToConsoleColorTable = new()
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
            { "white", ConsoleColor.White }
        };

        public static Dictionary<string, string> Aliases { get; private set; } = new();
        public static Dictionary<ConsoleKey, ShortcutInfo> Shortcuts { get; private set; } = new();

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
            Log.Write(String.Format("Encountered an unknwon node while deserializing (line {0})", e.LineNumber), "settings", important: true);
            CommandLine.SetOutput("Encountered an unknown node while deserializing", "settings", oType: CommandLine.OutputType.Error);
        }

        private static void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            Log.Write(String.Format("Encountered an unknwon attribute while deserializing (line {0})", e.LineNumber), "settings", important: true);
            CommandLine.SetOutput("Encountered an unknwon attribute while deserializing", "settings", oType: CommandLine.OutputType.Error);
        }

        public static void LoadSettings()
        {
            Properties = ReadSettingsFile<Properties>(PropertiesFile);
            Theme = ReadSettingsFile<Theme>(ThemeFile);
        }

        public static void LoadAliases()
        {
            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;

            if (!File.Exists(AliasesFile))
            {
                Log.Write("Aliases file doesn't exist at '" + AliasesFile + "'", "settings", important: true);
                return;
            }

            try
            {
                document.Load(AliasesFile);
            }
            catch (Exception e)
            {
                CommandLine.SetOutput("Encountered an exception while loading alias XML", "maple", oType: CommandLine.OutputType.Error);
                Log.Write("Encountered exception while loading alias XML: " + e.Message, "settings", important: true);
                return;
            }

            XmlNodeList aliases = document.GetElementsByTagName("alias");
            foreach (XmlNode node in aliases)
            {
                string command = "";
                string value = "";
                foreach (XmlAttribute a in node.Attributes)
                {
                    if (a.Name.ToLower() != "command")
                        return;
                    
                    command = a.Value.ToLower();
                }
                value = node.InnerText;

                Aliases.Add(value, command);
            }
        }

        public static void LoadShortcuts()
        {
            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;

            if (!File.Exists(ShortcutsFile))
            {
                Log.Write("Shortcuts file doesn't exist at '" + ShortcutsFile + "'", "settings", important: true);
                return;
            }

            try
            {
                document.Load(ShortcutsFile);
            }
            catch (Exception e)
            {
                CommandLine.SetOutput("Encountered an exception while loading shortcut XML", "maple", oType: CommandLine.OutputType.Error);
                Log.Write("Encountered exception while loading shortcut XML: " + e.Message, "settings", important: true);
            }

            XmlNodeList shortcuts = document.GetElementsByTagName("shortcut");
            foreach (XmlNode node in shortcuts)
            {
                ConsoleKey key = ConsoleKey.NoName;
                bool execute = false;
                string command = "";

                foreach (XmlAttribute a in node.Attributes)
                {
                    if (a.Name.ToLower().Equals("key"))
                        key = StringToConsoleKey(a.Value);
                    else if (a.Name.ToLower().Equals("execute"))
                        execute = IsTrue(a.Value.ToLower());
                }

                command = node.InnerText;

                Shortcuts.Add(key, new ShortcutInfo(command, execute));
            }
        }

        public static ConsoleKey StringToConsoleKey(string s)
        {
            try
            {
                return stringToConsoleKeyTable[s];
            }
            catch (Exception)
            {
                return ConsoleKey.NoName;
            }
        }

        public static ConsoleColor StringToConsoleColor(string s)
        {
            try
            {
                return stringToConsoleColorTable[s.ToLower()];
            }
            catch (Exception)
            {
                return ConsoleColor.Gray;
            }
        }

        public static bool IsTrue(string value)
        {
            return value.Equals("true") | value.Equals("t") | value.Equals("1");
        }

        public struct ShortcutInfo
        {
            public string Command { get; set; }
            public bool Execute { get; set; }

            public ShortcutInfo(string command, bool execute)
            {
                Command = command;
                Execute = execute;
            }
        } 

    }
}
