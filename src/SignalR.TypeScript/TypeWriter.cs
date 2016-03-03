using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SignalR.TypeScript.Helpers;
using SignalR.TypeScript.Models;

namespace SignalR.TypeScript
{
    public class TypeWriter
    {
        /// <summary>
        /// Gets the typescript string representation of a type reference to a "basic" type.
        /// </summary>
        /// <param name="context">The type info context.</param>
        /// <param name="typeRef">The type reference.</param>
        private string GetBasicTypeString(HubsContext context, TypeRef typeRef)
        {
            if (typeRef.BasicType.IsDefaultType())
            {
                return TypeHelpers.DefaultTypeMappings[typeRef.BasicType];
            }

            if (typeRef.BasicType.IsGenericParameter)
            {
                return typeRef.BasicType.Name;
            }

            var type = typeRef.BasicType;

            var typeInterfaces = type.ImplementedInterfaces.ToList();
            var dictionaryInterface =
                typeInterfaces.FirstOrDefault(
                    @interface =>
                        @interface == typeof(ICollection<>) &&
                        @interface.GenericTypeArguments[0] == typeof(KeyValuePair<,>));

            if (dictionaryInterface != null)
            {
                // { [key: keyType]: valueType; }
                return $"{{ [key: {GetTypeString(context, typeRef.GenericArguments[0])}]: {GetTypeString(context, typeRef.GenericArguments[1])}; }}";
            }
            
            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type) && typeRef.GenericArguments.Count > 0)
            {
                // genericType[]
                return $"{GetTypeString(context, typeRef.GenericArguments[0])}[]";
            }
            return $"any[]";
        }

        /// <summary>
        /// Gets the typescript string representation of a type reference.
        /// </summary>
        /// <param name="context">The type info context.</param>
        /// <param name="typeRef">The type reference.</param>
        private string GetTypeString(HubsContext context, TypeRef typeRef)
        {
            if (typeRef.IsBasicType)
            {
                return GetBasicTypeString(context, typeRef);
            }

            var type = context.TypeDefinitions[typeRef.TypeId];

            var typeDefinitionEntry =
                context.TypeDefinitions.Single(typeDef => typeDef.Value.FullName == type.FullName);

            var typeName = typeDefinitionEntry.Value.Type.GetFullNameWithoutGenericArity();
            if (typeRef.GenericArguments.Count > 0)
            {
                typeName += "<";
                for (var i = 0; i < typeRef.GenericArguments.Count; i++)
                {
                    typeName += GetTypeString(context, typeRef.GenericArguments[i]);
                    if (i < typeRef.GenericArguments.Count - 1)
                        typeName += ", ";
                }
                typeName += ">";
            }
            return typeName;
        }

        /// <summary>
        /// Turns a list of hub definitions into a typescript defintion file string.
        /// </summary>
        /// <param name="hubs">A list of hub definitions, with the first part being the "server" or hub type, and the second being the "client" type.</param>
        /// <returns>A well-formatted typescript definition file for the specififed hubs.</returns>
        public string WriteTypeDefinition(IReadOnlyCollection<Tuple<TypeInfo, TypeInfo>> hubs)
        {
            var context = new HubsContext(hubs);
            var def = new StringBuilder();
            def.AppendLine("interface SignalR");
            def.AppendLine("{");
            foreach (var hubDefinition in context.HubDefinitions)
            {
                def.AppendLine($"    {hubDefinition.Name}: {hubDefinition.TypeName}Proxy;");
            }
            def.AppendLine("}");
            def.AppendLine();
            foreach (var hubDefinition in context.HubDefinitions)
            {
                WriteHub(def, context, hubDefinition);
            }
            def.AppendLine();
            WriteTypes(def, context);
            return def.ToString();
        }
        
        private void WriteHub(StringBuilder def, HubsContext context, HubDefinition hub)
        {
            def.AppendLine($"interface {hub.TypeName}Proxy extends HubProxy");
            def.AppendLine("{");
            def.AppendLine($"    server: {hub.TypeName};");
            def.AppendLine($"    client: {hub.TypeName}Client;");
            def.AppendLine("}");
            def.AppendLine();
            def.AppendLine($"interface {hub.TypeName}");
            def.AppendLine("{");
            WriteMethods(def, context, hub.ServerMethods);
            def.AppendLine("}");
            def.AppendLine();
            def.AppendLine($"interface {hub.TypeName}Client");
            def.AppendLine("{");
            WriteMethods(def, context, hub.ClientMethods, true);
            def.AppendLine("}");
        }

        private void WriteMethods(StringBuilder def, HubsContext context, IReadOnlyCollection<MethodDefintion> hub, bool isClient = false)
        {
            foreach (var method in hub)
            {
                def.Append($"    {JavascriptHelpers.CamelCase(method.Name)}("); //    method(
                for (var i = 0; i < method.Parameters.Count; i++)
                {
                    var param = method.Parameters[i];
                    def.Append(param.Name);
                    def.Append(": ");
                    def.Append(GetTypeString(context, param.Type));
                    if (i < method.Parameters.Count - 1)
                    {
                        def.Append(", ");
                    }
                }
                def.Append(isClient ? "): void;" : $"): PromiseLike<{GetTypeString(context, method.ReturnType)}>;");
                def.AppendLine();
            }
        }

        private void WriteTypes(StringBuilder def, HubsContext context)
        {
            var namespaces = context.TypeDefinitions.GroupBy(defintion => GetNamespace(defintion.Value.FullName));
            foreach (var @namespace in namespaces)
            {
                def.AppendLine($"declare namespace {@namespace.Key}");
                def.AppendLine("{");
                foreach (var typePair in @namespace)
                {
                    WriteType(def, context, typePair.Value);
                    def.AppendLine();
                }
                def.AppendLine("}");
            }
        }

        private string GetNamespace(string fullName)
        {
            var index = fullName.LastIndexOf(".", StringComparison.Ordinal);
            return index == -1 ? fullName : fullName.Substring(0, index);
        }

        private void WriteType(StringBuilder def, HubsContext context, TypeDefinition type)
        {
            var typeInfo = type.Type;
            def.Append($"    interface {typeInfo.GetNameWithoutGenericArity()}");
            if(typeInfo.IsGenericType)
            {
                
                var args = typeInfo.GenericTypeParameters;
                if (args.Length == 1)
                {
                    def.Append($"<{args[0].Name}>");
                }
                else
                {
                    def.Append("<");
                    for (var i = 0; i < args.Length; i++)
                    {
                        def.Append(args[i].Name);
                        if (i < args.Length - 1)
                        {
                            def.Append(", ");
                        }
                    }
                    def.Append(">");
                }
            }
            def.AppendLine();
            def.AppendLine("    {");
            foreach (var property in type.Properties)
            {
                WriteProperty(def, context, property);
            }
            def.AppendLine("    }");
        }

        private void WriteProperty(StringBuilder def, HubsContext context, PropertyDefinition property)
        {
            def.Append("        ");
            def.Append(property.Name);
            if (property.IsOptional)
            {
                def.Append("?");
            }
            def.Append(": ");
            def.Append(GetTypeString(context, property.Type));
            def.AppendLine(";");
        }
    }
}
