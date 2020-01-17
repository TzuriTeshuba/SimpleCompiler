using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompiler
{
    public class VariableExpression : Expression
    {
        public string Name;

        public override void Parse(TokensStack sTokens)
        {
            Token token = sTokens.Pop();
            if (!(token is Identifier))
                throw new SyntaxErrorException("expected identifier, received " + token, token);
            Name = ((Identifier)token).Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
