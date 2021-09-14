using System.Collections.Generic;

namespace DiBK.RuleValidator.Models
{
    public class RuleSetGroup
    {
        public string Name { get; set; }
        public List<RuleInfo> Rules { get; set; } = new();
    }
}
