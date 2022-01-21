using System.Collections.Generic;

namespace DiBK.RuleValidator.Config
{
    public interface IRuleSettings
    {
        RuleConfigs RuleConfigs { get; }
        List<Translation> Translations { get; }
        List<Translation> CustomTranslations { get; }
        int MaxMessageCount { get; }
    }

    public class RuleSettings : IRuleSettings
    {
        public RuleConfigs RuleConfigs { get; }
        public List<Translation> Translations { get; }
        public List<Translation> CustomTranslations { get; }
        public int MaxMessageCount { get; }

        public RuleSettings(RuleConfigs ruleConfigs, List<Translation> translations, List<Translation> customTranslations, int maxMessageCount)
        {
            RuleConfigs = ruleConfigs;
            Translations = translations;
            CustomTranslations = customTranslations;
            MaxMessageCount = maxMessageCount;
        }
    }
}
