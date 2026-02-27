using UnityEngine;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// A weapon (Simple or Martial, melee or ranged).
    /// Whether a weapon is melee or ranged is derived from its <see cref="Properties"/>:
    /// the Range property indicates a ranged weapon; Thrown indicates a throwable melee weapon;
    /// neither means purely melee.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "DnD SDK/Items/Weapon")]
    public sealed class WeaponSO : ScriptableObject
    {
        /// <summary>Whether this weapon is Simple or Martial.</summary>
        public WeaponCategory Category;

        // ── Damage ────────────────────────────────────────────────────────────

        /// <summary>
        /// Damage dice for this weapon (e.g. 1d8 for a longsword).
        /// Unity cannot serialize <see cref="DiceExpression"/> directly; use
        /// <see cref="DamageExpression"/> to obtain the immutable value.
        /// </summary>
        public DiceExpressionData DamageDice = new DiceExpressionData();

        /// <summary>The damage type dealt by this weapon.</summary>
        public DamageType DamageType;

        /// <summary>Returns the damage dice as an immutable <see cref="DiceExpression"/>.</summary>
        public DiceExpression DamageExpression => DamageDice.ToValue();

        // ── Properties & Mastery ──────────────────────────────────────────────

        /// <summary>Intrinsic weapon properties (Finesse, Heavy, Light, Thrown, etc.).</summary>
        public WeaponProperty[] Properties;

        /// <summary>The weapon mastery property available to characters with mastery in this weapon.</summary>
        public WeaponMastery MasteryProperty;

        // ── Range ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Normal attack range in feet. For melee weapons without Reach this is typically 5.
        /// For ranged/thrown weapons this is the short range (no disadvantage).
        /// </summary>
        [Min(0)] public int RangeNormal = 5;

        /// <summary>
        /// Long attack range in feet (attacks beyond normal range have disadvantage).
        /// Irrelevant for non-ranged melee weapons — leave at 0.
        /// </summary>
        [Min(0)] public int RangeLong;

        // ── Ammunition ────────────────────────────────────────────────────────

        /// <summary>
        /// The ammunition type required by this weapon. <c>null</c> if the weapon does not
        /// have the Ammunition property.
        /// </summary>
        public AmmunitionSO RequiredAmmo;

        // ── Versatile ─────────────────────────────────────────────────────────

        /// <summary>
        /// Damage dice when wielded two-handed (only for weapons with the Versatile property).
        /// Ignored for all other weapons.
        /// </summary>
        public DiceExpressionData VersatileDamageDice = new DiceExpressionData();

        /// <summary>Returns the versatile damage dice as an immutable <see cref="DiceExpression"/>.</summary>
        public DiceExpression VersatileDamageExpression => VersatileDamageDice.ToValue();

        // ── Physical ──────────────────────────────────────────────────────────

        /// <summary>Weight in pounds.</summary>
        [Min(0f)] public float Weight;

        /// <summary>Market cost.</summary>
        public CurrencyData Cost = new CurrencyData();
    }
}