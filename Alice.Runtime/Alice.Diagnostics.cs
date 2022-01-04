using System.Collections.Generic;
using System.Diagnostics;

namespace AliceScript.NameSpaces
{
    internal static class Alice_Diagnostics_Initer
    {
        public static void Init()
        {
            //名前空間のメインエントリポイントです。
            NameSpace space = new NameSpace("Alice.Diagnostics");


            space.Add(new StopWatchClass());

            NameSpaceManerger.Add(space);
        }
    }


    internal class StopWatchClass : ObjectClass
    {
        public StopWatchClass()
        {
            this.Name = "stopwatch";
            this.AddFunction(new STWOFunc(0), "start");
            this.AddFunction(new STWOFunc(1), "stop");
            this.AddFunction(new STWOFunc(2), "reset");
            this.AddFunction(new STWOFunc(3), "restart");
            this.AddProperty(new ElapsedProperty());
            this.AddProperty(new ElapsedMillisecondsProperty());
            this.AddProperty(new ElapsedTicksProperty());
            this.AddProperty(new FrequencyProperty());
            this.AddProperty(new IsHighResolutionProperty());
            this.AddProperty(new IsRunningProperty());
        }
        public override ObjectBase GetImplementation(List<Variable> args, ParsingScript script = null, string className = "")
        {
            return new StopWatchObject();
        }

        private class ElapsedProperty : PropertyBase
        {
            public ElapsedProperty()
            {
                this.Name = "elapsed";
                this.CanSet = false;
                this.HandleEvents = true;
                NeedCallFromParent = true;
                this.Getting += ElapsedProperty_Getting;
            }
            private void ElapsedProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(((StopWatchObject)e.Instance).Stopwatch.Elapsed);
            }
        }

        private class ElapsedMillisecondsProperty : PropertyBase
        {
            public ElapsedMillisecondsProperty()
            {
                this.Name = "elapsedmilliseconds";
                this.CanSet = false;
                NeedCallFromParent = true;
                this.HandleEvents = true;
                this.Getting += ElapsedProperty_Getting;
            }
            private void ElapsedProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                new Variable(((StopWatchObject)e.Instance).Stopwatch.ElapsedMilliseconds);
            }
        }

        private class ElapsedTicksProperty : PropertyBase
        {
            public ElapsedTicksProperty()
            {
                this.Name = "elapsedticks";
                this.CanSet = false;
                NeedCallFromParent = true;
                this.HandleEvents = true;
                this.Getting += ElapsedProperty_Getting;
            }
            private void ElapsedProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                new Variable(((StopWatchObject)e.Instance).Stopwatch.ElapsedTicks);
            }
        }

        private class IsRunningProperty : PropertyBase
        {
            public IsRunningProperty()
            {
                this.Name = "isrunning";
                this.CanSet = false;
                NeedCallFromParent = true;
                this.HandleEvents = true;
                this.Getting += IsRunningProperty_Getting;
            }
            private void IsRunningProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                new Variable(((StopWatchObject)e.Instance).Stopwatch.IsRunning);
            }
        }

        private class IsHighResolutionProperty : PropertyBase
        {
            public IsHighResolutionProperty()
            {
                this.Name = "ishighresolution";
                this.CanSet = false;
                NeedCallFromParent = true;
                this.HandleEvents = true;
                this.Getting += IsRunningProperty_Getting;
            }
            private void IsRunningProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                new Variable(Stopwatch.IsHighResolution);
            }
        }

        private class FrequencyProperty : PropertyBase
        {
            public FrequencyProperty()
            {
                this.Name = "frequency";
                this.CanSet = false;
                NeedCallFromParent = true;
                this.HandleEvents = true;
                this.Getting += IsRunningProperty_Getting;
            }
            private void IsRunningProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(Stopwatch.Frequency);
            }
        }
        
        private class STWOFunc : FunctionBase
        {
            public STWOFunc(int mode)
            {
                Mode = mode;
                NeedCallFromParent = true;
                this.Run += STWOFunc_Run;
            }

            private void STWOFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                Stopwatch stopwatch = (e.Instance as StopWatchObject).Stopwatch;
                switch (Mode)
                {
                    case 0:
                        {
                            stopwatch.Start();
                            break;
                        }
                    case 1:
                        {
                            stopwatch.Stop();
                            break;
                        }
                    case 2:
                        {
                            stopwatch.Reset();
                            break;
                        }
                    case 3:
                        {
                            stopwatch.Restart();
                            break;
                        }
                }
            }
            private int Mode = 0;
        }
        internal class StopWatchObject : ObjectBase
        {
            public StopWatchObject()
            {
                Stopwatch = new Stopwatch();
            }
            internal Stopwatch Stopwatch { get; set; }
        }

    }
}
