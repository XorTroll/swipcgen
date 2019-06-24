using System;
using System.Collections.Generic;
using System.Text;

namespace swipcgen
{
    public class Command
    {
        public uint Id { get; set; }

        public string Name { get; set; }

        public VersionDecorator Version { get; set; }

        public List<Argument<Type>> InRawTypes { get; set; }

        public List<Argument<Type>> OutRawTypes { get; set; }

        public List<Argument<Interface>> InInterfaces { get; set; }

        public List<Argument<Interface>> OutInterfaces { get; set; }

        public Command()
        {
            InRawTypes = new List<Argument<Type>>();
            OutRawTypes = new List<Argument<Type>>();
            InInterfaces = new List<Argument<Interface>>();
            OutInterfaces = new List<Argument<Interface>>();
        }
    }
}
