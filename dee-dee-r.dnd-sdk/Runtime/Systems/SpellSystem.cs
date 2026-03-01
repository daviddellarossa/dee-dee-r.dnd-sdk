using System;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.State;

namespace DeeDeeR.DnD.Runtime.Systems
{
    /// <summary>
    /// Manages spell slot expenditure and concentration for a character per D&amp;D 2024 PHB rules.
    /// Stateless — create one instance and reuse it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This system handles the mechanical side of spellcasting: slot availability checks,
    /// slot expenditure, and concentration tracking. Spell effects, damage, saving throws,
    /// and area targeting are resolved by the caller (or deferred to Phase 11 components).
    /// </para>
    /// <para>
    /// Slot recovery on Short or Long Rest is handled by <see cref="RestSystem"/>.
    /// </para>
    /// </remarks>
    public sealed class SpellSystem
    {
        // ── Slot Checks ───────────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> if the character can cast <paramref name="spell"/> using the
        /// given slot level.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       Cantrips (<see cref="SpellSO.Level"/> == 0) require <paramref name="slotLevel"/> == 0;
        ///       they never expend a slot.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       Non-cantrip spells require <paramref name="slotLevel"/> ≥ <see cref="SpellSO.Level"/>
        ///       and at least one slot of that level to be available in <see cref="CharacterState.SpellSlots"/>.
        ///     </description>
        ///   </item>
        /// </list>
        /// This method does not check whether the character knows or has prepared the spell —
        /// that is the caller's responsibility via <see cref="SpellbookState"/>.
        /// </remarks>
        /// <param name="state">The character's mutable session state.</param>
        /// <param name="spell">The spell to cast.</param>
        /// <param name="slotLevel">
        /// The spell slot level to use (0 = cantrip / no slot, 1–9 = levelled slot).
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> or <paramref name="spell"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="slotLevel"/> is outside 0–9.</exception>
        public bool CanCastSpell(CharacterState state, SpellSO spell, int slotLevel)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (spell == null) throw new ArgumentNullException(nameof(spell));
            if (slotLevel < 0 || slotLevel > 9)
                throw new ArgumentOutOfRangeException(nameof(slotLevel), slotLevel,
                    "Slot level must be 0–9.");

            // Cantrip: no slot needed; slotLevel must be 0.
            if (spell.Level == 0) return slotLevel == 0;

            // Non-cantrip: slot must be at least the spell's own level.
            if (slotLevel < spell.Level) return false;

            return state.SpellSlots.HasSlot(slotLevel);
        }

        // ── Slot Expenditure ──────────────────────────────────────────────────

        /// <summary>
        /// Expends one spell slot at <paramref name="slotLevel"/> from the character's
        /// <see cref="CharacterState.SpellSlots"/>.
        /// Call <see cref="CanCastSpell"/> first to verify a slot is available.
        /// </summary>
        /// <param name="state">The character's mutable session state (mutated in place).</param>
        /// <param name="slotLevel">The slot level to expend (1–9).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="slotLevel"/> is outside 1–9.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no slot is available at the given level.</exception>
        public void ExpendSlot(CharacterState state, int slotLevel)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (slotLevel < 1 || slotLevel > 9)
                throw new ArgumentOutOfRangeException(nameof(slotLevel), slotLevel,
                    "Slot level must be 1–9.");

            if (!state.SpellSlots.HasSlot(slotLevel))
                throw new InvalidOperationException(
                    $"No spell slot available at level {slotLevel}.");

            state.SpellSlots = state.SpellSlots.WithExpended(slotLevel);
        }

        // ── Concentration ─────────────────────────────────────────────────────

        /// <summary>
        /// Sets <paramref name="spell"/> as the character's active concentration spell.
        /// Any previously concentrated spell is silently replaced — the caller is responsible
        /// for informing the player that previous concentration ended.
        /// </summary>
        /// <param name="state">The character's mutable session state (mutated in place).</param>
        /// <param name="spell">The concentration spell now being maintained.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> or <paramref name="spell"/> is null.</exception>
        public void BeginConcentration(CharacterState state, SpellSO spell)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (spell == null) throw new ArgumentNullException(nameof(spell));

            state.ConcentrationSpell = spell;
        }

        /// <summary>
        /// Clears the character's active concentration spell.
        /// Safe to call when the character is not currently concentrating (no-op).
        /// </summary>
        /// <param name="state">The character's mutable session state (mutated in place).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
        public void BreakConcentration(CharacterState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            state.ConcentrationSpell = null;
        }
    }
}