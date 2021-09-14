using DiBK.RuleValidator.Config;
using System;
using System.Collections.Generic;

namespace DiBK.RuleValidator
{
    public class ValidationConfig
    {
        public ValidationSettings ValidationOptions { get; set; }
        public RuleConfig RuleSetConfig { get; set; }

        public ValidationConfig(ValidationSettings validationOptions, RuleConfig ruleSetConfig)
        {
            ValidationOptions = validationOptions;
            RuleSetConfig = ruleSetConfig;
        }
    }

    public class ValidationSettings
    {
        internal List<Type> SkippedRules { get; } = new();
        internal List<string> SkippedGroups { get; } = new();
        internal List<ValidationGroupSettings> GroupSettings { get; set; } = new();
        internal List<ValidationRuleSettings> RuleSettings { get; set; } = new();
        internal Dictionary<string, object> Settings { get; } = new();

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

        public void AddSettings(string key, object value) => Settings[key] = value;

        public void ForGroup(string id, Action<ValidationGroupSettings> settings)
        {
            var groupSettings = new ValidationGroupSettings(id);
            settings.Invoke(groupSettings);

            if (groupSettings.Skipped)
                SkipGroup(id);

            GroupSettings.Add(groupSettings);
        }

        public void ForRule<T>(Action<ValidationRuleSettings> settings) where T : Rule
        {
            var ruleSettings = new ValidationRuleSettings(typeof(T));
            settings.Invoke(ruleSettings);

            if (ruleSettings.Skipped)
                SkipRule<T>();

            RuleSettings.Add(ruleSettings);
        }
    }

    public class ValidationGroupSettings
    {
        public string GroupId { get; private set; }
        public bool Skipped { get; private set; }
        public Dictionary<string, object> Settings { get; } = new();

        public ValidationGroupSettings(string groupId) => GroupId = groupId;

        public void Skip() => Skipped = true;
        public void AddSetting(string key, object value) => Settings[key] = value;
    }

    public class ValidationRuleSettings
    {
        public Type Type { get; private set; }
        public bool Skipped { get; private set; }
        public Dictionary<string, object> Settings { get; } = new();

        public ValidationRuleSettings(Type type) => Type = type;

        public void Skip() => Skipped = true;
        public void AddSetting(string key, object value) => Settings[key] = value;
    }
}
