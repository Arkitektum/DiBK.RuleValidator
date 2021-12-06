namespace DiBK.RuleValidator
{
    public enum MessageType
    {
        ERROR = 1,
        WARNING = 2,
        ERROR_AND_WARNING = 3,
        INFORMATION = 4
    }

    public enum Status
    {
        PASSED = 1,
        FAILED = 2,
        WARNING = 3,
        INFO = 4,
        SKIPPED = 5,
        SYSTEM_ERROR = 6,
        UNDEFINED = -1
    }
}
