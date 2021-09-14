using System.Collections.Generic;

namespace DiBK.RuleValidator.Models.Config
{
    public class GroupOptions
    {
        public string GroupId { get; internal set; }
        public string Name { get; internal set; }
        public List<RuleOptions> Rules { get; } = new();
        public Dictionary<string, object> Settings { get; } = new();
    }
}
