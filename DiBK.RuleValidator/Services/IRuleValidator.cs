using DiBK.RuleValidator.Models;
using System;
using System.Collections.Generic;

namespace DiBK.RuleValidator.Services
{
    public interface IRuleValidator
    {
        void Validate<T>(T validationData, Action<ValidationSettings> options = null) where T : class;
        void LoadRules<T>(Action<ValidationSettings> settings = null) where T : class;
        Rule<T> GetRule<U, T>() where T : class where U : Rule<T>;
        List<Rule> GetAllRules();
        List<Rule> GetExecutedRules();
        List<Rule> GetRulesBySettings(List<Rule> rules, Action<dynamic> optionsFilter);
        List<RuleSet> GetRuleInfo(IEnumerable<Type> ruleTypes, Action<RuleInfoSettings> options = null);
    }
}
