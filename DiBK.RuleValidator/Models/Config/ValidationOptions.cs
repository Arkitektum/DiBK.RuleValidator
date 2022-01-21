using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DiBK.RuleValidator.Config
{
    public sealed class ValidationOptions
    {
        internal List<Type> SkippedRules { get; } = new();
        internal List<string> SkippedGroups { get; } = new();
        internal List<ValidationGroupOptions> GroupOptions { get; set; } = new();
        internal List<ValidationRuleOptions> RuleOptions { get; set; } = new();
        internal Dictionary<string, object> GlobalSettings { get; } = new();
        internal List<Assembly> ResourceAssemblies { get; } = new();

        public void SkipRule<T>() where T : Rule
        {
            var type = typeof(T);

            if (!SkippedRules.Contains(type))
                SkippedRules.Add(type);
        }

        public void SkipGroup(string groupId)
        {
            if (!SkippedGroups.Contains(groupId))
                SkippedGroups.Add(groupId);
        }

        public void AddGlobalSettings(string key, object value) => GlobalSettings[key] = value;

        public void AddResources(Assembly assembly, string namespacePrefix) => ResourceAssemblies.Add(assembly);

        public void ForGroup(string id, Action<ValidationGroupOptions> options)
        {
            var groupOptions = new ValidationGroupOptions(id);
            options.Invoke(groupOptions);

            if (groupOptions.Skipped)
                SkipGroup(id);

            GroupOptions.Add(groupOptions);
        }

        public void ForRule<T>(Action<ValidationRuleOptions> options) where T : Rule
        {
            var ruleOptions = new ValidationRuleOptions(typeof(T));
            options.Invoke(ruleOptions);

            if (ruleOptions.Skipped)
                SkipRule<T>();

            RuleOptions.Add(ruleOptions);
        }
    }
}
