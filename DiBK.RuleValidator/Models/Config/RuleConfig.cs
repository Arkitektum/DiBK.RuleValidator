using System;
using System.Collections.Generic;

namespace DiBK.RuleValidator.Models.Config
{
    public class RuleConfig
    {
        public Type Type { get; internal set; }
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public List<GroupOptions> Groups { get; } = new();
        public Dictionary<string, object> GlobalSettings { get; } = new();

        public RuleConfig(Type type)
        {
            Type = type;
        }

        public RuleConfig(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        public RuleConfig(Type type, string name, string description)
        {
            Type = type;
            Name = name;
            Description = description;
        }

        public static RuleConfigBuilder<T> Create<T>() where T : class => new(typeof(T));
        public static RuleConfigBuilder<T> Create<T>(string name) where T : class => new(typeof(T), name);
        public static RuleConfigBuilder<T> Create<T>(string name, string description) where T : class => new(typeof(T), name, description);
    }
}
