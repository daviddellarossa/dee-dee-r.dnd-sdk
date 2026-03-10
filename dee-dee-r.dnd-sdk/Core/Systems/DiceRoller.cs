using System;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Core.Systems
{
    /// <summary>
    /// Rolls <see cref="DiceExpression"/> values using an injected <see cref="IRollProvider"/>.
    /// Stateless — create one instance per scene/system and inject a deterministic provider in tests.
    /// <para>
    /// <b>Advantage / Disadvantage:</b> call <see cref="Roll"/> twice with a <c>1d20</c> expression
    /// and let the calling system pick the higher (advantage) or lower (disadvantage) result.
    /// Do <em>not</em> pass a <c>2d20</c> expression — that suppresses crit flags and loses the
    /// individual die values needed for selection.
    /// </para>
    /// </summary>
    public class DiceRoller
    {
        private readonly IRollProvider _rollProvider;

        /// <summary>Creates a DiceRoller backed by the given randomness source.</summary>
        /// <param name="rollProvider">The source of random die values.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="rollProvider"/> is null.</exception>
        public DiceRoller(IRollProvider rollProvider)
        {
            _rollProvider = rollProvider ?? throw new ArgumentNullException(nameof(rollProvider));
        }

        /// <summary>
        /// Rolls the given dice expression and returns the total with all modifiers applied.
        /// <para>
        /// Critical success (natural 20) and critical fail (natural 1) flags are set only when
        /// rolling exactly one d20 — the standard ability check, attack roll, or saving throw die.
        /// Multi-die expressions (e.g. 2d20 for advantage) do not set crit flags; the caller
        /// (e.g. <c>AbilitySystem</c>) selects the preferred result from two separate 1d20 rolls.
        /// </para>
        /// </summary>
        /// <param name="expression">The dice expression to roll.</param>
        /// <returns>
        /// A <see cref="RollResult"/> containing the total and, for a single d20, crit flags.
        /// </returns>
        public RollResult Roll(DiceExpression expression)
        {
            if (expression.Count == 0)
                return new RollResult(expression.Modifier);

            int  total            = expression.Modifier;
            bool isCriticalSuccess = false;
            bool isCriticalFail   = false;
            bool isSingleD20      = expression.Count == 1 && expression.Die == DieType.D20;

            for (int i = 0; i < expression.Count; i++)
            {
                int dieResult = _rollProvider.RollDie((int)expression.Die);
                total += dieResult;

                if (isSingleD20)
                {
                    isCriticalSuccess = dieResult == 20;
                    isCriticalFail    = dieResult == 1;
                }
            }

            return new RollResult(total, isCriticalSuccess, isCriticalFail);
        }
    }
}