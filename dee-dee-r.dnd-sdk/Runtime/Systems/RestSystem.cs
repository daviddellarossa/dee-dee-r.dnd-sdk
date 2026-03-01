using System;
using System.Collections.Generic;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.State;

namespace DeeDeeR.DnD.Runtime.Systems
{
    /// <summary>
    /// Handles Short Rest and Long Rest recovery per D&amp;D 2024 PHB rules.
    /// Stateless — create one instance and reuse it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Short Rest:</b> The character spends one or more Hit Dice. For each die spent,
    /// roll the die and add the Constitution modifier; the total is healed.
    /// Spent Hit Dice are deducted from <see cref="CharacterState.HitDiceAvailable"/>.
    /// </para>
    /// <para>
    /// <b>Long Rest:</b> Per 2024 PHB — all Hit Points restored, Temporary HP cleared,
    /// all Hit Dice regained, all Spell Slots recovered, Exhaustion reduced by one level,
    /// and turn-economy flags reset.
    /// </para>
    /// <para>
    /// Class-specific Short Rest slot recovery (e.g. Warlock) is deferred to Phase 11
    /// components, which handle class-feature logic that requires class inspection.
    /// </para>
    /// </remarks>
    public sealed class RestSystem
    {
        // ── Short Rest ────────────────────────────────────────────────────────

        /// <summary>
        /// Executes a Short Rest: spends the specified Hit Dice, rolls them, adds the
        /// Constitution modifier to each roll (total may be negative but clamped when applied),
        /// heals the total, and deducts spent dice from the pool.
        /// </summary>
        /// <param name="record">
        /// The character's identity and ability scores (Constitution modifier used for healing).
        /// </param>
        /// <param name="state">The character's mutable session state (mutated in place).</param>
        /// <param name="hitDiceToSpend">
        /// How many Hit Dice of each die type to spend. Keys absent from
        /// <see cref="CharacterState.HitDiceAvailable"/> or with a value of zero are skipped.
        /// </param>
        /// <param name="roller">Source of random die values.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="hitDiceToSpend"/> requests more dice of a type than
        /// are currently available in <see cref="CharacterState.HitDiceAvailable"/>.
        /// </exception>
        public void TakeShortRest(
            CharacterRecord              record,
            CharacterState               state,
            Dictionary<DieType, int>     hitDiceToSpend,
            IRollProvider                roller)
        {
            if (record         == null) throw new ArgumentNullException(nameof(record));
            if (state          == null) throw new ArgumentNullException(nameof(state));
            if (hitDiceToSpend == null) throw new ArgumentNullException(nameof(hitDiceToSpend));
            if (roller         == null) throw new ArgumentNullException(nameof(roller));

            // Validate all requests before touching any state.
            foreach (var (die, count) in hitDiceToSpend)
            {
                if (count <= 0) continue;
                state.HitDiceAvailable.TryGetValue(die, out int available);
                if (count > available)
                    throw new InvalidOperationException(
                        $"Cannot spend {count} {die} hit dice; only {available} available.");
            }

            int conMod    = record.AbilityScores.GetModifier(AbilityType.Constitution);
            int totalHeal = 0;

            foreach (var (die, count) in hitDiceToSpend)
            {
                if (count <= 0) continue;

                for (int i = 0; i < count; i++)
                    totalHeal += roller.RollDie((int)die) + conMod;

                state.HitDiceAvailable[die] -= count;
            }

            // Use HitPointSystem.Heal so that regaining HP above 0 also resets death saves
            // and removes Condition.Unconscious — consistent with PHB "regaining any HP ends
            // the dying state" and with HitPointSystem.Heal's documented behaviour.
            if (totalHeal > 0)
                new HitPointSystem().Heal(state, totalHeal);
        }

        // ── Long Rest ─────────────────────────────────────────────────────────

        /// <summary>
        /// Executes a Long Rest: restores all Hit Points (clears Temporary HP), regains all
        /// Hit Dice, recovers all Spell Slots to their class-determined maximum, reduces
        /// Exhaustion by one level, and resets turn-economy flags.
        /// </summary>
        /// <param name="record">
        /// The character's identity and class data (used to recalculate Hit Dice maxima and
        /// spell slot maxima).
        /// </param>
        /// <param name="state">The character's mutable session state (mutated in place).</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public void TakeLongRest(CharacterRecord record, CharacterState state)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (state  == null) throw new ArgumentNullException(nameof(state));

            // Restore all HP; Temporary HP expire at the end of a Long Rest (2024 PHB).
            // Also reset death saves and remove Unconscious — regaining HP above 0 ends the
            // dying state, consistent with HitPointSystem.Heal behaviour.
            state.HitPoints  = new HitPointState(state.HitPoints.Maximum, state.HitPoints.Maximum);
            state.DeathSaves = DeathSaveState.Empty;
            state.Conditions.Remove(Condition.Unconscious);

            // Regain all Hit Dice (2024 PHB: all expended Hit Dice are recovered on a Long Rest).
            state.HitDiceAvailable = ComputeMaxHitDice(record);

            // Recover all Spell Slots.
            state.SpellSlots = new MulticlassSystem().CalculateCombinedSpellSlots(record.ClassLevels);

            // Reduce Exhaustion by 1 (clamped to 0 by ExhaustionLevel).
            state.Exhaustion = state.Exhaustion.Decrease(1);

            // Reset per-turn action economy flags.
            state.ActionUsed      = false;
            state.BonusActionUsed = false;
            state.ReactionUsed    = false;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static Dictionary<DieType, int> ComputeMaxHitDice(CharacterRecord record)
        {
            var max = new Dictionary<DieType, int>();
            foreach (var cl in record.ClassLevels)
            {
                if (cl?.Class == null) continue;
                var die = cl.Class.HitDie;
                max.TryGetValue(die, out int current);
                max[die] = current + cl.Level;
            }
            return max;
        }
    }
}