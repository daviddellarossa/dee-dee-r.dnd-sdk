using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// A subspecies option within a parent species (e.g. High Elf within Elf).
    /// Grants additional traits and languages on top of those from <see cref="ParentSpecies"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSubspecies", menuName = "DnD SDK/Subspecies")]
    public sealed class SubspeciesSO : ScriptableObject
    {
        /// <summary>The species this subspecies belongs to.</summary>
        public SpeciesSO ParentSpecies;

        [SerializeField] private LocalizedString _description = new LocalizedString();

        /// <summary>Flavour description of this subspecies.</summary>
        public LocalizedString Description => _description;

        /// <summary>Additional traits granted by this subspecies.</summary>
        public List<TraitSO> AdditionalTraits = new List<TraitSO>();

        /// <summary>Additional languages granted by this subspecies.</summary>
        public List<LanguageType> AdditionalLanguages = new List<LanguageType>();
    }
}