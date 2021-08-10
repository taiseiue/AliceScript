using System;
using System.Collections.Generic;
using System.Text;

namespace AliceScript.NameSpaces
{
  static class AliceScript_Diagnosis_Initer
    {
        public static void Init()
        {
            //名前空間のメインエントリポイントです。
            NameSpace space = new NameSpace("Alice.Diagnosis");

            space.Add(new gc_collectFunc());
            space.Add(new gc_gettotalmemoryFunc());
            space.Add(new gc_collectafterexecuteFunc());

            NameSpaceManerger.Add(space);
        }
    }
    class gc_collectFunc : FunctionBase
    {
        public gc_collectFunc()
        {
            this.Name = "gc_collect";
            this.MinimumArgCounts = 0;
            this.Run += Gc_collectFunc_Run;
        }

        private void Gc_collectFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            GC.Collect();
        }
    }
    class gc_gettotalmemoryFunc : FunctionBase
    {
        public gc_gettotalmemoryFunc()
        {
            this.Name = "gc_gettotalmemory";
            this.MinimumArgCounts = 1;
            this.Run += Gc_gettotalmemoryFunc_Run;
        }

        private void Gc_gettotalmemoryFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            GC.GetTotalMemory(e.Args[0].AsBool());
        }
    }
    class gc_collectafterexecuteFunc : FunctionBase
    {
        public gc_collectafterexecuteFunc()
        {
            this.Name = "gc_collectafterexecute";
            this.MinimumArgCounts = 0;
            this.Run += Gc_collectafterexecuteFunc_Run;
        }

        private void Gc_collectafterexecuteFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 0)
            {
                AliceScript.Interop.GCManerger.CollectAfterExecute = e.Args[0].AsBool();
            }
            e.Return = new Variable(Interop.GCManerger.CollectAfterExecute);
        }
    }
}
