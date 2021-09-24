﻿using DiBK.RuleValidator.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiBK.RuleValidator
{
    public abstract class Rule
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<IRuleMessage> Messages { get; set; } = new List<IRuleMessage>();
        public string Description { get; set; }
        public string PreCondition { get; set; }
        public string ChecklistReference { get; set; }
        public string Source { get; set; }
        public string Documentation { get; set; }
        public MessageType MessageType { get; set; } = MessageType.ERROR;
        public bool Passed => Status == Status.PASSED;
        public bool Executed => Status != Status.NOT_EXECUTED;
        public bool HasMessages => Messages.Any();
        public bool Disabled { get; set; }
        public Status Status { get; set; } = Status.NOT_EXECUTED;
        internal Type Parent { get; set; }
        internal Status ParentOutcome { get; set; } = Status.UNDEFINED;
        public abstract void Create();
        public override string ToString() => $"{Id}: {Name}";
    }

    public abstract class Rule<T> : Rule where T : class
    {
        private IRuleService _ruleService;
        private Dictionary<string, object> _settings;
        private ILogger<Rule> _logger;
        private bool _loaded;

        public Dependency<T> Dependency { get; set; }

        public void Setup(IRuleService ruleService, Dictionary<string, object> settings, ILogger<Rule> logger)
        {
            _ruleService = ruleService;
            _settings = settings;
            _logger = logger;
            _loaded = true;
        }

        protected Dependency<T> DependOn<U>() where U : Rule<T> => new(typeof(U), this);
        protected abstract Status Validate(T data);
        protected U GetData<U>(string key) where U : class => _ruleService.GetData<U>(key);
        protected void SetData(string key, object data) => _ruleService.SetData(key, data);
        
        protected U GetSetting<U>(string key) where U : class
        {
            if (_settings.ContainsKey(key) && _settings[key] is U u)
                return u;

            return null;
        }

        public void Execute(T data)
        {
            if (!CanExecute(data))
                return;

            var start = DateTime.Now;
            Status = Validate(data);
            double timeUsed = DateTime.Now.Subtract(start).TotalSeconds;

            _logger.LogInformation("{@Rule}", new
            {
                Id,
                Name,
                FullName = ToString(),
                Status,
                TimeUsed = timeUsed,
                MessageCount = Messages.Count
            });
        }

        private bool CanExecute(T data)
        {
            if (!_loaded)
                throw new RuleNotLoadedException($"Rule '{GetType().Name}' is not setup properly.");

            if (Disabled || Executed)
                return false;

            if (Dependency != null)
            {
                var rule = _ruleService.GetByType<T>(Dependency.Type);

                rule.Execute(data);

                if (Dependency.ShouldPass)
                    return rule.Status == Status.PASSED;
                else if (Dependency.ShouldFail)
                    return rule.Status == Status.FAILED;
                else if (Dependency.ShouldWarn)
                    return rule.Status == Status.WARNING;
                else if (Dependency.ShouldExecute)
                    return rule.Executed;
            }

            return true;
        }
    }
}
