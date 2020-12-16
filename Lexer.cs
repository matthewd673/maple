using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace maple
{
    public static class Lexer
    {

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
                    if(Regex.IsMatch(s, "(\\\")"))
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
                    if(Regex.IsMatch(s, "(\\\')"))
                    {
                        inCharLiteral = false;
                        firstChar = true;
                    }
                    tokens[lastI].Append(s);
                    continue;
                }

                //match character
                if(Regex.IsMatch(s, "[0-9]")) //numerals
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

                if(Regex.IsMatch(s, "[a-zA-Z]")) //alpha
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

                if(Regex.IsMatch(s, "( |;)")) //break
                {
                    tokens.Add(new Token(s, Token.TokenType.Break));
                    continue;
                }

                if(Regex.IsMatch(s, "(\\(|\\)|\\{|\\}|\\[|\\])")) //grouping
                {
                    tokens.Add(new Token(s, Token.TokenType.Grouping));
                    tokens.Add(new Token("", Token.TokenType.Break));
                    continue;
                }

                if(Regex.IsMatch(s, "\\/")) //potential comment
                {
                    if(firstChar)
                    {
                        tokens.Add(new Token(s, Token.TokenType.Misc));
                        firstChar = false;
                    }
                    else
                    {
                        if(tokens[lastI].GetText().Trim() == "/") //preceded by a single slash, so comment!
                        {
                            tokens[lastI].Append(s);
                            tokens[lastI].SetType(Token.TokenType.Comment);
                            inComment = true;
                        }
                    }
                    continue;
                }

                if(Regex.IsMatch(s, "(\\+|\\-|\\*|\\/|\\%|\\=)"))
                {
                    tokens.Add(new Token(s, Token.TokenType.Operator));
                    continue;
                }

                if(Regex.IsMatch(s, "\\\"")) //quotes
                {
                    if(!inStringLiteral)
                    {
                        inStringLiteral = true;
                        tokens.Add(new Token(s, Token.TokenType.StringLiteral));
                    }
                    continue;
                }

                if(Regex.IsMatch(s, "\\\'")) //single quote
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
                if(Styler.IsTerm(tokenText))
                    tokenArray[i] = new Token(tokenText, Token.TokenType.Keyword);
            }

            //return
            return tokenArray;

        }

    }
}