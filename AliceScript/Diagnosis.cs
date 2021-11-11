using System;
using System.Collections.Generic;
using System.Text;

namespace AliceScript
{
    public static class Diagnosis
    {
        public static Dictionary<string, Variable> Variables
        {
            get
            {
                Dictionary<string, Variable> dic = new Dictionary<string, Variable>();
                foreach (string s in ParserFunction.s_variables.Keys)
                {
                    dic.Add(s,Alice.Execute(s));
                }
                return dic;
            }
        }

    }
}
