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
        None     = 0,
        Verbal   = 1 << 0,
        Somatic  = 1 << 1,
        Material = 1 << 2
    }
}
