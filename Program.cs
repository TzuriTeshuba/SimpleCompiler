using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Ex3.3

namespace SimpleCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Test 4 started running bro");
            //TestSimplifyLetStatement();
            //TestParseAndErrors();
            Test3();
        }

        static void InitLCL(List<string> lAssembly)
        {
            lAssembly.Insert(0, "@20");
            lAssembly.Insert(1, "D=A");
            lAssembly.Insert(2, "@LCL");
            lAssembly.Insert(3, "M=D");


        }
        static void TestSimplifyLetStatement()
        {
            List<VarDeclaration> varDecs = new List<VarDeclaration>();
            
            char chr = 'a';
            while(chr <= 'g')
            {
                VarDeclaration varDec = new VarDeclaration(null, ""+chr);
                varDecs.Add(varDec);
                chr++;
            }
            VarDeclaration x = new VarDeclaration(null, "x");
            varDecs.Add(x);
            string strExpression = "(((a + b) - (c - d)) + (e - (f + g)))";

            Compiler c = new Compiler();
            List<Token> tokensList = c.Tokenize(strExpression, 0);
            TokensStack tokens= new TokensStack();
            for(int i = tokensList.Count - 1; i >= 0; i--)
            {
                tokens.Push(tokensList[i]);
            }
            LetStatement example = new LetStatement();
            Expression expression = Expression.Create(tokens);
            expression.Parse(tokens);
            LetStatement let = new LetStatement();
            let.Variable = "x";
            let.Value = expression;
            List<LetStatement> letStatements = c.SimplifyExpressions(let, varDecs);
            for(int i=0; i< letStatements.Count; i++)
            {
                Console.WriteLine(letStatements[i]);
            }

            Console.WriteLine("Simplifying Expressions completetd");

        }
        static void Test1()
        {
            Compiler c = new Compiler();
            List<string> lVars = new List<string>();
            lVars.Add("var int x;");
            List<VarDeclaration> vars = c.ParseVarDeclarations(lVars);

            string s = "let x = 5;";
            List<Token> lTokens = c.Tokenize(s, 0);
            LetStatement assignment = c.ParseStatement(lTokens);
            if(assignment.ToString() != s)
                Console.WriteLine("BUGBUG");


            List<LetStatement> l = new List<LetStatement>();
            l.Add(assignment);
            List<string> lAssembly = c.GenerateCode(l, vars);
            CPUEmulator cpu = new CPUEmulator();
            InitLCL(lAssembly);
            cpu.Code = lAssembly;
            cpu.Run(1000, false);
            if (cpu.M[20] != 5)
                Console.WriteLine("BUGBUG");
        }

        static void Test2()
        {
            Compiler c = new Compiler();
            List<string> lVars = new List<string>();
            lVars.Add("var int x;");
            lVars.Add("var int y;");
            lVars.Add("var int z;");
            List<VarDeclaration> vars = c.ParseVarDeclarations(lVars);

            List<string> lAssignments = new List<string>();
            lAssignments.Add("let x = 10;");
            lAssignments.Add("let y = 15;");
            lAssignments.Add("let z = (x + y);");

            List<LetStatement> ls = c.ParseAssignments(lAssignments);


            List<string> lAssembly = c.GenerateCode(ls, vars);
            CPUEmulator cpu = new CPUEmulator();
            InitLCL(lAssembly);
            cpu.Code = lAssembly;
            cpu.Run(1000, false);
            if (cpu.M[22] != 25)
                Console.WriteLine("BUGBUG");
        }
        static void Test3()
        {
            Compiler c = new Compiler();
            List<string> lVars = new List<string>();
            lVars.Add("var int x;");
            lVars.Add("var int y;");
            lVars.Add("var int z;");
            List<VarDeclaration> vars = c.ParseVarDeclarations(lVars);

            string s = "let x = ((x + 5) + (y - z));";
            List<Token> lTokens = c.Tokenize(s,0);
            LetStatement assignment = c.ParseStatement(lTokens);

            List<LetStatement> lSimple = c.SimplifyExpressions(assignment, vars);
            List<string> lAssembly = c.GenerateCode(lSimple, vars);

            CPUEmulator cpu = new CPUEmulator();
            InitLCL(lAssembly);
            cpu.Code = lAssembly;
            cpu.Run(1000, false);
            if (cpu.M[20] != 5)
                Console.WriteLine("BUGBUG");
        }

        static void Test4()
        {
            Compiler c = new Compiler();

            List<string> lVars = new List<string>();
            lVars.Add("var int x1;");
            lVars.Add("var int x2;");
            lVars.Add("var int x3;");
            lVars.Add("var int x4;");
            lVars.Add("var int x5;");
            List<VarDeclaration> vars = c.ParseVarDeclarations(lVars);


            List<string> lAssignments = new List<string>();
            lAssignments.Add("let x1 = 1;");
            lAssignments.Add("let x2 = 3;");
            lAssignments.Add("let x3 = (((x1 + 1) - 4) + ((x2 + x1) - 2));");
            lAssignments.Add("let x4 = ((x2 + x3) - (x2 -7));");
            lAssignments.Add("let x5 = (1000 - ((x1 + (((((x2 + x3) - x4) + x1) - x2) + x3)) - ((x1 - x2) + x4)));");

            List<LetStatement> ls = c.ParseAssignments(lAssignments);
            Dictionary<string, int> dValues = new Dictionary<string, int>();
            dValues["x1"] = 0;
            dValues["x2"] = 0;
            dValues["x3"] = 0;
            dValues["x4"] = 0;
            dValues["x5"] = 0;

            CPUEmulator cpu = new CPUEmulator();
            cpu.Compute(ls, dValues);

            List<LetStatement> lSimple = c.SimplifyExpressions(ls, vars);

            Dictionary<string, int> dValues2 = new Dictionary<string, int>();
            dValues2["x1"] = 0;
            dValues2["x2"] = 0;
            dValues2["x3"] = 0;
            dValues2["x4"] = 0;
            dValues2["x5"] = 0;

            cpu.Compute(lSimple, dValues2);

            foreach (string sKey in dValues.Keys)
                if (dValues[sKey] != dValues2[sKey])
                    Console.WriteLine("BGUBGU");

            List<string> lAssembly = c.GenerateCode(lSimple, vars);

            InitLCL(lAssembly);
            cpu.Code = lAssembly;
            cpu.Run(1000, false);
            if (cpu.M[24] != dValues2["x5"])
                Console.WriteLine("BUGBUG");

        }






 
     }
}
