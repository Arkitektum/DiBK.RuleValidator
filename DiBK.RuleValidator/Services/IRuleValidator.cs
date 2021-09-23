using DiBK.RuleValidator.Config;
using System;
using System.Collections.Generic;

namespace DiBK.RuleValidator
{
    public interface IRuleValidator
    {
        void Validate<T>(T validationData, Action<ValidationOptions> options = null) where T : class;
        void LoadRules<T>(Action<ValidationOptions> options = null) where T : class;
        Rule<T> GetRule<U, T>() where T : class where U : Rule<T>;
        List<Rule> GetAllRules();
        List<Rule> GetExecutedRules();
        List<Rule> GetRulesBySettings(List<Rule> rules, Action<dynamic> optionsFilter);
        List<RuleSetGroup> GetRuleInfo(IEnumerable<Type> ruleTypes, Action<ValidationOptions> options = null);
    }
}
