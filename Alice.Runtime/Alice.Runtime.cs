using System;
using AliceScript;
using AliceScript.Interop;

namespace AliceScript.NameSpaces
{
    public class Alice_Runtime : ILibrary
    {
        public string Name
        {
            get
            {
                return "Alice.Runtime";
            }
        }

        public void Main()
        {
            AliceScript_Diagnosis_Initer.Init();
            Alice_IO_Intiter.Init();
            Alice_Math_Initer.Init();
            Alice_Net_Initer.Init();
            Alice_Random_Initer.Init();
            
        }
    }
}
