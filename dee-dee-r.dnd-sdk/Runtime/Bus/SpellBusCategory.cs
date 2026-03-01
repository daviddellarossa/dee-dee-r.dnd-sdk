using System;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Bus.Args;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Runtime.Bus
{
    /// <summary>
    /// Bus channels for spellcasting events and queries (slots, concentration, learned spells).
    /// </summary>
    public sealed class SpellBusCategory
    {
        // ── Signals ───────────────────────────────────────────────────────────

        /// <summary>Published when a character casts a spell (slot expended or cantrip used).</summary>
        public readonly Signal<SpellCastArgs>     SpellCast;

        /// <summary>Published when a character's concentration on a spell is broken.</summary>
        public readonly Signal<ConcentrationArgs> ConcentrationBroken;

        /// <summary>Published when a spell slot is expended.</summary>
        public readonly Signal<SlotArgs>          SpellSlotExpended;

        /// <summary>Published when a character learns a new spell.</summary>
        public readonly Signal<SpellLearnedArgs>  SpellLearned;

        // ── Queries ───────────────────────────────────────────────────────────

        /// <summary>Returns the addressed character's available spell slots at each level.</summary>
        public readonly Query<EmptyArgs, SpellSlotState> GetAvailableSpellSlots;

        /// <summary>
        /// Returns the spell the addressed character is currently concentrating on,
        /// or <c>null</c> if they are not concentrating.
        /// </summary>
        public readonly Query<EmptyArgs, SpellSO> GetConcentrationSpell;

        // ── Constructor ───────────────────────────────────────────────────────

        public SpellBusCategory(IFrameScheduler scheduler)
        {
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            SpellCast              = new Signal<SpellCastArgs>(scheduler);
            ConcentrationBroken    = new Signal<ConcentrationArgs>(scheduler);
            SpellSlotExpended      = new Signal<SlotArgs>(scheduler);
            SpellLearned           = new Signal<SpellLearnedArgs>(scheduler);
            GetAvailableSpellSlots = new Query<EmptyArgs, SpellSlotState>(scheduler);
            GetConcentrationSpell  = new Query<EmptyArgs, SpellSO>(scheduler);
        }
    }
}