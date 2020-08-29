using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using al_linter;

namespace al_linter
{
    public class alObject
    {
        public string content;
        public List<alField> alField = new List<alField>();
        public List<alVariable> alVariable = new List<alVariable>();
        public List<alFunction> alFunction { get; set; } = new List<alFunction>();
        public List<alLine> alLine = new List<alLine>();
        public int numberOfFunctions = 0;
        public alObjectType alObjectType = alObjectType.undefined;
        public int objectID;
        public double maintainabilityIndex = 171.0;
        public string name;
        public int lastLineNumber;
        public bool hasDrillDownPageId = false;
        public bool hasLookupPageId = false;
        public string debug;
        public alObject(string theText, bool checkhungariannotation, string hungariannotationoptions)
        {
            this.content = theText;
            int p = 0, LineNo = 0;
            var functionContent = "";
            var startsAt = 0;
            var firstTime = false;
            var inVariableSection = false;
            var inFieldsSection = false;
            var inFunction = false;
            var beginEnd = 0;
            var inComment = false;


            var lines = this.content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("//") || line.Trim().StartsWith("/*"))
                    inComment = true;

                if (line.Trim().EndsWith("*/"))
                    inComment = false;

                if (inComment)
                {
                    this.alLine.Add(new alLine(line));
                    LineNo++;
                    continue;
                }

                this.alLine.Add(new alLine(line));

                if (LineNo == 0)
                {
                    var objectDetails = Regex.Matches(line, @"((?<="")\w+ \w+(?="")|\w+)");// line.Split(new char[] { ' ' }); //TODO: Check if this works

                    foreach (Match part in objectDetails)
                    {
                        if (p == 0)
                        {
                            this.alObjectType = getObjectType(part.Value);
                        }
                        if (p == 1)
                        {
                            try
                            {
                                this.objectID = Int32.Parse(part.Value);
                            }
                            catch (System.FormatException e)
                            {
                                this.objectID = 0;
                            }
                        }
                        if (p == 2)
                        {
                            this.name = part.Value;
                        }
                        p++;
                    }
                }
                if (validProcedureName(line))
                {
                    inFunction = true;
                    inFieldsSection = false;
                    inVariableSection = false;
                    firstTime = true;
                    startsAt = LineNo;
                }
                if (line.Trim().ToUpper().Contains("BEGIN") || line.Trim().ToUpper().Contains("CASE "))
                {
                    beginEnd += 1;
                }
                if (line.Trim().ToUpper().Contains("END;") || line.Trim().ToUpper().Contains("END ELSE"))
                {
                    beginEnd -= 1;

                    if (beginEnd == 0)
                    {
                        inFunction = false;
                        var tempfunction = new alFunction(functionContent, startsAt, LineNo, checkhungariannotation, hungariannotationoptions);


                        if (tempfunction.maintainabilityIndex < this.maintainabilityIndex)
                        {
                            this.maintainabilityIndex = tempfunction.maintainabilityIndex;
                        }
                        this.alFunction.Add(tempfunction);
                        functionContent = "";
                        firstTime = false;
                    }
                }
                this.alLine[LineNo].isCode = beginEnd >= 1;
                if (firstTime)
                {
                    functionContent = functionContent + line + "\n";
                    if (line == "}")
                    {
                        this.lastLineNumber = LineNo;
                    }
                }
                if ((inVariableSection) && (LineNo > 1))
                {
                    if (line.IndexOf(":") != -1)
                    {
                        var names = line.Substring(0, line.IndexOf(":"));
                        if (names.Contains(", "))
                        {
                            var type = line.Substring(line.IndexOf(":"));
                            foreach (var name in names.Split(", "))
                            {
                                this.alVariable.Add(new alVariable(String.Format("{0}{1}",name,type), LineNo, true, checkhungariannotation, hungariannotationoptions));
                            }
                        }
                        else
                            this.alVariable.Add(new alVariable(line.ToUpper(), LineNo, true, checkhungariannotation, hungariannotationoptions));
                    }
                    else
                    {
                        inVariableSection = false;
                    }
                }

