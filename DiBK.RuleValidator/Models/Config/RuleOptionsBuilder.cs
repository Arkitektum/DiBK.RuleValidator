namespace DiBK.RuleValidator.Models.Config
{
    public class RuleOptionsBuilder
    {
        public RuleOptions Rule { get; }

        public RuleOptionsBuilder() => Rule = new();

        public RuleOptionsBuilder WithSetting(string key, object value)
        {
            Rule.Settings[key] = value;
            return this;
        }

        internal RuleOptions Build() => Rule;
    }
}
