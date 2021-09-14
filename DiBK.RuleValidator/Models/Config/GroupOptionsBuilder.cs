using System;
using System.Linq;

namespace DiBK.RuleValidator.Config
{
    public class GroupOptionsBuilder
    {
        public GroupOptions Group { get; }

        public GroupOptionsBuilder() => Group = new();

        public GroupOptionsBuilder AddRule<T>() where T : Rule
        {
            var ruleType = typeof(T);

            if (Group.Rules.Any(rule => rule.Type == ruleType))
                throw new ArgumentException($"The rule '{nameof(T)}' is already added!");

            var ruleOptions = new RuleOptions { Type = ruleType };
            Group.Rules.Add(ruleOptions);

            return this;
        }

        public GroupOptionsBuilder AddRule<T>(Action<RuleOptionsBuilder> options) where T : Rule
        {
            var ruleType = typeof(T);

            if (Group.Rules.Any(rule => rule.Type == ruleType))
                throw new ArgumentException($"Regelen '{nameof(T)}' er allerede lagt til");

            var builder = new RuleOptionsBuilder();
            options(builder);

            var ruleOptions = builder.Build();
            ruleOptions.Type = ruleType;

            Group.Rules.Add(ruleOptions);
            return this;
        }

        public GroupOptionsBuilder WithSetting(string key, object value)
        {
            Group.Settings[key] = value;
            return this;
        }

        internal GroupOptions Build() => Group;
    }
}
