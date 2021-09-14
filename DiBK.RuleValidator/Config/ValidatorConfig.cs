using DiBK.RuleValidator.Services;
using DiBK.RuleValidator.Models.Config;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Validator = DiBK.RuleValidator.Services.RuleValidator;

namespace DiBK.RuleValidator.Config
{
    public static class ValidatorConfig
    {
        public static void AddRuleValidator(this IServiceCollection services, IEnumerable<Assembly> ruleAssemblies)
        {
            if (!ruleAssemblies.Any())
                throw new Exception();

            services.AddTransient<IRuleService, RuleService>();
            services.AddTransient<IRuleValidator, Validator>();

            var configs = GetRuleConfigs(ruleAssemblies);

            if (!configs.Any())
                throw new Exception();

            services.AddSingleton<IRuleConfigs>(new RuleConfigs(configs));
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
