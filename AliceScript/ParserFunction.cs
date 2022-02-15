using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
    public class ParserFunction
    {
        public static Action<string, Variable, bool> OnVariableChange;

        /// <summary>
        /// オーバーライド可能かどうかを表す値
        /// </summary>
        public bool IsVirtual { get; set; }

        public ParserFunction()
        {
            m_impl = this;
        }

        // "仮想"コントラクスタ
        public ParserFunction(ParsingScript script, string item, char ch, ref string action)
        {
            if (item.Length == 0 && (ch == Constants.START_ARG || !script.StillValid()))
            {
                bool isLambda = false;
                int pointbc = script.Pointer;
                while (script.StillValid())
                {
                    script.Pointer++;
                    if (script.Prev == Constants.ARROW[0] && script.Current == Constants.ARROW[1])
                    {
                        isLambda = true;
                        break;
                    }
                }
                script.Pointer = pointbc;
                if (!isLambda)
                {
                    // 括弧内の式を計算します
                    m_impl = s_idFunction;
                }
                else
                {
                    // ラムダ式です
                    string[] args = Utils.GetFunctionSignature(script);
                    if (args.Length == 1 && string.IsNullOrWhiteSpace(args[0]))
                    {
                        args = new string[0];
                    }

                    script.MoveForwardIf(Constants.START_GROUP, Constants.SPACE);
                    /*string line = */
                    script.GetOriginalLine(out _);

                    int parentOffset = script.Pointer;

                    if (script.CurrentClass != null)
                    {
                        parentOffset += script.CurrentClass.ParentOffset;
                    }

                    string body = Utils.GetBodyLambdaBetween(script);

                    script.MoveForwardIf(Constants.END_GROUP);
                    CustomFunction customFunc = new CustomFunction("", body, args, script, "DELEGATE",true);
                    customFunc.ParentScript = script;
                    customFunc.ParentOffset = parentOffset;
                    m_impl =new GetVarFunction(new Variable(customFunc));
                }
                return;
            }

            m_impl = CheckString(script, item, ch);
            if (m_impl != null)
            {
                return;
            }

            item = Constants.ConvertName(item);

            m_impl = GetRegisteredAction(item, script, ref action);
            if (m_impl != null)
            {
                return;
            }

            m_impl = GetArrayFunction(item, script, action);
            if (m_impl != null)
            {
                return;
            }

            m_impl = GetObjectFunction(item, script);
            if (m_impl != null)
            {
                return;
            }

            m_impl = GetVariable(item, script);
            if (m_impl != null)
            {
                return;
            }

            if (m_impl == s_strOrNumFunction && string.IsNullOrWhiteSpace(item))
            {
                string problem = (!string.IsNullOrWhiteSpace(action) ? action : ch.ToString());
                string restData = ch.ToString() + script.Rest;
                throw new ArgumentException("Couldn't parse [" + problem + "] in " + restData + "...");
            }

            // Function not found, will try to parse this as a string in quotes or a number.
            s_strOrNumFunction.Item = item;
            m_impl = s_strOrNumFunction;
        }

        public static ParserFunction CheckString(ParsingScript script, string item, char ch)
        {
            if (item.Length > 1 &&
              (((item[0] == Constants.QUOTE) && item[item.Length - 1] == Constants.QUOTE) ||
               (item[0] == Constants.QUOTE1 && item[item.Length - 1] == Constants.QUOTE1)))
            {
                // We are dealing with a string.
                s_strOrNumFunction.Item = item;
                return s_strOrNumFunction;
            }
            if (script.ProcessingList && ch == ':')
            {
                s_strOrNumFunction.Item = '"' + item + '"';
                return s_strOrNumFunction;
            }
            return null;
        }

        public static ParserFunction GetArrayFunction(string name, ParsingScript script, string action)
        {
            int arrayStart = name.IndexOf(Constants.START_ARRAY);
            if (arrayStart < 0)
            {
                return null;
            }

            if (arrayStart == 0)
            {
                Variable arr = Utils.ProcessArrayMap(new ParsingScript(name));
                return new GetVarFunction(arr);
            }

            string arrayName = name;

            int delta = 0;
            List<Variable> arrayIndices = Utils.GetArrayIndices(script, arrayName, delta, (string arr, int del) => { arrayName = arr; delta = del; });

            if (arrayIndices.Count == 0)
            {
                return null;
            }

            ParserFunction pf = ParserFunction.GetVariable(arrayName, script);
            GetVarFunction varFunc = pf as GetVarFunction;
            if (varFunc == null)
            {
                return null;
            }

            // we temporarily backtrack for the processing
            script.Backward(name.Length - arrayStart - 1);
            script.Backward(action != null ? action.Length : 0);
            // delta shows us how manxy chars we need to advance forward in GetVarFunction()
            delta -= arrayName.Length;
            delta += action != null ? action.Length : 0;

            varFunc.Indices = arrayIndices;
            varFunc.Delta = delta;
            return varFunc;
        }

        public static ParserFunction GetObjectFunction(string name, ParsingScript script)
        {
            if (script.CurrentClass != null && script.CurrentClass.Name == name)
            {
                script.Backward(name.Length + 1);
                return new FunctionCreator();
            }
            if (script.ClassInstance != null &&
               (script.ClassInstance.PropertyExists(name) || script.ClassInstance.FunctionExists(name)))
            {
                name = script.ClassInstance.Name + "." + name;
            }
            //int ind = name.LastIndexOf('.');
            int ind = name.IndexOf('.');
            if (ind <= 0)
            {
                return null;
            }
            string baseName = name.Substring(0, ind);

            string prop = name.Substring(ind + 1);

            ParserFunction pf = ParserFunction.GetVariable(baseName, script, true);
            if (pf == null || !(pf is GetVarFunction))
            {
                pf = ParserFunction.GetFunction(baseName, script);
                if (pf == null)
                {
                    pf = Utils.ExtractArrayElement(baseName);
                }
            }

            GetVarFunction varFunc = pf as GetVarFunction;
            if (varFunc == null)
            {
                string cname = (name + ".").Split('.')[0];
                var cls = ObjectClass.GetClass(cname,script);
                if (cls == null)
                {
                    return null;
                }
                else if(cls.Static_Properties.TryGetValue(prop,out PropertyBase pb))
                {
                    return new GetVarFunction(pb.GetProperty(null));
                }else if(cls.Static_Functions.TryGetValue(prop, out FunctionBase fb))
                {
                    return fb;
                }
                else
                {
                    return null;
                }
            }

            varFunc.PropertyName = prop;
            return varFunc;
        }

        private static bool ActionForUndefined(string action)
        {
            return !string.IsNullOrWhiteSpace(action) && action.EndsWith("=") && action.Length > 1;
        }

        public static ParserFunction GetRegisteredAction(string name, ParsingScript script, ref string action)
        {
            if (Constants.CheckReserved(name))
            {
                return null;
            }

            if (false && ActionForUndefined(action) && script.Rest.StartsWith(Constants.UNDEFINED))
            {
                IsUndefinedFunction undef = new IsUndefinedFunction(name, action);
                return undef;
            }

            ActionFunction actionFunction = GetAction(action);

            // If passed action exists and is registered we are done.
            if (actionFunction == null)
            {
                return null;
            }

            ActionFunction theAction = actionFunction.NewInstance() as ActionFunction;
            theAction.Name = name;
            theAction.Action = action;

            action = null;
            return theAction;
        }


        public static ParserFunction GetVariable(string name, ParsingScript script = null, bool force = false)
        {
            if (!force && script != null && script.TryPrev() == Constants.START_ARG)
            {
                return GetFunction(name, script);
            }


            name = Constants.ConvertName(name);

            ParserFunction impl;
            //ローカルスコープに存在するか確認
            string scopeName = script == null || script.Filename == null ? "" : script.Filename;
            impl = GetLocalScopeVariable(name, scopeName);
            if (impl != null)
            {
                return impl;
            }
            if (script != null && script.TryGetVariable(name, out impl))
            {
                return impl.NewInstance();
            }
            if (s_variables.TryGetValue(name, out impl))
            {
                return impl.NewInstance();
            }

            //定数に存在するか確認
            if(script!=null&&script.TryGetConst(name,out impl)&&impl!=null)
            {
                return impl.NewInstance();
            }
            if (Constants.CONSTS.ContainsKey(name))
            {
                return new GetVarFunction(Constants.CONSTS[name]);
            }

            //関数として取得を続行
            return GetFunction(name, script,true);
        }

     
        public static ParserFunction GetFunction(string name, ParsingScript script,bool toDelegate=false)
        {
            name = Constants.ConvertName(name);
            ParserFunction impl;
            if (script.TryGetFunction(name, out impl))
            {
                //ローカル関数として登録されている
                if(toDelegate&&impl is FunctionBase cf)
                {
                    //かっこなしで呼び出された場合
                    if (!cf.Attribute.HasFlag(FunctionAttribute.LANGUAGE_STRUCTURE) && !cf.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE) && !cf.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE_ONC))
                    {
                        return new GetVarFunction(new Variable(cf));
                    }
                }
                return impl.NewInstance();
            }
            if (s_functions.TryGetValue(name, out impl))
            {
                //グローバル関数として登録されている
                if (toDelegate && impl is FunctionBase cf)
                {
                    //かっこなしで呼び出された場合
                    if (!cf.Attribute.HasFlag(FunctionAttribute.LANGUAGE_STRUCTURE) && !cf.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE) && !cf.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE_ONC))
                    {
                        return new GetVarFunction(new Variable(cf));
                    }
                }
                return impl.NewInstance();
            }
            if (script.TryGetVariable(name, out impl) || s_variables.TryGetValue(name, out impl))
            {
                //それがデリゲート型の変数である場合
                if (impl is GetVarFunction gv && gv.Value.Type == Variable.VarType.DELEGATE && !gv.Value.IsNull())
                {
                    return gv.Value.Delegate.Function;
                }
            }

            return null;
        }

      
        public static ActionFunction GetAction(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return null;
            }

            ActionFunction impl;
            if (s_actions.TryGetValue(action, out impl))
            {
                // Action exists and is registered (e.g. =, +=, --, etc.)
                return impl;
            }

            return null;
        }

        public static bool FunctionExists(string item, ParsingScript script)
        {
            // If it is not defined locally, then check globally:
            return LocalNameExists(item, script) || GlobalNameExists(item);
        }

        public static void AddGlobalOrLocalVariable(string name, GetVarFunction function,
            ParsingScript script, bool localIfPossible = false, bool registVar = false,bool globalOnly=false)
        {
            name = Constants.ConvertName(name);
            Utils.CheckLegalName(name, script);


            function.Name = Constants.GetRealName(name);
            function.Value.ParamName = function.Name;

            if (globalOnly)
            {
                AddGlobal(name, function, false /* not native */, registVar);
            }
            else
            {
                AddLocalVariable(function, script, name,registVar);
            }
        }

    
        public static bool LocalNameExists(string name, ParsingScript script)
        {
            if (script != null && (script.ContainsVariable(name) || script.ContainsFunction(name) || script.ContainsConst(name)))
            {
                return true;
            }
            return false;
        }

        public static bool GlobalNameExists(string name)
        {
            name = Constants.ConvertName(name);
            return s_variables.ContainsKey(name) || s_functions.ContainsKey(name)||Constants.CONSTS.ContainsKey(name);
        }

        public static Variable RegisterEnum(string varName, string enumName,ParsingScript script=null)
        {
            Variable enumVar = EnumFunction.UseExistingEnum(enumName);
            if (enumVar == Variable.EmptyInstance)
            {
                return enumVar;
            }
            if (script == null)
            {
                RegisterFunction(varName, new GetVarFunction(enumVar));
            }
            else
            {
                RegisterScriptFunction(varName,new GetVarFunction(enumVar),script);
            }
            return enumVar;
        }

        public static void RegisterFunction(string name, ParserFunction function,
                                            bool isNative = true)
        {
            name = Constants.ConvertName(name);
            function.Name = Constants.GetRealName(name);
            if (!s_functions.ContainsKey(name) || (s_functions.ContainsKey(name) && s_functions[name].IsVirtual))
            {
                //まだ登録されていないか、すでに登録されていて、オーバーライド可能な場合
                s_functions[name] = function;
                function.isNative = isNative;
                if ((s_functions.ContainsKey(name) && s_functions[name].IsVirtual))
                {
                    //オーバーライドした関数にもVirtual属性を引き継ぐ
                    function.IsVirtual = true;
                }
            }
            else
            {
                ThrowErrorManerger.OnThrowError("指定された関数はすでに登録されていて、オーバーライドできません", Exceptions.FUNCTION_IS_ALREADY_DEFINED);
            }
        }
        public static void RegisterScriptFunction(string name,ParserFunction function,ParsingScript script,bool isNative=true,bool isLocal=true)
        {
            name = Constants.ConvertName(name);
            function.Name = Constants.GetRealName(name);

           
            ParserFunction impl=null;
            if(isLocal&&(!script.ContainsFunction(name)||(script.TryGetFunction(name,out impl) && impl.IsVirtual)))
            {
                //ローカル関数でまだ登録されていないか、すでに登録されていて、オーバーライド可能な場合
                script.Functions[name] = function;
                function.isNative = isNative;
                if (impl != null)
                {
                    impl.IsVirtual = true;
                }
            }
            else if (!isLocal&&(!s_functions.ContainsKey(name) || (s_functions.ContainsKey(name) && s_functions[name].IsVirtual)))
            {
                //まだ登録されていないか、すでに登録されていて、オーバーライド可能な場合
                s_functions[name] = function;
                function.isNative = isNative;
                if ((s_functions.ContainsKey(name) && s_functions[name].IsVirtual))
                {
                    //オーバーライドした関数にもVirtual属性を引き継ぐ
                    function.IsVirtual = true;
                }
            }
            else
            {
                ThrowErrorManerger.OnThrowError("指定された関数はすでに登録されていて、オーバーライドできません", Exceptions.FUNCTION_IS_ALREADY_DEFINED);
            }
        }
        public static bool UnregisterScriptFunction(string name,ParsingScript script)
        {
            name = Constants.ConvertName(name);
            if (script != null && script.Functions.Remove(name))
            {
                return true;
            }
            return s_functions.Remove(name);
        }
        public static bool UnregisterFunction(string name)
        {
            name = Constants.ConvertName(name);

            bool removed = s_functions.Remove(name);
            return removed;
        }

        public static bool RemoveGlobal(string name)
        {
            name = Constants.ConvertName(name);
            return s_variables.Remove(name);
        }

        private static void NormalizeValue(ParserFunction function)
        {
            GetVarFunction gvf = function as GetVarFunction;
            if (gvf != null)
            {
                gvf.Value.CurrentAssign = "";
            }
        }

        private static void AddVariables(List<Variable> vars, Dictionary<string, ParserFunction> dict)
        {
            foreach (var val in dict.Values)
            {
                if (val.isNative || !(val is GetVarFunction))
                {
                    continue;
                }
                Variable var = ((GetVarFunction)val).Value.DeepClone();
                var.ParamName = ((GetVarFunction)val).Name;
                vars.Add(var);
            }
        }

        public static List<Variable> VariablesSnaphot(ParsingScript script = null, bool includeGlobals = false)
        {
            List<Variable> vars = new List<Variable>();
            if (includeGlobals)
            {
                AddVariables(vars, s_variables);
            }
            return vars;
        }

        public static void AddGlobal(string name, ParserFunction function,
                                     bool isNative = true, bool registVar = false)
        {
            Utils.CheckLegalName(name);
            name = Constants.ConvertName(name);
            NormalizeValue(function);
            function.isNative = isNative;
            if (Constants.CONSTS.ContainsKey(name))
            {
                ThrowErrorManerger.OnThrowError("定数に値を代入することはできません", Exceptions.CANT_ASSIGN_VALUE_TO_CONSTANT);
                return;
            }
            var handle = OnVariableChange;
            bool exists = s_variables.ContainsKey(name);
            if (exists && registVar)
            {
                ThrowErrorManerger.OnThrowError("変数[" + name + "]はすでに定義されています", Exceptions.VARIABLE_ALREADY_DEFINED);
                return;
            }
            else if (!exists && !registVar)
            {
                ThrowErrorManerger.OnThrowError("変数[" + name + "]は定義されていません", Exceptions.COULDNT_FIND_VARIABLE);
                return;
            }
            s_variables[name] = function;

            function.Name = Constants.GetRealName(name);
            if (handle != null && function is GetVarFunction)
            {
                handle.Invoke(function.Name, ((GetVarFunction)function).Value, exists);
            }
        }

        public static void AddLocalScopeVariable(string name, string scopeName, ParserFunction variable)
        {
            name = Constants.ConvertName(name);
            variable.isNative = false;
            variable.Name = Constants.GetRealName(name);
            if (variable is GetVarFunction)
            {
                ((GetVarFunction)variable).Value.ParamName = variable.Name;
            }

            if (scopeName == null)
            {
                scopeName = "";
            }

            Dictionary<string, ParserFunction> localScope;
            if (!s_localScope.TryGetValue(scopeName, out localScope))
            {
                localScope = new Dictionary<string, ParserFunction>();
            }
            localScope[name] = variable;
            s_localScope[scopeName] = localScope;
        }

        private static ParserFunction GetLocalScopeVariable(string name, string scopeName)
        {
            scopeName = Path.GetFileName(scopeName);
            Dictionary<string, ParserFunction> localScope;
            if (!s_localScope.TryGetValue(scopeName, out localScope))
            {
                return null;
            }

            name = Constants.ConvertName(name);
            ParserFunction function = null;
            localScope.TryGetValue(name, out function);
            return function;
        }

        public static void AddAction(string name, ActionFunction action)
        {
            s_actions[name] = action;
        }


        public static void AddLocalVariable(ParserFunction local, ParsingScript script, string varName = "",bool registVar=false)
        {
            NormalizeValue(local);
            local.m_isGlobal = false;
                var name = Constants.ConvertName(string.IsNullOrWhiteSpace(varName) ? local.Name : varName);
                local.Name = Constants.GetRealName(name);
                if (local is GetVarFunction)
                {
                    ((GetVarFunction)local).Value.ParamName = local.Name;
                }
                bool exists = script.ContainsVariable(name);
                if (exists && registVar)
                {
                    ThrowErrorManerger.OnThrowError("変数[" + name + "]はすでに定義されています", Exceptions.VARIABLE_ALREADY_DEFINED);
                    return;
                }
                else if (!exists && !registVar)
                {
                    ThrowErrorManerger.OnThrowError("変数[" + name + "]は定義されていません", Exceptions.COULDNT_FIND_VARIABLE);
                    return;
                }
                script.Variables[name] = local;
           
        }

        public Variable GetValue(ParsingScript script)
        {
            return m_impl.Evaluate(script);
        }

        public async Task<Variable> GetValueAsync(ParsingScript script)
        {
            return await m_impl.EvaluateAsync(script);
        }

        protected virtual Variable Evaluate(ParsingScript script)
        {
            // The real implementation will be in the derived classes.
            return new Variable();
        }

        protected virtual Task<Variable> EvaluateAsync(ParsingScript script)
        {
            // If not overriden, the non-sync version will be called.
            return Task.FromResult(Evaluate(script));
        }

        // Derived classes may want to return a new instance in order to
        // not to use same object in calculations.
        public virtual ParserFunction NewInstance()
        {
            return this;
        }

        public static void CleanUp()
        {
            s_functions.Clear();
            s_actions.Clear();
            CleanUpVariables();
        }

        public static void CleanUpVariables()
        {
            s_variables.Clear();
            /*
            s_locals.Clear();
            s_localScope.Clear();
            s_namespaces.Clear();
            s_namespace = s_namespacePrefix = "";
            */
            //TODO:名前空間などのクリーンアップ
        }

        protected string m_name;
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        protected bool m_isGlobal = true;
        public bool isGlobal { get { return m_isGlobal; } set { m_isGlobal = value; } }

        protected bool m_isNative = true;
        public bool isNative { get { return m_isNative; } set { m_isNative = value; } }

        private ParserFunction m_impl;

        // Global functions:
        public static Dictionary<string, ParserFunction> s_functions = new Dictionary<string, ParserFunction>();

        // Global variables:
        public static Dictionary<string, ParserFunction> s_variables = new Dictionary<string, ParserFunction>();

        // Global actions to functions map:
        private static Dictionary<string, ActionFunction> s_actions = new Dictionary<string, ActionFunction>();

        // Local scope variables:
        private static Dictionary<string, Dictionary<string, ParserFunction>> s_localScope =
           new Dictionary<string, Dictionary<string, ParserFunction>>();


        private static StringOrNumberFunction s_strOrNumFunction =
          new StringOrNumberFunction();
        private static IdentityFunction s_idFunction =
          new IdentityFunction();

        public static int StackLevelDelta { get; set; }
    }

    public abstract class ActionFunction : ParserFunction
    {
        protected string m_action;
        public string Action { set { m_action = value; } }
    }
}