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
        public readonly int      Count;
        public readonly DieType  Die;
        public readonly int      Modifier;

        public DiceExpression(int count, DieType die, int modifier = 0)
        {
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
