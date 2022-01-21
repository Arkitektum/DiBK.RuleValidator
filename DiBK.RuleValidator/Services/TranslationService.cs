using DiBK.RuleValidator.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;

namespace DiBK.RuleValidator
{
    public class TranslationService : ITranslationService
    {
        private static readonly Regex _resourceRegex = new(@"_(?<culture>((\w+-?)+))\.resources$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly IRuleSettings _ruleSettings;

        public TranslationService(
            IRuleSettings ruleSettings)
        {
            _ruleSettings = ruleSettings;
        }

        public IReadOnlyDictionary<string, string> GetTranslationsForRule(Rule rule)
        {
            var resourceAttribute = rule.GetType().GetCustomAttributes(typeof(TranslationAttribute), true).FirstOrDefault() as TranslationAttribute;
            var resourceName = resourceAttribute?.ResourceName;

            if (resourceName == null)
                return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

            var customTranslationTexts = GetTranslation(_ruleSettings.CustomTranslations, resourceName)?.Texts ?? new();
            var translationTexts = GetTranslation(_ruleSettings.Translations, resourceName)?.Texts ?? new();
            
            return new ReadOnlyDictionary<string, string>(customTranslationTexts.Merge(translationTexts));
        }

        public static List<Translation> CreateTranslations(List<Assembly> assemblies)
        {
            return assemblies
                .SelectMany(assembly =>
                {
                    return assembly.GetManifestResourceNames()
                        .Where(name => name.EndsWith(".resources"))
                        .Select(name => CreateTranslation(assembly, name));
                })
                .ToList();
        }

        public static List<Translation> CreateTranslations(List<TranslationAssembly> translationAssemblies)
        {
            return translationAssemblies
                .SelectMany(translationAssembly =>
                {
                    return translationAssembly.Assembly.GetManifestResourceNames()
                        .Where(name => name.StartsWith(translationAssembly.RootNamespace ?? "") && name.EndsWith(".resources"))
                        .Select(name => CreateTranslation(translationAssembly.Assembly, name));
                })
                .ToList();
        }

        private static Translation GetTranslation(List<Translation> translations, string resourceName)
        {
            var resourceNameRegex = new Regex($@"{resourceName}(_((\w+-?)+))?\.resources$");
            var culture = CultureInfo.CurrentCulture;

            var matchedTranslations = translations
                .Where(translation => resourceNameRegex.IsMatch(translation.ResourceName));

            var translation = matchedTranslations
                .SingleOrDefault(translation => translation.Culture?.Name == culture.Name);

            if (translation != null)
                return translation;

            return matchedTranslations
                .SingleOrDefault(translation => translation.Culture == null);
        }

        private static Translation CreateTranslation(Assembly assembly, string resourceName)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new ResourceReader(stream);
            var enumerator = reader.GetEnumerator();
            var textDictionary = new Dictionary<string, string>();

            while (enumerator.MoveNext())
                textDictionary.Add((string)enumerator.Key, (string)enumerator.Value);

            return new Translation
            {
                ResourceName = resourceName,
                Texts = textDictionary,
                Culture = GetCultureInfoFromResourceName(resourceName)
            };
        }

        private static CultureInfo GetCultureInfoFromResourceName(string resourceName)
        {
            var match = _resourceRegex.Match(resourceName);

            if (!match.Success)
                return null;

            try
            {
                return new CultureInfo(match.Groups["culture"].Value);
            }
            catch (CultureNotFoundException)
            {
                throw;
            }
        }
    }
}
