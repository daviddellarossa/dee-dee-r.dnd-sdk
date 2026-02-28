using System;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;
using DeeDeeR.DnD.Runtime.State;

namespace DeeDeeR.DnD.Runtime.Systems
{
    /// <summary>
    /// Rolls initiative for a character per D&amp;D 2024 PHB rules:
    /// d20 + Dexterity modifier − exhaustion penalty.
    /// Stateless — create one instance and reuse it.
    /// </summary>
    public sealed class InitiativeSystem
    {
        /// <summary>
        /// Rolls initiative for the given character and returns the total.
        /// </summary>
        /// <param name="record">The character's identity and ability scores.</param>
        /// <param name="state">The character's mutable session state (provides exhaustion level).</param>
        /// <param name="roller">Source of random die values.</param>
        /// <param name="advantage">Whether the roll is made with advantage, disadvantage, or normally.</param>
        /// <returns>
        /// Initiative total: d20 result + DEX modifier − exhaustion penalty.
        /// When rolling with advantage, the higher of two d20 rolls is used; with disadvantage,
        /// the lower.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="record"/>, <paramref name="state"/>, or
        /// <paramref name="roller"/> is null.
        /// </exception>
        public int RollInitiative(
            CharacterRecord record,
            CharacterState  state,
            IRollProvider   roller,
            AdvantageState  advantage = AdvantageState.Normal)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (state  == null) throw new ArgumentNullException(nameof(state));
            if (roller == null) throw new ArgumentNullException(nameof(roller));

            int modifier = record.AbilityScores.GetModifier(AbilityType.Dexterity)
                         - state.Exhaustion.D20Penalty;

            int roll1 = roller.RollDie(20);

            if (advantage == AdvantageState.Normal)
                return roll1 + modifier;

            int roll2   = roller.RollDie(20);
            int rawRoll = advantage == AdvantageState.Advantage
                ? Math.Max(roll1, roll2)
                : Math.Min(roll1, roll2);

            return rawRoll + modifier;
        }
    }
}