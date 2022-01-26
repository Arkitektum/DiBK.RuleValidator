using DiBK.RuleValidator.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DiBK.RuleValidator
{
    public class RuleValidator : IRuleValidator
    {
        private readonly IRuleService _ruleService;
        private readonly ITranslationService _translationService;
        private readonly IRuleSettings _ruleSettings;
        private readonly ILogger<RuleValidator> _logger;
        private readonly ILogger<Rule> _ruleLogger;
        private readonly List<RuleConfig> _activeRuleConfigs = new();
        private readonly Dictionary<Type, Dictionary<string, object>> _activeRuleSettings = new();

        public RuleValidator(
            IRuleService ruleService,
            ITranslationService translationService,
            IRuleSettings ruleSettings,
            ILogger<RuleValidator> logger,
            ILogger<Rule> ruleLogger)
        {
            _ruleService = ruleService;
            _translationService = translationService;
            _ruleSettings = ruleSettings;
            _logger = logger;
            _ruleLogger = ruleLogger;
        }

        public async Task Validate<T>(T validationData, Action<ValidationOptions> settings = null) where T : class
        {
            var config = _ruleSettings.RuleConfigs.Get(typeof(T));
            var rules = GetRules<T>(config, settings);

            _ruleService.AddRules(rules);

            await ExecuteRules(rules, validationData);

            foreach (var rule in rules)
                rule.Dispose();
        }

        public void LoadRules<T>(Action<ValidationOptions> settings = null) where T : class
        {
            var config = _ruleSettings.RuleConfigs.Get(typeof(T));
            var rules = GetRules<T>(config, settings);

            _ruleService.AddRules(rules);
        }

        public List<RuleSetGroup> GetRuleInfo(IEnumerable<Type> ruleTypes, Action<ValidationOptions> options = null)
        {
            var validationOptions = new ValidationOptions();
            options?.Invoke(validationOptions);

            var ruleSetGroups = ruleTypes
                .Select(type =>
                {
                    var config = _ruleSettings.RuleConfigs.Get(type);
                    var ruleSet = new RuleSet { Name = config.Name, Description = config.Description };

                    ruleSet.Groups = config.Groups
                        .Where(groupOptions => !validationOptions.SkippedGroups.Contains(groupOptions.GroupId))
                        .Select(groupOptions =>
                        {
                            var group = new RuleSetGroup { Name = groupOptions.Name };

                            group.Rules = groupOptions.Rules
                                .Where(ruleOptions => !validationOptions.SkippedRules.Contains(ruleOptions.Type))
                                .Select(ruleOptions =>
                                {
                                    var rule = Activator.CreateInstance(ruleOptions.Type) as Rule;
                                    var translations = _translationService.GetTranslationsForRule(rule);

                                    rule.Create();
                                    TranslateRuleProperties(rule, translations);

                                    return new RuleInfo(rule.Id, rule.Name, rule.Description, rule.MessageType.ToString(), rule.Documentation);
                                })
                                .ToList();

                            return group;
                        })
                        .ToList();

                    return ruleSet;
                })
                .SelectMany(ruleSet => ruleSet.Groups);

            return ruleSetGroups
                .ToLookup(group => group.Name)
                .Select(grouping =>
                {
                    return new RuleSetGroup
                    {
                        Name = grouping.Key,
                        Rules = grouping
                            .SelectMany(group => group.Rules)
                            .ToList()
                    };
                })
                .ToList();
        }

        public List<Rule> GetRulesBySettings(List<Rule> rules, Action<dynamic> settingsFilter)
        {
            dynamic filter = new ExpandoObject();
            settingsFilter.Invoke(filter);

            var settings = new Dictionary<string, object>(filter);
            var ruleTypes = new List<Type>();

            foreach (var (type, ruleSettings) in _activeRuleSettings)
            {
                var isMatch = settings
                    .All(setting => ruleSettings.ContainsKey(setting.Key) && ruleSettings[setting.Key].Equals(setting.Value));

                if (isMatch)
                    ruleTypes.Add(type);
            }

            return rules
                .Where(rule => ruleTypes.Any(type => type == rule.GetType()))
                .ToList();
        }

        public Rule<T> GetRule<U, T>() where T : class where U : Rule<T>
        {
            return _ruleService.Get<T, U>();
        }

        public List<Rule> GetAllRules()
        {
            var rules = _ruleService.GetAll();

            return OrderRules(rules);
        }

        public List<Rule> GetExecutedRules()
        {
            var rules = _ruleService.GetAll()
                .Where(rule => rule.Status != Status.SKIPPED)
                .ToList();

            return OrderRules(rules);
        }

        private List<Rule> OrderRules(List<Rule> rules)
        {
            var ruleSettings = _activeRuleConfigs
                .SelectMany(config => config.Groups
                    .SelectMany(settings => settings.Rules));

            var orderedRules = new List<Rule>();

            foreach (var setting in ruleSettings)
            {
                var rule = rules.SingleOrDefault(rule => rule.GetType() == setting.Type);

                if (rule != null)
                    orderedRules.Add(rule);
            }

            return orderedRules;
        }

        private List<Rule<T>> GetRules<T>(RuleConfig ruleConfig, Action<ValidationOptions> settings = null) where T : class
        {
            var validationSettings = new ValidationOptions();
            settings?.Invoke(validationSettings);

            SetActiveConfig(ruleConfig, validationSettings);

            var rules = ruleConfig.Groups
                .Where(group => !validationSettings.SkippedGroups.Contains(group.GroupId))
                .SelectMany(group => group.Rules)
                .Where(ruleConfig => !validationSettings.SkippedRules.Contains(ruleConfig.Type))
                .Select(ruleSettings => CreateRule<T>(ruleSettings.Type, validationSettings.GlobalSettings.Merge(ruleConfig.GlobalSettings)))
                .Where(rule => !rule.Disabled)
                .ToList();

            var duplicates = rules
                .GroupBy(rule => rule.Id)
                .Where(grouping => grouping.Count() > 1)
                .Select(grouping => grouping.Key);

            if (duplicates.Any())
                throw new RuleException($"Rule ID duplicates detected: '{string.Join(", ", duplicates)}'. Rule IDs must be unique.");

            return rules;
        }

        private Rule<T> CreateRule<T>(Type ruleType, IReadOnlyDictionary<string, object> ruleSettings) where T : class
        {
            var rule = Activator.CreateInstance(ruleType) as Rule<T>;           
            rule.Create();
            
            if (rule.Id == null)
                throw new RuleException($"Rule with type '{ruleType.Name}' has no ID.");

            var translations = _translationService.GetTranslationsForRule(rule);
            TranslateRuleProperties(rule, translations);

            rule.Setup(_ruleService, ruleSettings, translations, _ruleSettings.MaxMessageCount, _ruleLogger);

            return rule;
        }

        private async Task ExecuteRules<T>(List<Rule<T>> rules, T validationData) where T : class
        {
            var sequentials = new List<Rule<T>>();
            var parallels = new List<Rule<T>>();

            foreach (var rule in rules)
            {
                if (rules.Any(r => r.Dependencies.Any(dependency => dependency.Type == rule.GetType())))
                    sequentials.Add(rule);
                else
                    parallels.Add(rule);
            }

            foreach (var rule in sequentials)
                await ExecuteRule(rule, validationData);

            await parallels.AsyncParallelForEach(async rule => await ExecuteRule(rule, validationData));
        }

        private async Task ExecuteRule<T>(Rule<T> rule, T validationData) where T : class
        {
            try
            {
                await rule.Execute(validationData);
            }
            catch (Exception exception)
            {
                rule.Status = Status.SYSTEM_ERROR;
                _logger.LogError(exception, $"Could not execute rule '{rule}'");
            }
        }

        private void SetActiveConfig(RuleConfig ruleConfig, ValidationOptions validationSettings)
        {
            _activeRuleConfigs.Add(ruleConfig);

            var skippedRules = GetSkippedRules(ruleConfig, validationSettings);
            var ruleSettingsDictionary = new Dictionary<Type, Dictionary<string, object>>();

            foreach (var groupSettings in ruleConfig.Groups)
            {
                foreach (var ruleSettings in groupSettings.Rules.Where(settings => !skippedRules.Contains(settings.Type)))
                {
                    ruleSettingsDictionary.Add(ruleSettings.Type, ruleSettings.Settings.Merge(groupSettings.Settings));
                }
            }

            if (!ruleSettingsDictionary.Any())
                return;

            foreach (var groupSettings in validationSettings.GroupOptions)
            {
                var groupTypes = ruleConfig.Groups
                    .SingleOrDefault(settings => settings.GroupId == groupSettings.GroupId);

                if (groupTypes == null)
                    continue;

                var ruleTypes = groupTypes.Rules
                    .Select(settings => settings.Type);

                foreach (var (type, settings) in ruleSettingsDictionary)
                {
                    if (ruleTypes.Contains(type))
                        settings.Append(groupSettings.Settings);
                }
            }

            foreach (var ruleSettings in validationSettings.RuleOptions)
            {
                foreach (var (type, settings) in ruleSettingsDictionary)
                {
                    if (type == ruleSettings.Type)
                        settings.Append(ruleSettings.Settings);
                }
            }

            _activeRuleSettings.Append(ruleSettingsDictionary);
        }

        private static void TranslateRuleProperties(Rule rule, IReadOnlyDictionary<string, string> translations)
        {
            var properties = rule.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var kvp in translations)
            {
                if (!Rule.TranslatableProperties.Contains(kvp.Key))
                    continue;

                var propertyInfo = properties.SingleOrDefault(property => property.Name == kvp.Key);

                if (propertyInfo != null)
                    propertyInfo.SetValue(rule, kvp.Value, null);
            }
        }

        private static List<Type> GetSkippedRules(RuleConfig ruleConfig, ValidationOptions validationSettings)
        {
            var skippedRules = new List<Type>();

            foreach (var groupId in validationSettings.SkippedGroups)
            {
                var groupSettings = ruleConfig.Groups.SingleOrDefault(settings => settings.GroupId == groupId);

                if (groupSettings != null)
                    skippedRules.AddRange(groupSettings.Rules.Select(ruleSettings => ruleSettings.Type));
            }

            foreach (var type in validationSettings.SkippedRules)
            {
                if (!skippedRules.Contains(type))
                    skippedRules.Add(type);
            }

            return skippedRules;
        }
    }
}
