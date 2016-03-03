using System;
using System.Collections.Generic;
using System.Reflection;

namespace SignalR.TypeScript.Helpers
{
    internal static class TypeHelpers
    {
        internal static readonly Dictionary<TypeInfo, string> DefaultTypeMappings = new Dictionary<TypeInfo, string>
        {
            {typeof(object).GetTypeInfo(), "any"},
            {typeof(bool).GetTypeInfo(), "boolean"},
            {typeof(byte).GetTypeInfo(), "number"},
            {typeof(sbyte).GetTypeInfo(), "number"},
            {typeof(short).GetTypeInfo(), "number"},
            {typeof(ushort).GetTypeInfo(), "number"},
            {typeof(int).GetTypeInfo(), "number"},
            {typeof(uint).GetTypeInfo(), "number"},
            {typeof(long).GetTypeInfo(), "number"},
            {typeof(ulong).GetTypeInfo(), "number"},
            {typeof(float).GetTypeInfo(), "number"},
            {typeof(double).GetTypeInfo(), "number"},
            {typeof(decimal).GetTypeInfo(), "number"},
            {typeof(string).GetTypeInfo(), "string"},
            {typeof(char).GetTypeInfo(), "string"},
            {typeof(DateTime).GetTypeInfo(), "string"},
            {typeof(DateTimeOffset).GetTypeInfo(), "string"},
            {typeof(byte[]).GetTypeInfo(), "string"},
            {typeof(Type).GetTypeInfo(), "string"},
            {typeof(Guid).GetTypeInfo(), "string"},
            {typeof(Exception).GetTypeInfo(), "string"},
            {typeof(void).GetTypeInfo(), "void"}
        };

        public static bool IsDefaultType(this Type type)
        {
            return DefaultTypeMappings.ContainsKey(type.GetTypeInfo());
        }

        public static bool IsDefaultType(this TypeInfo type)
        {
            return DefaultTypeMappings.ContainsKey(type);
        }

        // Author: Jon Skeet, Source: http://stackoverflow.com/a/6386234
        public static string GetNameWithoutGenericArity(this TypeInfo t)
        {
            var name = t.Name;
            var index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
        }

        // Author: Jon Skeet, Source: http://stackoverflow.com/a/6386234
        public static string GetFullNameWithoutGenericArity(this TypeInfo t)
        {
            var name = t.FullName;
            var index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
        }

        public static TypeInfo GetRawType(this TypeInfo t)
        {
            var name = t.FullName;
            var index = name.IndexOf('[');
            if (index == -1)
                return t;

            return t.Assembly.GetType(name.Substring(0, index)).GetTypeInfo();
        }
    }
}
