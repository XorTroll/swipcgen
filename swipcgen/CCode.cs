using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace swipcgen
{
    public class CCode
    {
        public string Header { get; set; }

        public string Source { get; set; }

        public void Save(string IDPath)
        {
            string outbase = Path.GetDirectoryName(IDPath) + "\\" + Path.GetFileNameWithoutExtension(IDPath);
            File.WriteAllText(outbase + ".h", Header);
            File.WriteAllText(outbase + ".c", "#include \"" + Path.GetFileNameWithoutExtension(IDPath) + ".h\"\n\n" + Source);
        }
    }
}
