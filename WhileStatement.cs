using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompiler
{
    public class WhileStatement : StatetmentBase
    {
        public Expression Term { get; private set; }
        public List<StatetmentBase> Body { get; private set; }

        public override void Parse(TokensStack sTokens)
        {
            //check for 'while'
            Token token = sTokens.Pop();
            if (!(token is Keyword) || ((Keyword)token).Name != "while")
                throw new SyntaxErrorException("expected 'while', received " + token, token);

            //check for '('
            token = sTokens.Pop();
            if (!(token is Parentheses) || ((Parentheses)token).Name != '(')
                throw new SyntaxErrorException("expected '(' for while condition, received " + token, token);

            //define and parse condition
            Term = Expression.Create(sTokens);
            Term.Parse(sTokens);

            //check for ')'
            token = sTokens.Pop();
            if (!(token is Parentheses) || ((Parentheses)token).Name != ')')
                throw new SyntaxErrorException("expected ')' for while condition, received " + token, token);

            //check for '{'
            token = sTokens.Pop();
            if (!(token is Parentheses) || ((Parentheses)token).Name != '{')
                throw new SyntaxErrorException("expected '{' for while body, received " + token, token);

            //create body for while
            Body = new List<StatetmentBase>();
            token = sTokens.Peek();
            StatetmentBase statetment = null;
            if (token is Keyword) statetment = StatetmentBase.Create(((Keyword)token).Name);
            while (statetment != null)
            {
                statetment.Parse(sTokens);
                Body.Add(statetment);
                token = sTokens.Peek();
                statetment = null;
                if (token is Keyword) statetment = StatetmentBase.Create(((Keyword)token).Name);
            }

            //check for '}'
            token = sTokens.Pop();
            if (!(token is Parentheses) || ((Parentheses)token).Name != '}')
                throw new SyntaxErrorException("expected '}' for while body, received " + token, token);
        }

        public override string ToString()
        {
            string sWhile = "while(" + Term + "){\n";
            foreach (StatetmentBase s in Body)
                sWhile += "\t\t\t" + s + "\n";
            sWhile += "\t\t}";
            return sWhile;
        }

    }
}
