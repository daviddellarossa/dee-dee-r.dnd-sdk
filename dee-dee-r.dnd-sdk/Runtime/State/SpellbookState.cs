using System;
using System.Collections.Generic;
using DeeDeeR.DnD.Runtime.Data;

namespace DeeDeeR.DnD.Runtime.State
{
    /// <summary>
    /// Tracks the spells a character knows and has prepared.
    /// Used for classes with spellbooks (Wizard) and prepared-spell casters (Cleric, Druid, Paladin).
    /// For known-spell casters (Sorcerer, Bard, Warlock), <see cref="PreparedSpells"/> holds the
    /// full known list and <see cref="KnownSpells"/> is unused.
    /// </summary>
    [Serializable]
    public sealed class SpellbookState
    {
        /// <summary>
        /// Spells inscribed in the character's spellbook (Wizards) or spells the character has
        /// learned over time. Wizards may prepare from this list each long rest; other casters
        /// leave this empty.
        /// </summary>
        public List<SpellSO> KnownSpells = new List<SpellSO>();

        /// <summary>
        /// Spells currently prepared and available to cast. For Wizards this is the daily
        /// selection from <see cref="KnownSpells"/>. For known-spell casters this holds all
        /// known spells directly.
        /// </summary>
        public List<SpellSO> PreparedSpells = new List<SpellSO>();
    }
}