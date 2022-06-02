﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Text;

namespace maple
{
    static class Settings
    {

        public static string MapleDirectory { get; private set; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string SettingsFile { get; set; } = Path.Combine(MapleDirectory, "properties", "properties.xml");
        public static string AliasesFile { get; set; } = Path.Combine(MapleDirectory, "properties", "aliases.xml");
        public static string ShortcutsFile { get; set; } = Path.Combine(MapleDirectory, "properties", "shortcuts.xml");

        //properties
        public static bool DebugTokens { get; set; } = false;
        public static bool NoHighlight { get; set; } = false;
        public static bool NoTokenize { get; set; } = false;
        public static bool CliNoHighlight { get; set; } = false;
        public static bool RelativePath { get; set; } = false;
        public static bool NavigatePastTabs { get; set; } = true;
        public static bool DeleteEntireTabs { get; set; } = true;
        public static bool EnableLogging { get; set; } = true;
        public static bool SummarizeLog { get; set; } = false;
        public static bool SaveOnClose { get; set; } = false;
        public static bool PreserveIndentOnEnter { get; set; } = true;
        public static bool ShiftSelect { get; set; } = true;
        public static bool ShiftDeindent { get; set; } = true;
        public static bool ArrowsDeselect { get; set; } = true;
        public static bool ClearOutputOnToggle { get; set; } = true;
        public static bool ColorOutputBackground { get; set; } = true;
        public static bool HighlightTrailingWhitespace { get; set; } = false;
        public static bool Autocomplete { get; set; } = true;
        public static bool AutoResize { get; set; } = true;

        public static string ThemeDirectory { get; private set; } = Path.Combine(MapleDirectory, "themes");
        public static string ThemeFile { get; private set; } = "maple.xml";
        public static string SyntaxDirectory { get; private set; } = Path.Combine(MapleDirectory, "syntax");
        public static int TabSpacesCount { get; private set; } = 4;
        public static int ScrollYIncrement { get; private set; } = -1;
        public static int ScrollXIncrement { get; private set; } = -1;

        //editor customizations
        public static string VanityFooter { get; private set; } = "maple";
        public static string FooterFormat { get; private set; } = "{vanity}{-}{filepath}{-}{lncol}"; //super simple default
        public static string FooterSeparator { get; private set; } = " ";
        public static char GutterLeftPad { get; private set; } = '0';
        public static char GutterBarrier { get; private set; } = ' ';
        public static char OverflowIndicator { get; private set; } = '…';
        public static Encoding DefaultEncoding { get; private set; } = Encoding.UTF8;
        public static List<char> AutocompleteOpeningChars { get; private set; } = new List<char>();
        public static List<char> AutocompleteEndingChars { get; private set; } = new List<char>();

        static List<string> ignoreList = new List<string>(); //stores a list of settings to ignore when loading

        public static Dictionary<string, string> Aliases { get; private set; } = new();
        public static Dictionary<ConsoleKey, ShortcutInfo> Shortcuts { get; private set; } = new();

        public static void IgnoreSetting(string name)
        {
            ignoreList.Add(name.ToLower());
        }

        public static void LoadSettings()
        {
            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;

            if (!File.Exists(SettingsFile))
            {
                Log.Write("Settings file doesn't exist at '" + SettingsFile + "'", "settings", important: true);
                return;
            }

            try
            {
                document.Load(SettingsFile);
            }
            catch (Exception e)
            {
                CommandLine.SetOutput("Encountered an exception while loading properties XML", "maple", oType: CommandLine.OutputType.Error);
                Log.Write("Encountered exception while loading properties XML: " + e.Message, "settings", important: true);
                return;
            }

            XmlNodeList properties = document.GetElementsByTagName("property");
            foreach(XmlNode node in properties)
            {
                string name = "";
                string value = "";
                foreach(XmlAttribute a in node.Attributes)
                {
                    if(a.Name.ToLower() != "name")
                        return;
                    
                    name = a.Value.ToLower();
                }
                value = node.InnerText.ToLower();

                if (ignoreList.Contains(name)) //skip all settings in ignore list
                    continue;

                switch(name)
                {
                    //SWITCHES
                    case "debugtokens":
                        DebugTokens = IsTrue(value);
                        break;
                    case "nohighlight":
                        NoHighlight = IsTrue(value);
                        break;
                    case "notokenize":
                        NoTokenize = IsTrue(value);
                        break;
                    case "clinohighlight":
                        CliNoHighlight = IsTrue(value);
                        break;
                    case "relativepath":
                        RelativePath = IsTrue(value);
                        if (RelativePath)
                            MapleDirectory = Directory.GetCurrentDirectory();
                        break;
                    case "navigatepasttabs":
                        NavigatePastTabs = IsTrue(value);
                        break;
                    case "deleteentiretabs":
                        DeleteEntireTabs = IsTrue(value);
                        break;
                    case "readonly":
                        Input.ReadOnly = IsTrue(value);
                        break;
                    case "enablelogging":
                        EnableLogging = IsTrue(value);
                        break;
                    case "summarizelog":
                        SummarizeLog = IsTrue(value);
                        break;
                    case "saveonclose":
                        SaveOnClose = IsTrue(value);
                        break;
                    case "preserveidentonenter":
                        PreserveIndentOnEnter = IsTrue(value);
                        break;
                    case "shiftselect":
                        ShiftSelect = IsTrue(value);
                        break;
                    case "shiftdeindent":
                        ShiftDeindent = IsTrue(value);
                        break;
                    case "arrowsdeselect":
                        ArrowsDeselect = IsTrue(value);
                        break;
                    case "clearoutputontoggle":
                        ClearOutputOnToggle = IsTrue(value);
                        break;
                    case "coloroutputbackground":
                        ColorOutputBackground = IsTrue(value);
                        break;
                    case "highlighttrailingwhitespace":
                        HighlightTrailingWhitespace = IsTrue(value);
                        break;
                    case "autocomplete":
                        Autocomplete = IsTrue(value);
                        break;
                    case "autoresize":
                        AutoResize = IsTrue(value);
                        break;

                    //ARGUMENTS
                    case "themedirectory":
                        ThemeDirectory = value;
                        if(!ThemeDirectory.EndsWith("/"))
                            ThemeDirectory += "/";
                        ThemeDirectory = ThemeDirectory.Replace("{mapledir}", MapleDirectory);
                        break;
                    case "themefile":
                        ThemeFile = value;
                        break;
                    case "syntaxdirectory":
                        SyntaxDirectory = value;
                        if (!SyntaxDirectory.EndsWith("/"))
                            SyntaxDirectory += "/";
                        SyntaxDirectory = SyntaxDirectory.Replace("{mapledir}", MapleDirectory);
                        break;
                    case "tabspacescount":
                        TabSpacesCount = Convert.ToInt32(value);
                        break;
                    case "scrollyincrement":
                        if (value.Equals("half"))
                            ScrollYIncrement = -1;
                        else if (value.Equals("full"))
                            ScrollYIncrement = -2;
                        else
                            ScrollYIncrement = Math.Abs(Convert.ToInt32(value));
                        break;
                    case "scrollxincrement":
                        if (value.Equals("half"))
                            ScrollXIncrement = -1;
                        else if (value.Equals("full"))
                            ScrollXIncrement = -2;
                        else
                            ScrollXIncrement = Math.Abs(Convert.ToInt32(value));
                        break;
                    case "defaultencoding":
                        if (value.Equals("utf8"))
                            DefaultEncoding = Encoding.UTF8;
                        else if (value.Equals("ascii"))
                            DefaultEncoding = Encoding.ASCII;
                        else
                            Log.Write("Invalid DefaultEncoding value", "styler", important: true);
                        break;

                    //EDITOR CUSTOMIZATIONS
                    case "vanityfooter":
                        VanityFooter = value;
                        break;
                    case "footerformat":
                        FooterFormat = value;
                        break;
                    case "footerseparator":
                        FooterSeparator = value;
                        break;
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
                    case "autocompletepairings":
                        if (value.Length % 2 == 1)
                        {
                            Log.Write("Uneven autocomplete pairings defined, they will not be loaded", "styler", important: true);
                            break;
                        }
                        for (int i = 0; i < value.Length; i++) {
                            if (i % 2 == 0)
                                AutocompleteOpeningChars.Add(value[i]);
                            else
                                AutocompleteEndingChars.Add(value[i]);
                        }
                        break;

                    default:
                        Log.Write("Encountered unknown property '" + name + "'", "settings", important: true);
                        break;
                }
            }

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
                        key = CharToConsoleKey(a.Value.ToLower().ToCharArray()[0]);
                    else if (a.Name.ToLower().Equals("execute"))
                        execute = IsTrue(a.Value.ToLower());
                }

                command = node.InnerText;

                Shortcuts.Add(key, new ShortcutInfo(command, execute));
            }
        }

