using System;
using System.Collections.Generic;

namespace SignalR.TypeScript.Models
{
    public class HubDefinition
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public IReadOnlyCollection<MethodDefintion> ClientMethods { get; set; } 
        public IReadOnlyCollection<MethodDefintion> ServerMethods { get; set; } 
    }
}
