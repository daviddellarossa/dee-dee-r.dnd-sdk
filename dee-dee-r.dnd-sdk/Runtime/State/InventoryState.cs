using System;
using System.Collections.Generic;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;

namespace DeeDeeR.DnD.Runtime.State
{
    /// <summary>
    /// Tracks everything a character is carrying or has equipped.
    /// Modified by the inventory system (Phase 9) and read by <c>CombatSystem</c>
    /// to resolve armor class, attack bonuses, and weapon properties.
    /// </summary>
    [Serializable]
    public sealed class InventoryState
    {
        // ── Carried Items ─────────────────────────────────────────────────────

        /// <summary>All items in the character's possession, each with a quantity.</summary>
        public List<OwnedItem> Items = new List<OwnedItem>();

        /// <summary>Currency the character is carrying.</summary>
        public Currency Currency;

        // ── Equipped ──────────────────────────────────────────────────────────

        /// <summary>
        /// Weapon or implement held in the main hand. <c>null</c> if nothing is wielded.
        /// </summary>
        public WeaponSO EquippedMainHand;

        /// <summary>
        /// Weapon held in the off hand. <c>null</c> if nothing is held in the off hand, or if
        /// a shield is equipped instead (see <see cref="EquippedOffHandShield"/>).
        /// </summary>
        public WeaponSO EquippedOffHandWeapon;

        /// <summary>
        /// Shield equipped in the off hand. <c>null</c> if nothing is held, or if an off-hand
        /// weapon is equipped instead (see <see cref="EquippedOffHandWeapon"/>).
        /// </summary>
        public ShieldSO EquippedOffHandShield;

        /// <summary>
        /// Armour currently worn. <c>null</c> if the character is unarmoured.
        /// </summary>
        public ArmorSO EquippedArmor;
    }

    /// <summary>
    /// A specific item and how many the character owns. Held inside <see cref="InventoryState.Items"/>.
    /// </summary>
    [Serializable]
    public sealed class OwnedItem
    {
        /// <summary>The item asset. May be any <see cref="ItemSO"/> subtype.</summary>
        public ItemSO Item;

        /// <summary>How many of this item the character owns.</summary>
        [UnityEngine.Min(1)] public int Quantity = 1;
    }
}