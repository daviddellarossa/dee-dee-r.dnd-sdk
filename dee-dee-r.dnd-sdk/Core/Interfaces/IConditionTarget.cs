using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Core.Interfaces
{
    /// <summary>
    /// Implemented by any entity that can have conditions applied or removed.
    /// </summary>
    public interface IConditionTarget
    {
        /// <summary>Applies the given <paramref name="condition"/> to this entity. Has no effect if already present.</summary>
        void ApplyCondition(Condition condition);

        /// <summary>Removes the given <paramref name="condition"/> from this entity. Has no effect if not present.</summary>
        void RemoveCondition(Condition condition);

        /// <summary>Returns true if this entity currently has the given <paramref name="condition"/>.</summary>
        bool HasCondition(Condition condition);
    }
}
