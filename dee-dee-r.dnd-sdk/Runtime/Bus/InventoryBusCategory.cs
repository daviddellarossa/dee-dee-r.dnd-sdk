using DeeDeeR.DnD.Runtime.Bus.Args;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Runtime.Bus
{
    /// <summary>
    /// Bus channels for inventory events and queries (equipping, item management).
    /// </summary>
    public sealed class InventoryBusCategory
    {
        // ── Signals ───────────────────────────────────────────────────────────

        /// <summary>Published when a character equips an item.</summary>
        public readonly Signal<ItemArgs> ItemEquipped;

        /// <summary>Published when a character unequips an item.</summary>
        public readonly Signal<ItemArgs> ItemUnequipped;

        /// <summary>Published when items are added to a character's inventory.</summary>
        public readonly Signal<ItemArgs> ItemAdded;

        /// <summary>Published when items are removed from a character's inventory.</summary>
        public readonly Signal<ItemArgs> ItemRemoved;

        // ── Queries ───────────────────────────────────────────────────────────

        /// <summary>Returns the weapon equipped in the specified hand slot, or <c>null</c> if empty.</summary>
        public readonly Query<GetEquippedWeaponArgs, WeaponSO> GetEquippedWeapon;

        /// <summary>Returns the armor currently worn by the addressed character, or <c>null</c> if unarmored.</summary>
        public readonly Query<EmptyArgs, ArmorSO>              GetEquippedArmor;

        // ── Constructor ───────────────────────────────────────────────────────

        public InventoryBusCategory(IFrameScheduler scheduler)
        {
            ItemEquipped      = new Signal<ItemArgs>(scheduler);
            ItemUnequipped    = new Signal<ItemArgs>(scheduler);
            ItemAdded         = new Signal<ItemArgs>(scheduler);
            ItemRemoved       = new Signal<ItemArgs>(scheduler);
            GetEquippedWeapon = new Query<GetEquippedWeaponArgs, WeaponSO>(scheduler);
            GetEquippedArmor  = new Query<EmptyArgs, ArmorSO>(scheduler);
        }
    }
}