                if ((line.ToUpper().Trim() == ("VAR")) && (inFunction == false))
                {
                    inVariableSection = true;
                }
                if (line.ToUpper().Trim() == ("KEYS"))
                {
                    inFieldsSection = false;
                }
                if ((inFieldsSection) && (LineNo > 1))
                {
                    if (line.ToUpper().IndexOf("FIELD") != -1)
                    {
                        this.alField.Add(new alField(line, LineNo, hungariannotationoptions));
                    }
                }
                if (line.ToUpper().Trim() == ("FIELDS"))
                {
                    inFieldsSection = true;
                }
                if (line.Trim().ToUpper().IndexOf("DRILLDOWNPAGEID") != -1)
                {
                    this.hasDrillDownPageId = true;
                }
                if (line.Trim().ToUpper().IndexOf("LOOKUPPAGEID") != -1)
                {
                    this.hasLookupPageId = true;
                }
                LineNo++;
            }

            // Add LocalVariables for easier diagnostics

            this.alFunction.ForEach(alFunction =>
            {
                alFunction.alVariable.ForEach(alVariable =>
                {
                    LineNo = 0;
                    this.alLine.ForEach(alLine =>
                    {
                        if ((LineNo >= alFunction.startsAtLineNo) && (LineNo <= alFunction.endsAtLineNo) && (alLine.isCode) && (alVariable.isUsed == false))
                        {
                            alVariable.isUsed = alLine.upperCase.IndexOf(alVariable.name) >= 0;
                        };
                        LineNo++;
                    });


                    this.alVariable.Add(alVariable);
                });


            });

            this.numberOfFunctions = this.alFunction.Count;
        }

        string getContent()
        {
            return this.content;
        }

        int getNumberOfFunctions()
        {
            return Regex.Split(this.content, "PROCEDURE ").Length - 1;
        }

        string getCurrentFunction(int lineNumber)
        {
            var currentFuctionName = "Not in function";
            this.alFunction.ForEach(function =>
            {
                if ((function.startsAtLineNo < lineNumber) && (function.endsAtLineNo > lineNumber))
                {
                    currentFuctionName = function.name;
                }
            });
            return (currentFuctionName);
        }

        double getMaintainabilityIndex(int lineNumber)
        {
            var currentMaintainabilityIndex = 0.0;
            this.alFunction.ForEach(function =>
            {
                if ((function.startsAtLineNo < lineNumber) && (function.endsAtLineNo > lineNumber))
                {
                    currentMaintainabilityIndex = function.maintainabilityIndex;
                }
            });
            return (currentMaintainabilityIndex);
        }
        double getCyclomaticComplexity(int lineNumber)
        {
            var currentCyclomaticComplexity = 0.0;
            this.alFunction.ForEach(function =>
            {
                if ((function.startsAtLineNo < lineNumber) && (function.endsAtLineNo > lineNumber))
                {
                    currentCyclomaticComplexity = function.cycolomaticComplexity;
                }
            });
            return (currentCyclomaticComplexity);
        }

        static bool validProcedureName(string value)
        {
            if (value.Trim().ToUpper().StartsWith("PROCEDURE"))
            {
                return (true);
            }
            if (value.Trim().ToUpper().StartsWith("LOCAL PROCEDURE"))
            {
                return (true);
            }
            if (value.Trim().ToUpper().StartsWith("TRIGGER"))
            {
                return (true);
            }
            return false;
        }

        static alObjectType getObjectType(string str)
        {
            switch (str.Trim().ToUpper())
            {
                case "TABLE":
                    return (alObjectType.table);
                case "CODEUNIT":
                    return (alObjectType.codeunit);

            }
            return (alObjectType.undefined);
        }

        static string getCharsBefore(string str, string chr)
        {
            var index = str.IndexOf(chr);
            if (index != -1)
            {
                return (str.Substring(0, index));
            }
            return ("");
        }
        alSummary getSummary()
        {
            var mySummary = new alSummary(this);
            return (mySummary);
        }
    }

    public class alSummary
    {
        string content;
        public alSummary(alObject alObject)
        {
            this.content = alObject.name;
        }
    }

    public enum alObjectType
    {
        undefined = 0,
        table = 1,
        page = 2,
        report = 3,
        codeunit = 4,
        query = 5,
        xmlport = 6,
        menusuite = 7
    }
}