using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DiBK.RuleValidator.Config
{
    public class RuleValidatorSettings
    {
        public List<Assembly> RuleAssemblies { get; } = new();
        public List<TranslationAssembly> TranslationAssemblies { get; } = new();
        public int MaxMessageCount { get; set; } = int.MaxValue;

        public void AddRules(params string[] assemblyStrings)
        {
            RuleAssemblies.AddRange(assemblyStrings.Select(assemblyString => Assembly.Load(assemblyString)));
        }

        public void AddTranslations(string assemblyString, string rootNamespace = null)
        {
            TranslationAssemblies.Add(new TranslationAssembly { Assembly = Assembly.Load(assemblyString), RootNamespace = rootNamespace });
        }
    }

    public class TranslationAssembly
    {
        public Assembly Assembly { get; set; }
        public string RootNamespace { get; set; }
    }
}
