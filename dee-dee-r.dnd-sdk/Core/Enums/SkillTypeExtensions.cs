using System;
using System.Collections.Generic;

namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Rules-constant mappings between SkillType and AbilityType as defined in D&D 2024 PHB.
    /// These mappings are fixed by the rules and never change at runtime.
    /// </summary>
    public static class SkillTypeExtensions
    {
        // Precomputed per-ability skill lists — allocated once, never again.
        private static readonly IReadOnlyList<SkillType> _strengthSkills = new[]
        {
            SkillType.Athletics
        };

        private static readonly IReadOnlyList<SkillType> _dexteritySkills = new[]
        {
            SkillType.Acrobatics,
            SkillType.SleightOfHand,
            SkillType.Stealth
        };

        private static readonly IReadOnlyList<SkillType> _constitutionSkills =
            Array.Empty<SkillType>();

        private static readonly IReadOnlyList<SkillType> _intelligenceSkills = new[]
        {
            SkillType.Arcana,
            SkillType.History,
            SkillType.Investigation,
            SkillType.Nature,
            SkillType.Religion
        };

        private static readonly IReadOnlyList<SkillType> _wisdomSkills = new[]
        {
            SkillType.AnimalHandling,
            SkillType.Insight,
            SkillType.Medicine,
            SkillType.Perception,
            SkillType.Survival
        };

        private static readonly IReadOnlyList<SkillType> _charismaSkills = new[]
        {
            SkillType.Deception,
            SkillType.Intimidation,
            SkillType.Performance,
            SkillType.Persuasion
        };

        /// <summary>
        /// Returns the ability that governs this skill (D&D 2024 PHB).
        /// </summary>
        public static AbilityType GetAbility(this SkillType skill) => skill switch
        {
            SkillType.Athletics      => AbilityType.Strength,

            SkillType.Acrobatics     => AbilityType.Dexterity,
            SkillType.SleightOfHand  => AbilityType.Dexterity,
            SkillType.Stealth        => AbilityType.Dexterity,

            SkillType.Arcana         => AbilityType.Intelligence,
            SkillType.History        => AbilityType.Intelligence,
            SkillType.Investigation  => AbilityType.Intelligence,
            SkillType.Nature         => AbilityType.Intelligence,
            SkillType.Religion       => AbilityType.Intelligence,

            SkillType.AnimalHandling => AbilityType.Wisdom,
            SkillType.Insight        => AbilityType.Wisdom,
            SkillType.Medicine       => AbilityType.Wisdom,
            SkillType.Perception     => AbilityType.Wisdom,
            SkillType.Survival       => AbilityType.Wisdom,

            SkillType.Deception      => AbilityType.Charisma,
            SkillType.Intimidation   => AbilityType.Charisma,
            SkillType.Performance    => AbilityType.Charisma,
            SkillType.Persuasion     => AbilityType.Charisma,

            _ => throw new ArgumentOutOfRangeException(nameof(skill), skill, "Unknown SkillType.")
        };

        /// <summary>
        /// Returns all skills governed by the given ability (D&D 2024 PHB).
        /// Constitution has no associated skills.
        /// </summary>
        public static IReadOnlyList<SkillType> GetSkills(this AbilityType ability) => ability switch
        {
            AbilityType.Strength     => _strengthSkills,
            AbilityType.Dexterity    => _dexteritySkills,
            AbilityType.Constitution => _constitutionSkills,
            AbilityType.Intelligence => _intelligenceSkills,
            AbilityType.Wisdom       => _wisdomSkills,
            AbilityType.Charisma     => _charismaSkills,

            _ => throw new ArgumentOutOfRangeException(nameof(ability), ability, "Unknown AbilityType.")
        };
    }
}
