using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using al_linter;

namespace al_linter
{

    public struct LinePos
    {
        public int lineNo { get; set; }
        public int characterPos { get; set; }

        public LinePos(int lineNo, int characterPos)
        {
            this.lineNo = lineNo;
            this.characterPos = characterPos;
        }
    }

    public enum severityLevel
    {
        Error = 1,
        Warning = 2,
        Information = 3,
        Hint = 4
    }

    public struct diagnosticMessage
    {
        public int severity { get; set; }
        public LinePos start { get; set; }
        public LinePos end { get; set; }
        public string message { get; set; }
        public string source { get; set; }

        public diagnosticMessage(severityLevel severity, LinePos start, LinePos end, string message, string source)
        {
            this.severity = (int)severity;
            this.start = start;
            this.end = end;
            this.message = message;
            this.source = source;
        }
    }

    public class alDiagnostics
    {
        public List<diagnosticMessage> messages { get; set; } = new List<diagnosticMessage>();

        public void checkForCommit(string line, int lineNo)
        {
            var keyword = "COMMIT;";
            var index = 0;

            index = line.ToUpper().IndexOf(keyword);

            if (index == -1)
            {
                keyword = "COMMIT();";
                index = line.ToUpper().IndexOf(keyword);
            };

            if (index >= 0)
            {
                this.messages.Add(new diagnosticMessage(
                        severityLevel.Warning,
                        new LinePos(lineNo, index),
                        new LinePos(lineNo, index + keyword.Length),
                        "A Commit() is an indication of poorly structured code(AL Lint Clean Code)",
                        "AlLint"
                    ));
            }
        }

        public void checkForWithInTableAndPage(string line, alObject myObject, int lineNo)
        {
            if (myObject.alObjectType == alObjectType.table || myObject.alObjectType == alObjectType.page)
            {
                var index = line.ToUpper().IndexOf("WITH ");
                if (index >= 0)
                {
                    this.messages.Add(new diagnosticMessage(
                        severityLevel.Warning,
                        new LinePos(lineNo, index),
                        new LinePos(lineNo, index + 4),
                        String.Format("A {0} should not be used in a table or page object (AL Lint Clean Code)", line.Substring(index, 4)),
                        "AlLint"
                    ));
                }
            }
        }

        public void checkForMissingDrillDownPageId(alObject myObject)
        {
            if (myObject.alObjectType == alObjectType.table && !myObject.hasDrillDownPageId)
            {
                this.messages.Add(new diagnosticMessage(
                    severityLevel.Warning,
                    new LinePos(0, 0),
                    new LinePos(0, 5),
                    "DrillDownPageID should be in a table (AL Lint Clean Code)",
                    "AlLint"
                ));
            }
        }

        public void checkForMissingLookupPageId(alObject myObject)
        {
            if (myObject.alObjectType == alObjectType.table && !myObject.hasLookupPageId)
            {
                this.messages.Add(new diagnosticMessage(
                    severityLevel.Warning,
                    new LinePos(0, 0),
                    new LinePos(0, 5),
                    "LookupPageID should be set in a table(AL Lint Clean Code)",
                    "AlLint"
                ));
            }
        }

        public void checkFunctionForHungarianNotation(alFunction alFunction, string line, int lineNo)
        {
            if (alFunction.isHungarianNotation)
            {
                var index = line.ToUpper().IndexOf(alFunction.name.ToUpper());
                if (index >= 0)
                {
                    this.messages.Add(new diagnosticMessage(
                        severityLevel.Warning,
                        new LinePos(lineNo, index),
                        new LinePos(lineNo, index + alFunction.name.Length),
                        String.Format("{0} has Hungarian Notation (AL Lint Clean Code)", line.Substring(index, alFunction.name.Length)),
                        "AlLint"
                    ));
                }
            }
        }

        public void checkFunctionReservedWord(alFunction alFunction, string line, int lineNo)
        {
            if (isReserved.check(alFunction.name.ToUpper()))
            {
                var index = line.ToUpper().IndexOf(alFunction.name.ToUpper());
                if (index >= 0)
                {
                    this.messages.Add(new diagnosticMessage(
                        severityLevel.Warning,
                        new LinePos(lineNo, index),
                        new LinePos(lineNo, index + alFunction.name.Length),
                        String.Format("{0} contains a reserved word (AL Lint Clean Code)", line.Substring(index, alFunction.name.Length)),
                        "AlLint"
                    ));
                }
            }
        }



