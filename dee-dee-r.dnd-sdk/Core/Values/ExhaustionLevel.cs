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
        /// <summary>Exhaustion level (0 = none; 6 = dead).</summary>
        public readonly int Value;

        /// <summary>Creates an exhaustion level, clamped to the valid range [0, 6].</summary>
        public ExhaustionLevel(int value)
        {
            Value = Math.Min(6, Math.Max(0, value));
        }

        /// <summary>Penalty applied to all d20 tests and spell save DCs (2024 PHB rule).</summary>
        public int D20Penalty => Value * 2;

        /// <summary>True when the exhaustion level is greater than zero.</summary>
        public bool IsExhausted => Value > 0;

        /// <summary>At level 6 the creature dies.</summary>
        public bool IsDead => Value >= 6;

        /// <summary>Returns a new level increased by <paramref name="by"/> steps, clamped to 6.</summary>
        public ExhaustionLevel Increase(int by = 1) => new ExhaustionLevel(Value + by);

        /// <summary>Returns a new level decreased by <paramref name="by"/> steps, clamped to 0.</summary>
        public ExhaustionLevel Decrease(int by = 1) => new ExhaustionLevel(Value - by);

        /// <summary>No exhaustion (level 0).</summary>
        public static readonly ExhaustionLevel None = new ExhaustionLevel(0);

        /// <summary>Fatal exhaustion (level 6). Creature dies.</summary>
        public static readonly ExhaustionLevel Dead = new ExhaustionLevel(6);

        /// <inheritdoc/>
        public bool Equals(ExhaustionLevel other) => Value == other.Value;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is ExhaustionLevel other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>Returns a display string such as "Exhaustion 3 (−6 to d20)".</summary>
        public override string ToString() => $"Exhaustion {Value} (−{D20Penalty} to d20)";

        /// <summary>Returns true if both exhaustion levels are equal.</summary>
        public static bool operator ==(ExhaustionLevel a, ExhaustionLevel b) => a.Equals(b);

        /// <summary>Returns true if the exhaustion levels differ.</summary>
        public static bool operator !=(ExhaustionLevel a, ExhaustionLevel b) => !a.Equals(b);

        /// <summary>Returns true if <paramref name="a"/> is less severe than <paramref name="b"/>.</summary>
        public static bool operator  <(ExhaustionLevel a, ExhaustionLevel b) => a.Value < b.Value;

        /// <summary>Returns true if <paramref name="a"/> is more severe than <paramref name="b"/>.</summary>
        public static bool operator  >(ExhaustionLevel a, ExhaustionLevel b) => a.Value > b.Value;
    }
}
