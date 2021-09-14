namespace DiBK.RuleValidator.Models
{
    public interface IRuleMessage
    {
        public string Message { get; set; }
    }

    public class RuleMessage : IRuleMessage
    {
        public string Message { get; set; }
        public string FileName { get; set; }
    }
}
