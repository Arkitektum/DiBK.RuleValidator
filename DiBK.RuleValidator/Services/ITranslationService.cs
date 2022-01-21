using System.Collections.Generic;

namespace DiBK.RuleValidator
{
    public interface ITranslationService
    {
        IReadOnlyDictionary<string, string> GetTranslationsForRule(Rule rule);
    }
}
