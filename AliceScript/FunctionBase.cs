using System;
using System.Collections.Generic;
using System.Text;

namespace AliceScript
{
    public class FunctionBase : ParserFunction
    {
        public string FunctionName { get; set; }

        public int MinimumArgCounts { get; set; }

        public FunctionAttribute Attribute { get
            {
                return m_Attribute;
            }
            set
            {
                m_Attribute = value;
            }
        }
        public new string Name
        {
            get
            {
                return FunctionName;
            }
            set
            {
                FunctionName = value;
            }
        }
        private FunctionAttribute m_Attribute = FunctionAttribute.GENERAL;
        public Variable.VarType RequestType { get; set; }
        protected override Variable Evaluate(ParsingScript script)
        {


            List<Variable> args;
            if (ObjectBase.GETTING)
            {
                args = ObjectBase.LaskVariable;
            }
            else
            {
                args = script.GetFunctionArgs(Constants.START_ARG, Constants.END_ARG);
            }



            if (MinimumArgCounts >= 1)
            {
                Utils.CheckArgs(args.Count, MinimumArgCounts, m_name);
            }
            FunctionBaseEventArgs ex = new FunctionBaseEventArgs();
            ex.Args = args;
            if (ex.Args == null) { ex.Args = new List<Variable>(); }
            ex.UseObjectResult = false;
            ex.ObjectResult = null;
            ex.OriginalScript = script.OriginalScript;
            ex.Return = Variable.EmptyInstance;
            ex.Script = script;
            Run?.Invoke(script, ex);
            if (ex.UseObjectResult) { return new Variable(ex.ObjectResult); }
            return ex.Return;
        }
        public Variable Evaluate(ParsingScript script, Variable currentVariable)
        {
            if (currentVariable == null) { return Variable.EmptyInstance; }
            if (this.RequestType != Variable.VarType.NONE)
            {
                if (!currentVariable.Type.HasFlag(this.RequestType)) { ThrowErrorManerger.OnThrowError("関数[" + FunctionName + "]は無効または定義されていません"); return Variable.EmptyInstance; }
            }
            List<Variable> args;
            if (ObjectBase.GETTING)
            {
                args = ObjectBase.LaskVariable;
            }
            else
            {
                args = script.GetFunctionArgs(Constants.START_ARG, Constants.END_ARG);
            }



            if (MinimumArgCounts >= 1)
            {
                Utils.CheckArgs(args.Count, MinimumArgCounts, m_name);
            }
            FunctionBaseEventArgs ex = new FunctionBaseEventArgs();
            ex.Args = args;
            if (ex.Args == null) { ex.Args = new List<Variable>(); }
            ex.UseObjectResult = false;
            ex.ObjectResult = null;
            ex.OriginalScript = script.OriginalScript;
            ex.Return = Variable.EmptyInstance;
            ex.Script = script;

            ex.CurentVariable = currentVariable;
            Run?.Invoke(script, ex);
            if (ex.UseObjectResult) { return new Variable(ex.ObjectResult); }
            return ex.Return;
        }
        public FunctionBase()
        {
            MinimumArgCounts = 0;
        }

        public event FunctionBaseEventHandler Run;
        public  Variable GetVaruableFromArgs(List<Variable> args)
        {
            if (MinimumArgCounts >= 1)
            {
                Utils.CheckArgs(args.Count, MinimumArgCounts, m_name);
            }
            FunctionBaseEventArgs ex = new FunctionBaseEventArgs();
            ex.Args = args;
            Run?.Invoke(null, ex);

            return ex.Return;
        }
        public void OnRun(List<Variable> args)
        {
            GetVaruableFromArgs(args);
        }

    }
    /// <summary>
    /// 関数の機能の種類を表します
    /// </summary>
    public enum FunctionAttribute
    {
        /// <summary>
        /// 通常の関数です
        /// </summary>
        GENERAL=0,
        /// <summary>
        /// 関数の引数に括弧を必要としません（すなわち、空白が使われます）
        /// </summary>
        FUNCT_WITH_SPACE =1,
        /// <summary>
        /// 関数の引数に括弧を必要としませんが、空白は唯一のものにする必要があります
        /// </summary>
        FUNCT_WITH_SPACE_ONC=2,
        /// <summary>
        /// フロー関数です。これらの関数の戻り値には意味はありません
        /// </summary>
        CONTROL_FLOW=3
    }
    
    public static class FunctionBaseManerger
    {
        public static void Add(FunctionBase func,string name="")
        {

            string fname =func.Name;
            if (!string.IsNullOrEmpty(name))
            {
                fname = name;
            }
            ParserFunction.RegisterFunction(fname,func);
            if (func.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE_ONC))
            {
                Constants.FUNCT_WITH_SPACE_ONCE.Add(fname);
            }
            else if (func.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE))
            {
                Constants.FUNCT_WITH_SPACE.Add(fname);
            }
            if (func.Attribute.HasFlag(FunctionAttribute.CONTROL_FLOW))
            {
                Constants.CONTROL_FLOW.Add(fname) ;
            }
        }
        public static void Remove(FunctionBase func,string name="")
        {
            string fname = name;
            if (fname == "") { fname = func.FunctionName; }
            ParserFunction.UnregisterFunction(fname);
            if (func.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE_ONC))
            {
                Constants.FUNCT_WITH_SPACE_ONCE.Remove(fname);
            }
            else if (func.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE))
            {
                Constants.FUNCT_WITH_SPACE.Remove(fname);
            }
            if (func.Attribute.HasFlag(FunctionAttribute.CONTROL_FLOW))
            {
                Constants.CONTROL_FLOW.Remove(fname);
            }
            
        }
        public static bool Exists(FunctionBase func)
        {
            return ParserFunction.s_functions.ContainsValue(func);
        }
        public static bool Exists(string name)
        {
            return ParserFunction.s_functions.ContainsKey(name);
        }
        public static List<string> Functions
        {
            get { return new List<string>(ParserFunction.s_functions.Keys); }
        }
    }
    public delegate void FunctionBaseEventHandler(object sender, FunctionBaseEventArgs e);
    public class FunctionBaseEventArgs : EventArgs
    {
        /// <summary>
        /// 呼び出し元のオリジナルなスクリプトを表します
        /// </summary>
        public string OriginalScript { get; set; }

        /// <summary>
        /// 現在の関数の戻り値を表します
        /// </summary>
        public Variable Return { get; set; }

        /// <summary>
        /// 現在の関数に対しての引数を表します
        /// </summary>
        public List<Variable> Args { get; set; }

        /// <summary>
        /// [使用されていません]
        /// </summary>
        public bool UseObjectResult { get; set; }

        /// <summary>
        /// [使用されていません]
        /// </summary>
        public object ObjectResult { get; set; }

        /// <summary>
        /// 呼び出し内容を含むスクリプト本文を表します
        /// </summary>
        public ParsingScript Script { get; set; }

        /// <summary>
        /// (Variableオブジェクト内のみ)呼び出し元のオブジェクトを表します
        /// </summary>
        public Variable CurentVariable { get; set; }


    }
   
}
