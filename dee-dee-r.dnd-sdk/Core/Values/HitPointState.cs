using System;

namespace DeeDeeR.DnD.Core.Values
{
    /// <summary>
    /// An immutable snapshot of a creature's hit points.
    /// Use the With* methods to produce updated copies (functional update pattern).
    /// </summary>
    public readonly struct HitPointState
    {
        /// <summary>Current hit points (0 = unconscious/dying; never negative).</summary>
        public readonly int Current;

        /// <summary>Maximum hit points (the ceiling for healing).</summary>
        public readonly int Maximum;

        /// <summary>Temporary hit points, consumed before regular HP. Do not stack (2024 PHB).</summary>
        public readonly int Temporary;

        /// <summary>True when <see cref="Current"/> is above zero.</summary>
        public bool IsAlive       => Current > 0;

        /// <summary>True when <see cref="Current"/> is zero or below (creature is dying or unconscious).</summary>
        public bool IsUnconscious => Current <= 0;

        /// <summary>
        /// True when <see cref="Current"/> is zero. Note: actual death is determined by
        /// <see cref="DeathSaveState.IsDead"/>, not by hit points alone.
        /// </summary>
        public bool IsDead        => Current <= 0;

        /// <summary>Creates a hit point state, clamping all values to zero minimum.</summary>
        /// <param name="current">Current HP. Clamped to [0, ∞).</param>
        /// <param name="maximum">Maximum HP. Clamped to [0, ∞).</param>
        /// <param name="temporary">Temporary HP. Clamped to [0, ∞).</param>
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

        /// <summary>Returns a display string such as "14/20 HP (+5 temp)" or "14/20 HP".</summary>
        public override string ToString() =>
            Temporary > 0
                ? $"{Current}/{Maximum} HP (+{Temporary} temp)"
                : $"{Current}/{Maximum} HP";
    }
}
