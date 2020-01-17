using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCompiler
{
    public abstract class StatetmentBase : JackProgramElement
    {
        public static StatetmentBase Create(string sName)
        {
            if (sName == "let")
                return new LetStatement();
            if (sName == "if")
                return new IfStatement();
            if (sName == "return")
                return new ReturnStatement();
            if (sName == "while")
                return new WhileStatement();
            return null;
        }
        public static StatetmentBase Create(Token t)
        {
            if (t is Statement)
            {
                Statement s = (Statement)t;

                StatetmentBase se = Create(s.Name);
                if (se == null)
                    throw new SyntaxErrorException("Expected statement type", t);
                return se;
            }
            throw new SyntaxErrorException("Expected statement type", t);
        }
    }
}
