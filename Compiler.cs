using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompiler
{
    public class Compiler
    {
        private char[] delimitters;
        private int numVirtualVars;



        public Compiler()
        {
            InitDelimitters();
            numVirtualVars = 0;
        }


        public List<VarDeclaration> ParseVarDeclarations(List<string> lVarLines)
        {
            List<VarDeclaration> lVars = new List<VarDeclaration>();
            for(int i = 0; i < lVarLines.Count; i++)
            {
                List<Token> lTokens = Tokenize(lVarLines[i], i);
                TokensStack stack = new TokensStack(lTokens);
                VarDeclaration var = new VarDeclaration();
                var.Parse(stack);
                lVars.Add(var);
            }
            return lVars;
        }


        public List<LetStatement> ParseAssignments(List<string> lLines)
        {
            List<LetStatement> lParsed = new List<LetStatement>();
            List<Token> lTokens = Tokenize(lLines);
            TokensStack sTokens = new TokensStack();
            for (int i = lTokens.Count - 1; i >= 0; i--)
                sTokens.Push(lTokens[i]);
            while(sTokens.Count > 0)
            {
                LetStatement ls = new LetStatement();
                ls.Parse(sTokens);
                lParsed.Add(ls);

            }
            return lParsed;
        }

 

        public List<string> GenerateCode(LetStatement aSimple, Dictionary<string, int> dSymbolTable)
        {
            List<string> lAssembly = new List<string>();
            //add here code for computing a single let statement containing only a simple expression

            return lAssembly;
        }


        public Dictionary<string, int> ComputeSymbolTable(List<VarDeclaration> lDeclerations)
        {
            Dictionary<string, int> dTable = new Dictionary<string, int>();
            //add here code to comptue a symbol table for the given var declarations
            //real vars should come before (lower indexes) than artificial vars (starting with _), and their indexes must be by order of appearance.
            //for example, given the declarations:
            //var int x;
            //var int _1;
            //var int y;
            //the resulting table should be x=0,y=1,_1=2
            //throw an exception if a var with the same name is defined more than once

            int numVars = 0;
            for(int i = 0; i < lDeclerations.Count; i++)
            {
                VarDeclaration varDec = lDeclerations[i];
                string name = varDec.Name;
                //need to check if variable already exists and throw exception if so??
                if (name[0] != '_')
                {
                    dTable.Add(name, numVars);
                    numVars++;
                }
            }
            for (int i = 0; i < lDeclerations.Count; i++)
            {
                VarDeclaration varDec = lDeclerations[i];
                string name = varDec.Name;
                //need to check if variable already exists and throw exception if so??
                if (name[0] == '_')
                {
                    dTable.Add(name, numVars);
                    numVars++;
                }
            }
            return dTable;
        }


        public List<string> GenerateCode(List<LetStatement> lSimpleAssignments, List<VarDeclaration> lVars)
        {
            List<string> lAssembly = new List<string>();
            Dictionary<string, int> dSymbolTable = ComputeSymbolTable(lVars);
            foreach (LetStatement aSimple in lSimpleAssignments)
                lAssembly.AddRange(GenerateCode(aSimple, dSymbolTable));
            return lAssembly;
        }

        public List<LetStatement> SimplifyExpressions(LetStatement s, List<VarDeclaration> lVars)
        {
            //add here code to simply expressins in a statement. 
            //add var declarations for artificial variables.
            //check that all vars appear in vardeclarations

            //
            //fix for allowing first expression to be binary
            //

            numVirtualVars = 0;
            Dictionary<Expression, int> registers = new Dictionary<Expression, int>();
            initVirtualIds(s.Value, registers);
            Stack<LetStatement> lets = initLetStatements(s.Value, registers);
            List<LetStatement> output = new List<LetStatement>();
            while (lets.Count > 0)
                output.Add(lets.Pop());

            return output;
        }



        //called after initVirtualIds(...)
        private Stack<LetStatement> initLetStatements(Expression expression, Dictionary<Expression,int> registers)
        {
            Stack<LetStatement> output = new Stack<LetStatement>();
            LetStatement letStatement = new LetStatement();
            letStatement.Variable = "_" + registers[expression];
            for (int i = 0; i < registers.Count; i++)
            {
                if (expression is VariableExpression || expression is NumericExpression)
                {
                    letStatement.Value = expression;
                }
                else
                {
                    BinaryOperationExpression binExp = (BinaryOperationExpression)expression;
                    BinaryOperationExpression value = new BinaryOperationExpression();
                    VariableExpression varExp1 = new VariableExpression();
                    VariableExpression varExp2 = new VariableExpression();
                    varExp1.Name = "_" + registers[binExp.Operand1];
                    varExp2.Name = "_" + registers[binExp.Operand2];
                    value.Operand1 = varExp1;
                    value.Operand2 = varExp2;
                    letStatement.Value = value;
                }
                output.Push(letStatement);
            }
            return output;
        }

        private void initVirtualIds(Expression expression, Dictionary<Expression,int> registers)
        {
            if(!(expression is BinaryOperationExpression))
            {
                numVirtualVars++;
                registers.Add(expression, numVirtualVars);
            }
            else
            {
                BinaryOperationExpression binaryExpression = (BinaryOperationExpression)expression;
                initVirtualIds(binaryExpression.Operand1, registers);
                initVirtualIds(binaryExpression.Operand2, registers);
            }
        }

        public List<LetStatement> SimplifyExpressions(List<LetStatement> ls, List<VarDeclaration> lVars)
        {
            List<LetStatement> lSimplified = new List<LetStatement>();
            foreach (LetStatement s in ls)
                lSimplified.AddRange(SimplifyExpressions(s, lVars));
            return lSimplified;
        }
        private LetStatement makeLetStatement(string vr1, string vr2, string oper)
        {
            return null;
        }

 
        public LetStatement ParseStatement(List<Token> lTokens)
        {
            TokensStack sTokens = new TokensStack();
            for (int i = lTokens.Count - 1; i >= 0; i--)
                sTokens.Push(lTokens[i]);
            LetStatement s = new LetStatement();
            s.Parse(sTokens);
            return s;
        }

 
        public List<Token> Tokenize(string sLine, int iLine)
        {
            //recycled code from Ex3.1
            List<Token> lTokens = new List<Token>();

            string codeLine = RemoveNotes(sLine);

            List<string> tokens = Split(codeLine, delimitters);
            int pos = 0;
            foreach (string tokenSTR in tokens)
            {
                Token token = MakeToken(tokenSTR, iLine, pos);
                if (token != null) lTokens.Add(token);
                pos += tokenSTR.Length;
            }
            return lTokens;
        }


        public List<Token> Tokenize(List<string> lCodeLines)
        {
            List<Token> lTokens = new List<Token>();
            for (int i = 0; i < lCodeLines.Count; i++)
            {
                string sLine = lCodeLines[i];
                List<Token> lLineTokens = Tokenize(sLine, i);
                lTokens.AddRange(lLineTokens);
            }
            return lTokens;
        }

        //
        //Ex3.1 code from here down
        //

        private List<string> Split(string s, char[] aDelimiters)
        {
            List<string> lTokens = new List<string>();
            while (s.Length > 0)
            {
                string sToken = "";
                int i = 0;
                for (i = 0; i < s.Length; i++)
                {
                    if (aDelimiters.Contains(s[i]))
                    {
                        if (sToken.Length > 0)
                            lTokens.Add(sToken);
                        lTokens.Add(s[i] + "");
                        break;
                    }
                    else
                        sToken += s[i];
                }
                if (i == s.Length)
                {
                    lTokens.Add(sToken);
                    s = "";
                }
                else
                    s = s.Substring(i + 1);
            }
            return lTokens;
        }

        public Token MakeToken(string s, int line, int pos)
        {
            if (Token.Statements.Contains(s)) return new Statement(s, line, pos);
            if (Token.VarTypes.Contains(s)) return new VarType(s, line, pos);
            if (Token.Constants.Contains(s)) return new Constant(s, line, pos);
            if (s.Length == 1 & Token.Operators.Contains(s[0])) return new Operator(s[0], line, pos);
            if (s.Length == 1 & Token.Parentheses.Contains(s[0])) return new Parentheses(s[0], line, pos);
            if (s.Length == 1 & Token.Separators.Contains(s[0])) return new Separator(s[0], line, pos);
            if (IsNumber(s)) return new Number(s, line, pos);
            if (s.Length > 0 & !s.Equals(" ") & s[0] != '\t') return new Identifier(s, line, pos);
            return null;

        }

        private string RemoveNotes(string codeLine)
        {
            if (codeLine.Contains("//"))
            {
                for (int i = 0; i < codeLine.Length - 1; i++)
                {
                    if (codeLine[i] == '/' & codeLine[i + 1] == '/') return codeLine.Substring(0, i);
                }
            }
            return codeLine;
        }

        public bool IsNumber(string tkn)
        {
            int num;
            Int32.TryParse(tkn, out num);
            if (num != 0) return true;
            else if (tkn.Equals("0")) return true;
            else return false;
        }

        private void InitDelimitters()
        {
            List<char> delims = new List<char>();
            foreach (char c in Token.Operators) delims.Add(c);
            foreach (char c in Token.Parentheses) delims.Add(c);
            foreach (char c in Token.Separators) delims.Add(c);
            delims.Add(' ');
            delims.Add('\t');
            delimitters = delims.ToArray<char>();

        }

    }
}
