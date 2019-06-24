using System;
using System.Collections.Generic;
using System.Text;

namespace swipcgen
{
    public static class ParseUtils
    {
        public static Type ParseArgument(string Raw)
        {
            if(Raw.StartsWith("unknown"))
            {
                string tmpsize = "";
                for(int i = Raw.IndexOf('<') + 1; i < Raw.Length; i++)
                {
                    if(Raw[i] == '>') break;
                    tmpsize += Raw[i];
                }
                return new Type { Name = null, Bytes = long.Parse(tmpsize) };
            }
            var bit = Static.GetBuiltInType(Raw);
            if(bit.IsValid) return bit;
            return null;
        }
    }
}
