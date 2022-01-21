using System;

namespace DiBK.RuleValidator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TranslationAttribute : Attribute
    {
        private readonly string _resourceName;

        public TranslationAttribute(string resourceName)
        {
            _resourceName = resourceName;
        }

        public virtual string ResourceName
        {
            get { return _resourceName; }
        }
    }
}