        public static ConsoleKey CharToConsoleKey(char c)
        {
            switch (c)
            {
                case 'A':
                case 'a':
                    return ConsoleKey.A;
                case 'B':
                case 'b':
                    return ConsoleKey.B;
                case 'C':
                case 'c':
                    return ConsoleKey.C;
                case 'D':
                case 'd':
                    return ConsoleKey.D;
                case 'E':
                case 'e':
                    return ConsoleKey.E;
                case 'F':
                case 'f':
                    return ConsoleKey.F;
                case 'G':
                case 'g':
                    return ConsoleKey.G;
                case 'H':
                case 'h':
                    return ConsoleKey.H;
                case 'I':
                case 'i':
                    return ConsoleKey.I;
                case 'J':
                case 'j':
                    return ConsoleKey.J;
                case 'K':
                case 'k':
                    return ConsoleKey.K;
                case 'L':
                case 'l':
                    return ConsoleKey.L;
                case 'M':
                case 'm':
                    return ConsoleKey.M;
                case 'N':
                case 'n':
                    return ConsoleKey.N;
                case 'O':
                case 'o':
                    return ConsoleKey.O;
                case 'P':
                case 'p':
                    return ConsoleKey.P;
                case 'Q':
                case 'q':
                    return ConsoleKey.Q;
                case 'R':
                case 'r':
                    return ConsoleKey.R;
                case 'S':
                case 's':
                    return ConsoleKey.S;
                case 'T':
                case 't':
                    return ConsoleKey.T;
                case 'U':
                case 'u':
                    return ConsoleKey.U;
                case 'V':
                case 'v':
                    return ConsoleKey.V;
                case 'W':
                case 'w':
                    return ConsoleKey.W;
                case 'X':
                case 'x':
                    return ConsoleKey.X;
                case 'Y':
                case 'y':
                    return ConsoleKey.Y;
                case 'Z':
                case 'z':
                    return ConsoleKey.Z;
            }

            return ConsoleKey.NoName; //there is not "null" so this is the best I can do
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
