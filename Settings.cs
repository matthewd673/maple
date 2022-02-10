using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Reflection;

namespace maple
{
    static class Settings
    {

        public static string MapleDirectory { get; set; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string SettingsFile { get; set; } = MapleDirectory + "\\properties\\properties.xml";
        public static string AliasesFile { get; set; } = MapleDirectory + "\\properties\\aliases.xml";

        //properties
        public static bool QuickCli { get; set; } = false;
        public static bool DebugTokens { get; set; } = false;
        public static bool NoHighlight { get; set; } = false;
        public static bool RelativePath { get; set; } = false;
        public static bool NavigatePastTabs { get; set; } = true;
        public static bool DeleteEntireTabs { get; set; } = true;

        public static string ThemeDirectory { get; private set; } = MapleDirectory + "\\themes\\";
        public static string ThemeFile { get; private set; } = "maple.xml";
        public static string SyntaxDirectory { get; private set; } = MapleDirectory + "\\syntax\\";
        public static int TabSpacesCount { get; private set; } = 4;

        static List<string> ignoreList = new List<string>(); //stores a list of settings to ignore when loading

        public static Dictionary<string, string> Aliases { get; private set; } = new Dictionary<string, string>();

        public static void IgnoreSetting(string name)
        {
            ignoreList.Add(name.ToLower());
        }

        public static void LoadSettings()
        {
            XmlDocument document = new XmlDocument();

            if (!File.Exists(SettingsFile))
                return;

            document.Load(SettingsFile);

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
                    case "quickcli":
                        QuickCli = IsTrue(value);
                        break;
                    case "debugtokens":
                        DebugTokens = IsTrue(value);
                        break;
                    case "nohighlight":
                        NoHighlight = IsTrue(value);
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
                }
            }

        }

        public static void LoadAliases()
        {
            XmlDocument document = new XmlDocument();

            if (!File.Exists(AliasesFile))
                return;

            document.Load(AliasesFile);
            
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

                Console.Title = value + ", " + command;

                Aliases.Add(value, command);
            }
        }

        static bool IsTrue(string value)
        {
            return value.Equals("true") | value.Equals("t") | value.Equals("1");
        }

    }
}