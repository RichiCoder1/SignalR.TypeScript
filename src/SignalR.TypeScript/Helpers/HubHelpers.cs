using System.Linq;
using System.Reflection;

namespace SignalR.TypeScript.Helpers
{
    public static class HubHelpers
    {
        public static string GetHubName(this TypeInfo hubType)
        {
            var attrs = hubType.CustomAttributes.ToList();
            if (attrs.All(attr => attr.AttributeType.Name != "HubNameAttribute"))
            {
                return JavascriptHelpers.CamelCase(hubType.Name);
            }

            var hubNameAttr = attrs.Single(attr => attr.AttributeType.Name == "HubNameAttribute");
            return (string) hubNameAttr.ConstructorArguments.First().Value;
        }
    }
}
