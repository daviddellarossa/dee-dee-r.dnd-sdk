using UnityEngine;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// Adventuring gear — equipment that is neither a weapon, armour, nor a specialist tool
    /// (e.g. rope, torch, grappling hook, backpack).
    /// Extends <see cref="ItemSO"/> with no additional fields; the subtype is used as a
    /// semantic tag so item lists and inspectors can filter by category.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAdventuringGear", menuName = "DnD SDK/Items/Adventuring Gear")]
    public sealed class AdventuringGearSO : ItemSO { }
}