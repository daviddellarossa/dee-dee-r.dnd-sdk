using System;

namespace DeeDeeR.DnD.Core.Values
{
    /// <summary>
    /// Exhaustion level (0–6) per D&D 2024 PHB rules.
    /// Each level imposes a -2 penalty to all d20 tests and spell save DCs.
    /// Level 6 is fatal.
    /// </summary>
    public readonly struct ExhaustionLevel : IEquatable<ExhaustionLevel>
    {
        public readonly int Value;

        public ExhaustionLevel(int value)
        {
            Value = Math.Min(6, Math.Max(0, value));
        }

        /// <summary>Penalty applied to all d20 tests and spell save DCs (2024 PHB rule).</summary>
        public int D20Penalty => Value * 2;

        public bool IsExhausted => Value > 0;

        /// <summary>At level 6 the creature dies.</summary>
        public bool IsDead => Value >= 6;

        public ExhaustionLevel Increase(int by = 1) => new ExhaustionLevel(Value + by);
        public ExhaustionLevel Decrease(int by = 1) => new ExhaustionLevel(Value - by);

        public static readonly ExhaustionLevel None = new ExhaustionLevel(0);
        public static readonly ExhaustionLevel Dead = new ExhaustionLevel(6);

        public bool Equals(ExhaustionLevel other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ExhaustionLevel other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => $"Exhaustion {Value} (−{D20Penalty} to d20)";

        public static bool operator ==(ExhaustionLevel a, ExhaustionLevel b) => a.Equals(b);
        public static bool operator !=(ExhaustionLevel a, ExhaustionLevel b) => !a.Equals(b);
        public static bool operator  <(ExhaustionLevel a, ExhaustionLevel b) => a.Value < b.Value;
        public static bool operator  >(ExhaustionLevel a, ExhaustionLevel b) => a.Value > b.Value;
    }
}
