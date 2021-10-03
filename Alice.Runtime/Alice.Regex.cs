using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AliceScript.NameSpaces
{
    static class Alice_Regex_Initer
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.Regex");

            space.Add(new RegexSingleArgFunc(RegexSingleArgFunc.FuncMode.Escape));
            space.Add(new RegexSingleArgFunc(RegexSingleArgFunc.FuncMode.IsMatch));
            space.Add(new RegexSingleArgFunc(RegexSingleArgFunc.FuncMode.Match));
            space.Add(new RegexSingleArgFunc(RegexSingleArgFunc.FuncMode.Matches));
            space.Add(new RegexSingleArgFunc(RegexSingleArgFunc.FuncMode.Replace));
            space.Add(new RegexSingleArgFunc(RegexSingleArgFunc.FuncMode.Split));

            NameSpaceManerger.Add(space);
        }
    }
    class RegexSingleArgFunc : FunctionBase
    {
        public enum FuncMode
        {
            Escape,IsMatch,Match,Matches,Replace,Split
        }
        
        public RegexSingleArgFunc(FuncMode mode)
        {
            Mode = mode;
           
            this.Run += RegexSingleArgFunc_Run;
            switch (Mode)
            {
                case FuncMode.Escape:
                    {
                        this.Name = "Regex_Escape";
                        this.MinimumArgCounts = 1;
                        break;
                    }
                case FuncMode.IsMatch:
                    {
                        this.Name = "Regex_IsMatch";
                        this.MinimumArgCounts = 2;
                        break;
                    }
                case FuncMode.Match:
                    {
                        this.Name = "Regex_Match";
                        this.MinimumArgCounts = 2;
                        break;
                    }
                case FuncMode.Matches:
                    {
                        this.Name = "Regex_Matches";
                        this.MinimumArgCounts = 2;
                        break;
                    }
                case FuncMode.Replace:
                    {
                        this.Name = "Regex_Replace";
                        this.MinimumArgCounts = 3;
                        break;
                    }
                case FuncMode.Split:
                    {
                        this.Name = "Regex_Split";
                        this.MinimumArgCounts = 2;
                        break;
                    }
            }
        }

        private void RegexSingleArgFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (Mode)
            {
                case FuncMode.Escape:
                    {
                        e.Return = new Variable(Regex.Escape(e.Args[0].AsString()));
                        break;
                    }
                case FuncMode.IsMatch:
                    {
                            e.Return = new Variable(Regex.IsMatch(e.Args[0].AsString(),e.Args[1].AsString()));
                        break;
                    }
                case FuncMode.Match:
                    {
                        e.Return = new Variable(Regex.Match(e.Args[0].AsString(), e.Args[1].AsString()).Value);
                        break;
                    }
                case FuncMode.Matches:
                    {
                        Variable v = new Variable(Variable.VarType.ARRAY);
                        foreach(Match m in Regex.Matches(e.Args[0].AsString(), e.Args[1].AsString()))
                        {
                            v.Tuple.Add(new Variable(m.Value));
                        }
                        e.Return = v;
                        break;
                    }
                case FuncMode.Replace:
                    {
                        e.Return = new Variable(Regex.Replace(e.Args[0].AsString(),e.Args[1].AsString(),e.Args[2].AsString()));
                        break;
                    }
                case FuncMode.Split:
                    {
                        e.Return = new Variable(Regex.Split(e.Args[0].AsString(),e.Args[1].AsString()));
                        break;
                    }
            }
        }

        private FuncMode Mode;
    }
}
