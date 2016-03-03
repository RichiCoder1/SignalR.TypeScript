using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SignalR.TypeScript.Models;
using System.Linq;
using System.Threading.Tasks;
using SignalR.TypeScript.Helpers;

namespace SignalR.TypeScript
{
    public class HubsContext
    {
        private readonly Dictionary<string, TypeDefinition> _typeDefinitions;
        private readonly List<HubDefinition> _hubDefinitions;

        public HubsContext(IReadOnlyCollection<Tuple<TypeInfo, TypeInfo>> hubs)
        {
            _typeDefinitions = new Dictionary<string, TypeDefinition>();
            _hubDefinitions = hubs.Select(ProcessHub).ToList();
        }

        internal static bool IsBasicType(TypeInfo type)
        {
            // Is this a default built-in?
            if (type.IsDefaultType())
                return true;

            if (type.IsGenericParameter)
                return true;

            // Is this a collection type?
            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type))
                return true;

            return false;
        }

        public IReadOnlyDictionary<string, TypeDefinition> TypeDefinitions => _typeDefinitions;

        public IReadOnlyCollection<HubDefinition> HubDefinitions => _hubDefinitions;

        private HubDefinition ProcessHub(Tuple<TypeInfo, TypeInfo> hub)
        {
            return new HubDefinition
            {
                Name = hub.Item1.GetHubName(),
                TypeName = hub.Item1.Name,
                ServerMethods = ProcessHubInterface(hub.Item1),
                ClientMethods = ProcessHubInterface(hub.Item2, true)
            };
        }

        private IReadOnlyCollection<MethodDefintion> ProcessHubInterface(TypeInfo hubInterface, bool allowVirtual = false)
        {
            if (hubInterface == null)
            {
                return new MethodDefintion[0];
            }

            // Get all public instance methods that are declared on the hub, exculding overrides (like OnConnecting).
            var methods =
                hubInterface.DeclaredMethods
                    .Where(method => !(method.IsVirtual && !allowVirtual) && !method.IsStatic && !method.IsPrivate);

            return methods.Select(GetMethodDefinition).ToList();

        }

        private TypeRef EnsureTypeDefinition(Type type)
        {
            return EnsureTypeDefinition(type.GetTypeInfo());
        }

        private TypeRef EnsureTypeDefinition(TypeInfo type)
        {
            if (IsBasicType(type))
            {
                return new TypeRef
                {
                    BasicType = type,
                    GenericArguments = type.GenericTypeArguments.Select(EnsureTypeDefinition).ToList()
                };
            }

            var rawType = type.GetRawType();
            var prexistingDefinition =
                _typeDefinitions
                    .FirstOrDefault(typeDef => typeDef.Value.FullName == rawType.FullName);
            if (prexistingDefinition.Key != null)
            {
                return new TypeRef
                {
                    TypeId = prexistingDefinition.Key,
                    GenericArguments = type.GenericTypeArguments.Select(EnsureTypeDefinition).ToList()
                };
            }

            var typeDefinition = new TypeDefinition
            {
                TypeId = Guid.NewGuid().ToString(),
                Type = rawType,
                Name = rawType.Name,
                FullName = rawType.FullName
            };

            _typeDefinitions.Add(typeDefinition.TypeId, typeDefinition);

            typeDefinition.Properties =
                rawType.DeclaredProperties
                    .Where(prop => prop.GetGetMethod() != null)
                    .Select(GetPropertyDefinition)
                    .Concat(rawType.DeclaredFields
                        .Where(field => field.IsPublic && !field.IsStatic)
                        .Select(GetPropertyDefinition)).ToList();

            return new TypeRef
            {
                TypeId = typeDefinition.TypeId,
                GenericArguments = type.GenericTypeArguments.Select(EnsureTypeDefinition).ToList()
            };
        }

        private MethodDefintion GetMethodDefinition(MethodInfo method)
        {
            var definition = new MethodDefintion
            {
                Name = method.Name,
                Parameters = method.GetParameters().Select(GetParameterDefinition).ToList()
            };

            if (method.ReturnType == typeof(Task))
            {
                definition.ReturnType = new TypeRef { BasicType = typeof(void).GetTypeInfo(), GenericArguments = new List<TypeRef>() };
            }
            else if (method.ReturnType == typeof(Task<>))
            {
                definition.ReturnType = EnsureTypeDefinition(method.ReturnType.GetGenericArguments()[0].GetTypeInfo());
            }
            else
            {
                definition.ReturnType = EnsureTypeDefinition(method.ReturnType);
            }

            return definition;
        }

        public PropertyDefinition GetPropertyDefinition(FieldInfo field)
        {
            return new PropertyDefinition
            {
                Name = field.Name,
                Type = EnsureTypeDefinition(field.FieldType),
                IsOptional =
                    !field.FieldType.GetTypeInfo().IsValueType ||
                    (Nullable.GetUnderlyingType(field.FieldType) != null)
            };
        }

        public PropertyDefinition GetPropertyDefinition(PropertyInfo property)
        {
            return new PropertyDefinition
            {
                Name = property.Name,
                Type = EnsureTypeDefinition(property.PropertyType),
                IsOptional =
                    !property.PropertyType.GetTypeInfo().IsValueType ||
                    (Nullable.GetUnderlyingType(property.PropertyType) != null)
            };
        }

        public ParameterDefinition GetParameterDefinition(ParameterInfo parameter)
        {
            return new ParameterDefinition
            {
                Name = parameter.Name,
                Type = EnsureTypeDefinition(parameter.ParameterType),
                HasDefault = parameter.HasDefaultValue
            };
        }
    }
}
