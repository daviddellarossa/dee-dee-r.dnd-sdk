using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// A playable species (e.g. Human, Elf, Dwarf) per D&amp;D 2024 PHB rules.
    /// Species grant traits and languages but do <b>not</b> grant Ability Score Increases —
    /// that is the role of <see cref="BackgroundSO"/> in D&amp;D 2024.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSpecies", menuName = "DnD SDK/Species")]
    public sealed class SpeciesSO : ScriptableObject
    {
        [SerializeField] private LocalizedString _description = new LocalizedString();

        /// <summary>Flavour description of this species.</summary>
        public LocalizedString Description => _description;

        /// <summary>Typical creature size for members of this species.</summary>
        public CreatureSize Size = CreatureSize.Medium;

        /// <summary>Base walking speed in feet (typically 30).</summary>
        [Min(0)] public int BaseMovementSpeed = 30;

        /// <summary>
        /// Darkvision range in feet. Set to 0 if this species does not have darkvision.
        /// </summary>
        [Min(0)] public int DarkvisionRange;

        /// <summary>Traits granted to all members of this species.</summary>
        public List<TraitSO> Traits = new List<TraitSO>();

        /// <summary>Languages known by all members of this species.</summary>
        public List<LanguageType> Languages = new List<LanguageType>();

        /// <summary>Available subspecies choices (e.g. High Elf, Wood Elf, Drow).</summary>
        public List<SubspeciesSO> Subspecies = new List<SubspeciesSO>();
    }
}