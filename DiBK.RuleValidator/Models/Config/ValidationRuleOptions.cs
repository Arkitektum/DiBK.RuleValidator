using System;
using System.Collections.Generic;

namespace DiBK.RuleValidator.Config
{
    public class ValidationRuleOptions
    {
        public Type Type { get; private set; }
        public bool Skipped { get; private set; }
        public Dictionary<string, object> Settings { get; } = new();

        public ValidationRuleOptions(Type type) => Type = type;

        public void Skip() => Skipped = true;
        public void AddSetting(string key, object value) => Settings[key] = value;
    }
}
