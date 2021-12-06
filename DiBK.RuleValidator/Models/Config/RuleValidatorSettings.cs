using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DiBK.RuleValidator.Config
{
    public class RuleValidatorSettings
    {
        public List<Assembly> RuleAssemblies { get; } = new();
        public int MaxMessageCount { get; set; } = int.MaxValue;

        public void AddAssemblies(params string[] assemblyStrings)
        {
            RuleAssemblies.AddRange(assemblyStrings.Select(assemblyString => Assembly.Load(assemblyString)));
        }
    }
}
