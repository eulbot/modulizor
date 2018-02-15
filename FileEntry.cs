using System.Collections.Generic;

namespace modulizor
{
    public class FileEntry {
        List<string> Exports { get; set; }        
        List<string> Dependencies { get; set; }

        public FileEntry(List<string> exports, List<string> dependencies) {
            Exports = exports;
            Dependencies = dependencies;
        }
    }
}