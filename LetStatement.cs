using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompiler
{
    public class LetStatement : StatetmentBase
    {
        public string Variable { get; set; }
        public Expression Value { get; set; }

        public override string ToString()
        {
            return "let " + Variable + " = " + Value + ";";
        }

        public override void Parse(TokensStack sTokens)
        {
            //check for "Let"
            Token token = sTokens.Pop();
            if (!(token is Keyword) || ((Keyword)token).Name != "let")
                throw new SyntaxErrorException("expected 'let' keyword, received " + token, token);

            // check for identifier
            token = sTokens.Pop();
            if(!(token is Identifier))
                throw new SyntaxErrorException("expected an identifier for a 'Let' statement, received " + token, token);
            Variable = ((Identifier)token).Name;

            //check for '='
            token = sTokens.Pop();
            if(!(token is Operator) || ((Operator)token).Name!='=' )
                throw new SyntaxErrorException("expected an '=' for a 'Let' statement, received " + token, token);

            //create and parse Expression
            Value = Expression.Create(sTokens);
            Value.Parse(sTokens);

            //check for';'
            token = sTokens.Pop();
            if (!(token is Separator) || ((Separator)token).Name != ';')
                throw new SyntaxErrorException("expected an ';' for 'Let' statement, received " + token, token);


        }

    }
}
