namespace DiBK.RuleValidator.Models
{
    public class RuleValidationResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public Status Status { get; set; }
        public double TimeUsed { get; set; }
        public int MessageCount { get; set; }
    }
}
