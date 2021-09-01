﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AliceScript
{
    public partial class Utils
    {
        public static string GetPathDetails(FileSystemInfo fs, string name)
        {
            string pathname = fs.FullName;
            bool isDir = Directory.Exists(pathname);

            char d = isDir ? 'd' : '-';
            string last = fs.LastWriteTime.ToString("MMM dd yyyy HH:mm");

            string user = string.Empty;
            string group = string.Empty;
            string links = null;
            string permissions = "rwx";
            long size = 0;




            if (isDir)
            {
                DirectoryInfo di = fs as DirectoryInfo;
                size = di.GetFileSystemInfos().Length;
            }
            else
            {

                FileInfo fi = fs as FileInfo;
                size = fi.Length;

                string[] execs = new string[] { "exe", "bat", "msi" };
                char x = execs.Contains(fi.Extension.ToLower()) ? 'x' : '-';
                char w = !fi.IsReadOnly ? 'w' : '-';
                permissions = string.Format("r{0}{1}", w, x);
            }


                string data = string.Format("{0}{1} {2,4} {3,8} {4,8} {5,9} {6,23} {7}",
                d, permissions, links, user, group, size, last, name);

            return data;
        }

        public static List<Variable> GetPathnames(string path)
        {
            string pathname = Path.GetFullPath(path);
            int index = pathname.IndexOf('*');
            if (index < 0 && !Directory.Exists(pathname) && !File.Exists(pathname))
            {
                throw new ArgumentException("Path [" + pathname + "] doesn't exist");
            }

            List<Variable> results = new List<Variable>();
            if (index < 0)
            {
                results.Add(new Variable(pathname));
                return results;
            }

            string dirName = Path.GetDirectoryName(path);

            try
            {
                string pattern = Path.GetFileName(pathname);

                pathname = index > 0 ? dirName : ".";

                /*if (index > 0) {
                  string prefix = pathname.Substring(0, index);
                  DirectoryInfo di = Directory.GetParent(prefix);
                  pathname = di.FullName;
                } else {
                  pathname = ".";
                }

                string dir = Path.GetFullPath(pathname);*/
                // First get contents of the directory
                DirectoryInfo dirInfo = new DirectoryInfo(dirName);
                FileInfo[] fileNames = dirInfo.GetFiles(pattern);
                foreach (FileInfo fi in fileNames)
                {
                    try
                    {
                        string newPath = Path.Combine(dirName, fi.Name);
                        results.Add(new Variable(newPath));
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                // Then get contents of all of the subdirs in the directory
                DirectoryInfo[] dirInfos = dirInfo.GetDirectories(pattern);
                foreach (DirectoryInfo di in dirInfos)
                {
                    try
                    {
                        string newPath = Path.Combine(dirName, di.Name);
                        results.Add(new Variable(newPath));
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception exc)
            {
                throw new ArgumentException("Couldn't get files from " + path + ": " + exc.Message);
            }
            return results;
        }

      

       

        public static List<string> GetFiles(string path, string[] patterns, bool addDirs = true)
        {
            List<string> files = new List<string>();
            GetFiles(path, patterns, ref files, addDirs);
            return files;
        }

        public static string GetFileEntry(string dir, int i, string startsWith)
        {
            List<string> files = new List<string>();
            string[] patterns = { startsWith + "*" };
            GetFiles(dir, patterns, ref files, true, false);

            if (files.Count == 0)
            {
                return "";
            }
            i = i % files.Count;

            string pathname = files[i];
            if (files.Count == 1)
            {
                pathname += Directory.Exists(Path.Combine(dir, pathname)) ?
                            Path.DirectorySeparatorChar.ToString() : " ";
            }
            return pathname;
        }

        public static void GetFiles(string path, string[] patterns, ref List<string> files,
          bool addDirs = true, bool recursive = true)
        {
            SearchOption option = recursive ? SearchOption.AllDirectories :
                                              SearchOption.TopDirectoryOnly;
            if (string.IsNullOrEmpty(path))
            {
                path = Directory.GetCurrentDirectory();
            }

            List<string> dirs = patterns.SelectMany(
              i => Directory.EnumerateDirectories(path, i, option)
            ).ToList<string>();

            List<string> extraFiles = patterns.SelectMany(
              i => Directory.EnumerateFiles(path, i, option)
            ).ToList<string>();

            if (addDirs)
            {
                files.AddRange(dirs);
            }
            files.AddRange(extraFiles);

            if (!recursive)
            {
                files = files.Select(p => Path.GetFileName(p)).ToList<string>();
                files.Sort();
                return;
            }
            /*foreach (string dir in dirs) {
              GetFiles (dir, patterns, addDirs);
            }*/
        }

        public static List<Variable> ConvertToResults(string[] items,
                                                            bool print = false)
        {
            List<Variable> results = new List<Variable>(items.Length);
            foreach (string item in items)
            {
                results.Add(new Variable(item));
                if (print)
                {
                    Interpreter.Instance.AppendOutput(item);
                }
            }

            return results;
        }

        private static void WriteBlinkingText(string text, int delay, bool visible)
        {
            if (visible)
            {
                Console.Write(text);
            }
            else
            {
                Console.Write(new string(' ', text.Length));
            }
            Console.CursorLeft -= text.Length;
            System.Threading.Thread.Sleep(delay);
        }

        private static readonly object s_mutexLock = new object();
        public static List<string> GetStringInFiles(string path, string search,
            string[] patterns, bool ignoreCase = true)
        {
            List<string> allFiles = GetFiles(path, patterns, false /* no dirs */);
            List<string> results = new List<string>();

            if (allFiles == null && allFiles.Count == 0)
            {
                return results;
            }

            StringComparison caseSense = ignoreCase ? StringComparison.OrdinalIgnoreCase :
                StringComparison.Ordinal;
            Parallel.ForEach(allFiles, (currentFile) =>
            {
                string contents = GetFileText(currentFile);
                if (contents.IndexOf(search, caseSense) >= 0)
                {
                    lock (s_mutexLock) { results.Add(currentFile); }
                }
            });

            return results;
        }

        public static void PrintColor(string output, ConsoleColor fgcolor)
        {
            ConsoleColor currentForeground = Console.ForegroundColor;
            Console.ForegroundColor = fgcolor;

            Interpreter.Instance.AppendOutput(output);
            //Console.Write(output);

            Console.ForegroundColor = currentForeground;
        }

        public static void GetDir(string dir = "./", bool recursive = true)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string dirPath = Path.Combine(documentsPath, dir);

            var directories = Directory.EnumerateDirectories(dirPath);
            var files = Directory.GetFiles(dirPath);
            foreach (var file in files)
            {
                Console.WriteLine("    " + file);
            }
            foreach (var directory in directories)
            {
                Console.WriteLine(directory);
                if (recursive)
                {
                    GetDir(directory, recursive);
                }
            }
        }
        public static bool SaveFile(string filename, Stream stream)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string filePath = Path.Combine(documentsPath, filename);

            try
            {
                var fileStream = File.Create(filePath);
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
                fileStream.Close();
            }
            catch (Exception exc)
            {
                Console.WriteLine("Couldn't save {0}: {1}", filePath, exc.Message);
                return false;
            }
            return true;
        }
        public static Stream OpenFile(string filename)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string filePath = Path.Combine(documentsPath, filename);
            MemoryStream ms = new MemoryStream();

            try
            {
                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int)file.Length);
                    ms.Write(bytes, 0, (int)file.Length);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Couldn't open {0}: {1}", filePath, exc.Message);
                return null;
            }
            return ms;
        }
        public static void PrintScriptColor(string script, ParsingScript parentSript)
        {
            StringBuilder item = new StringBuilder();

            bool inQuotes = false;

            for (int i = 0; i < script.Length; i++)
            {
                char ch = script[i];
                inQuotes = ch == Constants.QUOTE ? !inQuotes : inQuotes;

                if (inQuotes)
                {
                    Interpreter.Instance.AppendOutput(ch.ToString());
                    continue;
                }
                if (!Constants.TOKEN_SEPARATION.Contains(ch))
                {
                    item.Append(ch);
                    continue;
                }
                if (item.Length > 0)
                {
                    string token = item.ToString();
                    ParserFunction func = ParserFunction.GetFunction(token, parentSript);
                    bool isNative = Translation.IsNativeWord(token);
                    if (func != null || isNative)
                    {
                        ConsoleColor col = isNative || func.isNative ? ConsoleColor.Green :
                                                       func.isGlobal ? ConsoleColor.Magenta :
                                                           ConsoleColor.Gray;
                        Utils.PrintColor(token, col);
                    }
                    else
                    {
                        Interpreter.Instance.AppendOutput(token);
                    }
                    item.Clear();
                }
                Interpreter.Instance.AppendOutput(ch.ToString());
            }
        }


    }
}
