using System;

namespace DiBK.RuleValidator
{
    public class Dependency
    {
        private readonly Rule _rule;
        private readonly Type _type;

        public Dependency(Type type, Rule rule)
        {
            _type = type;
            _rule = rule;
        }

        public void ToPass()
        {
            _rule.Parent = _type;
            _rule.ParentOutcome = Status.PASSED;
        }

        public void ToFail()
        {
            _rule.Parent = _type;
            _rule.ParentOutcome = Status.FAILED;
        }

        public void ToWarn()
        {
            _rule.Parent = _type;
            _rule.ParentOutcome = Status.WARNING;
        }

        public void ToRun()
        {
            _rule.Parent = _type;
            _rule.ParentOutcome = Status.UNDEFINED;
        }
    }
}
