using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCompiler
{
    public class SyntaxErrorException : Exception
    {
        public Token Token { get; set; }
        public SyntaxErrorException(string sMsg, Token tProblematic)
            : base("line: "+tProblematic.Line +" pos: "+tProblematic.Position+" "+ sMsg)
        {
            Token = tProblematic;
        }
    }
}
