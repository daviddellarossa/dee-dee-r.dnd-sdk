using System.Collections.Generic;
using UnityEngine;
using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// A character class definition (Fighter, Wizard, Rogue, etc.) per D&amp;D 2024 PHB rules.
    /// This is a data asset only — no game logic. Class-level advancement, multiclassing
    /// validation, and spell slot calculation are handled by the systems layer (Phase 6+).
    /// </summary>
    [CreateAssetMenu(fileName = "NewClass", menuName = "DnD SDK/Class")]
    public sealed class ClassSO : ScriptableObject
    {
        // ── Core ──────────────────────────────────────────────────────────────

        /// <summary>Hit die for this class (D6, D8, D10, or D12).</summary>
        public DieType HitDie = DieType.D8;

        /// <summary>Primary ability scores for this class (used for multiclass prerequisites).</summary>
        public AbilityType[] PrimaryAbilities;

        // ── Proficiencies ──────────────────────────────────────────────────────

        /// <summary>The two saving throw proficiencies granted by this class.</summary>
        public AbilityType[] SavingThrowProficiencies = new AbilityType[2];

        /// <summary>Armour categories this class is proficient with.</summary>
        public ArmorCategory[] ArmorProficiencies;

        /// <summary>Weapon categories this class is proficient with.</summary>
        public WeaponCategory[] WeaponCategoryProficiencies;

        /// <summary>Whether this class grants shield proficiency.</summary>
        public bool HasShieldProficiency;

        /// <summary>Tool types this class is proficient with.</summary>
        public ToolSO[] ToolProficiencies;

        /// <summary>
        /// Skill proficiency choices offered during character creation.
        /// Specifies the allowed pool and how many skills may be chosen.
        /// </summary>
        public SkillChoiceOptions SkillChoices;

        // ── Spellcasting ───────────────────────────────────────────────────────

        /// <summary>
        /// Whether this class has spellcasting and how it contributes to the multiclass
        /// spell slot table (None, Third, Half, Full).
        /// </summary>
        public CasterType CasterType;

        /// <summary>
        /// The ability score used for spellcasting (e.g. Intelligence for Wizard,
        /// Wisdom for Cleric). Ignored when <see cref="CasterType"/> is None.
        /// </summary>
        public AbilityType SpellcastingAbility;

        // ── Multiclassing ──────────────────────────────────────────────────────

        /// <summary>
        /// Minimum ability score prerequisites that must be met to multiclass into this class.
        /// <see cref="MulticlassSystem"/> validates these before allowing a new class level.
        /// </summary>
        public List<AbilityPrerequisite> MulticlassPrerequisites = new List<AbilityPrerequisite>();

        // ── Subclass ───────────────────────────────────────────────────────────

        /// <summary>
        /// The class level at which the character chooses a subclass.
        /// Varies by class (1 for Cleric and Sorcerer, 2 for Fighter, 3 for Rogue and Wizard).
        /// </summary>
        [Range(1, 3)] public int SubclassLevel = 3;

        // ── Features ──────────────────────────────────────────────────────────

        /// <summary>
        /// All class features, each tagged with the level at which they are gained.
        /// Subclass features are listed on <see cref="SubclassSO.Features"/> instead.
        /// </summary>
        public List<ClassFeatureEntry> Features = new List<ClassFeatureEntry>();

        // ── Starting Equipment ─────────────────────────────────────────────────

        /// <summary>
        /// Default starting equipment for characters of this class.
        /// The PHB often presents equipment as a choice between options; this field captures
        /// one standard loadout. Choice logic can be expanded in a future phase.
        /// </summary>
        public List<ItemGrant> StartingEquipment = new List<ItemGrant>();
    }
}