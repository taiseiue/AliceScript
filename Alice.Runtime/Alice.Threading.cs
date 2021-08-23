using AliceScript;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AliceScript.NameSpaces
{
    static class Alice_Threading_Initer
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.Threading");

            space.Add(new thread_sleepFunc());
            space.Add(new thread_idFunc());
            space.Add(new thread_queueFunc());

            space.Add(new task_runFunc());

            NameSpaceManerger.Add(space);
        }
    }
    class thread_sleepFunc : FunctionBase
    {
        public thread_sleepFunc()
        {
            this.Name = "thread_sleep";
            this.MinimumArgCounts = 1;
            this.Run += Thred_sleepFunc_Run;
        }

        private void Thred_sleepFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Thread.Sleep(e.Args[0].AsInt());
        }
    }
    class thread_idFunc : FunctionBase
    {
        public thread_idFunc()
        {
            this.Name = "thread_id";
            this.MinimumArgCounts = 0;
            this.Run += Thread_idFunc_Run;
        }

        private void Thread_idFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Thread.CurrentThread.ManagedThreadId);
        }
    }
    class thread_queueFunc : FunctionBase
    {
        public thread_queueFunc()
        {
            this.Name = "thread_queue";
            this.MinimumArgCounts = 1;
            this.Attribute = FunctionAttribute.CONTROL_FLOW;
            this.Run += Thread_queueFunc_Run;
        }

        private void Thread_queueFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args[0].Type != Variable.VarType.DELEGATE) { ThrowErrorManerger.OnThrowError("不正な引数です",e.Script); }
            ThreadQueueStateInfo tqsi = new ThreadQueueStateInfo();
            tqsi.Delegate = e.Args[0].Delegate;
            tqsi.Script = e.Script;
            if (e.Args.Count > 1)
            {
                tqsi.Args = e.Args.GetRange(1, e.Args.Count - 1);
            }
            ThreadPool.QueueUserWorkItem(ThreadProc,tqsi);
            e.Return = Variable.EmptyInstance;
        }
        static void ThreadProc(Object stateInfo)
        {
            ThreadQueueStateInfo tqsi = (ThreadQueueStateInfo)stateInfo;
            tqsi.Delegate.Run(tqsi.Args,tqsi.Script);
        }
    }
    class ThreadQueueStateInfo
    {
        public List<Variable> Args { get; set; }
        public ParsingScript Script { get; set; }
        public CustomFunction Delegate { get; set; }
    }
    class task_runFunc : FunctionBase
    {
        public task_runFunc()
        {
            this.Name = "task_run";
            this.MinimumArgCounts = 0;
            this.Run += Task_runFunc_Run;
        }

        private void Task_runFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args[0].Type != Variable.VarType.DELEGATE) { ThrowErrorManerger.OnThrowError("不正な引数です",e.Script); }
            List<Variable> args = new List<Variable>();
            if (e.Args.Count > 1)
            {
                args = e.Args.GetRange(1, e.Args.Count - 1);
            }
            Task.Run(()=> { e.Args[0].Delegate.Run(args,e.Script); });
        }
    }
   
  
}
