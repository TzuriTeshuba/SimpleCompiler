﻿using System;
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
        private const string OPERAND1 = "OPERAND1";
        private const string OPERAND2 = "OPERAND2";
        private const string RESULT = "RESULT";


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

        private List<string> GenerateCodeForUnaryAssignment(LetStatement aSimple, Dictionary<string, int> dSymbolTable)
        {
            List<string> lAssembly = new List<string>();
            //add here code for computing a single let statement containing only a simple expression
            Expression value = aSimple.Value;
            if (value is NumericExpression)
            {
                lAssembly.Add("//Numeric Value " + aSimple);

                NumericExpression numExp = (NumericExpression)value;

                //next 8 lines rep @var
                lAssembly.Add("@LCL");
                lAssembly.Add("D=M");
                lAssembly.Add("@" + dSymbolTable[aSimple.Variable]);
                lAssembly.Add("D=A+D");//D=a's address
                lAssembly.Add("@_a");
                lAssembly.Add("M=D");
                lAssembly.Add("@"+RESULT);
                lAssembly.Add("M=D");

                lAssembly.Add("@" + numExp.Value);
                lAssembly.Add("D=A");
                lAssembly.Add("@_a");
                lAssembly.Add("A=M");
                lAssembly.Add("M=D");
                lAssembly.Add("@"+RESULT);
                lAssembly.Add("M=D");

            }
            else if (value is VariableExpression)
            { 

                lAssembly.Add("//Variable value" + aSimple);

                //Example: let a = b;
                //LCL[a]=LCL[b]
                VariableExpression valExp = (VariableExpression)value;
                int valuesIndex = dSymbolTable[valExp.ToString()];

                //first store value of LCL[b] in RESULT
                lAssembly.Add("@LCL");
                lAssembly.Add("D=M");
                lAssembly.Add("@" + dSymbolTable[valExp.ToString()]);
                lAssembly.Add("A=D+A");
                lAssembly.Add("D=M");
                lAssembly.Add("@"+RESULT);
                lAssembly.Add("M=D");

                //next store a's address in virtual register "_a"
                lAssembly.Add("@LCL");
                lAssembly.Add("D=M");
                lAssembly.Add("@" + dSymbolTable[valExp.ToString()]);
                lAssembly.Add("D=D+A");
                lAssembly.Add("@_a");
                lAssembly.Add("M=D");

                //update "a"
                lAssembly.Add("@"+RESULT);
                lAssembly.Add("D=M");
                lAssembly.Add("@_a");
                lAssembly.Add("A=M");
                lAssembly.Add("M=D");

            }

            return lAssembly;
        }




        public List<string> GenerateCode(LetStatement aSimple, Dictionary<string, int> dSymbolTable)
        {


            List<string> lAssembly = new List<string>();
            lAssembly.Add("//generation started" + aSimple);

            //add here code for computing a single let statement containing only a simple expression
            Expression value = aSimple.Value;
            string variable = aSimple.Variable;
            if(value is NumericExpression | value is VariableExpression)
            {
                lAssembly.AddRange( GenerateCodeForUnaryAssignment(aSimple, dSymbolTable));
            }
            else if(value is BinaryOperationExpression)
            {
                lAssembly.Add("//Binary Expression"+ aSimple);
                //
                //Example: a = b <op> c
                //first let a=b
                //next  let a=a <op> c
                //
                BinaryOperationExpression binExp = (BinaryOperationExpression)value;
                Expression operand1 = binExp.Operand1;
                Expression operand2 = binExp.Operand2;
                LetStatement letAEqualB = makeLetStatement(variable, operand1);

                //let a=b
                lAssembly.AddRange(GenerateCodeForUnaryAssignment(letAEqualB, dSymbolTable));
                lAssembly.Add("@"+RESULT);
                lAssembly.Add("D=M");
                lAssembly.Add("@"+OPERAND1);
                lAssembly.Add("M=D");


                //let a = a <op> c
                if (operand2 is NumericExpression)
                {
                    lAssembly.Add("//Opearand 2 is Numeric " + aSimple);

                    NumericExpression num = (NumericExpression)operand2;
                    lAssembly.Add("@"+num.Value);
                    lAssembly.Add("D=A");
                    lAssembly.Add("@_a");
                    lAssembly.Add("M=M"+binExp.Operator+"D");
                    lAssembly.Add("D=M");
                    lAssembly.Add("@"+RESULT);
                    lAssembly.Add("M=D");
                }
                else if(operand2 is VariableExpression)
                {
                    lAssembly.Add("//Opearand 2 is a Variable " + aSimple);

                    VariableExpression c = (VariableExpression)operand2;
                    lAssembly.Add("@LCL");
                    lAssembly.Add("D=M");
                    lAssembly.Add("@"+dSymbolTable[c.Name]);
                    lAssembly.Add("D=M");
                    lAssembly.Add("@"+ dSymbolTable[variable]);
                    lAssembly.Add("M=M"+binExp.Operator+"D");
                    lAssembly.Add("D=M");

                    lAssembly.Add("@"+RESULT);
                    lAssembly.Add("M=D");

                }

            }


            return lAssembly;
        }


        public Dictionary<string, int> ComputeSymbolTable(List<VarDeclaration> lDeclerations)
        {


            const int virtaulsAddress = 100;
            Dictionary<string, int> dTable = new Dictionary<string, int>();
            //add here code to comptue a symbol table for the given var declarations
            //real vars should come before (lower indexes) than artificial vars (starting with _), and their indexes must be by order of appearance.
            //for example, given the declarations:
            //var int x;
            //var int _1;
            //var int y;
            //the resulting table should be x=0,y=1,_1=2
            //throw an exception if a var with the same name is defined more than once
            dTable.Add(OPERAND1,96);
            dTable.Add(OPERAND2, 97);
            dTable.Add("_a", 98);
            dTable.Add(RESULT, 99);
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
            for (int i = 0; i <= numVirtualVars; i++)
            {
                dTable.Add("_" + i, virtaulsAddress + i);
                
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
            List<LetStatement> output = new List<LetStatement>();
            //first check that all vars in expression were declared;
            List<string> varNames = new List<string>();
            foreach (VarDeclaration varDec in lVars) varNames.Add(varDec.Name);
            if (!varNames.Contains(s.Variable)) throw new SyntaxErrorException(" the variable " + s.Variable + " was never defined in expression " + s, new Token());
            CheckThatAllVariablesWereDeclared(s.Value, varNames);

            if (s.Value is BinaryOperationExpression)
            {
                //init data
                numVirtualVars = 0;
                Dictionary<Expression, int> registers = new Dictionary<Expression, int>();
                initVirtualIds(s.Value, registers);
                Stack<LetStatement> lets = new Stack<LetStatement>();
                initLetStatements(s.Value, registers, lets);


                BinaryOperationExpression binExp = (BinaryOperationExpression)(s.Value);
                BinaryOperationExpression binVal = new BinaryOperationExpression();

                // let OPERATOR1 = _xxx
                VariableExpression var1 = makeVariable("_" + registers[binExp.Operand1]);
                binVal.Operand1 = var1;
                LetStatement letOP1 = makeLetStatement(OPERAND1, var1);

                // let OPERATOR2 = _yyy
                VariableExpression var2 = makeVariable("_" + registers[binExp.Operand2]);
                binVal.Operand2 = var2;
                LetStatement letOP2 = makeLetStatement(OPERAND2, var2);

                // let RESULT= _1 and variable=RESULT
                LetStatement letResult =  makeLetStatement(RESULT, makeVariable("_" + registers[s.Value]));
                LetStatement letVar = makeLetStatement(s.Variable, makeVariable(RESULT));



                while (lets.Count > 0)
                    output.Add(lets.Pop());

                output.Add(letOP1);
                output.Add(letOP2);
                output.Add(letResult);
                output.Add(letVar);

            }
            else
            {
                //need to put in RESULT???
                output.Add(s);
            }

            return output;
        }
        private bool CheckThatAllVariablesWereDeclared(Expression expression, List<string> varNames)
        {
            bool output = true;
            if (expression is VariableExpression)
            {
                output = varNames.Contains(expression.ToString());
            }
            else if(expression is BinaryOperationExpression)
            {
                BinaryOperationExpression binExp = (BinaryOperationExpression)expression;
                output = ((CheckThatAllVariablesWereDeclared(binExp.Operand1, varNames)) && (CheckThatAllVariablesWereDeclared(binExp.Operand2, varNames)));
            }
            if (!output) throw new SyntaxErrorException("the variable " + expression + " was never declared", new Token());
            return true;
        }

        public VariableExpression makeVariable(string s)
        {
            VariableExpression output = new VariableExpression();
            output.Name = s;
            return output;
        }
        public LetStatement makeLetStatement(string var, Expression value)
        {
            LetStatement output = new LetStatement();
            output.Variable = var;
            output.Value = value;
            return output;
        }

        //called after initVirtualIds(...)
        private void initLetStatements(Expression expression, Dictionary<Expression, int> registers, Stack<LetStatement> stack)
        {

            LetStatement letStatement = new LetStatement();
            letStatement.Variable = "_" + registers[expression];

            if (expression is VariableExpression || expression is NumericExpression)
            {
                letStatement.Value = expression;
                stack.Push(letStatement);
            }
            else
            {
                BinaryOperationExpression binExp = (BinaryOperationExpression)expression;

                BinaryOperationExpression value = new BinaryOperationExpression();
                VariableExpression varExp1 = new VariableExpression();
                VariableExpression varExp2 = new VariableExpression();
                value.Operator = binExp.Operator;
                varExp1.Name = "_" + registers[binExp.Operand1];
                varExp2.Name = "_" + registers[binExp.Operand2];
                value.Operand1 = varExp1;
                value.Operand2 = varExp2;
                letStatement.Value = value;
                stack.Push(letStatement);
                initLetStatements(binExp.Operand1, registers, stack);
                initLetStatements(binExp.Operand2, registers, stack);

            }

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
                numVirtualVars++;
                registers.Add(expression, numVirtualVars);
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
