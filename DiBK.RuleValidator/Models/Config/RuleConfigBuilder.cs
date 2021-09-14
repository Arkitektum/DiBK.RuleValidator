using DiBK.RuleValidator.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiBK.RuleValidator.Models.Config
{
    public class RuleConfigBuilder<T> where T : class
    {
        public RuleConfig RuleConfig { get; }

        public RuleConfigBuilder(Type type) => RuleConfig = new(type);

        public RuleConfigBuilder(Type type, string name) => RuleConfig = new(type, name);

        public RuleConfigBuilder(Type type, string name, string description) => RuleConfig = new(type, name, description);

        public RuleConfigBuilder<T> AddGroup(string id, string name, Action<GroupOptionsBuilder> settings)
        {
            var builder = new GroupOptionsBuilder();
            settings(builder);

            var group = builder.Build();
            group.GroupId = id;
            group.Name = name;

            RuleConfig.Groups.Add(group);
            return this;
        }

        public RuleConfigBuilder<T> WithGlobalSettings(Dictionary<string, object> settings)
        {
            RuleConfig.GlobalSettings.Append(settings);

            return this;
        }

        public RuleConfig Build() => RuleConfig;
    }
}
