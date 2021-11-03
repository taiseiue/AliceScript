﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
    public partial class Constants
    {
        public const string APPEND = "append";
        public const string APPENDLINE = "appendline";
        public const string APPENDLINES = "appendlines";
        public const string CALL_NATIVE = "CallNative";
        public const string PRINT_BLACK = "printblack";
        public const string PRINT_GRAY = "printgray";
        public const string PRINT_GREEN = "printgreen";
        public const string PRINT_RED = "printred";
        public const string PSINFO = "psinfo";
        public const string PWD = "pwd";
        public const string READ = "read";
        public const string READFILE = "readfile";
        public const string READNUMBER = "readnum";
        public const string RUN = "run";
        public const string SET_NATIVE = "SetNative";
        public const string STARTSRV = "startsrv";
        public const string STOPWATCH_ELAPSED = "StopWatchElapsed";
        public const string STOPWATCH_START = "StartStopWatch";
        public const string STOPWATCH_STOP = "StopStopWatch";
        public const string WRITELINES = "writelines";
        public const string WRITENL = "writenl";
        public const string WRITE_CONSOLE = "WriteConsole";
        public const string WRITE = "write";

        public const string ADD_COMP_DEFINITION = "add_comp_definition";
        public const string ADD_COMP_NAMESPACE = "add_comp_namespace";
        public const string CLEAR_COMP_DEFINITIONS = "clear_comp_definitions";
        public const string CLEAR_COMP_NAMESPACES = "clear_comp_namespaces";
        public const string CSHARP_FUNCTION = "csfunction";

        public const string ENGLISH = "en";
        public const string GERMAN = "de";
        public const string RUSSIAN = "ru";
        public const string SPANISH = "es";
        public const string SYNONYMS = "sy";

        public const string LABEL_OPERATOR = ":";
        public const string GOTO = "goto";
        public const string GOSUB = "gosub";

        public const string INCLUDE_SECURE = "includes";

        public const string ENCODE_FILE = "encodeFile";
        public const string DECODE_FILE = "decodeFile";

        public static string Language(string lang)
        {
            switch (lang)
            {
                case "English": return ENGLISH;
                case "German": return GERMAN;
                case "Russian": return RUSSIAN;
                case "Spanish": return SPANISH;
                case "Synonyms": return SYNONYMS;
                default: return ENGLISH;
            }
        }
    }
}
