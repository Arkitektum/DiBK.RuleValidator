namespace DiBK.RuleValidator.Models
{
    public enum MessageType
    {
        ERROR = 1,
        WARNING = 2,
        INFORMATION = 3
    }

    public enum Status
    {
        PASSED = 1,
        FAILED = 2,
        WARNING = 3,
        INFO = 4,
        NOT_EXECUTED = 5,
        SYSTEM_ERROR = 6,
        UNDEFINED = -1
    }
}
