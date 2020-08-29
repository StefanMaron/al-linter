using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace al_linter
{

    public class alHungarianOption
    {
        public string content;
        public string alType;
        public string abbreviation;
        public alHungarianOption(string value)
        {
            if (value != "")
            {
                this.alType = value.Substring(0, value.IndexOf(",")).ToUpper();
                this.abbreviation = value.Substring(value.IndexOf(",") + 1).ToUpper();
            }
        }
    }

    public class alHungarianOptions
    {
        public string content;
        public List<alHungarianOption> alHungarianOption = new List<alHungarianOption>();
        public alHungarianOptions(string value)
        {
            string[] hungariannotationoptions = value.Split(new char[] { ';' });
            foreach (var item in hungariannotationoptions)
            {
                this.alHungarianOption.Add(new alHungarianOption(item));
            }
        }
    }

    public class alVariable
    {
        public string content;
        public string name;
        public bool local;
        public bool isParameter;
        public bool byRef;
        public string type;
        public string length;
        public int used;
        public string objectId;
        public bool objectIdIsANumber;
        public int lineNumber;
        public bool isHungarianNotation = false;
        public bool isTemporary = false;
        public string suggestedName;
        public bool isGlobal;
        public string label;
        public bool nameContainsSpecialCharacters = false;
        public bool isUsed;
        public string debug;
        public alVariable(string value, int lineNo, bool setIsGlobal, bool checkhungariannotation, string hungariannotationoptions)
        {
            this.content = value.Trim().Replace(";", "").Replace(")", "").Replace("(", "");
            this.isGlobal = setIsGlobal;
            this.isUsed = setIsGlobal;
            this.lineNumber = lineNo;
            if (this.content.StartsWith("VAR"))
            {
                this.content = this.content.Substring(4); // remove var
                this.byRef = true;
            }
            this.name = this.content.Substring(0, this.content.IndexOf(":")).TrimEnd();
            
            this.type = this.content.Substring(this.content.IndexOf(":") + 2);
            if (this.type.IndexOf(" ") > 0)
            {
                this.objectId = this.type.Substring(this.type.IndexOf(" ") + 1);
                if (this.objectId.ToUpper().IndexOf("TEMPORARY") != -1)
                {
                    this.isTemporary = true;
                    this.objectId = this.objectId.Substring(0, this.objectId.IndexOf(" "));
                }
                this.objectIdIsANumber = int.TryParse(this.objectId, out int output);
                this.type = this.type.Substring(0, this.type.IndexOf(" "));
            }

            if (this.content.EndsWith("]"))
            {
                this.length = this.content.Substring(this.content.IndexOf("[") + 1, this.content.IndexOf("]") - this.content.IndexOf("[") - 1);
            }
            if (this.type == "TEXTCONST" || this.type == "LABEL")
            {

                this.label = this.objectId;
                this.objectId = null;
            }

            if (checkhungariannotation)
            {
                var hungarianOptions = new alHungarianOptions(hungariannotationoptions);

                foreach (var hungarianOption in hungarianOptions.alHungarianOption)
                {
                    if ((hungarianOption.alType == this.type) && (this.isHungarianNotation == false))
                    {
                        if (isHungarianException(this.name) == false)
                        {
                            this.isHungarianNotation = (this.name.StartsWith(hungarianOption.abbreviation));
                        }
                    }
                }
            }
            this.nameContainsSpecialCharacters = this.checkNameForSpecialCharacters();
        }
        public bool hasWrongTempName()
        {
            if (this.isTemporary == false)
                return false;
            if (this.name.ToUpper().IndexOf("TEMP") == -1)
                if (this.name.ToUpper().IndexOf("ARGS") == -1)
                    if (this.name.ToUpper().IndexOf("ARGUMENTS") == -1)
                        if (this.name.ToUpper().IndexOf("BUFFER") == -1)
                        {
                            return true;
                        }
            return false;
        }

        // public bool alsoExistAsGlobalOrLocal(alObject: alObject)
        // {
        //     var found: boolean = false;
        //     alObject.alVariable.forEach(alVariable =>
        //     {
        //         if ((alVariable.name == this.name) && (alVariable.isGlobal != this.isGlobal))
        //         {
        //             found = true;
        //         }
        //     });
        //     return found;
        // }

        public bool checkNameForSpecialCharacters()
        {
            return Regex.IsMatch(this.name, @"[_~`!#$%\^&*+=\-\[\]\\';,\/{}|\\"":<>\?\s]");  //TODO: Check if this is correct            
        }


        bool isHungarianException(string value)
        {
            if (value.ToUpper() == "REC")
            {
                return true;
            }
            if (value.ToUpper() == "XREC")
            {
                return true;
            }
            return false;
        }
    }
}