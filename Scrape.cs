using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace modulizor
{
    class Scrape 
    {
        Dictionary<string, FileEntry> NamespaceTree;

        public Scrape(string root) 
        {
            NamespaceTree = new Dictionary<string, FileEntry>();
            
            foreach(string path in Directory.EnumerateFiles(Path.GetFullPath(root) , "*.ts", SearchOption.AllDirectories))
            {
                if(!path.EndsWith(".d.ts"))
                    ParseTypeScript(path);
            }

            GenerateOut(root, "mapp");
        }

        void ParseTypeScript(string path) {

            var raw = File.ReadAllText(path);
            raw = RemoveStringBlocks(raw);

            string[] content = raw.Split(new String[]{" "}, StringSplitOptions.RemoveEmptyEntries);
            NamespaceTree.Add(path, new FileEntry(FindExports(content), FindDependencies(content)));

            Console.WriteLine(path);
        }

        void GenerateOut(string root, string module) {

            foreach(string path in NamespaceTree.Keys)
            {
                
                var raw = File.ReadAllText(path);
                var outfile = Path.GetRelativePath(".", path).Replace(Path.GetFileName(root), "output");
                var lineArray = raw.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var result = "";
                var reindent = false;

                Directory.CreateDirectory(Path.GetDirectoryName(outfile));

                for(var lineNumber = 0; lineNumber < lineArray.Length; lineNumber++){

                    var line = lineArray[lineNumber];
                    
                    if(line.StartsWith("///"))
                        continue;
                    
                    if(line.Trim().StartsWith(String.Concat("module ", module))) 
                    {
                        reindent = true;
                        continue;
                    }

                    if(reindent && (lineNumber + 1) == lineArray.Length)
                        continue;

                    result += (reindent && !line.Trim().Equals("") ? 
                        line.Substring(Math.Min(line.Length, 4)) : line) + Environment.NewLine;
                }

                System.IO.File.WriteAllText(outfile, result);


                // using(StreamWriter sw = new StreamWriter(File.Open(outfile, System.IO.FileMode.Append)))
                // {
                //     sw.Write("// Fingers crossed\r\n" + raw);
                // }
            }
        }

        List<String> FindExports(string[] content) {

            var result = new List<string>();
            
            foreach(var index in content.Select((s, i) => s == "export" ? i : -1).Where(i => i != -1))
            {
                string next = content[index + 1];

                if(content[index + 1].Equals("{"))
                {
                    int closingBracket = index + 1;
                    for(; content[closingBracket] != "}"; closingBracket++);
                    result.AddRange(String.Join("", content.Skip(index + 1).Take(closingBracket - index).ToArray()).Split(","));
                }
                else if((new[]{"abstract", "class", "interface", "const", "var", "enum"}).Contains(next)) {
                    var offset = next == "abstract" ? 3 : 2;
                    result.Add(content[index + offset]);
                }
                else
                    Console.WriteLine("Unknown export type");
            }

            return result.Select(s => RemoveGenericsAndType(s)).ToList();
        }

        List<String> FindDependencies(string[] content) {

            var result = new List<string>();

            foreach(var index in content.Select((s, i) => (s == "extends" || s == "implements" || s.EndsWith(":")) ? i : -1).Where(i => i != -1))
            {
                string next = content[index + 1];

                if(next.IndexOf('.') >= 0)
                {
                    var s = next.Split('.')[0];
                    if(!IsPrimitiveOrLiteral(s))
                        result.Add(s);   
                }
                else if(next.IndexOf('<') >= 0) {
                    result.AddRange(next.Split('<').Select(s => RemoveSpecialChars(s)).ToList());
                }
                else {
                    result.Add(RemoveSpecialChars(next));
                }
            }

            return result.FindAll(s => !IsPrimitiveOrLiteral(s)).Distinct().ToList();
        }

        string RemoveGenericsAndType(string s) 
        {
            if(s.IndexOf('<') >= 0) {
                s = s.Substring(0, s.IndexOf('<'));
            }
            
            if(s.IndexOf(':') >= 0) {
                s = s.Substring(0, s.IndexOf(':'));
            }

            return s;
        }

        string RemoveSpecialChars(string s)
        {
            if(s.StartsWith('\'') || s.StartsWith('\"'))
                return s;

            return Regex.Replace(s, "[^A-Za-z0-9]", "");
        }

        string RemoveStringBlocks(string s)
        {
            while(s.IndexOf('`') >= 0)
            {
                var first = s.IndexOf('`');
                var second = s.IndexOf('`', first + 1);
                
                if(second < 0)
                    break;
                else
                    s = s.Remove(first, second - first);
            }

            return s;
        }

        bool IsPrimitiveOrLiteral(string s) {

            if((new[]{"boolean", "number", "string", "any", "void", "undefined", "null", "never", "Array", "", "this", "T"}).Contains(s))
                return true;
            if(s.StartsWith('\'') || s.StartsWith('\"'))
                return true;
            
            return false;
        }
    }
}
