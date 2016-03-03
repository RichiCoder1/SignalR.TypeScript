using System;
using System.Linq;

namespace SignalR.TypeScript.Helpers
{
    public static class JavascriptHelpers
    {
        /// <summary>
        /// Converts the specified name to camel case.
        /// </summary>
        /// <param name="name">The name to convert.</param>
        /// <returns>A camel cased version of the specified name.</returns>
        public static string CamelCase(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return string.Join(".", name.Split('.').Select(n => char.ToLowerInvariant(n[0]) + n.Substring(1)));
        }
    }
}
