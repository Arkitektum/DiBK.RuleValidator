using System.Collections.Generic;

namespace DiBK.RuleValidator.Config
{
    public class ValidationGroupOptions
    {
        public string GroupId { get; private set; }
        public bool Skipped { get; private set; }
        public Dictionary<string, object> Settings { get; } = new();

        public ValidationGroupOptions(string groupId) => GroupId = groupId;

        public void Skip() => Skipped = true;
        public void AddSetting(string key, object value) => Settings[key] = value;
    }
}
