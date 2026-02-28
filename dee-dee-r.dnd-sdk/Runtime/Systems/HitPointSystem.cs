using System;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.State;

namespace DeeDeeR.DnD.Runtime.Systems
{
    /// <summary>
    /// Applies D&amp;D 2024 PHB rules for hit point changes, death saving throws, and the
    /// massive-damage instant-death threshold to a <see cref="CharacterState"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All methods mutate the <see cref="CharacterState"/> passed to them in place.
    /// </para>
    /// <para>
    /// <b>Damage type and resistances:</b> <see cref="ApplyDamage"/> accepts a
    /// <see cref="DamageType"/> parameter for future use (e.g. by a resistance/immunity layer),
    /// but does not apply resistance or immunity itself. That logic belongs in the calling game
    /// system, which should compute the final damage amount before calling this method.
    /// </para>
    /// <para>
    /// <b>Unconscious condition:</b> Dropping to 0 HP does not automatically add
    /// <see cref="Condition.Unconscious"/> to <see cref="CharacterState.Conditions"/>. Check
    /// <see cref="HitPointState.IsUnconscious"/> after calling <see cref="ApplyDamage"/> and
    /// use <see cref="ConditionSystem.Apply"/> if you want the condition applied.
    /// <see cref="Heal"/> does remove <see cref="Condition.Unconscious"/> automatically because
    /// regaining any HP always ends the unconscious/dying state.
    /// </para>
    /// </remarks>
    public sealed class HitPointSystem
    {
        // ── Damage ────────────────────────────────────────────────────────────

        /// <summary>
        /// Applies <paramref name="amount"/> damage to the character.
        /// Temporary HP absorbs damage first; the remainder reduces current HP.
        /// Negative or zero amounts are ignored.
        /// </summary>
        /// <param name="state">The character's mutable state. Modified in place.</param>
        /// <param name="amount">Damage to apply (pre-resistance/immunity). Negative values ignored.</param>
        /// <param name="type">
        /// Damage type (for caller context and future resistance layer). Not used internally.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
        public void ApplyDamage(CharacterState state, int amount, DamageType type)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (amount <= 0) return;

            state.HitPoints = state.HitPoints.WithDamage(amount);
        }

        // ── Healing ───────────────────────────────────────────────────────────

        /// <summary>
        /// Heals the character by <paramref name="amount"/> HP, capped at maximum.
        /// Also resets death saves and removes the <see cref="Condition.Unconscious"/> condition,
        /// since regaining any HP ends the dying state per D&amp;D 2024 PHB.
        /// Negative or zero amounts are ignored.
        /// </summary>
        /// <param name="state">The character's mutable state. Modified in place.</param>
        /// <param name="amount">HP to restore. Negative values ignored.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
        public void Heal(CharacterState state, int amount)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (amount <= 0) return;

            state.HitPoints  = state.HitPoints.WithHeal(amount);
            state.DeathSaves = DeathSaveState.Empty;
            state.Conditions.Remove(Condition.Unconscious);
        }

        // ── Temporary HP ──────────────────────────────────────────────────────

        /// <summary>
        /// Grants <paramref name="amount"/> temporary HP per D&amp;D 2024 rules.
        /// Temporary HP do not stack: the new value replaces the old only if it is higher.
        /// Negative or zero amounts are ignored.
        /// </summary>
        /// <param name="state">The character's mutable state. Modified in place.</param>
        /// <param name="amount">Temporary HP to grant. Negative values ignored.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
        public void GainTempHP(CharacterState state, int amount)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (amount <= 0) return;

            state.HitPoints = state.HitPoints.WithTempHP(amount);
        }

        // ── Death saving throws ───────────────────────────────────────────────

        /// <summary>
        /// Rolls one death saving throw for an unconscious character.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        ///   <listheader><term>Roll</term><description>Outcome</description></listheader>
        ///   <item><term>Natural 20</term><description>Regain 1 HP; death saves reset; <see cref="Condition.Unconscious"/> removed.</description></item>
        ///   <item><term>Natural 1</term><description>Two failures recorded.</description></item>
        ///   <item><term>10–19</term><description>One success recorded.</description></item>
        ///   <item><term>2–9</term><description>One failure recorded.</description></item>
        /// </list>
        /// Three successes: <see cref="DeathSaveState.IsStabilized"/> becomes <c>true</c>;
        /// the character is stable (remains at 0 HP, but stops rolling).
        /// Three failures: <see cref="DeathSaveState.IsDead"/> becomes <c>true</c>;
        /// the character dies.
        /// </remarks>
        /// <param name="state">The character's mutable state. Modified in place.</param>
        /// <param name="roller">Die roll provider.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="state"/> or <paramref name="roller"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the character is not unconscious (current HP &gt; 0).
        /// </exception>
        public void RollDeathSave(CharacterState state, IRollProvider roller)
        {
            if (state == null)  throw new ArgumentNullException(nameof(state));
            if (roller == null) throw new ArgumentNullException(nameof(roller));
            if (state.HitPoints.IsAlive)
                throw new InvalidOperationException(
                    "Death saving throws can only be made while the character is at 0 HP.");
            if (state.DeathSaves.IsStabilized)
                throw new InvalidOperationException(
                    "Character is already stabilized; no further death saves are needed.");
            if (state.DeathSaves.IsDead)
                throw new InvalidOperationException(
                    "Character is already dead; death saving throws can no longer be made.");

            int roll = roller.RollDie(20);

            if (roll == 20)
            {
                // Natural 20: regain 1 HP and stop dying.
                state.HitPoints  = new HitPointState(1, state.HitPoints.Maximum, state.HitPoints.Temporary);
                state.DeathSaves = DeathSaveState.Empty;
                state.Conditions.Remove(Condition.Unconscious);
            }
            else if (roll == 1)
            {
                // Natural 1: two failures.
                state.DeathSaves = state.DeathSaves.WithFailure().WithFailure();
            }
            else if (roll >= 10)
            {
                state.DeathSaves = state.DeathSaves.WithSuccess();
            }
            else
            {
                state.DeathSaves = state.DeathSaves.WithFailure();
            }
        }

        // ── Massive damage ────────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> if <paramref name="amount"/> meets or exceeds half the
        /// character's maximum HP, triggering the instant-death massive-damage threshold
        /// per D&amp;D 2024 PHB.
        /// </summary>
        /// <remarks>
        /// Uses <c>amount * 2 &gt;= Maximum</c> to avoid integer rounding issues with odd
        /// maximum HP values (e.g. max 21 → threshold is 11, not 10).
        /// This method only checks the threshold; applying the instant-death outcome is the
        /// responsibility of the caller.
        /// </remarks>
        /// <param name="state">The character's current state.</param>
        /// <param name="amount">The damage amount to test.</param>
        /// <returns><c>true</c> if the damage is massive.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
        public bool MassiveDamageCheck(CharacterState state, int amount)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return (long)amount * 2 >= state.HitPoints.Maximum;
        }
    }
}