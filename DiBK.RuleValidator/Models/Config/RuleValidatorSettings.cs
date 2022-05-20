using System;
using System.Collections.Generic;
using System.Reflection;

namespace DiBK.RuleValidator.Config
{
    public class RuleValidatorSettings
    {
        public List<Assembly> RuleAssemblies { get; } = new();
        public List<TranslationAssembly> TranslationAssemblies { get; } = new();
        public int MaxMessageCount { get; set; } = int.MaxValue;

        public void AddRuleAssembly(string assemblyName)
        {
            RuleAssemblies.Add(Assembly.Load(assemblyName));
        }

        public void AddTranslationAssembly(string assemblyName, string rootNamespace = null)
        {
            TranslationAssemblies.Add(new TranslationAssembly { Assembly = Assembly.Load(assemblyName), RootNamespace = rootNamespace });
        }
    }
}
