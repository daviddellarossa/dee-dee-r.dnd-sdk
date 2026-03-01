using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Runtime.Bus.Args
{
    /// <summary>Which weapon slot to query via <see cref="DeeDeeR.DnD.Runtime.Bus.InventoryBusCategory.GetEquippedWeapon"/>.</summary>
    public enum EquipHand { Main, OffHand }

    /// <summary>
    /// Published when an item is equipped, unequipped, added, or removed from a character's inventory.
    /// </summary>
    public readonly struct ItemArgs
    {
        public readonly EndpointId Character;
        public readonly ItemSO     Item;
        /// <summary>Number of items added or removed. Defaults to 1 for equip/unequip events.</summary>
        public readonly int        Quantity;

        public ItemArgs(EndpointId character, ItemSO item, int quantity = 1)
        {
            Character = character;
            Item      = item;
            Quantity  = quantity;
        }
    }

    /// <summary>Query args carrying the hand slot whose equipped weapon is requested.</summary>
    public readonly struct GetEquippedWeaponArgs
    {
        public readonly EquipHand Hand;

        public GetEquippedWeaponArgs(EquipHand hand) => Hand = hand;
    }
}