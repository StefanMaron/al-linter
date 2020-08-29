using System;
using System.Text.RegularExpressions;
using al_linter;

namespace al_linter
{

    public class alField
    {
        public string content;
        public string name;
        public string id;
        public string type;
        public bool editable;
        public bool enabled;
        public int lineNumber;
        public bool isHungarianNotation = false;
        public alField(string content, int startsAt, string newHungariannotationoptions)
        {
            this.content = content.Trim();
            this.lineNumber = startsAt;

            string[] lines = this.content.Split(new Char[] { ';' });

            int i = 0;
            foreach (var line in lines)
            {
                if (i == 0)
                { // Id
                    this.id = line.ToUpper().Replace("FIELD(", "");
                }
                if (i == 1)
                { // Name
                    this.name = line.Replace("\"", ""); //TODO: Check if this is correct
                    this.name = this.name.Replace("\"", "");
                }
                if (i == 2)
                { /// Tyoe
                    this.type = line.Replace(")", "");
                }
                i++;
            }
            var hungarianOptions = new alHungarianOptions(newHungariannotationoptions);

            foreach (var hungarianOption in hungarianOptions.alHungarianOption)
            {
                if ((hungarianOption.alType == "FIELD") && (this.isHungarianNotation == false) && (this.name != ""))
                {
                    this.isHungarianNotation = (this.name.ToUpper().IndexOf(hungarianOption.abbreviation) != -1);
                }
            }
        }
    }

}