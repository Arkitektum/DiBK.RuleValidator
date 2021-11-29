using System;

namespace DiBK.RuleValidator
{
    public class Dependency<T> where T : class
    {
        private readonly Rule<T> _rule;
        public Type Type { get; private set; }
        public bool ShouldPass { get; private set; }
        public bool ShouldFail { get; private set; }
        public bool ShouldWarn { get; private set; }
        public bool ShouldExecute { get; private set; }

        public Dependency(Type type, Rule<T> rule)
        {
            Type = type;
            _rule = rule;
        }

        public void ToPass()
        {
            ShouldPass = true;
            _rule.Dependencies.Add(this);
        }

        public void ToFail()
        {
            ShouldFail = true;
            _rule.Dependencies.Add(this);
        }

        public void ToWarn()
        {
            ShouldWarn = true;
            _rule.Dependencies.Add(this);
        }

        public void ToExecute()
        {
            ShouldExecute = true;
            _rule.Dependencies.Add(this);
        }
    }
}
