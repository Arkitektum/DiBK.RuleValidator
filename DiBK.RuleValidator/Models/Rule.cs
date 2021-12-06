using DiBK.RuleValidator.Exceptions;
using DiBK.RuleValidator.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiBK.RuleValidator
{
    public abstract class Rule
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PreCondition { get; set; }
        public string ChecklistReference { get; set; }
        public string Source { get; set; }
        public string Documentation { get; set; }
        public MessageType MessageType { get; set; } = MessageType.ERROR;
        public List<IRuleMessage> Messages { get; set; } = new(1000);
        public bool HasMessages => Messages.Any();
        public bool Passed => Status == Status.PASSED;
        public bool Executed => Status != Status.SKIPPED;
        public Status Status { get; set; } = Status.SKIPPED;
        public abstract void Create();
        public override string ToString() => $"{Id}: {Name}";
    }

    public abstract class ExecutableRule : Rule
    {
        public bool Disabled { get; protected set; }
        protected int MaxMessageCount { get; set; }
        protected CancellationTokenSource TokenSource { get; } = new();

        public void AddMessage(IRuleMessage message)
        {
            if (Messages.Count == MaxMessageCount)
            {
                TokenSource.Cancel();
                TokenSource.Token.ThrowIfCancellationRequested();
            }

            Messages.Add(message);
        }
    }

    public abstract class Rule<T> : ExecutableRule where T : class
    {
        private IRuleService _ruleService;
        private IReadOnlyDictionary<string, object> _settings;
        private ILogger<Rule> _logger;

        public void Setup(IRuleService ruleService, IReadOnlyDictionary<string, object> settings, int maxMessageCount, ILogger<Rule> logger)
        {
            _ruleService = ruleService;
            _settings = settings;
            _logger = logger;
            MaxMessageCount = maxMessageCount;
            Loaded = true;
        }

        private bool Loaded { get; set; }
        private bool Skipped { get; set; }
        public List<Dependency<T>> Dependencies { get; } = new();
        protected Dependency<T> DependOn<U>() where U : Rule<T> => new(typeof(U), this);
        protected abstract void Validate(T data);
        protected U GetData<U>(string key) where U : class => _ruleService.GetData<U>(key);
        protected void SetData(string key, object data) => _ruleService.SetData(key, data);

        protected void SkipRule()
        {
            Skipped = true;
            TokenSource.Cancel();
            TokenSource.Token.ThrowIfCancellationRequested();
        }

        protected U GetSetting<U>(string key) where U : class
        {
            if (_settings.ContainsKey(key) && _settings[key] is U u)
                return u;

            return null;
        }

        public async Task Execute(T data)
        {
            if (!await CanExecute(data))
                return;

            var start = DateTime.Now;
            var validationTask = Task.Run(() => { Validate(data); }, TokenSource.Token);

            try
            {
                await validationTask;
            }
            catch (OperationCanceledException)
            {       
            }
            finally
            {
                double timeUsed = DateTime.Now.Subtract(start).TotalSeconds;

                TokenSource.Dispose();

                SetStatus();

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
        }

        private async Task<bool> CanExecute(T data)
        {
            if (!Loaded)
                throw new RuleNotLoadedException($"Rule '{GetType().Name}' is not setup properly.");

            if (Disabled || Executed)
                return false;

            if (!Dependencies.Any())
                return true;

            return await Dependencies
                .AllAsync(async dependency =>
                {
                    var rule = _ruleService.GetByType<T>(dependency.Type);

                    await rule.Execute(data);

                    if (dependency.ShouldPass)
                        return rule.Status == Status.PASSED;
                    else if (dependency.ShouldFail)
                        return rule.Status == Status.FAILED;
                    else if (dependency.ShouldWarn)
                        return rule.Status == Status.WARNING;
                    else if (dependency.ShouldExecute)
                        return rule.Executed;

                    return true;
                });
        }

        private void SetStatus()
        {
            if (Skipped)
                Status = Status.SKIPPED;
            else if (!HasMessages)
                Status = Status.PASSED;
            else if (MessageType == MessageType.ERROR)
                Status = Status.FAILED;
            else if (MessageType == MessageType.WARNING)
                Status = Status.WARNING;
            else if (MessageType == MessageType.INFORMATION)
                Status = Status.INFO;
        }
    }
}
