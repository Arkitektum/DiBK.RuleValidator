﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiBK.RuleValidator
{
    public interface IRuleService
    {
        void AddRules(IEnumerable<Rule> rules);
        List<Rule> GetAll();
        Rule<T> Get<T, U>() where T : class;
        Rule<T> GetByType<T>(Type type) where T : class;
        Task<bool> RulePassed<T, U>(U validationData) where T : Rule<U> where U : class;
        Task Execute<T, U>(U validationData) where T : Rule<U> where U : class;
        Task<Status> GetRuleStatus<U>(Type type, U validationData) where U : class;
        void SetData(string key, object data);
        U GetData<U>(string key) where U : class;
    }
}
