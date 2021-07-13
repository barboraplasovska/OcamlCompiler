using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Compiler
{
    public class Compiler
    {
        public static string SaveFunc(string[] func, string funcname)
        {
            // temporarily saving the function to a path
            string path = $"../../../{funcname}.ml";
            
            // rewriting the function into a file
            StreamWriter writer = new StreamWriter(path);
            foreach (string line in func)
            {
                writer.WriteLine(line);
            }
            writer.Close();
            return path;
        }

        public static bool Compile(string filepath, ref List<string> eval, string ocamlPath)
        {
            // Tries to compile the user's function
            // if there is an error -> returns false and ref eval contains the error message
            // otherwise returns true and we can do builtin-test runs (ref eval contains evaluation)
            bool compiled = true;
            
            // starting a process to execute code in ocaml
            using (var myProcess = new Process())
            {
                myProcess.StartInfo.FileName = ocamlPath;
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.RedirectStandardInput = true;
                myProcess.StartInfo.RedirectStandardOutput = true;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.Start();
                
                // redirecting the standart input to the file with the function to execute
                using (var streamReader = new StreamReader(filepath))
                {
                    myProcess.StandardInput.Write(streamReader.ReadToEnd());
                    myProcess.StandardInput.Flush();
                }
                myProcess.StandardInput.Close();
                int i = 0;
                int index = 0;
                
                // reading the standart output and formating it
                while (!myProcess.StandardOutput.EndOfStream)
                {
                    var line = myProcess.StandardOutput.ReadLine();
                    if (line != null && i >= 2 && i <= 5)
                    {
                        if (index == 2)
                        {
                            line = line.Replace("#", "");
                            eval.Add(line);
                        }
                        else if (line.Replace("#", "").Replace(" ", "").Length != 0)
                        {
                            line = line.Replace("#", "") .Trim();
                            eval.Add(line);
                        }
                        index++;
                    }
                    i++;
                }
                
                // ending process
                myProcess.WaitForExit();
                myProcess.Close();
            }

            if (eval.Count == 4)
            {
                compiled = false;
                File.Delete(filepath);
            }
            return compiled;
        }

        public static List<string> Rewrite(string file, TextWriter w)
        {
            // Rewrites the user's function and the builtin tests into one file
            List<string> expected = new List<string>();
            using (StreamReader reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] lines = line?.Split('=');
                    line = lines?[0].Trim() + ";;";
                    expected.Add(lines?[1].Trim().Replace(";;",""));
                    w.WriteLine(line);
                }
            }

            return expected;
        }

        public static List<(string expected,string got)> BuiltInTests(string testfile, string func, string ocamlPath)
        {
            // tests users function with the test functions in a file
            List<string> expected;
            using (StreamWriter w = File.AppendText(func))
            {
                 expected = Rewrite(testfile, w);
            }

            // creating a list for the ocaml evaluation
            List<string> eval = new List<string>();
            
            // test results
            List<(string expected, string got)> result = new List<(string expected, string got)>();
            
            // starting a process to execute code in ocaml
            using (var myProcess = new Process())
            {
                myProcess.StartInfo.FileName = ocamlPath;
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.RedirectStandardInput = true;
                myProcess.StartInfo.RedirectStandardOutput = true;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.Start();
                
                // redirecting the standart input to the file with the function to execute
                using (var streamReader = new StreamReader(func))
                {
                    myProcess.StandardInput.Write(streamReader.ReadToEnd());
                    myProcess.StandardInput.Flush();
                }
                myProcess.StandardInput.Close();
                int i = 0;
                int index = 0;
                
                // reading the standart output and formating it
                while (!myProcess.StandardOutput.EndOfStream)
                {
                    var line = myProcess.StandardOutput.ReadLine();
                    if (line != null&& i >= 3)
                    {
                        if (line.Replace("#", "").Replace(" ", "").Length != 0)
                        {
                            line = line.Replace("#", "").Replace(" ", "");
                            line = line.Substring(line.IndexOf('=') + 1);
                            eval.Add(line);
                        }
                        index++;
                    }
                    i++;
                }
                
                // ending process
                myProcess.WaitForExit();
                myProcess.Close();
            }

            for (int i = 0; i < eval.Count; i++)
            {
                result.Add((expected[i],eval[i].Trim()));
            }
            
            File.Delete(func);
            return result;
        }
        
    }
}