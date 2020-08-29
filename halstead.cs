using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace al_linter
{

    public class Halstead
    {
        public static int getHalstead(string businessLogic, bool unique)
        {

            var vocabulary = 0;
            var length = 0;
            var word = "";
            List<string> allWords = new List<string>();
            var useSpace = false;
            var usePeriod = false;
            var useComma = false;
            var useColon = false;
            var useSemiColon = false;
            var useParentheses = false;

            // TODO: String is one operator
            for (var i = 0; i < businessLogic.Length; i++)
            {
                if (businessLogic[i] == ' ')
                {
                    length++;
                    if (word != "")
                    {
                        allWords.Add(word);
                    }
                    word = "";
                    useSpace = true;
                }
                else if (businessLogic[i] == '.')
                {
                    length++;
                    if (word != "")
                    {
                        allWords.Add(word);
                    }
                    word = "";
                    usePeriod = true;
                }
                else if (businessLogic[i] == ',')
                {
                    length++;
                    if (word != "")
                    {
                        allWords.Add(word);
                    }
                    word = "";
                    useComma = true;
                }
                else if (businessLogic[i] == ';')
                {
                    length++;
                    if (word != "")
                    {
                        allWords.Add(word);
                    }
                    word = "";
                    useSemiColon = true;
                }
                else if (businessLogic[i] == ')')
                {
                    length++;
                    if (word != "")
                    {
                        allWords.Add(word);
                    }
                    word = "";
                    useParentheses = true;
                }
                else if (businessLogic[i] == '(')
                {
                    length++;
                    if (word != "")
                    {
                        allWords.Add(word);
                    }
                    word = "";
                }
                else if (businessLogic[i] == ':')
                {
                    length++;
                    if (word != "")
                    {
                        allWords.Add(word);
                    }
                    word = "";
                    useColon = true;
                }
                else
                    word = word + businessLogic[i];
            }

            if (unique)
            {
                if (useColon)
                {
                    vocabulary++;
                }
                if (useComma)
                {
                    vocabulary++;
                }
                if (useParentheses)
                {
                    vocabulary++;
                }
                if (usePeriod)
                {
                    vocabulary++;
                }
                if (useSemiColon)
                {
                    vocabulary++;
                }
                if (useSpace)
                {
                    vocabulary++;
                }

                var distinctWords = new HashSet<string>(allWords);

                vocabulary = vocabulary + distinctWords.Count;
                return vocabulary;
            }
            else
            {
                return length;
            }
        }
    }
}