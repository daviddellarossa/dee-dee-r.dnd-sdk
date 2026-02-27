using UnityEngine;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// A shield — a separate equipment slot distinct from body armour.
    /// Shield proficiency is tracked as <c>bool HasShieldProficiency</c> on the character record,
    /// not in the <see cref="DeeDeeR.DnD.Core.Enums.ArmorCategory"/> proficiency set.
    /// </summary>
    [CreateAssetMenu(fileName = "NewShield", menuName = "DnD SDK/Items/Shield")]
    public sealed class ShieldSO : ScriptableObject
    {
        /// <summary>Armor Class bonus granted while wielding this shield (default +2).</summary>
        [Min(0)] public int AcBonus = 2;

        /// <summary>
        /// Minimum Strength score required to wield this shield without a speed penalty.
        /// Set to 0 if there is no requirement.
        /// </summary>
        [Min(0)] public int StrengthRequirement;

        /// <summary>Weight in pounds.</summary>
        [Min(0f)] public float Weight;

        /// <summary>Market cost.</summary>
        public CurrencyData Cost = new CurrencyData();
    }
}