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
            space.Add(new Function_ShowFunc());

            space.Add(new StopWatchObject());

            NameSpaceManerger.Add(space);
        }
    }
    class Function_ShowFunc : FunctionBase
    {
        public Function_ShowFunc()
        {
            this.Name = "function_show";
            this.MinimumArgCounts = 1;
            this.Run += Function_ShowFunc_Run;
        }

        private void Function_ShowFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string functionName = e.Args[0].AsString();

            CustomFunction custFunc = ParserFunction.GetFunction(functionName, e.Script) as CustomFunction;
            Utils.CheckNotNull(functionName, custFunc, e.Script);



            string body = Utils.BeautifyScript(custFunc.Body, custFunc.Header);
            Utils.PrintScript(body, e.Script);
            e.Return = new Variable(body);
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
            this.AddFunction(new STWOFunc(this,0),"start");
            this.AddFunction(new STWOFunc(this, 1), "stop");
            this.AddFunction(new STWOFunc(this, 2), "reset");
            this.AddFunction(new STWOFunc(this, 3), "restart");
            this.AddProperty(new ElapsedProperty(stopwatch));
            this.AddProperty(new ElapsedMillisecondsProperty(stopwatch));
            this.AddProperty(new ElapsedTicksProperty(stopwatch));
            this.AddProperty(new FrequencyProperty(stopwatch));
            this.AddProperty(new IsHighResolutionProperty(stopwatch));
            this.AddProperty(new IsRunningProperty(stopwatch));
        }


        private Stopwatch stopwatch = new Stopwatch();

        class ElapsedProperty : PropertyBase
        {
            public ElapsedProperty(Stopwatch stopwatch)
            {
                this.Name = "elapsed";
                this.CanSet = false;
                this.Stopwatch = stopwatch;
                this.HandleEvents = true;
                this.Getting += ElapsedProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void ElapsedProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(Stopwatch.Elapsed);
            }
        }
        class ElapsedMillisecondsProperty : PropertyBase
        {
            public ElapsedMillisecondsProperty(Stopwatch stopwatch)
            {
                this.Name = "elapsedmilliseconds";
                this.CanSet = false;
                this.Stopwatch = stopwatch;
                this.HandleEvents = true;
                this.Getting += ElapsedProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void ElapsedProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(Stopwatch.ElapsedMilliseconds);
            }
        }
        class ElapsedTicksProperty : PropertyBase
        {
            public ElapsedTicksProperty(Stopwatch stopwatch)
            {
                this.Name = "elapsedticks";
                this.CanSet = false;
                this.Stopwatch = stopwatch;
                this.HandleEvents = true;
                this.Getting += ElapsedProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void ElapsedProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(Stopwatch.ElapsedTicks);
            }
        }
        class IsRunningProperty : PropertyBase
        {
            public IsRunningProperty(Stopwatch stopwatch)
            {
                this.Name = "isrunning";
                this.CanSet = false;
                this.Stopwatch = stopwatch;
                this.HandleEvents = true;
                this.Getting += IsRunningProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void IsRunningProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(Stopwatch.IsRunning);
            }
        }
        class IsHighResolutionProperty : PropertyBase
        {
            public IsHighResolutionProperty(Stopwatch stopwatch)
            {
                this.Name = "ishighresolution";
                this.CanSet = false;
                this.Stopwatch = stopwatch;
                this.HandleEvents = true;
                this.Getting += IsRunningProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void IsRunningProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(Stopwatch.IsHighResolution);
            }
        }
        class FrequencyProperty : PropertyBase
        {
            public FrequencyProperty(Stopwatch stopwatch)
            {
                this.Name = "frequency";
                this.CanSet = false;
                this.Stopwatch = stopwatch;
                this.HandleEvents = true;
                this.Getting += IsRunningProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void IsRunningProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(Stopwatch.Frequency);
            }
        }

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
