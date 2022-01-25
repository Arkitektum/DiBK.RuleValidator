using DiBK.RuleValidator;
using System;
using System.Collections.Generic;

namespace DiBK.RuleValidator.Config
{
    public interface IRuleConfigs
    {
        RuleConfig Get(Type type);
    }

    public class RuleConfigs : IRuleConfigs
    {
        private readonly Dictionary<Type, RuleConfig> _ruleSets;

        public RuleConfigs(Dictionary<Type, RuleConfig> ruleSets) => _ruleSets = ruleSets;

        public RuleConfig Get(Type type)
        {
            if (_ruleSets.ContainsKey(type))
                return _ruleSets[type];

            throw new RuleException($"Rule configuration not found for type '{type.FullName}'.");
        }
    }
}
