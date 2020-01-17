using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompiler
{
    public class IfStatement : StatetmentBase
    {
        public Expression Term { get; private set; }
        public List<StatetmentBase> DoIfTrue { get; private set; }
        public List<StatetmentBase> DoIfFalse { get; private set; }

        public override void Parse(TokensStack sTokens)
        {
            //init lists
            DoIfTrue = new List<StatetmentBase>();
            DoIfFalse = new List<StatetmentBase>();
            //check for 'if'
            Token token = sTokens.Pop();
            if (!(token is Keyword) || ((Keyword)token).Name != "if")
                throw new SyntaxErrorException("expected 'if', received " + token, token);

            //check for '('
            token = sTokens.Pop();
            if (!(token is Parentheses) || ((Parentheses)token).Name != '(')
                throw new SyntaxErrorException("expected '(' for if condition, received " + token, token);

            //define and parse condition
            Term = Expression.Create(sTokens);
            Term.Parse(sTokens);

            //check for ')'
            token = sTokens.Pop();
            if (!(token is Parentheses) || ((Parentheses)token).Name != ')')
                throw new SyntaxErrorException("expected ')' for if condition, received " + token, token);

            //check for '{'
            token = sTokens.Pop();
            if (!(token is Parentheses) || ((Parentheses)token).Name != '{')
                throw new SyntaxErrorException("expected '{' for if body, received " + token, token);

            //create body for 'if true'
            token = sTokens.Peek();
            StatetmentBase statetment=null;
            if (token is Keyword) statetment = StatetmentBase.Create(((Keyword)token).Name);
            while(statetment != null)
            {
                statetment.Parse(sTokens);
                DoIfTrue.Add(statetment);
                token = sTokens.Peek();
                statetment = null;
                if (token is Keyword) statetment = StatetmentBase.Create(((Keyword)token).Name);
            }

            //check for '}'
            token = sTokens.Pop();
            if (!(token is Parentheses) || ((Parentheses)token).Name != '}')
                throw new SyntaxErrorException("expected '}' for if body, received " + token, token);

            //check for 'else'
            token = sTokens.Peek();
            if ((token is Keyword) && ((Keyword)token).Name == "else")
            {
                sTokens.Pop();

                //check for '{'
                token = sTokens.Pop();
                if (!(token is Parentheses) || ((Parentheses)token).Name != '{')
                    throw new SyntaxErrorException("expected '{' for else body, received " + token, token);

                //create body for 'else'
                statetment = null;
                token = sTokens.Peek();
                if (token is Keyword) statetment = StatetmentBase.Create(((Keyword)token).Name);
                while (statetment != null)
                {
                    statetment.Parse(sTokens);
                    DoIfFalse.Add(statetment);
                    token=sTokens.Peek();
                    statetment = null;
                    if (token is Keyword) statetment = StatetmentBase.Create(((Keyword)token).Name);
                }

                //check for '}'
                token = sTokens.Pop();
                Console.WriteLine(this);
                if (!(token is Parentheses) || ((Parentheses)token).Name != '}')
                    throw new SyntaxErrorException("expected '}' for else body, received " + token, token);
            }


        }

        public override string ToString()
        {
            string sIf = "if(" + Term + "){\n";
            foreach (StatetmentBase s in DoIfTrue)
                sIf += "\t\t\t" + s + "\n";
            sIf += "\t\t}";
            if (DoIfFalse.Count > 0)
            {
                sIf += "else{";
                foreach (StatetmentBase s in DoIfFalse)
                    sIf += "\t\t\t" + s + "\n";
                sIf += "\t\t}";
            }
            return sIf;
        }

    }
}
