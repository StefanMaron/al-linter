using System;
using System.Text.RegularExpressions;

namespace al_linter
{

    public class alLine
    {
        public string content;
        public bool isCode;
        public string upperCase;
        public string trim;
        public alLine(string content)
        {
            this.content = content;
            this.trim = content.Trim();
            this.upperCase = this.trim.ToUpper();
        }
    }
}