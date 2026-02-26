namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Defines how a class contributes to the combined spell slot table when multiclassing.
    /// Used by MulticlassSystem to compute total available spell slots (D&D 2024 PHB).
    /// </summary>
    public enum CasterType
    {
        /// <summary>No spellcasting (e.g. Fighter base, Barbarian).</summary>
        None  = 0,

        /// <summary>Adds 1/3 of class level (round down) to caster level (e.g. Eldritch Knight, Arcane Trickster).</summary>
        Third = 1,

        /// <summary>Adds 1/2 of class level (round down) to caster level (e.g. Paladin, Ranger).</summary>
        Half  = 2,

        /// <summary>Adds full class level to caster level (e.g. Wizard, Cleric, Druid, Bard, Sorcerer, Warlock).</summary>
        Full  = 3
    }
}
