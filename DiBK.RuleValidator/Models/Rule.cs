using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiBK.RuleValidator
{
    public abstract class Rule
    {
        private readonly List<RuleMessage> _messages = new(1000);
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PreCondition { get; set; }
        public string ChecklistReference { get; set; }
        public string Source { get; set; }
        public string Documentation { get; set; }
        public MessageType MessageType { get; set; } = MessageType.ERROR;
        public ReadOnlyCollection<RuleMessage> Messages => _messages.AsReadOnly();
        public bool HasMessages => Messages.Any();
        public bool Passed => Status == Status.PASSED;
        public bool Executed => Status != Status.SKIPPED;
        public Status Status { get; set; } = Status.SKIPPED;
        public abstract void Create();
        public override string ToString() => $"{Id}: {Name}";
        public virtual void AddMessage(RuleMessage message) => _messages.Add(message);

        public static readonly IEnumerable<string> TranslatableProperties = new[] { "Name", "Description", "PreCondition", "ChecklistReference", "Source", "Documentation" };
    }

    public abstract class ExecutableRule : Rule, IDisposable
    {
        private bool _disposed = false;
        public bool Disabled { get; protected set; }
        public CancellationTokenSource TokenSource { get; } = new();
        protected int MaxMessageCount { get; set; }

        public override void AddMessage(RuleMessage message)
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
            int maxMessageCount, 
            ILogger<Rule> logger)
        {
            _ruleService = ruleService;
            _settings = settings;
            _translations = translations;
            _logger = logger;
            MaxMessageCount = maxMessageCount;
            _loaded = true;
        }

        private bool Skipped { get; set; }
        public List<Dependency<T>> Dependencies { get; } = new();
        protected Dependency<T> DependOn<U>() where U : Rule<T> => new(typeof(U), this);
        protected abstract void Validate(T data);
        protected U GetData<U>(string key) where U : class => _ruleService.GetData<U>(key);
        protected void SetData(string key, object data) => _ruleService.SetData(key, data);

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

        public async Task Execute(T data)
        {
            if (!await CanExecute(data))
                return;

            var startTime = DateTime.Now;
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
                SetStatus();

                _logger.LogInformation("{@Rule}", new
                {
                    Id,
                    Name,
                    FullName = ToString(),
                    Status,
                    TimeUsed = DateTime.Now.Subtract(startTime).TotalSeconds,
                    MessageCount = Messages.Count
                });
            }
        }

        private async Task<bool> CanExecute(T data)
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
