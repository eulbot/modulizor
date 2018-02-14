using System;

namespace modulizor
{
    class Program
    {
        static void Main(string[] args) => new Scrape(Program.getRoot(args));

        static string getRoot(string[] args) 
        {
            if(args.Length > 1 && args[0].Equals("--path"))
                return args[1];
            
            return null;
        }
    }
}
