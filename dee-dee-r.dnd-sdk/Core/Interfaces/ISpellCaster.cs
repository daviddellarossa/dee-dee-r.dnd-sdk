using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Core.Interfaces
{
    /// <summary>
    /// Implemented by any entity capable of expending and recovering spell slots.
    /// </summary>
    public interface ISpellCaster
    {
        /// <summary>
        /// Attempts to expend one spell slot of the given level.
        /// Returns true if a slot was available and successfully expended.
        /// </summary>
        bool TryExpendSlot(int level);

        /// <summary>
        /// Recovers spell slots according to the given rest type
        /// (e.g. Long Rest recovers all slots; some class features recover on Short Rest).
        /// </summary>
        void RecoverSlots(RestType restType);
    }
}
