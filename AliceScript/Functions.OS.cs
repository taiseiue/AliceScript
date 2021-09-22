using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AliceScript
{
    interface INumericFunction { }
    interface IArrayFunction { }
    interface IStringFunction { }

    // Prints passed list of argumentsand
    class PrintFunction : FunctionBase
    {
        public PrintFunction()
        {
           
                this.Name = "print";
           
            this.MinimumArgCounts = 1;
            this.Run += PrintFunction_Run;
        }

        private void PrintFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 1)
            {
                AddOutput(e.Args[0].AsString(), e.Script,true);
            }
            else
            {
                string text = e.Args[0].AsString();
                MatchCollection mc = Regex.Matches(text, @"{[0-9]+}");
                foreach (Match match in mc)
                {
                    int mn = -1;
                    if (int.TryParse(match.Value.TrimStart('{').TrimEnd('}'), out mn))
                    {
                        if (mn == -1) { ThrowErrorManerger.OnThrowError("複合書式指定\"{" + mn + "}\"で" + mn + "番目の引数が見つかりません", e.Script); break; }
                        if (e.Args.Count > mn + 1)
                        {
                            text = text.Replace(match.Value, e.Args[mn + 1].AsString());
                        }
                        else
                        {
                            ThrowErrorManerger.OnThrowError("複合書式指定\"{" + mn + "}\"で" + mn + "番目の引数が見つかりません", e.Script);
                            break;
                        }
                    }
                    else
                    {
                        ThrowErrorManerger.OnThrowError("複合書式指定\"" + match.Value + "\"は無効です", e.Script);
                        break;
                    }
                }
                AddOutput(text,e.Script,true);
            }
        }
        
        public static void AddOutput(string text, ParsingScript script = null,
                                     bool addLine = true, bool addSpace = true, string start = "")
        {
            
            string output = text + (addLine ? Environment.NewLine : string.Empty);
            output = output.Replace("\\t", "\t").Replace("\\n", "\n");
            Interpreter.Instance.AppendOutput(output);

            Debugger debugger = script != null && script.Debugger != null ?
                                script.Debugger : Debugger.MainInstance;
            if (debugger != null)
            {
                debugger.AddOutput(output, script);
            }
        }
    }
  
    class DataFunction : ParserFunction
    {
        public enum DataMode { ADD, SUBSCRIBE, SEND };

        DataMode m_mode;

        static string s_method;
        static string s_tracking;
        static bool s_updateImmediate = false;

        static StringBuilder s_data = new StringBuilder();

        public DataFunction(DataMode mode = DataMode.ADD)
        {
            m_mode = mode;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            string result = "";

            switch (m_mode)
            {
                case DataMode.ADD:
                    Collect(args);
                    break;
                case DataMode.SUBSCRIBE:
                    Subscribe(args);
                    break;
                case DataMode.SEND:
                    result = SendData(s_data.ToString());
                    s_data.Clear();
                    break;
            }

            return new Variable(result);
        }

        public void Subscribe(List<Variable> args)
        {
            s_data.Clear();

            s_method = Utils.GetSafeString(args, 0);
            s_tracking = Utils.GetSafeString(args, 1);
            s_updateImmediate = Utils.GetSafeDouble(args, 2) > 0;
        }

        public void Collect(List<Variable> args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var arg in args)
            {
                sb.Append(arg.AsString());
            }
            if (s_updateImmediate)
            {
                SendData(sb.ToString());
            }
            else
            {
                s_data.AppendLine(sb.ToString());
            }
        }

        public string SendData(string data)
        {
            if (!string.IsNullOrWhiteSpace(s_method))
            {
                CustomFunction.Run(s_method, new Variable(s_tracking),
                                   new Variable(data));
                return "";
            }
            return data;
        }
    }

    class CurrentPathFunction : ParserFunction, INumericFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            return new Variable(script.PWD);
        }
    }

    class TokenizeFunction : ParserFunction, IArrayFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();

            Utils.CheckArgs(args.Count, 1, m_name);
            string data = Utils.GetSafeString(args, 0);

            string sep = Utils.GetSafeString(args, 1, "\t");
            var option = Utils.GetSafeString(args, 2);

            return Tokenize(data, sep, option);
        }

        static public Variable Tokenize(string data, string sep, string option = "", int max = int.MaxValue - 1)
        {
            if (sep == "\\t")
            {
                sep = "\t";
            }
            if (sep == "\\n")
            {
                sep = "\n";
            }

            string[] tokens;
            var sepArray = sep.ToCharArray();
            if (sepArray.Count() == 1)
            {
                tokens = data.Split(sepArray, max);
            }
            else
            {
                List<string> tokens_ = new List<string>();
                var rx = new System.Text.RegularExpressions.Regex(sep);
                tokens = rx.Split(data);
                for (int i = 0; i < tokens.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(tokens[i]) || sep.Contains(tokens[i]))
                    {
                        continue;
                    }
                    tokens_.Add(tokens[i]);
                }
                tokens = tokens_.ToArray();
            }

            List<Variable> results = new List<Variable>();
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (i > 0 && string.IsNullOrWhiteSpace(token) &&
                    option.StartsWith("prev", StringComparison.OrdinalIgnoreCase))
                {
                    token = tokens[i - 1];
                }
                results.Add(new Variable(token));
            }

            return new Variable(results);
        }
    }

    // Append a string to another string
    class AppendFunction : ParserFunction, IStringFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            // 1. Get the name of the variable.
            string varName = Utils.GetToken(script, Constants.NEXT_ARG_ARRAY);
            Utils.CheckNotEmpty(script, varName, m_name);

            // 2. Get the current value of the variable.
            ParserFunction func = ParserFunction.GetVariable(varName, script);
            Variable currentValue = func.GetValue(script);

            // 3. Get the value to be added or appended.
            Variable newValue = Utils.GetItem(script);

            // 4. Take either the string part if it is defined,
            // or the numerical part converted to a string otherwise.
            string arg1 = currentValue.AsString();
            string arg2 = newValue.AsString();

            // 5. The variable becomes a string after adding a string to it.
            newValue.Reset();
            newValue.String = arg1 + arg2;

            ParserFunction.AddGlobalOrLocalVariable(varName, new GetVarFunction(newValue), script);

            return newValue;
        }
    }

 
   
    class LockFunction : ParserFunction
    {
        static Object lockObject = new Object();

        protected override Variable Evaluate(ParsingScript script)
        {
            string body = Utils.GetBodyBetween(script, Constants.START_ARG,
                                                       Constants.END_ARG);
            ParsingScript threadScript = new ParsingScript(body);

            // BUGBUG: Alfred - what is this actually locking?
            // Vassili - it's a global (static) lock. used when called from different threads
            lock (lockObject)
            {
                threadScript.ExecuteAll();
            }
            return Variable.EmptyInstance;
        }
    }

    class DateTimeFunction : ParserFunction, IStringFunction
    {
        bool m_stringVersion;

        public DateTimeFunction(bool stringVersion = true)
        {
            m_stringVersion = stringVersion;
        }

        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            string strFormat = m_stringVersion ? Utils.GetSafeString(args, 0, "HH:mm:ss.fff") :
                                          Utils.GetSafeString(args, 1, "yyyy/MM/dd HH:mm:ss");
            Utils.CheckNotEmpty(strFormat, m_name);


            if (m_stringVersion)
            {
                return new Variable(DateTime.Now.ToString(strFormat));
            }

            var date = DateTime.Now;
            string when = Utils.GetSafeString(args, 0);

            if (!string.IsNullOrWhiteSpace(when) && !DateTime.TryParseExact(when, strFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out date))
            {
                throw new ArgumentException("Couldn't parse [" + when + "] using format [" +
                                            strFormat + "].");
            }
            if (!strFormat.Contains("yy") && !strFormat.Contains("MM") && !strFormat.Contains("dd"))
            {
                date = date.Subtract(new TimeSpan(date.Date.Ticks));
            }

            return new Variable(date);
        }

        public static DateTime Add(DateTime current, string delta)
        {
            int sign = 1;
            string part = "";
            int partInt;
            for (int i = 0; i < delta.Length; i++)
            {
                switch (delta[i])
                {
                    case '-':
                        sign *= -1;
                        continue;
                    case 'y':
                        partInt = string.IsNullOrWhiteSpace(part) ? 1 : !int.TryParse(part, out partInt) ? 0 : partInt;
                        current = current.AddYears(partInt * sign);
                        break;
                    case 'M':
                        partInt = string.IsNullOrWhiteSpace(part) ? 1 : !int.TryParse(part, out partInt) ? 0 : partInt;
                        current = current.AddMonths(partInt * sign);
                        break;
                    case 'd':
                        partInt = string.IsNullOrWhiteSpace(part) ? 1 : !int.TryParse(part, out partInt) ? 0 : partInt;
                        current = current.AddDays(partInt * sign);
                        break;
                    case 'H':
                    case 'h':
                        partInt = string.IsNullOrWhiteSpace(part) ? 1 : !int.TryParse(part, out partInt) ? 0 : partInt;
                        current = current.AddHours(partInt * sign);
                        break;
                    case 'm':
                        partInt = string.IsNullOrWhiteSpace(part) ? 1 : !int.TryParse(part, out partInt) ? 0 : partInt;
                        current = current.AddMinutes(partInt * sign);
                        break;
                    case 's':
                        partInt = string.IsNullOrWhiteSpace(part) ? 1 : !int.TryParse(part, out partInt) ? 0 : partInt;
                        current = current.AddSeconds(partInt * sign);
                        break;
                    case 'f':
                        partInt = string.IsNullOrWhiteSpace(part) ? 1 : !int.TryParse(part, out partInt) ? 0 : partInt;
                        current = current.AddTicks(partInt * sign);
                        break;
                    default:
                        part += delta[i];
                        continue;
                }
                part = "";
            }
            return current;
        }
    }

  
   

    

  
}
