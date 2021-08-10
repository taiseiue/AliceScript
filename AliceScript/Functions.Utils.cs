using System.Collections.Generic;

namespace AliceScript
{
   
 
   


    class LabelFunction : ActionFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            // Just skip this label. m_name is equal to the lable name.
            return Variable.EmptyInstance;
        }
    }
    //デリゲートを作成する関数クラスです
    class DelegateCreator : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {


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

            string body = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP);
            script.MoveForwardIf(Constants.END_GROUP);

            CustomFunction customFunc = new CustomFunction(" ", body, args, script);
            customFunc.ParentScript = script;
            customFunc.ParentOffset = parentOffset;



            return new Variable(customFunc);
        }
    }
    class PointerFunction : ActionFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<string> args = Utils.GetTokens(script);
            Utils.CheckArgs(args.Count, 1, m_name);

            var result = new Variable(Variable.VarType.POINTER);
            result.Pointer = args[0];
            ParserFunction.AddGlobalOrLocalVariable(m_name,
                                        new GetVarFunction(result), script);
            return result;
        }
    }

    class PointerReferenceFunction : ActionFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            var pointer = Utils.GetToken(script, Constants.TOKEN_SEPARATION);

            var result = GetRefValue(pointer, script);
            return result;
        }

        public Variable GetRefValue(string pointer, ParsingScript script)
        {
            if (string.IsNullOrWhiteSpace(pointer))
            {
                return Variable.Undefined;
            }
            var refPointer = ParserFunction.GetVariable(pointer, null, true) as GetVarFunction;
            if (refPointer == null || string.IsNullOrWhiteSpace(refPointer.Value.Pointer))
            {
                return Variable.Undefined;
            }

            var result = ParserFunction.GetVariable(refPointer.Value.Pointer, null, true);
            if (result is GetVarFunction)
            {
                return ((GetVarFunction)result).Value;
            }

            if (result is CustomFunction)
            {
                script.Forward();
                List<Variable> args = script.GetFunctionArgs();
                return ((CustomFunction)result).Run(args, script);
            }
            return Variable.Undefined;
        }
    }

    class GotoGosubFunction : ParserFunction
    {
        bool m_isGoto = true;

        public GotoGosubFunction(bool gotoMode = true)
        {
            m_isGoto = gotoMode;
        }

        protected override Variable Evaluate(ParsingScript script)
        {
            var labelName = Utils.GetToken(script, Constants.TOKEN_SEPARATION);

            Dictionary<string, int> labels;
            if (script.AllLabels == null || script.LabelToFile == null |
               !script.AllLabels.TryGetValue(script.FunctionName, out labels))
            {
                Utils.ThrowErrorMsg("Couldn't find labels in function [" + script.FunctionName + "].",
                                    script, m_name);
                return Variable.EmptyInstance;
            }

            int gotoPointer;
            if (!labels.TryGetValue(labelName, out gotoPointer))
            {
                Utils.ThrowErrorMsg("Couldn't find label [" + labelName + "].",
                                    script, m_name);
                return Variable.EmptyInstance;
            }

            string filename;
            if (script.LabelToFile.TryGetValue(labelName, out filename) &&
                filename != script.Filename && !string.IsNullOrWhiteSpace(filename))
            {
                var newScript = script.GetIncludeFileScript(filename);
                script.Filename = filename;
                script.String = newScript.String;
            }

            if (!m_isGoto)
            {
                script.PointersBack.Add(script.Pointer);
            }

            script.Pointer = gotoPointer;
            if (string.IsNullOrWhiteSpace(script.FunctionName))
            {
                script.Backward();
            }

            return Variable.EmptyInstance;
        }
    }

 
    
}
