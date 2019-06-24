using System;
using System.Collections.Generic;
using System.Linq;

namespace swipcgen
{
    public static class Static
    {
        public static List<Type> BuiltInTypes = new List<Type>()
        {
            new Type { Name = "u8", CName = "u8", Bytes = 1 },
            new Type { Name = "u16", CName = "u16", Bytes = 2 },
            new Type { Name = "u32", CName = "u32", Bytes = 4 },
            new Type { Name = "u64", CName = "u64", Bytes = 8 },
            new Type { Name = "i8", CName = "s8", Bytes = 1 },
            new Type { Name = "i16", CName = "s16", Bytes = 2 },
            new Type { Name = "i32", CName = "s32", Bytes = 4 },
            new Type { Name = "i64", CName = "s64", Bytes = 8 },
            new Type { Name = "b8", CName = "bool", Bytes = 1 },
            new Type { Name = "f32", CName = "f32", Bytes = 4 },
        };

        public static List<Type> CustomTypes = new List<Type>();

        public static Type GetBuiltInType(string Name)
        {
            return BuiltInTypes.FirstOrDefault(t => t.Name == Name);
        }

        public static Type GetCustomType(string Name)
        {
            return CustomTypes.FirstOrDefault(t => t.Name == Name);
        }
    }
}
