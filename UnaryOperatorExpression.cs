using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompiler
{
    class UnaryOperatorExpression : Expression
    {
        public string Operator { get; set; }
        public Expression Operand { get; set; }

        public override string ToString()
        {
            return Operator + Operand;
        }

        public override void Parse(TokensStack sTokens)
        {
            Token token = sTokens.Pop();
            if(token is Operator)
            {
                if (((Operator)token).Name == '!') Operator = "!";
                else if (((Operator)token).Name == '-') Operator = "-";
                else throw new SyntaxErrorException("expected unary operator, received " + token, token);
                Operand = Expression.Create(sTokens);
                Operand.Parse(sTokens);
                //Token tEnd = sTokens.Pop();// check for ";"
                //if (!(tEnd is Separator) || ((Separator)tEnd).Name != ';') throw new SyntaxErrorException("; expected, received " + tEnd, tEnd);
            }
            else throw new SyntaxErrorException("expected unary operator, received " + token, token);
        }
    }
}