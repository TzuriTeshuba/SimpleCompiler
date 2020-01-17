using System;
using System.Collections.Generic;

namespace SimpleCompiler
{
    public class FunctionCallExpression : Expression
    {
        public string FunctionName { get; private set; }
        public List<Expression> Args { get; private set; }

        public override void Parse(TokensStack sTokens)
        {
            Args = new List<Expression>();


            //function name
            Token token = sTokens.Pop();
            if( !(token is Identifier))
                throw new SyntaxErrorException("Expected function name Identifier, received " + token, token);
            FunctionName = ((Identifier)token).Name;

            //check for '('
            token = sTokens.Pop();
            if (!(token is Parentheses) || ((Parentheses)token).Name != '(')
                throw new SyntaxErrorException("Expected '(' for function args, received " + token, token);


            //define args until closing ')' for args
            token = sTokens.Peek();
            while( !(token is Parentheses) || (((Parentheses)token).Name != ')'))
            {
                Expression expression = Expression.Create(sTokens);
                expression.Parse(sTokens);
                Args.Add(expression);

                //expect additional arg if theres a comma
                if (sTokens.Count > 0 && sTokens.Peek() is Separator)//,
                {
                    token = sTokens.Pop();
                    if (((Separator)token).Name != ',') throw new SyntaxErrorException(@"Expected , (comma), received " + token, token);
                }
                token = sTokens.Peek();

            }

            //check for ')'
            token = sTokens.Pop();
            if (!(token is Parentheses) || ((Parentheses)token).Name != ')')
                throw new SyntaxErrorException("Expected ')' for function args, received " + token, token);



        }

        public override string ToString()
        {
            string sFunction = FunctionName + "(";
            for (int i = 0; i < Args.Count - 1; i++)
                sFunction += Args[i] + ",";
            if (Args.Count > 0)
                sFunction += Args[Args.Count - 1];
            sFunction += ")";
            return sFunction;
        }
    }
}