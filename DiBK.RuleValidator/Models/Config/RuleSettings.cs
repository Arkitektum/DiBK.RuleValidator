namespace DiBK.RuleValidator.Config
{
    public interface IRuleSettings
    {
        RuleConfigs RuleConfigs { get; }
        int MaxMessageCount { get; }
    }

    public class RuleSettings : IRuleSettings
    {
        public RuleConfigs RuleConfigs { get; }
        public int MaxMessageCount { get; }

        public RuleSettings(RuleConfigs ruleConfigs, int maxMessageCount)
        {
            RuleConfigs = ruleConfigs;
            MaxMessageCount = maxMessageCount;
        }
    }
}
