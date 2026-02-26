using System;

namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Spell casting components as defined in D&D 2024 PHB.
    /// Flagged enum — a spell may require any combination of V, S, and M.
    /// </summary>
    [Flags]
    public enum SpellComponent
    {
        /// <summary>No components required.</summary>
        None     = 0,

        /// <summary>Requires spoken words. A silenced caster cannot cast spells with this component.</summary>
        Verbal   = 1 << 0,

        /// <summary>Requires specific hand movements. A bound caster cannot cast spells with this component.</summary>
        Somatic  = 1 << 1,

        /// <summary>Requires a physical material component. A spellcasting focus can substitute unless the component has a listed cost.</summary>
        Material = 1 << 2
    }
}
