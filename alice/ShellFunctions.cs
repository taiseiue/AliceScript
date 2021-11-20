using System;
using System.Collections.Generic;
using System.Text;
using AliceScript;

namespace alice
{
    internal class shell_dumpFunc:FunctionBase
    {
        public shell_dumpFunc()
        {
            this.Name = "shell_dump";
            this.Run += Shell_dumpFunc_Run;
        }

        private void Shell_dumpFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Shell.DumpVariables();
        }
    }
}
