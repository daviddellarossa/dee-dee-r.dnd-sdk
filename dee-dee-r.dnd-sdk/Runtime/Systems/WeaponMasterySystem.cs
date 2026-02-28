using System;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.State;

namespace DeeDeeR.DnD.Runtime.Systems
{
    /// <summary>
    /// Queries weapon mastery availability and maps mastery properties to their mechanical
    /// effects per D&amp;D 2024 PHB rules.
    /// Stateless — create one instance and reuse it.
    /// </summary>
    public sealed class WeaponMasterySystem
    {
        // ── Availability ──────────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> if the character can apply the mastery property of
        /// <paramref name="weapon"/> this turn — i.e. the weapon has a non-<c>None</c> mastery
        /// property and that property is present in <see cref="CharacterState.WeaponMasteries"/>.
        /// </summary>
        /// <param name="state">The character's mutable session state.</param>
        /// <param name="weapon">
        /// The weapon being used. Returns <c>false</c> when <c>null</c> or when
        /// <see cref="WeaponSO.MasteryProperty"/> is <see cref="WeaponMastery.None"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
        public bool CanUseMastery(CharacterState state, WeaponSO weapon)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return weapon != null
                && weapon.MasteryProperty != WeaponMastery.None
                && state.WeaponMasteries.Contains(weapon.MasteryProperty);
        }

        // ── Effects ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the mechanical effects of the given mastery property as a
        /// <see cref="MasteryEffect"/> descriptor. Callers interpret the descriptor and apply
        /// consequences through existing systems (e.g. <see cref="ConditionSystem.Apply"/> for
        /// Topple's Prone result, <see cref="HitPointSystem.ApplyDamage"/> for Graze damage).
        /// </summary>
        /// <param name="mastery">The mastery property to describe.</param>
        /// <returns>
        /// A <see cref="MasteryEffect"/> describing what the mastery does on a hit (or miss for Graze).
        /// </returns>
        public MasteryEffect GetMasteryEffect(WeaponMastery mastery) => mastery switch
        {
            WeaponMastery.Cleave => new MasteryEffect(grantsFreeAttack:        true),
            WeaponMastery.Graze  => new MasteryEffect(dealsDamageOnMiss:       true),
            WeaponMastery.Nick   => new MasteryEffect(offHandAttackIsFree:     true),
            WeaponMastery.Push   => new MasteryEffect(pushesTarget:            true, pushDistance:   10),
            WeaponMastery.Sap    => new MasteryEffect(sapsTarget:              true),
            WeaponMastery.Slow   => new MasteryEffect(slowsTarget:             true, speedReduction: 10),
            WeaponMastery.Topple => new MasteryEffect(topplesToTarget:         true),
            WeaponMastery.Vex    => new MasteryEffect(grantsAttackerAdvantage: true),
            _                    => MasteryEffect.None,
        };
    }
}