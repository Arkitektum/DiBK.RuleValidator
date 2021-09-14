using System.Collections.Generic;

namespace DiBK.RuleValidator
{
    public class RuleSet
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<RuleSetGroup> Groups { get; set; } = new();

        public RuleSet()
        {
        }
    }
}
