using System;
using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Core.Values
{
    /// <summary>
    /// An immutable representation of a dice expression such as "2d6+3" or "1d8".
    /// Count == 0 represents a flat modifier with no dice (e.g. Flat(5) → "+5").
    /// </summary>
    public readonly struct DiceExpression : IEquatable<DiceExpression>
    {
        /// <summary>Number of dice to roll. Zero means no dice (flat modifier only — see <see cref="Flat"/>).</summary>
        public readonly int      Count;

        /// <summary>The type of die to roll (D4, D6, D8, D10, D12, D20, D100).</summary>
        public readonly DieType  Die;

        /// <summary>Flat bonus (or penalty) added to the total after rolling. May be negative.</summary>
        public readonly int      Modifier;

        /// <summary>Creates a dice expression (e.g. 2d6+3).</summary>
        /// <param name="count">Number of dice. Use 0 for a flat value with no dice. Must be non-negative.</param>
        /// <param name="die">Die type to roll.</param>
        /// <param name="modifier">Flat modifier added after rolling.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
        public DiceExpression(int count, DieType die, int modifier = 0)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be non-negative.");

            Count    = count;
            Die      = die;
            Modifier = modifier;
        }

        /// <summary>Creates an expression with no dice — a flat value (e.g. a fixed bonus).</summary>
        public static DiceExpression Flat(int value) => new DiceExpression(0, default, value);

        /// <summary>Average expected value of this expression.</summary>
        public float Average
        {
            get
            {
                float dieAvg = Count > 0 ? Count * ((int)Die + 1) / 2f : 0f;
                return dieAvg + Modifier;
            }
        }

        /// <summary>
        /// Returns the expression as a human-readable string (e.g. "2d6+3", "1d8", "−2", "5").
        /// </summary>
        public override string ToString()
        {
            if (Count <= 0)
                return Modifier.ToString();

            string dice = $"{Count}d{(int)Die}";
            if (Modifier == 0) return dice;
            return Modifier > 0 ? $"{dice}+{Modifier}" : $"{dice}{Modifier}";
        }

        public bool Equals(DiceExpression other) =>
            Count == other.Count && Die == other.Die && Modifier == other.Modifier;

        public override bool Equals(object obj) => obj is DiceExpression other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Count, Die, Modifier);

        public static bool operator ==(DiceExpression a, DiceExpression b) => a.Equals(b);
        public static bool operator !=(DiceExpression a, DiceExpression b) => !a.Equals(b);
    }
}
