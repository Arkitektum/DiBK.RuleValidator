using System;

namespace DiBK.RuleValidator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TranslationAttribute : Attribute
    {
        private readonly string _resXName;

        public TranslationAttribute(string resXName)
        {
            _resXName = resXName;
        }

        public virtual string ResXName
        {
            get { return _resXName; }
        }
    }
}
