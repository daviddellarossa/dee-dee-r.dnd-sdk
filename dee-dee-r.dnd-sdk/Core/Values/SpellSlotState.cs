using System;

namespace DeeDeeR.DnD.Core.Values
{
    /// <summary>
    /// Immutable snapshot of available spell slots at each level (1–9).
    /// The maximum slots per level are computed from class levels by
    /// <c>MulticlassSystem.CalculateCombinedSpellSlots</c> (Phase 9) and stored here at the
    /// start of a long rest. Functional update methods return a new state without modifying
    /// the original, consistent with the rest of the Core value types.
    /// </summary>
    public readonly struct SpellSlotState : IEquatable<SpellSlotState>
    {
        /// <summary>Available 1st-level spell slots.</summary>
        public readonly int Slot1;
        /// <summary>Available 2nd-level spell slots.</summary>
        public readonly int Slot2;
        /// <summary>Available 3rd-level spell slots.</summary>
        public readonly int Slot3;
        /// <summary>Available 4th-level spell slots.</summary>
        public readonly int Slot4;
        /// <summary>Available 5th-level spell slots.</summary>
        public readonly int Slot5;
        /// <summary>Available 6th-level spell slots.</summary>
        public readonly int Slot6;
        /// <summary>Available 7th-level spell slots.</summary>
        public readonly int Slot7;
        /// <summary>Available 8th-level spell slots.</summary>
        public readonly int Slot8;
        /// <summary>Available 9th-level spell slots.</summary>
        public readonly int Slot9;

        /// <summary>
        /// Creates a spell slot state. Each slot count is clamped to zero — negative values
        /// are not valid.
        /// </summary>
        public SpellSlotState(
            int s1 = 0, int s2 = 0, int s3 = 0,
            int s4 = 0, int s5 = 0, int s6 = 0,
            int s7 = 0, int s8 = 0, int s9 = 0)
        {
            Slot1 = Math.Max(0, s1);
            Slot2 = Math.Max(0, s2);
            Slot3 = Math.Max(0, s3);
            Slot4 = Math.Max(0, s4);
            Slot5 = Math.Max(0, s5);
            Slot6 = Math.Max(0, s6);
            Slot7 = Math.Max(0, s7);
            Slot8 = Math.Max(0, s8);
            Slot9 = Math.Max(0, s9);
        }

        /// <summary>Total available slots across all levels.</summary>
        public int Total => Slot1 + Slot2 + Slot3 + Slot4 + Slot5 + Slot6 + Slot7 + Slot8 + Slot9;

        /// <summary>
        /// Returns the number of available slots at the given spell level.
        /// </summary>
        /// <param name="level">Spell slot level (1–9).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="level"/> is outside 1–9.</exception>
        public int GetAvailable(int level) => level switch
        {
            1 => Slot1, 2 => Slot2, 3 => Slot3,
            4 => Slot4, 5 => Slot5, 6 => Slot6,
            7 => Slot7, 8 => Slot8, 9 => Slot9,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Spell slot level must be 1–9.")
        };

        /// <summary>Returns <c>true</c> if at least one slot is available at the given level.</summary>
        /// <param name="level">Spell slot level (1–9).</param>
        public bool HasSlot(int level) => GetAvailable(level) > 0;

        /// <summary>
        /// Returns a new state with one slot at <paramref name="level"/> expended.
        /// If no slot is available at that level the count is clamped to zero — callers should
        /// check <see cref="HasSlot"/> (or use <c>SpellSystem.CanCastSpell</c>) before expending.
        /// </summary>
        /// <param name="level">Spell slot level to expend (1–9).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="level"/> is outside 1–9.</exception>
        public SpellSlotState WithExpended(int level)
        {
            if (level < 1 || level > 9)
                throw new ArgumentOutOfRangeException(nameof(level), level, "Spell slot level must be 1–9.");

            return new SpellSlotState(
                level == 1 ? Math.Max(0, Slot1 - 1) : Slot1,
                level == 2 ? Math.Max(0, Slot2 - 1) : Slot2,
                level == 3 ? Math.Max(0, Slot3 - 1) : Slot3,
                level == 4 ? Math.Max(0, Slot4 - 1) : Slot4,
                level == 5 ? Math.Max(0, Slot5 - 1) : Slot5,
                level == 6 ? Math.Max(0, Slot6 - 1) : Slot6,
                level == 7 ? Math.Max(0, Slot7 - 1) : Slot7,
                level == 8 ? Math.Max(0, Slot8 - 1) : Slot8,
                level == 9 ? Math.Max(0, Slot9 - 1) : Slot9
            );
        }

        /// <summary>
        /// Returns a new state with <paramref name="count"/> slots recovered at <paramref name="level"/>.
        /// Callers are responsible for not exceeding the character's maximum slots for that level.
        /// </summary>
        /// <param name="level">Spell slot level to recover (1–9).</param>
        /// <param name="count">Number of slots to recover. Negative values are ignored (clamped to 0).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="level"/> is outside 1–9.</exception>
        public SpellSlotState WithRecovered(int level, int count)
        {
            if (level < 1 || level > 9)
                throw new ArgumentOutOfRangeException(nameof(level), level, "Spell slot level must be 1–9.");

            int delta = Math.Max(0, count);
            return new SpellSlotState(
                level == 1 ? Slot1 + delta : Slot1,
                level == 2 ? Slot2 + delta : Slot2,
                level == 3 ? Slot3 + delta : Slot3,
                level == 4 ? Slot4 + delta : Slot4,
                level == 5 ? Slot5 + delta : Slot5,
                level == 6 ? Slot6 + delta : Slot6,
                level == 7 ? Slot7 + delta : Slot7,
                level == 8 ? Slot8 + delta : Slot8,
                level == 9 ? Slot9 + delta : Slot9
            );
        }

        /// <summary>A spell slot state with zero slots at every level.</summary>
        public static readonly SpellSlotState Empty = new SpellSlotState();

        /// <summary>Returns a human-readable summary of non-zero slot counts (e.g. "1st×4 2nd×3 3rd×2").</summary>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            void Append(int count, string label) { if (count > 0) { if (sb.Length > 0) sb.Append(' '); sb.Append($"{label}×{count}"); } }
            Append(Slot1, "1st"); Append(Slot2, "2nd"); Append(Slot3, "3rd");
            Append(Slot4, "4th"); Append(Slot5, "5th"); Append(Slot6, "6th");
            Append(Slot7, "7th"); Append(Slot8, "8th"); Append(Slot9, "9th");
            return sb.Length == 0 ? "none" : sb.ToString();
        }

        /// <inheritdoc/>
        public bool Equals(SpellSlotState other) =>
            Slot1 == other.Slot1 && Slot2 == other.Slot2 && Slot3 == other.Slot3 &&
            Slot4 == other.Slot4 && Slot5 == other.Slot5 && Slot6 == other.Slot6 &&
            Slot7 == other.Slot7 && Slot8 == other.Slot8 && Slot9 == other.Slot9;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is SpellSlotState other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() =>
            HashCode.Combine(Slot1, Slot2, Slot3, Slot4, Slot5,
                HashCode.Combine(Slot6, Slot7, Slot8, Slot9));

        /// <summary>Returns true if both states have identical slot counts at every level.</summary>
        public static bool operator ==(SpellSlotState a, SpellSlotState b) => a.Equals(b);

        /// <summary>Returns true if the states differ in at least one slot level.</summary>
        public static bool operator !=(SpellSlotState a, SpellSlotState b) => !a.Equals(b);
    }
}