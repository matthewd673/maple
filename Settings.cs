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
        public static bool quickCli = false;
        public static bool debugTokens = false;
        public static bool noHighlight = false;
        public static bool relativePath = false;

        public static string themeDirectory = mapleDirectory + "\\themes\\";
        public static string themeFile = "maple.xml";
        public static string syntaxDirectory = mapleDirectory + "\\syntax\\";

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
                    case "quickcli":
                        quickCli = IsTrue(value);
                        break;
                    case "debugtokens":
                        debugTokens = IsTrue(value);
                        break;
                    case "nohighlight":
                        noHighlight = IsTrue(value);
                        break;
                    case "relativepath":
                        relativePath = IsTrue(value);
                        if (relativePath)
                            mapleDirectory = Directory.GetCurrentDirectory();
                        break;
                    case "themedirectory":
                        themeDirectory = value;
                        if(!themeDirectory.EndsWith("/"))
                            themeDirectory += "/";
                        themeDirectory = themeDirectory.Replace("{mapledir}", mapleDirectory);
                        break;
                    case "themefile":
                        themeFile = value;
                        break;
                    case "syntaxdirectory":
                        syntaxDirectory = value;
                        if (!syntaxDirectory.EndsWith("/"))
                            syntaxDirectory += "/";
                        syntaxDirectory = syntaxDirectory.Replace("{mapledir}", mapleDirectory);
                        break;
                }
            }

        }

        static bool IsTrue(string value)
        {
            return value == "true";
        }

    }
}