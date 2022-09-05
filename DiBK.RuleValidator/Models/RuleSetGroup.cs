using System.Collections.Generic;

namespace DiBK.RuleValidator
{
    public class RuleSetGroup
    {
        public string Name { get; set; }
        public string GroupId { get; set; }
        public List<RuleInfo> Rules { get; set; } = new();
    }
}
