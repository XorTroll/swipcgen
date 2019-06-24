using System;
using System.Collections.Generic;
using System.Text;

namespace swipcgen
{
    public class Argument<T>
    {
        public T ArgType { get; set; }

        public string Name { get; set; }

        public Argument(T ArgType)
        {
            this.ArgType = ArgType;
            Name = null;
        }

        public Argument(T ArgType, string Name)
        {
            this.ArgType = ArgType;
            this.Name = Name;
        }
    }
}
