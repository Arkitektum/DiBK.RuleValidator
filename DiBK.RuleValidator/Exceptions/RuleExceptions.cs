using System;

namespace DiBK.RuleValidator
{
    public class RuleException : Exception
    {
        public RuleException()
        {
        }

        public RuleException(string message) : base(message)
        {
        }

        public RuleException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
