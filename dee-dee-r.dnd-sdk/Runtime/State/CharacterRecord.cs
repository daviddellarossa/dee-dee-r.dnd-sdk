using System;
using System.Collections.Generic;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;

namespace DeeDeeR.DnD.Runtime.State
{
    /// <summary>
    /// Semi-static identity data for a character — information that changes rarely and defines
    /// who the character is rather than their current condition. Constructed by
    /// <c>CharacterFactory</c> (Phase 6) and updated by <c>LevelUpSystem</c>.
    /// </summary>
    /// <remarks>
    /// Mutable fields (HP, conditions, spell slots) live in <see cref="CharacterState"/>.
    /// Inventory lives in <see cref="InventoryState"/>.
    /// </remarks>
    [Serializable]
    public sealed class CharacterRecord
    {
        // ── Identity ──────────────────────────────────────────────────────────

        /// <summary>The character's name.</summary>
        public string Name;

        /// <summary>The name of the player controlling this character.</summary>
        public string PlayerName;

        /// <summary>The character's alignment.</summary>
        public Alignment Alignment;

        // ── Origin ────────────────────────────────────────────────────────────

        /// <summary>The character's species (e.g. Elf, Dwarf).</summary>
        public SpeciesSO Species;

        /// <summary>
        /// The character's subspecies (e.g. High Elf). <c>null</c> if the species has no
        /// subspecies or none was chosen.
        /// </summary>
        public SubspeciesSO Subspecies;

        /// <summary>
        /// The character's background (e.g. Acolyte, Soldier). Determines ASIs, origin feat,
        /// skill and tool proficiencies, and starting equipment per D&amp;D 2024 rules.
        /// </summary>
        public BackgroundSO Background;

        // ── Class / Level ─────────────────────────────────────────────────────

        /// <summary>
        /// One entry per class the character has taken levels in. Single-classed characters
        /// have exactly one entry; multiclass characters have one per class.
        /// </summary>
        public List<ClassLevel> ClassLevels = new List<ClassLevel>();

        // ── Ability Scores ────────────────────────────────────────────────────

        /// <summary>
        /// The character's ability scores after all increases from background, feats, and
        /// level-up ASI choices have been applied. Systems read scores directly from this set.
        /// </summary>
        public AbilityScoreSet AbilityScores;

        // ── Proficiencies ─────────────────────────────────────────────────────

        /// <summary>Saving throws the character is proficient in (granted by starting class).</summary>
        public HashSet<AbilityType> SavingThrowProficiencies = new HashSet<AbilityType>();

        /// <summary>Skills the character is proficient in.</summary>
        public HashSet<SkillType> SkillProficiencies = new HashSet<SkillType>();

        /// <summary>
        /// Skills the character has expertise in (proficiency bonus doubled).
        /// A skill in this set must also be in <see cref="SkillProficiencies"/>.
        /// </summary>
        public HashSet<SkillType> SkillExpertise = new HashSet<SkillType>();

        /// <summary>Weapon categories the character is proficient with (Simple and/or Martial).</summary>
        public HashSet<WeaponCategory> WeaponCategoryProficiencies = new HashSet<WeaponCategory>();

        /// <summary>Armour categories the character is proficient with.</summary>
        public HashSet<ArmorCategory> ArmorProficiencies = new HashSet<ArmorCategory>();

        /// <summary>
        /// Whether the character is proficient with shields. Tracked separately from
        /// <see cref="ArmorProficiencies"/> as shields are a distinct equipment slot.
        /// </summary>
        public bool HasShieldProficiency;

        /// <summary>Tools the character is proficient with.</summary>
        public List<ToolSO> ToolProficiencies = new List<ToolSO>();

        // ── Feats ─────────────────────────────────────────────────────────────

        /// <summary>
        /// All feats the character has acquired (from background, level-up ASI replacements,
        /// and Epic Boon at level 19+).
        /// </summary>
        public List<FeatSO> Feats = new List<FeatSO>();

        // ── Languages ─────────────────────────────────────────────────────────

        /// <summary>Languages the character can speak, read, and write.</summary>
        public HashSet<LanguageType> Languages = new HashSet<LanguageType>();

        // ── Personality ───────────────────────────────────────────────────────

        /// <summary>
        /// Player-authored personality traits describing how the character behaves.
        /// Plain text — not localised game content.
        /// </summary>
        public string PersonalityTraits;

        /// <summary>Player-authored ideals the character believes in.</summary>
        public string Ideals;

        /// <summary>Player-authored bonds that tie the character to the world.</summary>
        public string Bonds;

        /// <summary>Player-authored flaws or weaknesses the character has.</summary>
        public string Flaws;
    }
}