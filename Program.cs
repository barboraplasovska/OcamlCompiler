using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            // tests
            string[] func =
            {
                "let substractpoly (a,b) (d,e) = (a-d,b-e);;"
            };
            string[] testfunc =
            {
                "substractpoly (1,2) (3,2) = (-2,0);;",
                "substractpoly (1,1) (1,1) = (0,0);;",
                "substractpoly (1,1) (-1,-1) = (2,2);;",
                "substractpoly (0,0) (1,1) = (-1,-1);;",
                "substractpoly (10,10) (1000,100) = (-990,-90);;"
            };
            
            
              string path = Compiler.SaveFunc(func, "power25");
             
              string testpath = Compiler.SaveFunc(testfunc, "testfunc");
              List<string> eval = new List<string>();
               string str = "ocaml";
               

              string ocamlPath = "/usr/local/bin/ocaml"; // on mac

             if (Compiler.Compile(path, ref eval, ocamlPath))
             {
                 int nb = 0;
                 foreach (var l in eval)
                 {
                     Console.WriteLine($"{nb} {l}");
                     nb++;
                 }
                 List<(string expected,string got)> res = Compiler.BuiltInTests(testpath,path, ocamlPath);
                 foreach (var val in res)
                 {
                     Console.WriteLine($"Expected: {val.expected} Got: {val.got} -> {val.expected == val.got}");
                 }
             }
             else
             {
                 Console.WriteLine("ERROR");
                 foreach (var l in eval)
                 {
                     Console.WriteLine($"{l}");
                 }
             }
        }
    }
}