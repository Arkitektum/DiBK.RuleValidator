using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DiBK.RuleValidator.Config
{
    public static class RuleValidatorConfig
    {
        public static void AddRuleValidator(this IServiceCollection services, Action<RuleValidatorSettings> settings)
        {
            var ruleValidatorSettings = new RuleValidatorSettings();
            settings.Invoke(ruleValidatorSettings);

            if (!ruleValidatorSettings.RuleAssemblies?.Any() ?? true)
                throw new Exception();

            services.AddTransient<IRuleService, RuleService>();
            services.AddTransient<IRuleValidator, RuleValidator>();

            var configs = GetRuleConfigs(ruleValidatorSettings.RuleAssemblies);

            if (!configs.Any())
                throw new Exception();

            var ruleSettings = new RuleSettings(new RuleConfigs(configs), ruleValidatorSettings.MaxMessageCount);

            services.AddSingleton<IRuleSettings>(ruleSettings);
        }

        private static Dictionary<Type, RuleConfig> GetRuleConfigs(IEnumerable<Assembly> ruleAssemblies)
        {
            return ruleAssemblies
                .SelectMany(assembly =>
                {
                    return assembly.GetTypes()
                        .Where(type => typeof(IRuleSetup).IsAssignableFrom(type) &&
                            type.GetConstructor(Type.EmptyTypes) != null)
                        .Select(type =>
                        {
                            var setup = Activator.CreateInstance(type) as IRuleSetup;
                            var config = setup.CreateConfig();

                            return new KeyValuePair<Type, RuleConfig>(config.Type, config);
                        });
                })
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
