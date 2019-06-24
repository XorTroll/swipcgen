using System;
using System.Collections.Generic;
using System.Text;

namespace swipcgen
{
    public class Type
    {
        public string Name { get; set; }

        public string CName { get; set; }

        public long Bytes { get; set; }

        public Type EqType { get; set; }

        public VersionDecorator Version { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(Name);
            }
        }

        public string FormattedName
        {
            get
            {
                return Name.Replace("::", "_");
            }
        }

        public Type()
        {
            Name = null;
        }

        public Type(string Name, long Size)
        {
            this.Name = Name;
            Bytes = Size;
            CName = FormattedName;
        }

        public Type(string Name, Type Typedef)
        {
            this.Name = Name;
            EqType = Typedef;
            CName = FormattedName;
        }

        public override string ToString()
        {
            return "Type { " + (IsValid ? Name : "<no name>") + ", " + Bytes.ToString() + " }";
        }
    }
}
