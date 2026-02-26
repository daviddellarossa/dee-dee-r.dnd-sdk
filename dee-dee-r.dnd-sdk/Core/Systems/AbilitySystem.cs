using System;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Core.Systems
{
    /// <summary>
    /// Resolves d20 checks per D&amp;D 2024 PHB rules.
    /// Stateless — create one instance and inject a <see cref="DiceRoller"/> backed by the
    /// appropriate <see cref="Interfaces.IRollProvider"/> (deterministic in tests, Unity random at runtime).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="RollCheck"/> is the single entry point for all d20 rolls. The caller is
    /// responsible for computing the total modifier before calling it:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <term>Raw ability check</term>
    ///     <description>
    ///       <c>modifier = scores.GetModifier(ability) - exhaustion.D20Penalty</c>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Skill check</term>
    ///     <description>
    ///       <c>modifier = proficiencySystem.GetSkillBonus(scores, skill, isProficient, hasExpertise, profBonus)
    ///       - exhaustion.D20Penalty</c>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Saving throw</term>
    ///     <description>
    ///       <c>modifier = scores.GetModifier(ability) + (isProficient ? profBonus : 0)
    ///       - exhaustion.D20Penalty</c>
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// <b>Advantage / Disadvantage:</b> <see cref="RollCheck"/> calls
    /// <see cref="DiceRoller.Roll"/> twice (each with a <c>1d20</c> expression) and picks the
    /// higher or lower <see cref="RollResult"/>. The crit flags on the chosen result are therefore
    /// always accurate — a natural 20 discarded by disadvantage will not appear in the returned result.
    /// </para>
    /// <para>
    /// <b>Passive checks</b> require no dice roll; use the static <see cref="PassiveCheck"/> method.
    /// </para>
    /// </remarks>
    public sealed class AbilitySystem
    {
        private readonly DiceRoller _roller;

        /// <summary>Creates an AbilitySystem backed by the given dice roller.</summary>
        /// <param name="roller">The dice roller to use for all d20 rolls.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="roller"/> is null.</exception>
        public AbilitySystem(DiceRoller roller)
        {
            _roller = roller ?? throw new ArgumentNullException(nameof(roller));
        }

        /// <summary>
        /// Rolls a d20, adds <paramref name="totalModifier"/>, and returns the result.
        /// For advantage or disadvantage, two dice are rolled and the appropriate result is returned;
        /// the crit flags on the returned <see cref="RollResult"/> reflect the chosen die.
        /// </summary>
        /// <param name="totalModifier">
        /// The combined modifier to add to the d20: ability modifier ± proficiency bonus
        /// − exhaustion penalty, etc. See the class remarks for per-check-type formulas.
        /// </param>
        /// <param name="advantage">Whether to roll with advantage, disadvantage, or normally.</param>
        public RollResult RollCheck(int totalModifier, AdvantageState advantage)
        {
            var expr  = new DiceExpression(1, DieType.D20, totalModifier);
            var roll1 = _roller.Roll(expr);

            if (advantage == AdvantageState.Normal)
                return roll1;

            var roll2 = _roller.Roll(expr);

            return advantage == AdvantageState.Advantage
                ? (roll1.Total >= roll2.Total ? roll1 : roll2)
                : (roll1.Total <= roll2.Total ? roll1 : roll2);
        }

        /// <summary>
        /// Returns the passive check score (10 + <paramref name="totalModifier"/>) without rolling.
        /// Used for Passive Perception and similar always-on checks.
        /// </summary>
        /// <param name="totalModifier">
        /// The combined modifier (e.g. Perception skill bonus including proficiency if applicable).
        /// </param>
        public static int PassiveCheck(int totalModifier) => 10 + totalModifier;
    }
}
