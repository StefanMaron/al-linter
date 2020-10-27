using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using al_linter;

namespace al_linter
{
    public class alFunction
    {
        public string content;
        public string contentUpperCase;
        public string name { get; set; }
        public List<alVariable> alVariable = new List<alVariable>();
        public int numberOfLines;
        public int cycolomaticComplexity { get; set; }
        public double maintainabilityIndex { get; set; }
        public int distinctOperators;
        public int distinctOperands;
        public int numberOfOperators;
        public int numberOfOperands;
        public double vocabulary;
        public double Length;
        public double halsteadVolume;
        public string returnValue;
        public string businessLogic;
        public int startsAtLineNo { get; set; }
        public int endsAtLineNo { get; set; }
        public bool isHungarianNotation = false;
        public bool isLocal;
        public bool isInternal;
        public bool isTrigger;
        public string debug;
        public alFunction(string content, int startsAt, int endsAt, bool checkhungariannotation, string hungariannotationoptions)
        {
            this.content = content.Trim();
            if (this.content == "")
                return;

            this.startsAtLineNo = startsAt;
            this.endsAtLineNo = endsAt;
            this.contentUpperCase = this.content.ToUpper();
            this.numberOfLines = 0;
            this.name = getCharsBefore(this.content, "(");
            this.isLocal = this.name.ToUpper().StartsWith("LOCAL");
            this.isInternal = this.name.ToUpper().StartsWith("INTERNAL");
            this.isTrigger = this.name.ToUpper().StartsWith("TRIGGER");

            if (this.isTrigger)
            {
                this.name = this.name.Substring(8);
            }
            else if (this.isLocal)
            {
                this.name = this.name.Substring(16);
            }
            else if (this.isInternal)
            {
                this.name = this.name.Substring(19);
            }
            else
            {
                this.name = this.name.Substring(10);
            }
            this.name = this.name.Trim();
            //Regex.Replace(this.content,@"\/{2}.*?$","",RegexOptions.Multiline)

            var lines = Regex.Replace(this.content, @"\/{2}.*?$", "", RegexOptions.Multiline).ToUpper().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);// remove all lines with comments

            //lines = lines.filter(a => !a.Trim().StartsWith("//")) // remove all lines with comments

            var inCodeSection = false;
            var inVariableSection = false;

            // this.alVariable = [];
            this.businessLogic = "";
            int p = 0, LineNo = 0;

            foreach (var line in lines)
            {
                // Parameters
                if ((LineNo == 0) && (!line.Contains("()")) && (line != ""))
                {
                    this.debug = line;
                    var variableString = "";

                    if (line.IndexOf("(") > 0)
                        variableString = line.Substring(line.IndexOf("("));

                    if (variableString != "")
                    {
                        if (variableString.EndsWith(");") || variableString.EndsWith(")"))
                        { // Void
                            variableString = variableString.Substring(1, variableString.Length - 1);
                            variableString = variableString.Replace(");", ")");
                        }
                        else
                        { // Return Value
                            this.returnValue = variableString.Substring(variableString.IndexOf(")") + 1);
                            if (this.returnValue != "")
                            {
                                variableString = variableString.Substring(1, variableString.Length - this.returnValue.Length);
                                this.returnValue.Replace(":", "").Replace(";", "");
                            }
                        }
                        var variables = variableString.Split(new char[] { ';' });

                        foreach (var variable in variables)
                        {
                            var names = variable.Substring(0, variable.IndexOf(":"));
                            if (names.Contains(", "))
                            {
                                var type = variable.Substring(variable.IndexOf(":"));
                                foreach (var name in names.Split(", "))
                                {
                                    this.alVariable.Add(new alVariable(String.Format("{0}{1}", name, type), LineNo + startsAt, true, checkhungariannotation, hungariannotationoptions));
                                }
                            }
                            else
                                this.alVariable.Add(new alVariable(variable, LineNo + startsAt, false, checkhungariannotation, hungariannotationoptions));
                            this.alVariable[p].local = true;
                            this.alVariable[p].isParameter = true;
                            p++;
                        }
                    }
                    // breaddownProcedureVariables(line);
                }
                // Local variables
                if ((LineNo == 1) && (line.IndexOf("VAR") > 0))
                {
                    inVariableSection = true;
                }
                if ((inCodeSection) && (line.Length > 0))
                {
                    this.businessLogic = this.businessLogic + line.Trim();
                    this.numberOfLines++;
                }
                if (line.IndexOf("BEGIN") > 0)
                {
                    inVariableSection = false;
                    inCodeSection = true;
                }
                if ((inVariableSection) && (LineNo > 1))
                {
                    if (line != "")
                    {
                        var names = line.Substring(0, line.IndexOf(":"));
                        if (names.Contains(", "))
                        {
                            var type = line.Substring(line.IndexOf(":"));
                            foreach (var name in names.Split(", "))
                            {
                                this.alVariable.Add(new alVariable(String.Format("{0}{1}", name, type), LineNo + startsAt, true, checkhungariannotation, hungariannotationoptions));
                            }
                        }
                        else
                            this.alVariable.Add(new alVariable(line, LineNo + startsAt, false, checkhungariannotation, hungariannotationoptions));
                        p++;
                    }
                }
                LineNo++;
            }

            this.Length = Halstead.getHalstead(this.businessLogic, false);
            if (this.Length > 0)
            {
                this.vocabulary = Halstead.getHalstead(this.businessLogic, true);
                RegexOptions im = RegexOptions.IgnoreCase | RegexOptions.Multiline;
                RegexOptions ism = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Multiline;

                this.cycolomaticComplexity =
                    (Regex.Matches(this.contentUpperCase, @"if\s{1}", im).Count) +
                    (Regex.Matches(this.contentUpperCase, @"(?<!:):(?!:|=)(?=[\r\n])", ism).Count) +
                    (Regex.Matches(this.contentUpperCase, @"^\s *? (else|else begin | end else begin)\s *?$", im).Count);

                this.halsteadVolume = this.Length * Math.Log(this.vocabulary);
                this.maintainabilityIndex = Math.Round(Math.Max(0.0, (171.0 - 5.2 * Math.Log(this.halsteadVolume) - 0.23 * (this.cycolomaticComplexity) - 16.2 * Math.Log(this.numberOfLines)) * 100.0 / 171.0));
            }
            var hungarianOptions = new alHungarianOptions(hungariannotationoptions);

            foreach (var hungarianOption in hungarianOptions.alHungarianOption)
            {
                if ((hungarianOption.alType == "FUNCTION") && (this.isHungarianNotation == false))
                {
                    this.isHungarianNotation = (this.name.ToUpper().IndexOf(hungarianOption.abbreviation) != -1);
                }
            }

        }

        string getCharsBefore(string str, string chr)
        {
            var index = str.IndexOf(chr);
            if (index != -1)
            {
                return (str.Substring(0, index));
            }
            return ("");
        }
    }
}
