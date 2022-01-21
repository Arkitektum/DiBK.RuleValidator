using System.Collections.Generic;
using System.Globalization;

namespace DiBK.RuleValidator.Config
{
    public class Translation
    {
        public string ResourceName { get; set; }
        public CultureInfo Culture { get; set; }
        public Dictionary<string, string> Texts { get; set; } = new();
    }
}
