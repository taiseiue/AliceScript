using System;
using System.Collections.Generic;
using System.Text;


namespace AliceScript.Interop
{
    class NetLibraryLoader
    {
        public static void LoadLibrary(string path)
        {
            try
                {
                    string ipluginName = typeof(ILibrary).FullName;
                    //アセンブリとして読み込む
                    System.Reflection.Assembly asm =
                        System.Reflection.Assembly.LoadFrom(path);
                    foreach (Type t in asm.GetTypes())
                    {
                        try
                        {
                            //アセンブリ内のすべての型について、
                            //プラグインとして有効か調べる
                            if (t.IsClass && t.IsPublic && !t.IsAbstract &&
                                t.GetInterface(ipluginName) != null)
                            {
                                ((ILibrary)asm.CreateInstance(t.FullName)).Main();
                            }
                        }
                        catch { }
                    }
                }
                catch
                {
                }
           
        }
        public static void LoadLibrary(byte[] rawassembly)
        {

            {
                try
                {
                    string ipluginName = typeof(ILibrary).FullName;
                    //アセンブリとして読み込む
                    System.Reflection.Assembly asm =
                        System.Reflection.Assembly.Load(rawassembly);
                    foreach (Type t in asm.GetTypes())
                    {
                        try
                        {
                            //アセンブリ内のすべての型について、
                            //プラグインとして有効か調べる
                            if (t.IsClass && t.IsPublic && !t.IsAbstract &&
                                t.GetInterface(ipluginName) != null)
                            {
                                ((ILibrary)asm.CreateInstance(t.FullName)).Main();
                            }
                        }
                        catch { }
                    }
                }
                catch
                {
                }
            }

        }
    }
   public static class GCManerger
    {
        public static bool CollectAfterExecute = false;
    }
    public interface ILibrary
    {
        string Name { get; }
        void Main();
    }
}