        public void checkVariableForHungarianNotation(alVariable alVariable, string line, int lineNo)
        {
            if ((alVariable.isHungarianNotation))
            {
                var index = line.ToUpper().IndexOf(alVariable.name.ToUpper());
                if (index >= 0)
                {
                    this.messages.Add(new diagnosticMessage(
                        severityLevel.Warning,
                        new LinePos(lineNo, index),
                        new LinePos(lineNo, index + alVariable.name.Length),
                        String.Format("{0} has Hungarian Notation (AL Lint Clean Code)", line.Substring(index, alVariable.name.Length)),
                        "AlLint"
                    ));
                }
            }
        }

        public void checkVariableForIntegerDeclaration(alVariable alVariable, string line, int lineNo)
        {
            if ((alVariable.objectIdIsANumber) && (alVariable.type == "Record"))
            {
                var index = line.ToUpper().IndexOf(alVariable.objectId.ToUpper());
                if (index >= 0)
                {
                    this.messages.Add(new diagnosticMessage(
                        severityLevel.Warning,
                        new LinePos(lineNo, index),
                        new LinePos(lineNo, index + alVariable.name.Length),
                        "Objects should be declared by name, not by number(AL Lint Clean Code)",
                        "AlLint"
                    ));
                }
            }
        }

        public void checkVariableForTemporary(alVariable alVariable, string line, int lineNo)
        {
            if ((alVariable.isTemporary) && (alVariable.hasWrongTempName()))
            {
                var index = line.ToUpper().IndexOf(alVariable.objectId.ToUpper());
                if (index >= 0)
                {
                    this.messages.Add(new diagnosticMessage(
                        severityLevel.Warning,
                        new LinePos(lineNo, index),
                        new LinePos(lineNo, index + alVariable.name.Length),
                        "Temporary variables should be named TEMP, BUFFER, ARGS or ARGUMENTS as prefix or suffix (AL Lint Clean Code)",
                        "AlLint"
                    ));
                }
            }
        }

        public void checkVariableForReservedWords(alVariable alVariable, string line, int lineNo)
        {
            if (isReserved.check(alVariable.name.ToUpper()))
            {
                var index = line.ToUpper().IndexOf(alVariable.name.ToUpper());
                if (index >= 0)
                {
                    this.messages.Add(new diagnosticMessage(
                        severityLevel.Warning,
                        new LinePos(lineNo, index),
                        new LinePos(lineNo, index + alVariable.name.Length),
                        String.Format("{0} contains a reserved word (AL Lint Clean Code)", line.Substring(index, alVariable.name.Length)),
                        "AlLint"
                    ));
                }
            }
        }

        public void checkFieldForHungarianNotation(alField alField, string line, int lineNo)
        {
            if (alField.isHungarianNotation)
            {
                var index = line.ToUpper().IndexOf(alField.name.ToUpper());
                if (index >= 0)
                {
                    this.messages.Add(new diagnosticMessage(
                        severityLevel.Warning,
                        new LinePos(lineNo, index),
                        new LinePos(lineNo, index + alField.name.Length),
                        String.Format("{0} has Hungarian Notation (AL Lint Clean Code)", line.Substring(index, alField.name.Length)),
                        "AlLint"
                    ));
                }
            }
        }

        public void checkVariableNameForUnderScore(alVariable alVariable, string line, int lineNo)
        {
            if (alVariable.nameContainsSpecialCharacters)
            {
                var index = line.ToUpper().IndexOf(alVariable.name.ToUpper());
                if (index >= 0)
                {
                    this.messages.Add(new diagnosticMessage(
                        severityLevel.Warning,
                        new LinePos(lineNo, index),
                        new LinePos(lineNo, index + alVariable.name.Length),
                        "Variable names should not contain special characters or whitespaces in their name(AL Lint Clean Code)",
                        "AlLint"
                    ));
                }
            }
        }

        public void checkFunctionForNoOfLines(alFunction alFunction, string line, int lineNo, int maxnumberoffunctionlines)
        {
            if (maxnumberoffunctionlines == 0)
                return;

            var index = line.ToUpper().IndexOf(alFunction.name.ToUpper());
            if ((alFunction.numberOfLines) > maxnumberoffunctionlines && index >= 0)
            {
                this.messages.Add(new diagnosticMessage(
                    severityLevel.Warning,
                    new LinePos(lineNo, index),
                    new LinePos(lineNo, index + alFunction.name.Length),
                    String.Format("Functions should generally not exceed {0} lines but it has {1} lines.Time to refactor! (AL Lint Clean Code)", maxnumberoffunctionlines, alFunction.numberOfLines),
                    "AlLint"
                ));
            }
        }
    }
}













