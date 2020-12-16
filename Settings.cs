using System;
using System.IO;
using System.Xml;

namespace maple
{
    static class Settings
    {

        public static String settingsFile = "properties.xml";

        //properties
        public static bool quickCli = false;
        public static bool debugTokens = false;
        public static bool noHighlight = false;

        public static String themeDirectory = "themes/";
        public static String themeFile = "maple.xml";
        public static String syntaxDirectory = "syntax/";

        public static void LoadSettings()
        {
            XmlDocument document = new XmlDocument();

            if(!File.Exists(settingsFile))
                return;

            document.Load(settingsFile);

            XmlNodeList properties = document.GetElementsByTagName("property");
            foreach(XmlNode node in properties)
            {
                String name = "";
                String value = "";
                foreach(XmlAttribute a in node.Attributes)
                {
                    if(a.Name.ToLower() != "name")
                        return;
                    
                    name = a.Value.ToLower();
                }
                value = node.InnerText.ToLower();

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
                    case "themedirectory":
                        themeDirectory = value;
                        if(!themeDirectory.EndsWith("/"))
                            themeDirectory += "/";
                        break;
                    case "themefile":
                        themeFile = value;
                        break;
                    case "syntaxdirectory":
                        syntaxDirectory = value;
                        if (!syntaxDirectory.EndsWith("/"))
                            syntaxDirectory += "/";
                        break;
                }
            }
        }

        static bool IsTrue(String value)
        {
            return value == "true";
        }

    }
}