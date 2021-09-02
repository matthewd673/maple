using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Reflection;

namespace maple
{
    static class Settings
    {

        public static string mapleDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string settingsFile = mapleDirectory + "\\properties.xml";

        //properties
        public static bool QuickCli { get; set; } = false;
        public static bool DebugTokens { get; set; } = false;
        public static bool NoHighlight { get; set; } = false;
        public static bool RelativePath { get; set; } = false;
        public static bool NavigatePastTabs { get; set; } = true;

        public static string ThemeDirectory { get; private set; } = mapleDirectory + "\\themes\\";
        public static string ThemeFile { get; private set; } = "maple.xml";
        public static string SyntaxDirectory { get; private set; } = mapleDirectory + "\\syntax\\";
        public static int TabSpacesCount { get; private set; } = 4;

        static List<string> ignoreList = new List<string>(); //stores a list of settings to ignore when loading

        public static void IgnoreSetting(string name)
        {
            ignoreList.Add(name.ToLower());
        }

        public static void LoadSettings()
        {
            XmlDocument document = new XmlDocument();

            if (!File.Exists(settingsFile))
                return;

            document.Load(settingsFile);

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
                            mapleDirectory = Directory.GetCurrentDirectory();
                        break;
                    case "navigatepasttabs":
                        NavigatePastTabs = IsTrue(value);
                        break;
                    //ARGUMENTS
                    case "themedirectory":
                        ThemeDirectory = value;
                        if(!ThemeDirectory.EndsWith("/"))
                            ThemeDirectory += "/";
                        ThemeDirectory = ThemeDirectory.Replace("{mapledir}", mapleDirectory);
                        break;
                    case "themefile":
                        ThemeFile = value;
                        break;
                    case "syntaxdirectory":
                        SyntaxDirectory = value;
                        if (!SyntaxDirectory.EndsWith("/"))
                            SyntaxDirectory += "/";
                        SyntaxDirectory = SyntaxDirectory.Replace("{mapledir}", mapleDirectory);
                        break;
                    case "tabspacescount":
                        TabSpacesCount = Convert.ToInt32(value);
                        break;
                }
            }

        }

        static bool IsTrue(string value)
        {
            return value.Equals("true") | value.Equals("t") | value.Equals("1");
        }

    }
}