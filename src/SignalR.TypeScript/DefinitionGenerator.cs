using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SignalR.TypeScript
{
    public class DefinitionGenerator
    {
        private readonly Assembly _targetAssembly;

        public DefinitionGenerator(Assembly targetAssembly)
        {
            _targetAssembly = targetAssembly;
        }

        public string GetTypeDefinitionFile()
        {
            var hubs = GetHubs();
            var typeWriter = new TypeWriter();
            return typeWriter.WriteTypeDefinition(hubs);
        }

        /// <summary>
        /// Get all declared hub types in the assembly.
        /// </summary>
        private IReadOnlyList<Tuple<TypeInfo, TypeInfo>> GetHubs()
        {
            var hubTypes = _targetAssembly
                .GetTypes()
                .Select(type => type.GetTypeInfo())
                .Where(t => t.BaseType != null && (t.BaseType.Name == "Hub" || t.BaseType.Name == "Hub`1"));

            return hubTypes
                .Select(hubType => Tuple.Create(hubType, hubType.BaseType?.Name == "Hub`1" ? hubType.BaseType?.GenericTypeArguments.First().GetTypeInfo() : null)).ToList();
        }
    }
}
