using System;
using AliceScript;

namespace Alice.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            string filename = "main.txt";
            Interpreter.Instance.OnOutput += Instance_OnOutput;
            Variable tuple = new Variable(Variable.VarType.ARRAY);
            bool isArg = false;
            foreach(string arg in args)
            {
                if (isArg==true)
                {
                    tuple.Tuple.Add(new Variable(arg));
                }else 
                {
                    filename = arg;
                }
                if (isArg == false) { isArg = true; }
            }
            ParserFunction.s_variables.Add("args",new GetVarFunction(tuple));
            AlicePackage.Load(filename);
        }

        private static void Instance_OnOutput(object sender, OutputAvailableEventArgs e)
        {
            Console.Write(e.Output);
        }
    }
}
