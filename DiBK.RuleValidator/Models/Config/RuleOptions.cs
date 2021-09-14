using System;
using System.Collections.Generic;

namespace DiBK.RuleValidator.Models.Config
{
    public class RuleOptions
    {
        public Type Type { get; internal set; }
        public Dictionary<string, object> Settings { get; } = new();
    }
}
