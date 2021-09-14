namespace DiBK.RuleValidator
{
    public class RuleInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Documentation { get; set; }
        public string MessageType { get; set; }

        public RuleInfo()
        {
        }

        public RuleInfo(string id, string name, string description, string messageType, string documentation)
        {
            Id = id;
            Name = name;
            Description = description;
            MessageType = messageType;
            Documentation = documentation;
        }
    }
}
