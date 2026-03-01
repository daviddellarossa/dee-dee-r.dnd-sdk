using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Runtime.Bus.Args
{
    /// <summary>Published when a spell is cast (slot expended or cantrip used).</summary>
    public readonly struct SpellCastArgs
    {
        public readonly EndpointId Caster;
        public readonly SpellSO    Spell;
        /// <summary>0 for cantrips; 1–9 for levelled spells (may exceed the spell's base level when upcast).</summary>
        public readonly int        SlotLevel;

        public SpellCastArgs(EndpointId caster, SpellSO spell, int slotLevel)
        {
            Caster    = caster;
            Spell     = spell;
            SlotLevel = slotLevel;
        }
    }

    /// <summary>Published when concentration on a spell begins or is broken.</summary>
    public readonly struct ConcentrationArgs
    {
        public readonly EndpointId Caster;
        public readonly SpellSO    Spell;

        public ConcentrationArgs(EndpointId caster, SpellSO spell)
        {
            Caster = caster;
            Spell  = spell;
        }
    }

    /// <summary>Published when a spell slot at a given level is expended.</summary>
    public readonly struct SlotArgs
    {
        public readonly EndpointId Caster;
        public readonly int        SlotLevel;

        public SlotArgs(EndpointId caster, int slotLevel)
        {
            Caster    = caster;
            SlotLevel = slotLevel;
        }
    }

    /// <summary>Published when a character learns a new spell.</summary>
    public readonly struct SpellLearnedArgs
    {
        public readonly EndpointId Caster;
        public readonly SpellSO    Spell;

        public SpellLearnedArgs(EndpointId caster, SpellSO spell)
        {
            Caster = caster;
            Spell  = spell;
        }
    }
}