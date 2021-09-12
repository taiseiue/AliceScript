using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AliceScript.NameSpaces
{
    //このクラスはデフォルトで読み込まれるため読み込み処理が必要です
    static class Alice_Initer
    {
        public static void Init()
        {
            FunctionBaseManerger.Add(new Console_BeepFunc());
            FunctionBaseManerger.Add(new Console_ClearFunc());
            FunctionBaseManerger.Add(new Console_MoveBufferAreaFunc());
            FunctionBaseManerger.Add(new Console_ReadsFunc(0));
            FunctionBaseManerger.Add(new Console_ReadsFunc(1));
            FunctionBaseManerger.Add(new Console_ReadsFunc(2));
            FunctionBaseManerger.Add(new Console_WriteLineFunc());
            FunctionBaseManerger.Add(new Console_WriteLineFunc(false));
            FunctionBaseManerger.Add(new Console_GetCursorPositionFunc(0));
            FunctionBaseManerger.Add(new Console_GetCursorPositionFunc(1));
            FunctionBaseManerger.Add(new Console_GetCursorPositionFunc(2));
            FunctionBaseManerger.Add(new Console_GetCursorPositionFunc(3));
            FunctionBaseManerger.Add(new Console_GetBufferSizenFunc(0));
            FunctionBaseManerger.Add(new Console_GetBufferSizenFunc(1));
            FunctionBaseManerger.Add(new Console_SetBufferSizeFunc());
            FunctionBaseManerger.Add(new Console_SetCursorPositionFunc());
            FunctionBaseManerger.Add(new Console_SetWindowPositionFunc());
            FunctionBaseManerger.Add(new Console_SetWindowSizeFunc());
            FunctionBaseManerger.Add(new Console_GetWindowFunc(0));
            FunctionBaseManerger.Add(new Console_GetWindowFunc(1));
            FunctionBaseManerger.Add(new Console_GetWindowFunc(2));
            FunctionBaseManerger.Add(new Console_GetWindowFunc(3));
            FunctionBaseManerger.Add(new Console_GetWindowFunc(4));
            FunctionBaseManerger.Add(new Console_GetWindowFunc(5));
            FunctionBaseManerger.Add(new Console_GetColorFunc());
            FunctionBaseManerger.Add(new Console_GetColorFunc(false));
            FunctionBaseManerger.Add(new Console_SetColorFunc());
            FunctionBaseManerger.Add(new Console_SetColorFunc(false));
            FunctionBaseManerger.Add(new Console_ResetColorFunc());
            FunctionBase.RegisterEnum("ConsoleColor","System.ConsoleColor");
        }
    }
    class Console_ResetColorFunc : FunctionBase
    {
        public Console_ResetColorFunc()
        {
            this.Name = "Console_ResetColor";
            this.Run += Console_ResetColorFunc_Run;
        }

        private void Console_ResetColorFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Console.ResetColor();
        }
    }
    class Console_GetColorFunc : FunctionBase
    {
        public Console_GetColorFunc(bool bgcolor = true)
        {
            m_BGColor = bgcolor;
            if (m_BGColor)
            {
                this.Name = "Console_GetBackgroundColor";
            }
            else
            {
                this.Name = "Console_GetForegroundColor";
            }
            this.Run += Console_GetColorFunc_Run;
        }

        private void Console_GetColorFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (m_BGColor)
            {
                e.Return = new Variable((int)Console.BackgroundColor);
            }
            else
            {
                e.Return = new Variable((int)Console.ForegroundColor);
            }
        }

        private bool m_BGColor = true;
    }
    class Console_SetColorFunc : FunctionBase
    {
        public Console_SetColorFunc(bool bgcolor = true)
        {
            m_BGColor = bgcolor;
            if (m_BGColor)
            {
                this.Name = "Console_SetBackgroundColor";
            }
            else
            {
                this.Name = "Console_SetForegroundColor";
            }
            this.Run += Console_GetColorFunc_Run;
        }

        private void Console_GetColorFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (m_BGColor)
            {
                Console.BackgroundColor = ((ConsoleColor)e.Args[0].AsInt());
            }
            else
            {
                Console.ForegroundColor = ((ConsoleColor)e.Args[0].AsInt());
            }
        }

        private bool m_BGColor = true;
    }
    class Console_GetCursorPositionFunc : FunctionBase
    {
        public Console_GetCursorPositionFunc(int mode=0)
        {
            m_mode = mode;
            switch (m_mode)
            {
                case 0:
                    {
                        this.Name = "Console_GetCursorLeft";
                        break;
                    }
                case 1:
                    {
                        this.Name = "Console_GetCursorTop";
                        break;
                    }
                case 2:
                    {
                        this.Name = "Console_GetCursorSize";
                        break;
                    }
                case 3:
                    {
                        this.Name = "Console_CursorVisible";
                        break;
                    }
            }
            this.Run += Console_GetCursorPositionFunc_Run;
        }
        private int m_mode = 0;
        private void Console_GetCursorPositionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (m_mode)
            {
                case 0:
                    {
                        e.Return = new Variable(Console.CursorLeft);
                        break;
                    }
                case 1:
                    {
                        e.Return = new Variable(Console.CursorTop);
                        break;
                    }
                case 2:
                    {
                        e.Return = new Variable(Console.CursorSize);
                        break;
                    }
                case 3:
                    {
                        int i = 0;
                        if (Console.CursorVisible)
                        {
                            i = 1;
                        }
                        Console.CursorVisible=(Utils.GetSafeInt(e.Args, 0, i) != 0);
                        e.Return = new Variable(Console.CursorVisible);
                        break;
                    }
            }
        }
    }
    class Console_GetWindowFunc : FunctionBase
    {
        public Console_GetWindowFunc(int mode = 0)
        {
            m_mode = mode;
            switch (m_mode)
            {
                case 0:
                    {
                        this.Name = "Console_GetWindowHeight";
                        break;
                    }
                case 1:
                    {
                        this.Name = "Console_GetWindowWidth";
                        break;
                    }
                case 2:
                    {
                        this.Name = "Console_GetWIndowLeft";
                        break;
                    }
                case 3:
                    {
                        this.Name = "Console_GetWindowTop";
                        break;
                    }
                case 4:
                    {
                        this.Name = "Console_GetLargestWindowHeight";
                        break;
                    }
                case 5:
                    {
                        this.Name = "COnsole_GetLargestWindowWidth";
                        break;
                    }
            }
            this.Run += Console_GetCursorPositionFunc_Run;
        }
        private int m_mode = 0;
        private void Console_GetCursorPositionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (m_mode)
            {
                case 0:
                    {
                        e.Return = new Variable(Console.WindowHeight);
                        break;
                    }
                case 1:
                    {
                        e.Return = new Variable(Console.WindowWidth);
                        break;
                    }
                case 2:
                    {
                        e.Return = new Variable(Console.WindowLeft);
                        break;
                    }
                case 3:
                    {
                        e.Return = new Variable(Console.WindowTop);
                        break;
                    }
                case 4:
                    {
                        e.Return = new Variable(Console.LargestWindowHeight);
                        break;
                    }
                case 5:
                    {
                        e.Return = new Variable(Console.LargestWindowWidth);
                        break;
                    }
            }
        }
    }
    class Console_GetBufferSizenFunc : FunctionBase
    {
        public Console_GetBufferSizenFunc(int mode = 0)
        {
            m_mode = mode;
            switch (m_mode)
            {
                case 0:
                    {
                        this.Name = "Console_GetBufferHeight";
                        break;
                    }
                case 1:
                    {
                        this.Name = "Console_GetBufferWidth";
                        break;
                    }
              
            }
            this.Run += Console_GetCursorPositionFunc_Run;
        }
        private int m_mode = 0;
        private void Console_GetCursorPositionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (m_mode)
            {
                case 0:
                    {
                        e.Return = new Variable(Console.BufferHeight);
                        break;
                    }
                case 1:
                    {
                        e.Return = new Variable(Console.BufferWidth);
                        break;
                    }
               
            }
        }
    }
    class Console_SetCursorPositionFunc : FunctionBase
    {
        public Console_SetCursorPositionFunc()
        {
            this.Name = "Console_SetCursorPosition";
            this.MinimumArgCounts = 2;
            this.Run += Console_SetCursorPositionFunc_Run;
        }

        private void Console_SetCursorPositionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Console.SetCursorPosition(e.Args[0].AsInt(),e.Args[1].AsInt());
        }
    }
    class Console_SetBufferSizeFunc : FunctionBase
    {
        public Console_SetBufferSizeFunc()
        {
            this.Name = "Console_SetBufferSize";
            this.MinimumArgCounts = 2;
            this.Run += Console_SetCursorPositionFunc_Run;
        }

        private void Console_SetCursorPositionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Console.SetBufferSize(e.Args[0].AsInt(), e.Args[1].AsInt());
        }
    }
    class Console_SetWindowSizeFunc : FunctionBase
    {
        public Console_SetWindowSizeFunc()
        {
            this.Name = "Console_SetWindowSize";
            this.MinimumArgCounts = 2;
            this.Run += Console_SetCursorPositionFunc_Run;
        }

        private void Console_SetCursorPositionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Console.SetWindowSize(e.Args[0].AsInt(), e.Args[1].AsInt());
        }
    }
    class Console_SetWindowPositionFunc : FunctionBase
    {
        public Console_SetWindowPositionFunc()
        {
            this.Name = "Console_SetWindowPosition";
            this.MinimumArgCounts = 2;
            this.Run += Console_SetCursorPositionFunc_Run;
        }

        private void Console_SetCursorPositionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Console.SetWindowPosition(e.Args[0].AsInt(), e.Args[1].AsInt());
        }
    }
    class Console_BeepFunc : FunctionBase
    {
        public Console_BeepFunc()
        {
            this.Name = "Console_Beep";
            this.Run += Console_BeepFunc_Run;
        }

        private void Console_BeepFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count >= 2)
            {
                Console.Beep(e.Args[0].AsInt(),e.Args[1].AsInt());
            }
            else
            {
                Console.Beep();
            }
        }
    }
    class Console_ClearFunc : FunctionBase
    {
        public Console_ClearFunc()
        {
            this.Name = "Console_Clear";
            this.Run += Console_ClearFunc_Run;
        }

        private void Console_ClearFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Console.Clear();
        }
    }
    class Console_MoveBufferAreaFunc : FunctionBase
    {
        public Console_MoveBufferAreaFunc()
        {
            this.Name = "Console_MoveBufferArea";
            this.MinimumArgCounts = 6;
            this.Run += Console_MoveBufferAreaFunc_Run;
        }

        private void Console_MoveBufferAreaFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Console.MoveBufferArea(e.Args[0].AsInt(),e.Args[1].AsInt(),e.Args[2].AsInt(),e.Args[3].AsInt(),e.Args[4].AsInt(),e.Args[5].AsInt());
        }
    }
    class Console_ReadsFunc : FunctionBase
    {
        public Console_ReadsFunc(int mode = 0)
        {
            m_Mode = mode;
            this.Run += Console_ReadsFunc_Run;
            switch (m_Mode)
            {
                case 0:
                    {
                        this.Name = "Console_Read";
                        break;
                    }
                case 1:
                    {
                        this.Name = "Console_ReadKey";
                        break;
                    }
                case 2:
                    {
                        this.Name = "Console_ReadLine";
                        break;
                    }
            }
        }

        private void Console_ReadsFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (m_Mode)
            {
                case 0:
                    {
                        e.Return = new Variable(Console.Read());
                        break;
                    }
                case 1:
                    {
                        e.Return = new Variable(Console.ReadKey().KeyChar);
                        break;
                    }
                case 2:
                    {
                        e.Return = new Variable(Console.ReadLine());
                        break;
                    }
            }
        }

        private int m_Mode = 0;
    }
    class Console_WriteLineFunc : FunctionBase
    {
        public Console_WriteLineFunc(bool wline = false)
        {
            m_WLine = wline;
            this.Run += Console_WriteLineFunc_Run;
            if (m_WLine)
            {
                this.Name = "Console_WriteLine";
            }
            else
            {
                this.Name = "Console_Write";
            }
        }

        private void Console_WriteLineFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 1)
            {
                if (m_WLine)
                {
                    Console.WriteLine(e.Args[0].AsString());
                }
                else
                {
                    Console.Write(e.Args[0].AsString());
                }
            }
            else
            {
                string text = e.Args[0].AsString();
                MatchCollection mc = Regex.Matches(text, @"{[0-9]+}");
                foreach (Match match in mc)
                {
                    int mn = -1;
                    if (int.TryParse(match.Value.TrimStart('{').TrimEnd('}'),out mn))
                    {
                        if (mn == -1) { ThrowErrorManerger.OnThrowError("複合書式指定\"{" + mn + "}\"で" + mn + "番目の引数が見つかりません", e.Script); break; }
                        if (e.Args.Count > mn + 1)
                        {
                            text = text.Replace(match.Value, e.Args[mn + 1].AsString());
                        }
                        else
                        {
                            ThrowErrorManerger.OnThrowError("複合書式指定\"{" + mn + "}\"で" + mn + "番目の引数が見つかりません", e.Script);
                                break;
                        }
                    }
                    else
                    {
                        ThrowErrorManerger.OnThrowError("複合書式指定\""+ match.Value + "\"は無効です", e.Script);
                        break;
                    }
                }
                if (m_WLine)
                {
                    Console.WriteLine(text);
                }
                else
                {
                    Console.Write(text);
                }
            }
        }

        private bool m_WLine = false;
    }
}
