using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiBK.RuleValidator
{
    public abstract class Rule
    {
        private readonly ConcurrentBag<RuleMessage> _messages = new();
        public string Id { get; protected set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public string Documentation { get; set; }
        public MessageType MessageType { get; set; } = MessageType.ERROR;
        public ReadOnlyCollection<RuleMessage> Messages => _messages.Take(MaxMessageCount).ToList().AsReadOnly();
        public double TimeUsed { get; protected set; }
        public bool HasMessages => Messages.Any();
        public bool Passed => Status == Status.PASSED;
        public bool Executed => Status != Status.SKIPPED;
        public Status Status { get; set; } = Status.SKIPPED;
        public abstract void Create();
        public override string ToString() => $"{Id}: {Name}";
        public virtual void AddMessage(RuleMessage message) => _messages.Add(message);
        public int MaxMessageCount { get; internal set; } = int.MaxValue;

        public static readonly IEnumerable<string> TranslatableProperties = new[] { "Name", "Description", "PreCondition", "ChecklistReference", "Source", "Documentation" };
    }

    public abstract class ExecutableRule : Rule, IDisposable
    {
        private bool _disposed = false;
        public bool Disabled { get; protected set; }
        public CancellationTokenSource TokenSource { get; } = new();

        public override sealed void AddMessage(RuleMessage message)
        {
            if (Messages.Count == MaxMessageCount)
            {
                TokenSource.Cancel();
                TokenSource.Token.ThrowIfCancellationRequested();
            }

            base.AddMessage(message);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    TokenSource.Dispose();

                _disposed = true;
            }
        }
    }

    public abstract class Rule<T> : ExecutableRule where T : class
    {
        private IRuleService _ruleService;
        private IReadOnlyDictionary<string, object> _settings;
        private IReadOnlyDictionary<string, string> _translations;
        private ILogger<Rule> _logger;
        private bool _loaded;

        public void Setup(
            IRuleService ruleService, 
            IReadOnlyDictionary<string, object> settings, 
            IReadOnlyDictionary<string, string> translations,
            Func<RuleResult, Task> onRuleExecuted,
            ILogger<Rule> logger,
            int maxMessageCount)
        {
            _ruleService = ruleService;
            _settings = settings;
            _translations = translations;
            _logger = logger;
            MaxMessageCount = maxMessageCount;
            OnRuleExecuted = onRuleExecuted;
            _loaded = true;
        }

        private bool Skipped { get; set; }
        public List<Dependency<T>> Dependencies { get; } = new();
        protected Dependency<T> DependOn<U>() where U : Rule<T> => new(typeof(U), this);
        protected virtual void Validate(T input) { }
        protected virtual Task ValidateAsync(T input) => Task.CompletedTask;
        protected U GetData<U>(string key) where U : class => _ruleService.GetData<U>(key);
        protected void SetData(string key, object data) => _ruleService.SetData(key, data);
        public Func<RuleResult, Task> OnRuleExecuted { get; private set; }

        protected string Translate(string key, params object[] arguments)
        {
            if (_translations.TryGetValue(key, out var translation))
                return string.Format(translation, arguments);

            return key;
        }

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

        public async Task Execute(T input)
        {
            if (!await CanExecute(input))
                return;

            var startTime = DateTime.Now;
            
            var validationTask = Task.Run(async () => 
            { 
                Validate(input); 
                await ValidateAsync(input);  
            }, TokenSource.Token);

            try
            {
                await validationTask;
            }
            catch (Exception exception)
            {
                if (exception is not OperationCanceledException && exception is not AggregateException)
                    throw;
            }
            finally
            {
                SetStatus();

                TimeUsed = DateTime.Now.Subtract(startTime).TotalSeconds;

                var result = new RuleResult(Id, Name, Status, TimeUsed, Messages.Count);

                _logger.LogInformation("{@Rule}", result);

                OnRuleExecuted?.Invoke(result);
            }
        }

        private async Task<bool> CanExecute(T input)
        {
            if (!_loaded)
                throw new RuleException($"Rule '{GetType().Name}' is not setup properly.");

            if (Disabled || Executed)
                return false;

            if (!Dependencies.Any())
                return true;

            return await Dependencies
                .AllAsync(async dependency =>
                {
                    var rule = _ruleService.GetByType<T>(dependency.Type);

                    if (rule == null)
                        return false;

                    await rule.Execute(input);

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
