using System;

namespace DeeDeeR.DnD.Core.Values
{
    /// <summary>
    /// An immutable snapshot of a creature's hit points.
    /// Use the With* methods to produce updated copies (functional update pattern).
    /// </summary>
    public readonly struct HitPointState
    {
        public readonly int Current;
        public readonly int Maximum;
        public readonly int Temporary;

        public bool IsAlive       => Current > 0;
        public bool IsUnconscious => Current <= 0;
        public bool IsDead        => Current <= 0; // death is determined by death saves, not HP alone

        public HitPointState(int current, int maximum, int temporary = 0)
        {
            Current   = Math.Max(0, current);
            Maximum   = Math.Max(0, maximum);
            Temporary = Math.Max(0, temporary);
        }

        /// <summary>
        /// Applies damage: temporary HP is consumed first, then current HP.
        /// Returns a new state; never goes below 0 current HP.
        /// </summary>
        public HitPointState WithDamage(int amount)
        {
            if (amount <= 0) return this;

            int absorbed  = Math.Min(Temporary, amount);
            int remainder = amount - absorbed;
            return new HitPointState(
                Math.Max(0, Current - remainder),
                Maximum,
                Temporary - absorbed
            );
        }

        /// <summary>Heals current HP, capped at Maximum. Does not affect temporary HP.</summary>
        public HitPointState WithHeal(int amount)
        {
            if (amount <= 0) return this;
            return new HitPointState(Math.Min(Current + amount, Maximum), Maximum, Temporary);
        }

        /// <summary>
        /// Grants temporary HP. Per 2024 PHB: if new value is higher, it replaces the old value;
        /// temporary HP do not stack.
        /// </summary>
        public HitPointState WithTempHP(int amount) =>
            new HitPointState(Current, Maximum, Math.Max(Temporary, Math.Max(0, amount)));

        /// <summary>Sets maximum HP (e.g. after level-up). Scales current HP if it exceeded new max.</summary>
        public HitPointState WithMaximum(int newMax) =>
            new HitPointState(Math.Min(Current, newMax), newMax, Temporary);

        public override string ToString() =>
            Temporary > 0
                ? $"{Current}/{Maximum} HP (+{Temporary} temp)"
                : $"{Current}/{Maximum} HP";
    }
}
