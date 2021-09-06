using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AliceScript.NameSpaces
{
    static class Alice_Diagnostics_Initer
    {
        public static void Init()
        {
            //名前空間のメインエントリポイントです。
            NameSpace space = new NameSpace("Alice.Diagnostics");

            space.Add(new gc_collectFunc());
            space.Add(new gc_gettotalmemoryFunc());
            space.Add(new gc_collectafterexecuteFunc());

            space.Add(new StopWatchObject());

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
    class StopWatchObject : ObjectBase
    {
        public StopWatchObject()
        {
            this.Name = "stopwatch";
            this.Properties.Add("Elapsed".ToLower(), new Variable(stopwatch.ElapsedMilliseconds));
            this.Properties.Add("Ticks".ToLower(), new Variable(stopwatch.ElapsedTicks));
            this.Properties.Add("IsRunning".ToLower(), new Variable(stopwatch.IsRunning));
            this.Properties.Add("Frequency".ToLower(), new Variable(Stopwatch.Frequency));
            this.Properties.Add("IsHighResolution".ToLower(), new Variable(Stopwatch.IsHighResolution));
            this.Functions.Add("Start".ToLower(), new STWOFunc(this, 0));
            this.Functions.Add("Stop".ToLower(), new STWOFunc(this, 1));
            this.Functions.Add("Reset".ToLower(), new STWOFunc(this, 2));
            this.Functions.Add("Restart".ToLower(), new STWOFunc(this, 3));
            this.PropertyGetting += StopWatchObject_PropertyGetting;

        }

        private void StopWatchObject_PropertyGetting(object sender, PropertyGettingEventArgs e)
        {
            if ("Elapsed".ToLower() == e.PropertyName.ToLower())
            {
                e.Variable = new Variable(stopwatch.ElapsedMilliseconds);
            }
            else if ("Ticks".ToLower() == e.PropertyName.ToLower())
            {
                e.Variable = new Variable(stopwatch.ElapsedTicks);
            }
            else if ("IsRunning".ToLower() == e.PropertyName.ToLower())
            {
                e.Variable = new Variable(stopwatch.IsRunning);
            }
            else if ("Frequency".ToLower() == e.PropertyName.ToLower())
            {
                e.Variable = new Variable(Stopwatch.Frequency);
            }
            else if ("IsHighResolution".ToLower() == e.PropertyName.ToLower())
            {
                e.Variable = new Variable(Stopwatch.IsHighResolution);
            }
        }

        private Stopwatch stopwatch = new Stopwatch();

        class STWOFunc : FunctionBase
        {
            public STWOFunc(StopWatchObject sto, int mode)
            {
                Host = sto;
                Mode = mode;
                this.Run += STWOFunc_Run;
            }

            private void STWOFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                switch (Mode)
                {
                    case 0:
                        {
                            Host.stopwatch.Start();
                            break;
                        }
                    case 1:
                        {
                            Host.stopwatch.Stop();
                            break;
                        }
                    case 2:
                        {
                            Host.stopwatch.Reset();
                            break;
                        }
                    case 3:
                        {
                            Host.stopwatch.Restart();
                            break;
                        }
                }
            }

            private StopWatchObject Host;
            private int Mode = 0;
        }

    }
}
