using System;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.State;

namespace DeeDeeR.DnD.Runtime.Systems
{
    /// <summary>
    /// Applies and removes conditions, queries exhaustion penalties, and maps conditions to
    /// their mechanical effects per D&amp;D 2024 PHB rules.
    /// </summary>
    public sealed class ConditionSystem
    {
        // ── Apply / Remove ────────────────────────────────────────────────────

        /// <summary>
        /// Adds <paramref name="condition"/> to the character's active conditions.
        /// Idempotent — adding a condition the character already has has no effect.
        /// </summary>
        /// <param name="state">The character's mutable state. Modified in place.</param>
        /// <param name="condition">The condition to apply.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
        public void Apply(CharacterState state, Condition condition)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            state.Conditions.Add(condition);
        }

        /// <summary>
        /// Removes <paramref name="condition"/> from the character's active conditions.
        /// Safe to call when the condition is not present.
        /// </summary>
        /// <param name="state">The character's mutable state. Modified in place.</param>
        /// <param name="condition">The condition to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
        public void Remove(CharacterState state, Condition condition)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            state.Conditions.Remove(condition);
        }

        // ── Exhaustion penalty ────────────────────────────────────────────────

        /// <summary>
        /// Returns the exhaustion penalty for all d20 tests and spell save DCs as a positive
        /// magnitude (exhaustion level × 2 per D&amp;D 2024 PHB). Callers subtract this value
        /// from d20 rolls. Returns 0 when the character is not exhausted.
        /// Delegates directly to <see cref="DeeDeeR.DnD.Core.Values.ExhaustionLevel.D20Penalty"/>.
        /// </summary>
        /// <param name="state">The character's current state.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
        public int GetD20Penalty(CharacterState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return state.Exhaustion.D20Penalty;
        }

        // ── Condition effects ─────────────────────────────────────────────────

        /// <summary>
        /// Returns the mechanical effects imposed by <paramref name="condition"/> as a
        /// <see cref="ConditionEffects"/> descriptor. Other systems use this to apply condition
        /// consequences without hard-coding condition logic internally.
        /// </summary>
        /// <remarks>
        /// See <see cref="ConditionEffects"/> for documented simplifications (Prone melee/ranged
        /// distinction, Frightened visibility dependency, Petrified resistance rules).
        /// </remarks>
        /// <param name="condition">The condition to describe.</param>
        /// <returns>A <see cref="ConditionEffects"/> describing the condition's mechanical flags.</returns>
        public ConditionEffects GetConditionEffects(Condition condition) => condition switch
        {
            Condition.Blinded => new ConditionEffects(
                attackRollsHaveDisadvantage:       true,
                attackRollsAgainstHaveAdvantage:   true),

            Condition.Charmed => ConditionEffects.None,  // Social/roleplay effects only.

            Condition.Deafened => ConditionEffects.None, // Hearing effects only.

            Condition.Frightened => new ConditionEffects(
                attackRollsHaveDisadvantage: true),      // Simplified: visibility check deferred to caller.

            Condition.Grappled => new ConditionEffects(
                speedReducedToZero: true),

            Condition.Incapacitated => new ConditionEffects(
                incapacitated: true),

            Condition.Invisible => new ConditionEffects(
                attackRollsHaveAdvantage:              true,
                attackRollsAgainstHaveDisadvantage:    true),

            Condition.Paralyzed => new ConditionEffects(
                speedReducedToZero:                  true,
                incapacitated:                       true,
                cannotMove:                          true,
                cannotSpeak:                         true,
                attackRollsAgainstHaveAdvantage:     true,
                autoFailStrengthSaves:               true,
                autoFailDexteritySaves:              true,
                meleeHitsAreCritical:                true),

            Condition.Petrified => new ConditionEffects(
                speedReducedToZero:                  true,
                incapacitated:                       true,
                cannotMove:                          true,
                cannotSpeak:                         true,
                attackRollsAgainstHaveAdvantage:     true,
                autoFailStrengthSaves:               true,
                autoFailDexteritySaves:              true),

            Condition.Poisoned => new ConditionEffects(
                attackRollsHaveDisadvantage: true),

            Condition.Prone => new ConditionEffects(
                attackRollsHaveDisadvantage:         true,
                attackRollsAgainstHaveAdvantage:     true), // Simplified: melee only; ranged has disadvantage.

            Condition.Restrained => new ConditionEffects(
                speedReducedToZero:                  true,
                attackRollsHaveDisadvantage:         true,
                attackRollsAgainstHaveAdvantage:     true),

            Condition.Stunned => new ConditionEffects(
                speedReducedToZero:                  true,
                incapacitated:                       true,
                cannotMove:                          true,
                cannotSpeak:                         true,
                attackRollsAgainstHaveAdvantage:     true,
                autoFailStrengthSaves:               true,
                autoFailDexteritySaves:              true),

            Condition.Unconscious => new ConditionEffects(
                speedReducedToZero:                  true,
                incapacitated:                       true,
                cannotMove:                          true,
                cannotSpeak:                         true,
                attackRollsAgainstHaveAdvantage:     true,
                autoFailStrengthSaves:               true,
                autoFailDexteritySaves:              true,
                meleeHitsAreCritical:                true),

            _ => ConditionEffects.None,
        };
    }
}