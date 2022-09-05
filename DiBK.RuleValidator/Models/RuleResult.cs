namespace DiBK.RuleValidator
{
    public class RuleResult
    {
        public RuleResult(string id, string name, Status status, double timeUsed, int messageCount)
        {
            Id = id;
            Name = name;
            Status = status;
            TimeUsed = timeUsed;
            MessageCount = messageCount;
        }
        
        public string Id { get; private set; }
        public string Name { get; private set; }
        public Status Status { get; private set; }
        public double TimeUsed { get; private set; }
        public int MessageCount { get; private set; }
        public override string ToString() => $"{Id}: {Name}";
    }
}
