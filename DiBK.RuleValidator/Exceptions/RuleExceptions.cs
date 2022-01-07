using System;

namespace DiBK.RuleValidator
{
    public class RuleNotFoundException : Exception
    {
        public RuleNotFoundException()
        {
        }

        public RuleNotFoundException(string message) : base(message)
        {
        }

        public RuleNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class RuleAlreadyLoadedException : Exception
    {
        public RuleAlreadyLoadedException()
        {
        }

        public RuleAlreadyLoadedException(string message) : base(message)
        {
        }

        public RuleAlreadyLoadedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class RuleNotLoadedException : Exception
    {
        public RuleNotLoadedException()
        {
        }

        public RuleNotLoadedException(string message) : base(message)
        {
        }

        public RuleNotLoadedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class RuleConfigNotFoundException : Exception
    {
        public RuleConfigNotFoundException()
        {
        }

        public RuleConfigNotFoundException(string message) : base(message)
        {
        }

        public RuleConfigNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class InvalidTypeException : Exception
    {
        public InvalidTypeException()
        {
        }

        public InvalidTypeException(string message) : base(message)
        {
        }

        public InvalidTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
