using DiBK.RuleValidator.Models;
using System;
using System.Collections.Generic;

namespace DiBK.RuleValidator.Services
{
    public interface IRuleService
    {
        void AddRules(IEnumerable<Rule> rules);
        List<Rule> GetAll();
        Rule<T> Get<T, U>() where T : class;
        Rule<T> GetByType<T>(Type type) where T : class;
        bool RulePassed<T, U>(U validationData) where T : Rule<U> where U : class;
        void Execute<T, U>(U validationData) where T : Rule<U> where U : class;
        Status GetRuleStatus<U>(Type type, U validationData) where U : class;
        void SetData(string key, object data);
        object GetData(string key);
    }
}
