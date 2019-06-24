using System;
using System.Collections.Generic;
using System.Text;

namespace swipcgen
{
    public enum HandleMode
    {
        Move,
        Copy
    }

    public class Handle
    {
        public HandleMode Mode { get; set; }

        public Handle(HandleMode Mode)
        {
            this.Mode = Mode;
        }
    }
}
