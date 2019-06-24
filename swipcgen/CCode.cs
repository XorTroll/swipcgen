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

        public void Save(string Header, string Source)
        {
            File.WriteAllText(Header, this.Header);
            File.WriteAllText(Source, "#include \"" + Path.GetFileNameWithoutExtension(Source) + ".h\"\n\n" + this.Source);
        }
    }
}
