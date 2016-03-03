using System;
using System.Collections.Generic;
using System.Reflection;

namespace SignalR.TypeScript.Models
{
    public class TypeDefinition
    {
        public string TypeId { get; set; }
        public TypeInfo Type { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public IReadOnlyList<PropertyDefinition> Properties { get; set; }
    }

    public class TypeRef
    {
        public string TypeId { get; set; }
        public IReadOnlyList<TypeRef> GenericArguments { get; set; } 
        public TypeInfo BasicType { get; set; }
        public bool IsBasicType => BasicType != null;
    }

    public class PropertyDefinition
    {
        public string Name { get; set; }
        public TypeRef Type { get; set; }
        public bool IsOptional { get; set; }
    }

    public class ParameterDefinition
    {
        public string Name { get; set; }
        public TypeRef Type { get; set; }
        public bool HasDefault { get; set; }
    }

    public class MethodDefintion
    {
        public string Name { get; set; }
        public TypeRef ReturnType { get; set; }
        public IReadOnlyList<ParameterDefinition> Parameters { get; set; } 
    }
}
