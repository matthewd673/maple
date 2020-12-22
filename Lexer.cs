using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;

namespace maple
{
    public static class Lexer
    {

        static String numberLiteralPattern = "";
        static String alphabeticalPattern = "";
        static String breakPattern = "";
        static String groupingPattern = "";
        static String stringMarkerPattern = "";
        static String characterMarkerPattern = "";
        static String commentPrefixPattern = "";
        static String operatorPattern = "";

        static List<String> keywords = new List<String>();

        public static void LoadSyntax(String syntaxPath)
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
                String type = "";
                String value = "";
                foreach(XmlAttribute a in node.Attributes)
                {
                    if(a.Name.ToLower() != "type")
                        return;

                    type = a.Value.ToLower();
                }
                value = node.InnerText.ToLower();

                switch(type)
                {
                    case "numberliteral":
                        numberLiteralPattern = value; break;
                    case "alphabetical":
                        alphabeticalPattern = value; break;
                    case "break":
                        breakPattern = value; break;
                    case "grouping":
                        groupingPattern = value; break;
                    case "stringmarker":
                        stringMarkerPattern = value; break;
                    case "charactermarker":
                        characterMarkerPattern = value; break;
                    case "commentprefix":
                        commentPrefixPattern = value; break;
                    case "operator":
                        operatorPattern = value; break;
                }
            }

            //build keyword list
            XmlNodeList keywordNodes = document.GetElementsByTagName("keyword");
            foreach (XmlNode node in keywordNodes)
                keywords.Add(node.InnerText);
        }

        public static Token[] Tokenize(String text)
        {
            //skip tokenizing if no highlight
            if(Settings.noHighlight)
                return new Token[] { new Token(text, Token.TokenType.Misc) };

            List<Token> tokens = new List<Token>();
            char[] characters = text.ToCharArray();

            bool firstChar = false;
            bool inComment = false;
            bool inStringLiteral = false;
            bool inCharLiteral = false;

            for(int i = 0; i < characters.Length; i++)
            {
                String s = characters[i].ToString(); //get current character
                int lastI = tokens.Count - 1;
                Token.TokenType lastType = Token.TokenType.Misc;
                if(lastI >= 0)
                    lastType = tokens[lastI].GetTokenType();

                //check if it should be treated as first char
                if(!firstChar && (
                    lastType == Token.TokenType.Break ||
                    lastType == Token.TokenType.Operator ||
                    lastType == Token.TokenType.Grouping ||
                    lastI < 0
                    ))
                {
                    firstChar = true;
                }

                //check if its in a comment
                if(inComment)
                {
                    tokens[lastI].Append(s);
                    continue;
                }

                //check if its in a string literal
                if(inStringLiteral)
                {
                    if(Regex.IsMatch(s, stringMarkerPattern))
                    {
                        inStringLiteral = false;
                        firstChar = true;
                    }
                    tokens[lastI].Append(s);
                    continue;
                }

                //check if its in a char literal
                if(inCharLiteral)
                {
                    if(Regex.IsMatch(s, characterMarkerPattern))
                    {
                        inCharLiteral = false;
                        firstChar = true;
                    }
                    tokens[lastI].Append(s);
                    continue;
                }

                //match character
                if(Regex.IsMatch(s, numberLiteralPattern)) //numerals
                {
                    if(firstChar)
                    {
                        tokens.Add(new Token(s, Token.TokenType.NumberLiteral));
                        firstChar = false;
                    }
                    else
                        tokens[lastI].Append(s);
                    continue;
                }

                if(Regex.IsMatch(s, alphabeticalPattern)) //alpha
                {
                    if(firstChar)
                    {
                        tokens.Add(new Token(s, Token.TokenType.Misc));
                        firstChar = false;
                    }
                    else
                        tokens[lastI].Append(s);
                    continue;
                }

                if(Regex.IsMatch(s, breakPattern)) //break
                {
                    tokens.Add(new Token(s, Token.TokenType.Break));
                    continue;
                }

                if(Regex.IsMatch(s, groupingPattern)) //grouping
                {
                    tokens.Add(new Token(s, Token.TokenType.Grouping));
                    tokens.Add(new Token("", Token.TokenType.Break));
                    continue;
                }

                if(Regex.IsMatch(s, commentPrefixPattern)) //potential comment
                {
                    if(firstChar)
                    {
                        tokens.Add(new Token(s, Token.TokenType.Misc));
                        firstChar = false;
                    }
                    else
                    {
                        if(Regex.IsMatch(tokens[lastI].GetText().Trim(), commentPrefixPattern)) //preceded by a single slash, so comment!
                        {
                            tokens[lastI].Append(s);
                            tokens[lastI].SetType(Token.TokenType.Comment);
                            inComment = true;
                        }
                    }
                    continue;
                }

                if(Regex.IsMatch(s, operatorPattern)) //operator
                {
                    tokens.Add(new Token(s, Token.TokenType.Operator));
                    continue;
                }

                if(Regex.IsMatch(s, stringMarkerPattern)) //quotes
                {
                    if(!inStringLiteral)
                    {
                        inStringLiteral = true;
                        tokens.Add(new Token(s, Token.TokenType.StringLiteral));
                    }
                    continue;
                }

                if(Regex.IsMatch(s, characterMarkerPattern)) //single quote
                {
                    if(!inCharLiteral)
                    {
                        inCharLiteral = true;
                        if(firstChar)
                            tokens.Add(new Token(s, Token.TokenType.CharLiteral));
                    }
                    continue;
                }

                //unknown character
                if(lastI >= 0 && !firstChar && tokens[lastI].GetTokenType() == Token.TokenType.Misc)
                    tokens[lastI].Append(s);
                else
                    tokens.Add(new Token(s, Token.TokenType.Misc));
            }

            //convert to array, no more adding/removing
            Token[] tokenArray = tokens.ToArray();

            //check misc tokens for keywords
            for(int i = 0; i < tokenArray.Length; i++)
            {
                String tokenText = tokenArray[i].GetText();
                if(IsKeyword(tokenText))
                    tokenArray[i] = new Token(tokenText, Token.TokenType.Keyword);
            }

            //return
            return tokenArray;
        }

        static bool IsKeyword(String term)
        {
            if(keywords.Contains(term))
                return true;
            return false;
        }

    }
}