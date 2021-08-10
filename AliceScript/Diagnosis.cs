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

        public static bool CheckScript(string script, string filename = "", bool mainFile = false)
        {
            Dictionary<int, int> char2Line;

            string data = Utils.ConvertToScript(script, out char2Line, filename);

            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }

            ParsingScript toParse = new ParsingScript(data, 0, char2Line);
            toParse.OriginalScript = script;
            toParse.Filename = filename;

            if (mainFile)
            {
                toParse.MainFilename = toParse.Filename;
            }
            int arrayIndexDepth = 0;
            bool inQuotes = false;
            int negated = 0;
            char ch;
            string action;
            char[] to = null;
            string token = Parser.ExtractNextToken(toParse, to, ref inQuotes, ref arrayIndexDepth, ref negated, out ch, out action);



            // We are done getting the next token. The GetValue() call below may
            // recursively call WSOFTScript(). This will happen if extracted
            // item is a function or if the next item is starting with a START_ARG '('.


            if (ParserFunction.CheckString(toParse, token, ch) != null)
            {
                return true;
            }

            token = Constants.ConvertName(token);

            if (ParserFunction.GetRegisteredAction(token, toParse, ref action) != null)
            {
                return true;
            }


            if (ParserFunction.GetArrayFunction(token, toParse, action) != null)
            {
                return true;
            }

            if (ParserFunction.GetObjectFunction(token, toParse) != null)
            {
                return true;
            }


            if (ParserFunction.GetVariable(token, toParse) != null)
            {
                return true;
            }



            return false;
        }
        /// <summary>
        /// Falseにすると関数を実行しません
        /// </summary>
        public static bool IsRunFunction = true;
    }
}
