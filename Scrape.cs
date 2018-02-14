using System;
using System.IO;
using System.Collections.Generic;

namespace modulizor
{
    class Scrape 
    {
     
        Dictionary<string, List<string>> NamespaceTree;

        public Scrape(string root) 
        {
            NamespaceTree = new Dictionary<string, List<string>>();
            
            foreach(string path in Directory.EnumerateFiles(Path.GetFullPath(root) , "*.ts", SearchOption.AllDirectories))
            {
                NamespaceTree.Add(path, new List<string>());
            }

            Console.Write(String.Format("{0} files found", NamespaceTree.Count));
        }
    }
}
