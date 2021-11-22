using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
    public class OutputAvailableEventArgs : EventArgs
    {
        public OutputAvailableEventArgs(string output)
        {
            Output = output;
        }
        public string Output { get; set; }
    }


    public partial class Interpreter
    {

        private static Interpreter instance;
        private bool m_bHasBeenInitialized = false;

        private Interpreter()
        {
            Init();
        }

        public static Interpreter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Interpreter();
                }
                return instance;
            }
        }

        private int MAX_LOOPS;

        private StringBuilder m_output = new StringBuilder();
        public string Output
        {
            get
            {
                string output = m_output.ToString().Trim();
                m_output.Clear();
                return output;
            }
        }

        public event EventHandler<OutputAvailableEventArgs> OnOutput;
        public event EventHandler<OutputAvailableEventArgs> OnData;
        public event EventHandler<OutputAvailableEventArgs> OnDebug;

        public void AppendOutput(string text, bool newLine = false)
        {
            EventHandler<OutputAvailableEventArgs> handler = OnOutput;
            if (handler != null)
            {
                OutputAvailableEventArgs args = new OutputAvailableEventArgs(text +
                                     (newLine ? Environment.NewLine : string.Empty));
                handler(this, args);
            }
        }
        public void AppendDebug(string text, bool newLine = false)
        {
            EventHandler<OutputAvailableEventArgs> handler = OnDebug;
            if (handler != null)
            {
                OutputAvailableEventArgs args = new OutputAvailableEventArgs(text +
                                     (newLine ? Environment.NewLine : string.Empty));
                handler(this, args);
            }
        }

        public bool AppendData(string text, bool newLine = false)
        {
            EventHandler<OutputAvailableEventArgs> handler = OnData;
            if (handler != null)
            {
                OutputAvailableEventArgs args = new OutputAvailableEventArgs(text +
                                     (newLine ? Environment.NewLine : string.Empty));
                handler(this, args);
                return true;
            }
            return false;
        }

        public void Init()
        {
            if (m_bHasBeenInitialized)
            {
                return;
            }

            m_bHasBeenInitialized = true; // このメソッドは一度のみ呼び出すことができます


            RegisterFunctions();
            RegisterEnums();
            RegisterActions();

            CompiledClass.Init();
        }

        public void RegisterFunctions()
        {
            FunctionBaseManerger.Add(new IfStatement());
            FunctionBaseManerger.Add(new DoWhileStatement());
            FunctionBaseManerger.Add(new WhileStatement());
            FunctionBaseManerger.Add(new SwitchStatement());
            FunctionBaseManerger.Add(new CaseStatement());
            FunctionBaseManerger.Add(new CaseStatement(), Constants.DEFAULT);
            FunctionBaseManerger.Add(new ForStatement());
            FunctionBaseManerger.Add(new ForeachStatement());
            FunctionBaseManerger.Add(new GotoGosubFunction(true));
            FunctionBaseManerger.Add(new GotoGosubFunction(false));
            FunctionBaseManerger.Add(new IncludeFile());
            FunctionBaseManerger.Add(new ThrowFunction());
            FunctionBaseManerger.Add(new TryBlock());

            ParserFunction.RegisterFunction(Constants.CLASS, new ClassCreator());
            ParserFunction.RegisterFunction(Constants.ENUM, new EnumFunction());
            ParserFunction.RegisterFunction(Constants.ISNAN, new IsNaNFunction());
            ParserFunction.RegisterFunction(Constants.NEW, new NewObjectFunction());
            ParserFunction.RegisterFunction(Constants.RETURN, new ReturnStatement());
            ParserFunction.RegisterFunction(Constants.FUNCTION, new FunctionCreator());
            ParserFunction.RegisterFunction(Constants.GET_PROPERTIES, new GetPropertiesFunction());
            ParserFunction.RegisterFunction(Constants.GET_PROPERTY, new GetPropertyFunction());
            ParserFunction.RegisterFunction(Constants.SET_PROPERTY, new SetPropertyFunction());

            FunctionBaseManerger.Add(new ExitFunction());
            FunctionBaseManerger.Add(new wsverFunc());
            FunctionBaseManerger.Add(new DelayFunc());
            FunctionBaseManerger.Add(new ImportFunc());
            FunctionBaseManerger.Add(new ImportFunc(true));
            FunctionBaseManerger.Add(new DllImportFunc());
            FunctionBaseManerger.Add(new IceImportFunc());
            FunctionBaseManerger.Add(new DelegateCreator());
            FunctionBaseManerger.Add(new DelegateCreator(), "_");
            FunctionBaseManerger.Add(new PrintFunction());
            FunctionBaseManerger.Add(new PrintFunction(true));
            FunctionBaseManerger.Add(new StringFormatFunction());
            FunctionBaseManerger.Add(new LockFunction());

            ParserFunction.RegisterFunction(Constants.ADD_TO_HASH, new AddVariableToHashFunction());
            ParserFunction.RegisterFunction(Constants.ADD_ALL_TO_HASH, new AddVariablesToHashFunction());
            ParserFunction.RegisterFunction(Constants.CANCEL, new CancelFunction());
            ParserFunction.RegisterFunction(Constants.DEFINE_LOCAL, new DefineLocalFunction());
            ParserFunction.RegisterFunction(Constants.GET_COLUMN, new GetColumnFunction());
            ParserFunction.RegisterFunction(Constants.GET_KEYS, new GetAllKeysFunction());
            ParserFunction.RegisterFunction(Constants.NAMESPACE, new NamespaceFunction());
            ParserFunction.RegisterFunction(Constants.SINGLETON, new SingletonFunction());
            ParserFunction.RegisterFunction(Constants.VAR, new VarFunction());

            ParserFunction.RegisterFunction(Constants.ADD_DATA, new DataFunction(DataFunction.DataMode.ADD));
            ParserFunction.RegisterFunction(Constants.COLLECT_DATA, new DataFunction(DataFunction.DataMode.SUBSCRIBE));
            ParserFunction.RegisterFunction(Constants.GET_DATA, new DataFunction(DataFunction.DataMode.SEND));

            ParserFunction.RegisterFunction(Constants.OBJECT_DEFPROP, new ObjectPropsFunction());

            ParserFunction.AddAction(Constants.LABEL_OPERATOR, new LabelFunction());
            ParserFunction.AddAction(Constants.POINTER, new PointerFunction());
            ParserFunction.AddAction(Constants.POINTER_REF, new PointerReferenceFunction());

            if (File.Exists(Alice.Runtime_File_Path))
            {
                Interop.NetLibraryLoader.LoadLibrary(Alice.Runtime_File_Path);
            }

            VariableFunctionIniter.Init();
        }

        public void RegisterEnums()
        {
            //ParserFunction.RegisterEnum(Constants.VARIABLE_TYPE, "AliceScript.Variable.VarType");
        }

        public void RegisterActions()
        {
            ParserFunction.AddAction(Constants.ASSIGNMENT, new AssignFunction());
            ParserFunction.AddAction(Constants.INCREMENT, new IncrementDecrementFunction());
            ParserFunction.AddAction(Constants.DECREMENT, new IncrementDecrementFunction());

            for (int i = 0; i < Constants.OPER_ACTIONS.Length; i++)
            {
                ParserFunction.AddAction(Constants.OPER_ACTIONS[i], new OperatorAssignFunction());
            }
        }

        public Variable ProcessFile(string filename, bool mainFile = false)
        {
            string script = Utils.GetFileContents(filename);
            return Process(script, filename, mainFile);
        }

        public async Task<Variable> ProcessFileAsync(string filename, bool mainFile = false)
        {
            string script = Utils.GetFileContents(filename);
            Variable result = await ProcessAsync(script, filename, mainFile);
            return result;
        }

        public Variable Process(string script, string filename = "", bool mainFile = false,object tag=null,AlicePackage package=null)
        {
            Dictionary<int, int> char2Line;
            string data = Utils.ConvertToScript(script, out char2Line, filename);
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            ParsingScript toParse = new ParsingScript(data, 0, char2Line);
            toParse.OriginalScript = script;
            toParse.Filename = filename;
            toParse.Tag = tag;
            toParse.Package = package;

            if (mainFile)
            {
                toParse.MainFilename = toParse.Filename;
            }

            Variable result = null;

            while (toParse.Pointer < data.Length)
            {
                result = toParse.Execute();
                toParse.GoToNextStatement();
            }
            if (Interop.GCManerger.CollectAfterExecute)
            {
                GC.Collect();
            }
            return result;
        }
        public async Task<Variable> ProcessAsync(string script, string filename = "", bool mainFile = false)
        {
            Dictionary<int, int> char2Line;
            string data = Utils.ConvertToScript(script, out char2Line, filename);
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            ParsingScript toParse = new ParsingScript(data, 0, char2Line);
            toParse.OriginalScript = script;
            toParse.Filename = filename;

            if (mainFile)
            {
                toParse.MainFilename = toParse.Filename;
            }

            Variable result = null;

            while (toParse.Pointer < data.Length)
            {
                result = await toParse.ExecuteAsync();
                toParse.GoToNextStatement();
            }

            return result;
        }

        public Variable ProcessFor(ParsingScript script)
        {
            string forString = Utils.GetBodyBetween(script, Constants.START_ARG, Constants.END_ARG);
            script.Forward();
            //for(init; condition; loopStatemen;)の形式です
            ProcessCanonicalFor(script, forString);
            return Variable.EmptyInstance;
        }
        public async Task<Variable> ProcessForAsync(ParsingScript script)
        {
            string forString = Utils.GetBodyBetween(script, Constants.START_ARG, Constants.END_ARG);
            script.Forward();
            //for(init; condition; loopStatemen;)の形式です
            await ProcessCanonicalForAsync(script, forString);
            return Variable.EmptyInstance;
        }
        public Variable ProcessForeach(ParsingScript script)
        {
            string forString = Utils.GetBodyBetween(script, Constants.START_ARG, Constants.END_ARG);
            script.Forward();
            //foreach(var in ary)の形式です
            //AliceScript925からforeach(var : ary)またはforeach(var of ary)の形は使用できなくなりました。同じ方法をとるとき、複数の方法が存在するのは好ましくありません。
            ProcessArrayFor(script, forString);
            return Variable.EmptyInstance;
        }
        public async Task<Variable> ProcessForeachAsync(ParsingScript script)
        {
            string forString = Utils.GetBodyBetween(script, Constants.START_ARG, Constants.END_ARG);
            script.Forward();

            //foreach(var in ary)の形式です
            //AliceScript925からforeach(var : ary)またはforeach(var of ary)の形は使用できなくなりました。同じ方法をとるとき、複数の方法が存在するのは好ましくありません。
            await ProcessArrayForAsync(script, forString);
            return Variable.EmptyInstance;
        }

        private void ProcessArrayFor(ParsingScript script, string forString)
        {
            var tokens = forString.Split(' ');
            var sep = tokens.Length > 2 ? tokens[1] : "";
            string varName = tokens[0];
            //AliceScript925からforeach(var : ary)またはforeach(var of ary)の形は使用できなくなりました。同じ方法をとるとき、複数の方法が存在するのは好ましくありません。

            if (sep != Constants.FOR_IN)
            {
                int index = forString.IndexOf(Constants.FOR_EACH);
                if (index <= 0 || index == forString.Length - 1)
                {
                    Utils.ThrowErrorMsg("foreach文はforeach(variable in array)の形をとるべきです", Exceptions.INVALID_SYNTAX
                                     , script, Constants.FOREACH);
                }
                varName = forString.Substring(0, index);
            }

            ParsingScript forScript = script.GetTempScript(forString, varName.Length + sep.Length + 1);

            Variable arrayValue = Utils.GetItem(forScript);

            if (arrayValue.Type == Variable.VarType.STRING)
            {
                arrayValue = new Variable(new List<string>(arrayValue.ToString().ToCharArray().Select(c => c.ToString())));
            }

            int cycles = arrayValue.Count;
            if (cycles == 0)
            {
                SkipBlock(script);
                return;
            }
            int startForCondition = script.Pointer;

            for (int i = 0; i < cycles; i++)
            {
                script.Pointer = startForCondition;
                Variable current = arrayValue.GetValue(i);
                ParserFunction.AddGlobalOrLocalVariable(varName,
                               new GetVarFunction(current));
                Variable result = ProcessBlock(script);
                if (result.IsReturn || result.Type == Variable.VarType.BREAK)
                {
                    //script.Pointer = startForCondition;
                    //SkipBlock(script);
                    //return;
                    break;
                }
            }
            script.Pointer = startForCondition;
            SkipBlock(script);
        }

        private async Task ProcessArrayForAsync(ParsingScript script, string forString)
        {
            var tokens = forString.Split(' ');
            var sep = tokens.Length > 2 ? tokens[1] : "";
            string varName = tokens[0];
            //AliceScript925からforeach(var : ary)またはforeach(var of ary)の形は使用できなくなりました。同じ方法をとるとき、複数の方法が存在するのは好ましくありません。

            if (sep != Constants.FOR_IN)
            {
                int index = forString.IndexOf(Constants.FOR_EACH);
                if (index <= 0 || index == forString.Length - 1)
                {
                    Utils.ThrowErrorMsg("foreach文はforeach(variable in array)の形をとるべきです", Exceptions.INVALID_SYNTAX,
                                     script, Constants.FOREACH);
                }
                varName = forString.Substring(0, index);
            }

            ParsingScript forScript = script.GetTempScript(forString, varName.Length + sep.Length + 1);

            Variable arrayValue = await Utils.GetItemAsync(forScript);

            if (arrayValue.Type == Variable.VarType.STRING)
            {
                arrayValue = new Variable(new List<string>(arrayValue.ToString().ToCharArray().Select(c => c.ToString())));
            }

            int cycles = arrayValue.Count;
            if (cycles == 0)
            {
                SkipBlock(script);
                return;
            }
            int startForCondition = script.Pointer;

            for (int i = 0; i < cycles; i++)
            {
                script.Pointer = startForCondition;
                Variable current = arrayValue.GetValue(i);
                ParserFunction.AddGlobalOrLocalVariable(varName,
                               new GetVarFunction(current));
                Variable result = await ProcessBlockAsync(script);
                if (result.IsReturn || result.Type == Variable.VarType.BREAK)
                {
                    //script.Pointer = startForCondition;
                    //SkipBlock(script);
                    //return;
                    break;
                }
            }
            script.Pointer = startForCondition;
            SkipBlock(script);
        }

        private void ProcessCanonicalFor(ParsingScript script, string forString)
        {
            string[] forTokens = forString.Split(Constants.END_STATEMENT);
            if (forTokens.Length != 3)
            {
                Utils.ThrowErrorMsg("for文はfor(init; condition; loopStatement;)の形である必要があります", Exceptions.INVALID_SYNTAX,
                                     script, Constants.FOR);
            }

            int startForCondition = script.Pointer;

            ParsingScript initScript = script.GetTempScript(forTokens[0] + Constants.END_STATEMENT);
            ParsingScript condScript = script.GetTempScript(forTokens[1] + Constants.END_STATEMENT);
            ParsingScript loopScript = script.GetTempScript(forTokens[2] + Constants.END_STATEMENT);

            initScript.Execute(null, 0);

            int cycles = 0;
            bool stillValid = true;

            while (stillValid)
            {
                Variable condResult = condScript.Execute(null, 0);
                stillValid = condResult.AsBool();
                if (!stillValid)
                {
                    break;
                }

                if (MAX_LOOPS > 0 && ++cycles >= MAX_LOOPS)
                {
                    ThrowErrorManerger.OnThrowError("現在の設定では" + MAX_LOOPS + "以上の繰り返しを行うことはできません", Exceptions.TOO_MANY_REPETITIONS, script);
                    return;
                }

                script.Pointer = startForCondition;
                Variable result = ProcessBlock(script);
                if (result.IsReturn || result.Type == Variable.VarType.BREAK)
                {
                    //script.Pointer = startForCondition;
                    //SkipBlock(script);
                    //return;
                    break;
                }
                loopScript.Execute(null, 0);
            }

            script.Pointer = startForCondition;
            SkipBlock(script);
        }

        private async Task ProcessCanonicalForAsync(ParsingScript script, string forString)
        {
            string[] forTokens = forString.Split(Constants.END_STATEMENT);
            if (forTokens.Length != 3)
            {
                Utils.ThrowErrorMsg("for文はfor(init; condition; loopStatement;)の形である必要があります", Exceptions.INVALID_SYNTAX,
                                     script, Constants.FOR);
            }

            int startForCondition = script.Pointer;

            ParsingScript initScript = script.GetTempScript(forTokens[0] + Constants.END_STATEMENT);
            ParsingScript condScript = script.GetTempScript(forTokens[1] + Constants.END_STATEMENT);
            ParsingScript loopScript = script.GetTempScript(forTokens[2] + Constants.END_STATEMENT);

            await initScript.ExecuteAsync(null, 0);

            int cycles = 0;
            bool stillValid = true;

            while (stillValid)
            {
                Variable condResult = await condScript.ExecuteAsync(null, 0);
                stillValid = condResult.AsBool();
                if (!stillValid)
                {
                    break;
                }

                if (MAX_LOOPS > 0 && ++cycles >= MAX_LOOPS)
                {
                    ThrowErrorManerger.OnThrowError("現在の設定では" + MAX_LOOPS + "以上の繰り返しを行うことはできません", Exceptions.TOO_MANY_REPETITIONS, script);
                    return;
                }

                script.Pointer = startForCondition;
                Variable result = await ProcessBlockAsync(script);
                if (result.IsReturn || result.Type == Variable.VarType.BREAK)
                {
                    //script.Pointer = startForCondition;
                    //SkipBlock(script);
                    //return;
                    break;
                }
                await loopScript.ExecuteAsync(null, 0);
            }

            script.Pointer = startForCondition;
            SkipBlock(script);
        }

        public Variable ProcessWhile(ParsingScript script)
        {
            int startWhileCondition = script.Pointer;

            // 無限ループを抑制するための変数
            int cycles = 0;
            bool stillValid = true;
            Variable result = Variable.EmptyInstance;

            while (stillValid)
            {
                script.Pointer = startWhileCondition;

                //int startSkipOnBreakChar = from;
                Variable condResult = script.Execute(Constants.END_ARG_ARRAY);
                stillValid = condResult.AsBool();
                if (!stillValid)
                {
                    break;
                }

                // 無限ループを抑制するための判定
                if (MAX_LOOPS > 0 && ++cycles >= MAX_LOOPS)
                {
                    ThrowErrorManerger.OnThrowError("このインタプリタでは" + MAX_LOOPS + "以上の繰り返しを行うことはできません", Exceptions.TOO_MANY_REPETITIONS, script);
                    return Variable.EmptyInstance;
                }

                result = ProcessBlock(script);
                if (result.IsReturn || result.Type == Variable.VarType.BREAK)
                {
                    script.Pointer = startWhileCondition;
                    break;
                }
            }

            //whileステートメントの条件がtrueでなくなったためこのブロックをスキップ
            SkipBlock(script);
            return result.IsReturn ? result : Variable.EmptyInstance;
        }
        //AliceScript925からNWhileは実装されなくなりました。否定条件のループはwhile(!bool)を使用するべきです
        public async Task<Variable> ProcessWhileAsync(ParsingScript script)
        {
            int startWhileCondition = script.Pointer;

            // A check against an infinite loop.
            int cycles = 0;
            bool stillValid = true;
            Variable result = Variable.EmptyInstance;

            while (stillValid)
            {
                script.Pointer = startWhileCondition;

                //int startSkipOnBreakChar = from;
                Variable condResult = await script.ExecuteAsync(Constants.END_ARG_ARRAY);
                stillValid = condResult.AsBool();
                if (!stillValid)
                {
                    break;
                }

                // Check for an infinite loop if we are comparing same values:
                if (MAX_LOOPS > 0 && ++cycles >= MAX_LOOPS)
                {
                    ThrowErrorManerger.OnThrowError("繰り返しの回数が多すぎます", Exceptions.TOO_MANY_REPETITIONS);
                }

                result = await ProcessBlockAsync(script);
                if (result.IsReturn || result.Type == Variable.VarType.BREAK)
                {
                    script.Pointer = startWhileCondition;
                    break;
                }
            }

            // The while condition is not true anymore: must skip the whole while
            // block before continuing with next statements.
            SkipBlock(script);
            return result.IsReturn ? result : Variable.EmptyInstance;
        }


        public Variable ProcessDoWhile(ParsingScript script)
        {
            int startDoCondition = script.Pointer;
            bool stillValid = true;
            Variable result = Variable.EmptyInstance;

            while (stillValid)
            {
                script.Pointer = startDoCondition;

                result = ProcessBlock(script);
                if (result.IsReturn || result.Type == Variable.VarType.BREAK)
                {
                    script.Pointer = startDoCondition;
                    break;
                }
                script.Forward(Constants.WHILE.Length + 1);
                Variable condResult = script.Execute(Constants.END_ARG_ARRAY);
                stillValid = condResult.AsBool();
                if (!stillValid)
                {
                    break;
                }
            }

            SkipBlock(script);
            return result.IsReturn ? result : Variable.EmptyInstance;
        }

        public Variable ProcessCase(ParsingScript script, string reason)
        {
            if (reason == Constants.CASE)
            {
                /*var token = */
                Utils.GetToken(script, Constants.TOKEN_SEPARATION);
            }
            script.MoveForwardIf(':');

            Variable result = ProcessBlock(script);
            script.MoveBackIfPrevious('}');

            return result;
        }

        public Variable ProcessSwitch(ParsingScript script)
        {
            Variable switchValue = Utils.GetItem(script);
            script.Forward();

            Variable result = Variable.EmptyInstance;
            var caseSep = ":".ToCharArray();

            bool caseDone = false;

            while (script.StillValid())
            {
                var nextToken = Utils.GetBodySize(script, Constants.CASE, Constants.DEFAULT);
                if (string.IsNullOrEmpty(nextToken))
                {
                    break;
                }
                if (nextToken == Constants.DEFAULT && !caseDone)
                {
                    result = ProcessBlock(script);
                    break;
                }
                if (!caseDone)
                {
                    Variable caseValue = script.Execute(caseSep);
                    script.Forward();

                    if (switchValue.Type == caseValue.Type && switchValue.Equals(caseValue))
                    {
                        caseDone = true;
                        result = ProcessBlock(script);
                        if (script.Prev == '}')
                        {
                            break;
                        }
                        script.Forward();
                    }
                }
            }
            script.MoveForwardIfNotPrevious('}');
            script.GoToNextStatement();
            return result;
        }

        public Variable ProcessIf(ParsingScript script)
        {
            int startIfCondition = script.Pointer;

            Variable result = script.Execute(Constants.END_ARG_ARRAY);
            bool isTrue = result.AsBool();

            if (isTrue)
            {
                result = ProcessBlock(script);

                if (result.IsReturn ||
                    result.Type == Variable.VarType.BREAK ||
                    result.Type == Variable.VarType.CONTINUE)
                {
                    // We are here from the middle of the if-block. Skip it.
                    script.Pointer = startIfCondition;
                    SkipBlock(script);
                }
                SkipRestBlocks(script);

                //return result;
                return result.IsReturn ||
                       result.Type == Variable.VarType.BREAK ||
                       result.Type == Variable.VarType.CONTINUE ? result : Variable.EmptyInstance;
            }

            // We are in Else. Skip everything in the If statement.
            SkipBlock(script);

            ParsingScript nextData = new ParsingScript(script);
            nextData.ParentScript = script;

            string nextToken = Utils.GetNextToken(nextData);

            if (Constants.ELSE_IF == nextToken)
            {
                script.Pointer = nextData.Pointer + 1;
                result = ProcessIf(script);
            }
            else if (Constants.ELSE == nextToken)
            {
                script.Pointer = nextData.Pointer + 1;
                result = ProcessBlock(script);
            }

            return result.IsReturn ||
                   result.Type == Variable.VarType.BREAK ||
                   result.Type == Variable.VarType.CONTINUE ? result : Variable.EmptyInstance;
        }

        public async Task<Variable> ProcessIfAsync(ParsingScript script)
        {
            int startIfCondition = script.Pointer;

            Variable result = await script.ExecuteAsync(Constants.END_ARG_ARRAY);
            bool isTrue = result.AsBool();

            if (isTrue)
            {
                result = await ProcessBlockAsync(script);

                if (result.IsReturn ||
                    result.Type == Variable.VarType.BREAK ||
                    result.Type == Variable.VarType.CONTINUE)
                {
                    // We are here from the middle of the if-block. Skip it.
                    script.Pointer = startIfCondition;
                    SkipBlock(script);
                }
                SkipRestBlocks(script);

                //return result;
                return result.IsReturn ||
                       result.Type == Variable.VarType.BREAK ||
                       result.Type == Variable.VarType.CONTINUE ? result : Variable.EmptyInstance;
            }

            // We are in Else. Skip everything in the If statement.
            SkipBlock(script);

            ParsingScript nextData = new ParsingScript(script);
            nextData.ParentScript = script;

            string nextToken = Utils.GetNextToken(nextData);

            if (Constants.ELSE_IF == nextToken)
            {
                script.Pointer = nextData.Pointer + 1;
                result = await ProcessIfAsync(script);
            }
            else if (Constants.ELSE == nextToken)
            {
                script.Pointer = nextData.Pointer + 1;
                result = await ProcessBlockAsync(script);
            }

            return result.IsReturn ||
                   result.Type == Variable.VarType.BREAK ||
                   result.Type == Variable.VarType.CONTINUE ? result : Variable.EmptyInstance;
        }

        public Variable ProcessTry(ParsingScript script)
        {
            int startTryCondition = script.Pointer - 1;
            int currentStackLevel = ParserFunction.GetCurrentStackLevel();
            Exception exception = null;

            Variable result = null;

            bool alreadyInTryBlock = script.InTryBlock;
            script.InTryBlock = true;
            try
            {
                result = ProcessBlock(script);
            }
            catch (Exception exc)
            {
                exception = exc;
            }
            finally
            {
                script.InTryBlock = alreadyInTryBlock;
            }

            if (exception != null || result.IsReturn ||
                result.Type == Variable.VarType.BREAK ||
                result.Type == Variable.VarType.CONTINUE)
            {
                // We are here from the middle of the try-block either because
                // an exception was thrown or because of a Break/Continue. Skip it.
                script.Pointer = startTryCondition;
                SkipBlock(script);
            }

            string catchToken = Utils.GetNextToken(script);
            script.Forward(); // skip opening parenthesis
                              // The next token after the try block must be a catch.
            if (Constants.CATCH != catchToken)
            {
                ThrowErrorManerger.OnThrowError("Catchステートメントがありません", Exceptions.MISSING_CATCH_STATEMENT, script);
            }

            string exceptionName = Utils.GetNextToken(script);
            script.Forward(); // skip closing parenthesis

            if (exception != null)
            {
                string excStack = CreateExceptionStack(exceptionName, currentStackLevel);
                ParserFunction.InvalidateStacksAfterLevel(currentStackLevel);

                GetVarFunction excMsgFunc = new GetVarFunction(new Variable(exception.Message));
                ParserFunction.AddGlobalOrLocalVariable(exceptionName, excMsgFunc);
                GetVarFunction excStackFunc = new GetVarFunction(new Variable(excStack));
                ParserFunction.AddGlobalOrLocalVariable(exceptionName + ".Stack", excStackFunc);

                result = ProcessBlock(script);
                ParserFunction.PopLocalVariable(exceptionName);
            }
            else
            {
                SkipBlock(script);
            }

            SkipRestBlocks(script);
            return result;
        }
        public async Task<Variable> ProcessTryAsync(ParsingScript script)
        {
            int startTryCondition = script.Pointer - 1;
            int currentStackLevel = ParserFunction.GetCurrentStackLevel();
            Exception exception = null;

            Variable result = null;

            bool alreadyInTryBlock = script.InTryBlock;
            script.InTryBlock = true;
            try
            {
                result = await ProcessBlockAsync(script);
            }
            catch (Exception exc)
            {
                exception = exc;
            }
            finally
            {
                script.InTryBlock = alreadyInTryBlock;
            }

            if (exception != null || result.IsReturn ||
                result.Type == Variable.VarType.BREAK ||
                result.Type == Variable.VarType.CONTINUE)
            {
                // We are here from the middle of the try-block either because
                // an exception was thrown or because of a Break/Continue. Skip it.
                script.Pointer = startTryCondition;
                SkipBlock(script);
            }

            string catchToken = Utils.GetNextToken(script);
            script.Forward(); // skip opening parenthesis
                              // The next token after the try block must be a catch.
            if (Constants.CATCH != catchToken)
            {
                ThrowErrorManerger.OnThrowError("Catchステートメントがありません", Exceptions.MISSING_CATCH_STATEMENT, script);
            }

            string exceptionName = Utils.GetNextToken(script);
            script.Forward(); // skip closing parenthesis

            if (exception != null)
            {
                string excStack = CreateExceptionStack(exceptionName, currentStackLevel);
                ParserFunction.InvalidateStacksAfterLevel(currentStackLevel);

                GetVarFunction excMsgFunc = new GetVarFunction(new Variable(exception.Message));
                ParserFunction.AddGlobalOrLocalVariable(exceptionName, excMsgFunc);
                GetVarFunction excStackFunc = new GetVarFunction(new Variable(excStack));
                ParserFunction.AddGlobalOrLocalVariable(exceptionName + ".Stack", excStackFunc);

                result = await ProcessBlockAsync(script);
                ParserFunction.PopLocalVariable(exceptionName);
            }
            else
            {
                SkipBlock(script);
            }

            SkipRestBlocks(script);
            return result;
        }

        private static string CreateExceptionStack(string exceptionName, int lowestStackLevel)
        {
            string result = "";
            Stack<ParserFunction.StackLevel> stack = ParserFunction.ExecutionStack;
            int level = stack.Count;
            foreach (ParserFunction.StackLevel stackLevel in stack)
            {
                if (level-- < lowestStackLevel)
                {
                    break;
                }
                if (string.IsNullOrWhiteSpace(stackLevel.Name))
                {
                    continue;
                }
                result += Environment.NewLine + "  " + stackLevel.Name + "()";
            }

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = " --> " + exceptionName + result;
            }

            return result;
        }
        public static string GetStack(int lowestStackLevel = 0)
        {
            string result = "";
            Stack<ParserFunction.StackLevel> stack = ParserFunction.ExecutionStack;
            int level = stack.Count;
            foreach (ParserFunction.StackLevel stackLevel in stack)
            {
                if (level-- < lowestStackLevel)
                {
                    break;
                }
                if (string.IsNullOrWhiteSpace(stackLevel.Name))
                {
                    continue;
                }
                result += Environment.NewLine + "  " + stackLevel.Name + "()";
            }

            return result;
        }

        private Variable ProcessBlock(ParsingScript script)
        {
            int blockStart = script.Pointer;
            Variable result = null;

            while (script.StillValid())
            {
                int endGroupRead = script.GoToNextStatement();
                if (endGroupRead > 0 || !script.StillValid())
                {
                    return result != null ? result : new Variable();
                }

                result = script.Execute();

                if (result.IsReturn ||
                    result.Type == Variable.VarType.BREAK ||
                    result.Type == Variable.VarType.CONTINUE)
                {
                    return result;
                }
            }
            return result;
        }

        private async Task<Variable> ProcessBlockAsync(ParsingScript script)
        {
            int blockStart = script.Pointer;
            Variable result = null;

            while (script.StillValid())
            {
                int endGroupRead = script.GoToNextStatement();
                if (endGroupRead > 0 || !script.StillValid())
                {
                    return result != null ? result : new Variable();
                }

                result = await script.ExecuteAsync();

                if (result.IsReturn ||
                    result.Type == Variable.VarType.BREAK ||
                    result.Type == Variable.VarType.CONTINUE)
                {
                    return result;
                }
            }
            return result;
        }

        private void SkipBlock(ParsingScript script)
        {
            int blockStart = script.Pointer;
            int startCount = 0;
            int endCount = 0;
            bool inQuotes = false;
            bool inQuotes1 = false;
            bool inQuotes2 = false;
            char previous = Constants.EMPTY;
            char prevprev = Constants.EMPTY;
            while (startCount == 0 || startCount > endCount)
            {
                if (!script.StillValid())
                {
                    ThrowErrorManerger.OnThrowError("次のブロックを実行できませんでした [" +
                    script.Substr(blockStart, Constants.MAX_CHARS_TO_SHOW) + "]", Exceptions.COULDNT_EXECUTE_BLOCK, script);
                }
                char currentChar = script.CurrentAndForward();
                switch (currentChar)
                {
                    case Constants.QUOTE1:
                        if (!inQuotes2 && (previous != '\\' || prevprev == '\\'))
                        {
                            inQuotes = inQuotes1 = !inQuotes1;
                        }
                        break;
                    case Constants.QUOTE:
                        if (!inQuotes1 && (previous != '\\' || prevprev == '\\'))
                        {
                            inQuotes = inQuotes2 = !inQuotes2;
                        }
                        break;
                    case Constants.START_GROUP:
                        if (!inQuotes)
                        {
                            startCount++;
                        }
                        break;
                    case Constants.END_GROUP:
                        if (!inQuotes)
                        {
                            endCount++;
                        }
                        break;
                }
                prevprev = previous;
                previous = currentChar;
            }
            if (startCount > endCount)
            {
                ThrowErrorManerger.OnThrowError("波括弧が必要です", Exceptions.NEED_BRACKETS, script);
            }
            else if (startCount < endCount)
            {
                ThrowErrorManerger.OnThrowError("終端の波括弧は不要です", Exceptions.UNNEED_TO_BRACKETS, script);
            }
        }

        private void SkipRestBlocks(ParsingScript script)
        {
            while (script.StillValid())
            {
                int endOfToken = script.Pointer;
                ParsingScript nextData = new ParsingScript(script);
                string nextToken = Utils.GetNextToken(nextData);
                if (Constants.ELSE_IF != nextToken &&
                    Constants.ELSE != nextToken)
                {
                    return;
                }
                script.Pointer = nextData.Pointer;
                SkipBlock(script);
            }
        }

        public static Variable Run(string functionName, Variable arg1 = null, Variable arg2 = null, Variable arg3 = null, ParsingScript script = null)
        {
            System.Threading.Tasks.Task<Variable> task = null;
            try
            {
                task = CustomFunction.Run(functionName, arg1, arg2, arg3, script);
            }
            catch (Exception exc)
            {
                task = CustomFunction.Run(Constants.ON_EXCEPTION, new Variable(functionName),
                                          new Variable(exc.Message), arg2, script);
                if (task == null)
                {
                    throw;
                }
            }
            return task == null ? Variable.EmptyInstance : task.Result;
        }


        public static Variable Run(CustomFunction function, List<Variable> args, ParsingScript script = null)
        {
            Variable result = null;
            try
            {
                result = function.Run(args, script);
            }
            catch (Exception exc)
            {
                var task = CustomFunction.Run(Constants.ON_EXCEPTION, new Variable(function.Name),
                                          new Variable(exc.Message), args.Count > 0 ? args[0] : Variable.EmptyInstance, script);
                if (task == null)
                {
                    throw;
                }
                result = task.Result;
            }
            return result;
        }
    }
}

