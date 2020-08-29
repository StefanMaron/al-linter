using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using al_linter;

namespace al_linter
{
    public class AlLintSettings
    {
        public bool enabled { get; set; }
        public bool statusbar { get; set; }
        public bool checkcommit { get; set; }
        public bool checkhungariannotation { get; set; }
        public bool checkspecialcharactersinvariablenames { get; set; }
        public string hungariannotationoptions { get; set; }
        public bool checkdrilldownpageid { get; set; }
        public bool checklookuppageid { get; set; }
        public int maxnumberoffunctionlines { get; set; }

        public AlLintSettings()
        {
            this.enabled = true;
            this.statusbar = true;
            this.checkcommit = true;
            this.checkhungariannotation = true;
            this.checkspecialcharactersinvariablenames = true;
            this.hungariannotationoptions = "Record,Rec;Integer,Int;Code,Cod;Function,Func;Codeunit,Cdu;Page,Pag;Text,Txt;Field,Fld";
            this.checkdrilldownpageid = true;
            this.checklookuppageid = true;
            this.maxnumberoffunctionlines = 40;
        }
    }

    public class alAnalyzer
    {
        static void Main(string[] args)
        {
            string FileContent;
            AlLintSettings settings;
            if (args[0].ToUpper() == "GETOBJECT")
            {
                FileContent = GetFileContent(args[1]);
                settings = JsonSerializer.Deserialize<AlLintSettings>(args[2]);
                Console.Write(JsonSerializer.Serialize(new alObject(FileContent, settings.checkhungariannotation, settings.hungariannotationoptions)));
            }
            else
            {
                settings = JsonSerializer.Deserialize<AlLintSettings>(args[1]);
                FileContent = GetFileContent(args[0]);

                var diagnostics = validateAlDocument(FileContent, settings);

                Console.Write(JsonSerializer.Serialize(diagnostics.messages));
            }
            
            static string GetFileContent(string arg)
            {
                Uri FileUri;
                string FileContent;
                if (Uri.TryCreate(arg, UriKind.Absolute, out FileUri))
                    FileContent = System.IO.File.ReadAllText(String.Format(@"{0}", FileUri.LocalPath));
                else
                    FileContent = System.IO.File.ReadAllText(String.Format(@"{0}", arg));
                return FileContent;
            }
        }
        static public alDiagnostics validateAlDocument(string TextDocument, AlLintSettings settings)
        {
            var diagnostics = new alDiagnostics();

            var myObject = new alObject(TextDocument, false, "");

            if (settings.checkdrilldownpageid)
                diagnostics.checkForMissingDrillDownPageId(myObject);

            if (settings.checklookuppageid)
                diagnostics.checkForMissingLookupPageId(myObject);

            var lines = TextDocument.ToUpper().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var CurrentLineNo = 0;
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("//"))
                {
                    CurrentLineNo++;
                    continue;
                }

                if (myObject.alLine[CurrentLineNo].isCode)
                {
                    if (settings.checkcommit)
                        diagnostics.checkForCommit(line, CurrentLineNo);

                    diagnostics.checkForWithInTableAndPage(line, myObject, CurrentLineNo);
                }
                foreach (var alFunction in myObject.alFunction)
                {
                    if (alFunction.startsAtLineNo == CurrentLineNo)
                    {
                        diagnostics.checkFunctionForNoOfLines(alFunction, line, CurrentLineNo, settings.maxnumberoffunctionlines);
                        diagnostics.checkFunctionReservedWord(alFunction, line, CurrentLineNo);

                        if (settings.checkhungariannotation)
                            diagnostics.checkFunctionForHungarianNotation(alFunction, line, CurrentLineNo);
                    }
                }

                foreach (var alField in myObject.alField)
                {
                    if (alField.lineNumber == CurrentLineNo)
                    {
                        if (settings.checkhungariannotation)
                            diagnostics.checkFieldForHungarianNotation(alField, line, CurrentLineNo);
                    }
                }

                foreach (var alVariable in myObject.alVariable)
                {
                    if (alVariable.lineNumber == CurrentLineNo)
                    {
                        if (settings.checkhungariannotation)
                            diagnostics.checkVariableForHungarianNotation(alVariable, line, CurrentLineNo);

                        diagnostics.checkVariableForIntegerDeclaration(alVariable, line, CurrentLineNo);
                        diagnostics.checkVariableForTemporary(alVariable, line, CurrentLineNo);
                        diagnostics.checkVariableForReservedWords(alVariable, line, CurrentLineNo);

                        if (settings.checkspecialcharactersinvariablenames)
                            diagnostics.checkVariableNameForUnderScore(alVariable, line, CurrentLineNo);
                    }
                }
                CurrentLineNo++;
            }
            return diagnostics;
        }
    }
}