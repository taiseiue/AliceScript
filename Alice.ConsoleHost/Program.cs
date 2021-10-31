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
            AliceScript.Alice.ExecuteFile(filename,true);
        }

        private static void Instance_OnOutput(object sender, OutputAvailableEventArgs e)
        {
            Console.Write(e.Output);
        }
    }
}
