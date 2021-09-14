using DiBK.RuleValidator.Exceptions;
using DiBK.RuleValidator.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiBK.RuleValidator.Services
{
    public class RuleService : IRuleService
    {
        private readonly List<Rule> _rules;
        private readonly Dictionary<string, object> _ruleData;

        public RuleService()
        {
            _rules = new();
            _ruleData = new();
        }

        public void AddRules(IEnumerable<Rule> rules)
        {
            foreach (var rule in rules)
            {
                if (_rules.Any(r => r.GetType() == rule.GetType()))
                    throw new RuleAlreadyLoadedException($"The rule '{rule.GetType().Name}' is already loaded");

                _rules.Add(rule);
            }
        }

        public List<Rule> GetAll()
        {
            return _rules;
        }

        public List<Rule<T>> GetAll<T>() where T : class
        {
            return _rules.OfType<Rule<T>>().ToList();
        }

        public Rule<T> Get<T, U>() where T : class
        {
            var rule = _rules
                .SingleOrDefault(rule => rule.GetType() == typeof(U));

            return rule as Rule<T>;
        }

        public Rule<T> GetByType<T>(Type type) where T : class
        {
            var rule = _rules
                .SingleOrDefault(rule => rule.GetType() == type);

            return rule as Rule<T>;
        }

        public bool RulePassed<U, T>(T validationData) 
            where T : class
            where U : Rule<T>
        {
            var rule = ExecuteAndGet<U, T>(validationData);

            return rule.Passed;
        }

        public void Execute<U, T>(T validationData) 
            where T : class 
            where U : Rule<T>
        {
            var _ = ExecuteAndGet<U, T>(validationData);
        }

        public Status GetRuleStatus<T>(Type type, T validationData) 
            where T : class
        {
            if (!type.IsSubclassOf(typeof(Rule<T>)))
                throw new InvalidTypeException();

            var rule = GetByType<T>(type);

            rule.Execute(validationData);

            return rule.Status;
        }

        public void SetData(string key, object data)
        {
            _ruleData.Add(key, data);
        }

        public object GetData(string key)
        {
            return _ruleData[key];
        }

        private Rule<T> ExecuteAndGet<U, T>(T validationData) 
            where T : class 
            where U : Rule<T> 
        {
            var rule = Get<T, U>();

            if (rule == null)
                throw new RuleNotFoundException();

            rule.Execute(validationData);

            return rule;
        }
    }
}
