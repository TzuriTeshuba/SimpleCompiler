using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompiler
{
    public class BinaryOperationExpression : Expression
    {
        public string Operator { get;  set; }
        public Expression Operand1 { get;  set; }
        public Expression Operand2 { get;  set; }

        public override string ToString()
        {
            return "(" + Operand1 + " " + Operator + " " + Operand2 + ")";
        }

        //
        //recode Operand1, shitty code
        //
        public override void Parse(TokensStack sTokens)
        {

            //check for '('
            Token t = sTokens.Pop();
            if(!(t is Parentheses) || ((Parentheses)t).Name!='(')
                throw new SyntaxErrorException("'(' expected, received " + t, t);

            //define operand1
            Operand1 = Expression.Create(sTokens);
            Operand1.Parse(sTokens);


            //define operator
            t = sTokens.Pop();
            if(t is Operator)
            {
                if (((Operator)t).Name == '+') Operator = "+";
                else if (((Operator)t).Name == '-') Operator = "-";
                else if (((Operator)t).Name == '<') Operator = "<";
                else if (((Operator)t).Name == '>') Operator = ">";
                else if (((Operator)t).Name == '=') Operator = "=";
                else if (((Operator)t).Name == '|') Operator = "|";
                else if (((Operator)t).Name == '&') Operator = "&";
                else if (((Operator)t).Name == '*') Operator = "*";
                else if (((Operator)t).Name == '/') Operator = "/";



                else throw new SyntaxErrorException("Binary operator expected, received " + t, t);
            }
            else throw new SyntaxErrorException("Binary operator expected, received " + t, t);

            //define operand2
            Operand2 = Expression.Create(sTokens);
            Operand2.Parse(sTokens);

            //check for ')'
            t = sTokens.Pop();
            if (!(t is Parentheses) || ((Parentheses)t).Name != ')')
                throw new SyntaxErrorException("')' expected, received " + t, t);




        }
    }
}
