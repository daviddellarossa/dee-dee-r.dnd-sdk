using UnityEngine;
using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// A suit of armour (Light, Medium, or Heavy).
    /// Shields are a separate equipment slot — see <see cref="ShieldSO"/>.
    /// Shield proficiency is tracked as <c>bool HasShieldProficiency</c> on the character record,
    /// not in the armour proficiency set.
    /// </summary>
    [CreateAssetMenu(fileName = "NewArmor", menuName = "DnD SDK/Items/Armor")]
    public sealed class ArmorSO : ScriptableObject
    {
        /// <summary>The armour category (Light, Medium, or Heavy).</summary>
        public ArmorCategory Category;

        /// <summary>
        /// The base Armor Class provided by this armour before adding the Dexterity modifier.
        /// </summary>
        [Min(0)] public int BaseArmorClass;

        /// <summary>
        /// The maximum Dexterity modifier that may be added to the AC.
        /// Set to <c>-1</c> to indicate no cap (Light armour adds full Dex modifier).
        /// Heavy armour uses 0 (no Dex added).
        /// </summary>
        public int MaxDexBonus = -1;

        /// <summary>
        /// Minimum Strength score required to wear this armour without a speed penalty.
        /// Set to 0 if there is no requirement.
        /// </summary>
        [Min(0)] public int StrengthRequirement;

        /// <summary>Whether wearing this armour imposes disadvantage on Stealth checks.</summary>
        public bool StealthDisadvantage;

        /// <summary>Weight in pounds.</summary>
        [Min(0f)] public float Weight;

        /// <summary>Market cost.</summary>
        public CurrencyData Cost = new CurrencyData();
    }
}