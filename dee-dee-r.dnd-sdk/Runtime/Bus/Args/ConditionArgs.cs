using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Runtime.Bus.Args
{
    /// <summary>Published when a condition is applied to or removed from a character.</summary>
    public readonly struct ConditionChangedArgs
    {
        public readonly EndpointId Character;
        public readonly Condition  Condition;

        public ConditionChangedArgs(EndpointId character, Condition condition)
        {
            Character = character;
            Condition = condition;
        }
    }

    /// <summary>Published when a character's exhaustion level changes.</summary>
    public readonly struct ExhaustionArgs
    {
        public readonly EndpointId Character;
        public readonly int        Previous;
        public readonly int        Current;

        public ExhaustionArgs(EndpointId character, int previous, int current)
        {
            Character = character;
            Previous  = previous;
            Current   = current;
        }
    }
}