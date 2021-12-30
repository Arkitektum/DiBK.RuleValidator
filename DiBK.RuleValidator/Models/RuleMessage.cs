using System.Collections.Generic;

namespace DiBK.RuleValidator
{
    public class RuleMessage
    {
        public string Message { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
