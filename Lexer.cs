using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;

namespace maple
{
    public static class Lexer
    {

        /*
        static string numberLiteralPattern = "";
        static string alphabeticalPattern = "";
        static string breakPattern = "";
        static string groupingPattern = "";
        static string stringMarkerPattern = "";
        static string characterMarkerPattern = "";
        static string commentPattern = "";
        static string operatorPattern = "";
        */
        static List<LexerRule> rules = new List<LexerRule>();
        static List<string> keywords = new List<string>();

        public static void LoadSyntax(string syntaxPath)
        {
            XmlDocument document = new XmlDocument();

            if(!File.Exists(syntaxPath))
            {
                Settings.noHighlight = true;
                return;
            }

            document.Load(syntaxPath);
            
            //build syntax rules
            XmlNodeList syntaxRules = document.GetElementsByTagName("syntax");
            foreach (XmlNode node in syntaxRules)
            {
                string type = "";
                string value = "";
                foreach(XmlAttribute a in node.Attributes)
                {
                    if(a.Name.ToLower() != "type")
                        return;

                    type = a.Value.ToLower();
                }
                value = node.InnerText.ToLower();

                rules.Add(new LexerRule(type, value));
            }

            //build keyword list
            XmlNodeList keywordNodes = document.GetElementsByTagName("keyword");
            foreach (XmlNode node in keywordNodes)
                keywords.Add(node.InnerText);
        }

        public static Token[] Tokenize(string text)
        {
            List<Token> tokens = new List<Token>();

            while (text.Length > 0)
            {
                Match nearestMatch = null;
                string nearestMatchRuleName = "";

                bool foundPerfect = false;
                for (int i = 0; i < rules.Count; i++)
                {
                    LexerRule rule = rules[i];
                    Match firstMatch = rule.pattern.Match(text);

                    if (!firstMatch.Success) //no match, keep checking
                        continue;

                    if (firstMatch.Index == 0) //next token matches - jobs done
                    {
                        Token.TokenType tokenType = GetTokenTypeFromRuleName(rule.name);
                        //if alphabetical, check for keyword
                        if (rule.name.Equals("alphabetical") && keywords.Contains(firstMatch.Value))
                            tokenType = Token.TokenType.Keyword;

                        tokens.Add(new Token(firstMatch.Value, tokenType));
                        text = text.Remove(firstMatch.Index, firstMatch.Value.Length);
                        foundPerfect = true;
                        break;
                    }

                    //there is a match, but it isn't at index 0
                    nearestMatch = firstMatch;
                    nearestMatchRuleName = rule.name;
                }

                //all rules have been checked
                if (!foundPerfect) //the closest match isn't at 0
                {
                    if (nearestMatch != null) //there is a match somewhere
                    {
                        //remove unmatchable text and add "none" token
                        string unmatchSubstring = text.Substring(0, nearestMatch.Index);
                        tokens.Add(new Token(unmatchSubstring, Token.TokenType.None));
                        text = text.Remove(0, nearestMatch.Index);
                        //eat first matched token
                        text = text.Remove(0, nearestMatch.Value.Length);
                        Token.TokenType tokenType = GetTokenTypeFromRuleName(nearestMatchRuleName);
                        //if alphabetical, check for keyword
                        if (nearestMatchRuleName.Equals("alphabetical") && keywords.Contains(nearestMatch.Value))
                            tokenType = Token.TokenType.Keyword;

                        tokens.Add(new Token(nearestMatch.Value, tokenType));
                    }
                    else //there is no match anywhere
                    {
                        tokens.Add(new Token(text, Token.TokenType.None)); //add rest of text with "none" token
                        text = ""; //clear text
                        break;
                    }
                }
            }

            return tokens.ToArray();

        }

        static bool IsKeyword(String term)
        {
            if(keywords.Contains(term))
                return true;
            return false;
        }

        static Token.TokenType GetTokenTypeFromRuleName(string name)
        {
            switch (name)
            {
                case "numberliteral":
                    return Token.TokenType.NumberLiteral;
                case "alphabetical":
                    return Token.TokenType.Variable;
                case "break":
                    return Token.TokenType.Break;
                case "grouping":
                    return Token.TokenType.Grouping;
                case "stringliteral":
                    return Token.TokenType.StringLiteral;
                case "characterliteral":
                    return Token.TokenType.CharLiteral;
                case "comment":
                    return Token.TokenType.Comment;
                case "operator":
                    return Token.TokenType.Operator;
                case "keyword":
                    return Token.TokenType.Keyword;
                default:
                    return Token.TokenType.None;
            }
        }

        struct LexerRule
        {

            public string name;
            public Regex pattern;

            public LexerRule(string name, string pattern)
            {
                this.name = name;
                this.pattern = new Regex(pattern);
            }
        }

    }
}