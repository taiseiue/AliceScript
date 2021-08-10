using System;
using AliceScript;

namespace alice_repl
{
    class Program
    {
        static void Main(string[] args)
        {
            Variable.AddFunc(new DisposeFunc(),"dispose");
            while (true)
            {
                Interpreter.Instance.OnOutput += Instance_OnOutput;
                Console.Write("REPL>");
                Alice.Execute(Console.ReadLine());
                
            }
        }

     

        private static void Instance_OnOutput(object sender, OutputAvailableEventArgs e)
        {
            Console.Write(e.Output);
        }
    }
    public class DisposeFunc : FunctionBase
    {

        public DisposeFunc()
        {
            this.FunctionName = "Dispose";
            this.RequestType = Variable.VarType.STRING;
            this.Run += DisposeFunc_Run;

        }

        private void DisposeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.CurentVariable.Reset();

        }
    }

}
