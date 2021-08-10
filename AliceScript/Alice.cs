using System;
using System.Collections.Generic;
using System.Text;

namespace AliceScript
{
   public static class Alice
    {
        public static Variable Execute(string code)
        {
           return Interpreter.Instance.Process(code);
        }
    }
}
