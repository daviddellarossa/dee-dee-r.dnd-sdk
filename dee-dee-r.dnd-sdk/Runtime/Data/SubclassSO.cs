using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// A subclass (archetype) for a specific parent class (e.g. Evocation for Wizard,
    /// Battle Master for Fighter). Chosen at the level defined by
    /// <see cref="ClassSO.SubclassLevel"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSubclass", menuName = "DnD SDK/Subclass")]
    public sealed class SubclassSO : ScriptableObject
    {
        /// <summary>The class this subclass belongs to.</summary>
        public ClassSO ParentClass;

        [SerializeField] private LocalizedString _description = new LocalizedString();

        /// <summary>Flavour description of this subclass.</summary>
        public LocalizedString Description => _description;

        /// <summary>
        /// Expanded spell list granted by this subclass. Applicable to subclasses such as
        /// Cleric domains and Paladin oaths that add spells to the class spell list.
        /// Empty for subclasses that do not expand the spell list.
        /// </summary>
        public SpellSO[] SpellList;

        /// <summary>
        /// Weapon masteries unlocked by this subclass, if any.
        /// Most subclasses leave this empty; some martial subclasses add extra mastery options.
        /// </summary>
        public WeaponMastery[] GrantedWeaponMasteries;

        /// <summary>
        /// Features granted by this subclass, each tagged with the class level at which
        /// they are gained. Combined with <see cref="ClassSO.Features"/> during level-up.
        /// </summary>
        public List<ClassFeatureEntry> Features = new List<ClassFeatureEntry>();
    }
}