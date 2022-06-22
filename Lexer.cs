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

        const string CliStringRule = "\".*\"";
        const string CliSwitchRule = "(-{1,2})([a-zA-Z0-9]|-)+";
        const string CliFullCommandRule = "^[a-zA-Z]+";
        static List<LexerRule> cliRules = new List<LexerRule>();
        static List<string> cliKeywords = new List<string>();

        public static string CurrentSyntaxFile { get; private set; } = "None";

        /// <summary>
        /// Build a set of lexer rules and keywords from a syntax spec file (XML expected).
        /// </summary>
        /// <param name="syntaxPath">The path to the syntax spec file.</param>
        public static void LoadSyntax(string syntaxPath)
        {
            XmlDocument document = new XmlDocument();

            //reset
            rules.Clear();
            keywords.Clear();

            if(!File.Exists(syntaxPath))
            {
                Log.Write("Syntax path doesn't exist at '" + syntaxPath + "', falling back to default", "lexer", important: true);

                //fallback to default
                syntaxPath = Settings.Properties.SyntaxDirectory + "default.xml";
                if (!File.Exists(syntaxPath))
                {
                    Log.Write("Default syntax file doesn't exist at '" + syntaxPath + "'", "lexer", important: true);
                    CurrentSyntaxFile = "None";
                    return;
                }                
            }

            Log.Write("Loading syntax from '" + syntaxPath + "'", "lexer");
            CurrentSyntaxFile = syntaxPath;

            try
            {
                document.Load(syntaxPath);
            }
            catch (Exception e)
            {
                CommandLine.SetOutput("Encountered an exception while loading syntax XML", "maple", oType: CommandLine.OutputType.Error);
                Log.Write("Encountered exception while loading syntax XML: " + e.Message, "lexer", important: true);
                return;
            }
            
            //build syntax rules
            XmlNodeList syntaxRules = document.GetElementsByTagName("syntax");
            foreach (XmlNode node in syntaxRules)
            {
                string type = "";
                string value = "";
                bool insensitive = false;
                foreach(XmlAttribute a in node.Attributes)
                {
                    if(a.Name.ToLower().Equals("type"))
                        type = a.Value.ToLower();
                    if(a.Name.ToLower().Equals("insensitive"))
                        insensitive = Settings.IsTrue(a.Value);
                }
                value = node.InnerText.ToLower();

                RegexOptions options = RegexOptions.None;
                if (insensitive)
                    options = options | RegexOptions.IgnoreCase;

                rules.Insert(0, new LexerRule(Token.StringToTokenType(type), value, options));
            }

            Log.Write("Loaded " + rules.Count + " lexer rules", "lexer");

            //build keyword list
            XmlNodeList keywordNodes = document.GetElementsByTagName("keyword");
            foreach (XmlNode node in keywordNodes)
                keywords.Add(node.InnerText);
            Log.Write("Loaded " + keywords.Count + " lexer keywords", "lexer");
        }

        public static void LoadCommandLineSyntax()
        {
            //build rules
            //rules are colored based on existing theme colors (for now)
            cliRules.Add(new LexerRule(TokenType.Alphabetical, CliFullCommandRule));
            cliRules.Add(new LexerRule(TokenType.CliSwitch, CliSwitchRule));
            cliRules.Add(new LexerRule(TokenType.CliString, CliStringRule));

            Log.Write("Loaded " + cliRules.Count + " command line lexer rules", "lexer");

            //build command list
            foreach (string c in CommandLine.CommandMasterList)
                cliKeywords.Add(c);
            foreach (string a in Settings.Aliases.Keys)
                cliKeywords.Add(a);

            Log.Write("Loaded " + cliKeywords.Count + " command line keywords", "lexer");
        }

        /// <summary>
        /// Turn a piece of text into a series of <c>Token</c>s according to the given lexer rules.
        /// </summary>
        /// <param name="text">The text to be tokenized.</param>
        /// <returns>A list of <c>Token</c>s</returns>
        static List<Token> InternalTokenizer(string text, List<LexerRule> rules, List<string> keywords)
        {
            List<Token> tokens = new List<Token>();

            while (text.Length > 0)
            {
                Match nearestMatch = null;
                TokenType nearestMatchRuleType = TokenType.None;

                bool foundPerfect = false;
                for (int i = 0; i < rules.Count; i++)
                {
                    LexerRule rule = rules[i];
                    Match firstMatch = rule.Pattern.Match(text);

                    if (!firstMatch.Success || firstMatch.Value.Equals("")) // no match, keep checking
                    {
                        continue;
                    }

                    if (firstMatch.Index == 0) // next token matches - jobs done
                    {
                        TokenType currentType = rule.TType;
                        // if alphabetical, check for keyword
                        if ((rule.TType == TokenType.Alphabetical || rule.TType == TokenType.Function) && keywords.Contains(firstMatch.Value.Trim()))
                        {
                            currentType = TokenType.Keyword;
                        }

                        tokens.Add(new Token(firstMatch.Value, currentType));
                        text = text.Remove(firstMatch.Index, firstMatch.Value.Length);
                        foundPerfect = true;
                        break;
                    }

                    //there is a match, but it isn't at index 0
                    if (nearestMatch == null || firstMatch.Index < nearestMatch.Index)
                    {
                        nearestMatch = firstMatch;
                        nearestMatchRuleType = rule.TType;
                    }
                }

                //all rules have been checked
                if (!foundPerfect) //the closest match isn't at 0
                {
                    if (nearestMatch != null) //there is a match somewhere
                    {
                        //remove unmatchable text and add "none" token
                        string unmatchSubstring = text.Substring(0, nearestMatch.Index);
                        tokens.Add(new Token(unmatchSubstring, TokenType.None));
                        text = text.Remove(0, nearestMatch.Index);
                        //eat first matched token
                        text = text.Remove(0, nearestMatch.Value.Length);

                        TokenType currentType = nearestMatchRuleType;
                        //if alphabetical, check for keyword
                        if ((nearestMatchRuleType == TokenType.Alphabetical  || nearestMatchRuleType == TokenType.Function) && keywords.Contains(nearestMatch.Value.Trim()))
                        {
                            currentType = TokenType.Keyword;
                        }

                        tokens.Add(new Token(nearestMatch.Value, currentType));
                    }
                    else //there is no match anywhere
                    {
                        tokens.Add(new Token(text, TokenType.None)); //add rest of text with "none" token
                        text = ""; //clear text
                        break;
                    }
                }
            }

            // post-process
            // search for trailing whitespace and mark it as such
            if (Settings.Properties.HighlightTrailingWhitespace && tokens.Count > 0 && tokens[tokens.Count - 1].TType == TokenType.Whitespace)
            {
                tokens[tokens.Count - 1].TType = TokenType.TrailingWhitespace;
            }

            return tokens;
        }

        public static List<Token> Tokenize(string text)
        {
            if (Settings.Properties.NoTokenize)
                return new List<Token> { new Token(text, TokenType.None) };
            else
                return InternalTokenizer(text, rules, keywords);
        }

        public static List<Token> TokenizeCommandLine(string text)
        {
            List<Token> tokens = InternalTokenizer(text, cliRules, new List<string>()); //will handle keywords manually, no need
            
            //if first token isn't a keyword, and there's more than 1 token, user is trying an unknown command
            if (tokens.Count > 1 && tokens[0].TType == TokenType.Alphabetical)
            {
                bool firstIsValid = cliKeywords.Contains(tokens[0].Text);
                if (firstIsValid)
                    tokens[0].TType = TokenType.CliCommandValid;
                else
                    tokens[0].TType = TokenType.CliCommandInvalid;
            }
            
            return tokens;
        }

        struct LexerRule
        {
            public TokenType TType { get; set; }
            public Regex Pattern { get; set; }

            /// <summary>
            /// <c>LexerRule</c> represents a named pattern that the tokenizer will search for.
            /// </summary>
            /// <param name="name">The name of the pattern.</param>
            /// <param name="pattern">The RegEx pattern to search.</param>
            public LexerRule(TokenType tType, string pattern, RegexOptions options = RegexOptions.None)
            {
                TType = tType;
                Pattern = new Regex(pattern, options | RegexOptions.Compiled);
            }
        }

    }
}