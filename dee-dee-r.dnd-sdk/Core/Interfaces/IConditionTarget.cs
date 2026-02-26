using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Core.Interfaces
{
    /// <summary>
    /// Implemented by any entity that can have conditions applied or removed.
    /// </summary>
    public interface IConditionTarget
    {
        void ApplyCondition(Condition condition);
        void RemoveCondition(Condition condition);
        bool HasCondition(Condition condition);
    }
}
