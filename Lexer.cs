using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;

namespace maple
{
    public static class Lexer
    {

        static List<LexerRule> rules = new List<LexerRule>();
        static List<string> keywords = new List<string>();

        /// <summary>
        /// Build a set of lexer rules and keywords from a syntax spec file (XML expected).
        /// </summary>
        /// <param name="syntaxPath">The path to the syntax spec file.</param>
        public static void LoadSyntax(string syntaxPath)
        {
            XmlDocument document = new XmlDocument();

            if(!File.Exists(syntaxPath))
            {
                Settings.NoHighlight = true;
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

        /// <summary>
        /// Turn a piece of text into a series of <c>Token</c>s according to the given lexer rules.
        /// </summary>
        /// <param name="text">The text to be tokenized.</param>
        /// <returns>An array of <c>Token</c>s</returns>
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
                    Match firstMatch = rule.Pattern.Match(text);

                    if (!firstMatch.Success) //no match, keep checking
                        continue;

                    if (firstMatch.Index == 0) //next token matches - jobs done
                    {
                        Token.TokenType tokenType = GetTokenTypeFromRuleName(rule.Name);
                        //if alphabetical, check for keyword
                        if (rule.Name.Equals("alphabetical") && keywords.Contains(firstMatch.Value))
                            tokenType = Token.TokenType.Keyword;

                        tokens.Add(new Token(firstMatch.Value, tokenType));
                        text = text.Remove(firstMatch.Index, firstMatch.Value.Length);
                        foundPerfect = true;
                        break;
                    }

                    //there is a match, but it isn't at index 0
                    nearestMatch = firstMatch;
                    nearestMatchRuleName = rule.Name;
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

        /// <summary>
        /// Given the name of <c>LexerRule</c>, find the <c>TokenType</c> that it searches for.
        /// </summary>
        /// <param name="name">The name of the rule to check against.</param>
        /// <returns>The <c>TokenType</c> that the rule searches for. If invalid, returns <c>TokenType.None</c>.</returns>
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
                case "booleanliteral":
                    return Token.TokenType.BooleanLiteral;
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

            public string Name { get; set; }
            public Regex Pattern { get; set; }

            /// <summary>
            /// <c>LexerRule</c> represents a named pattern that the tokenizer will search for.
            /// </summary>
            /// <param name="name">The name of the pattern.</param>
            /// <param name="pattern">The RegEx pattern to search.</param>
            public LexerRule(string name, string pattern)
            {
                Name = name;
                Pattern = new Regex(pattern);
            }
        }

    }
}