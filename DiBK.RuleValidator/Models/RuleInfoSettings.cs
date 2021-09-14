using System;
using System.Collections.Generic;

namespace DiBK.RuleValidator.Models
{
    public class RuleInfoSettings
    {
        internal List<Type> SkippedRules { get; } = new();
        internal List<string> SkippedGroups { get; } = new();

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
    }
}
