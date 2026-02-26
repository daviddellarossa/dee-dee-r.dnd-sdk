using System;
using System.Collections.Generic;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Core.Systems
{
    /// <summary>
    /// Computes proficiency bonuses and skill bonuses per D&amp;D 2024 PHB rules.
    /// Stateless — no dependencies; instantiate once and share across systems.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Exhaustion is not included in <see cref="GetSkillBonus"/>.</b> The d20 penalty
    /// (<see cref="ExhaustionLevel.D20Penalty"/>) is subtracted inside
    /// <see cref="AbilitySystem.RollCheck"/> so it is never applied twice.
    /// </para>
    /// <para>
    /// Proficiency set parameters use <see cref="ISet{T}"/> because character proficiency
    /// collections are sets — no duplicates, O(1) lookup. Pass a <see cref="System.Collections.Generic.HashSet{T}"/>
    /// or any other <see cref="ISet{T}"/> implementation.
    /// </para>
    /// </remarks>
    public sealed class ProficiencySystem
    {
        /// <summary>
        /// Returns the proficiency bonus for the given total character level (1–20),
        /// per the D&amp;D 2024 PHB proficiency bonus table.
        /// </summary>
        /// <param name="totalLevel">Sum of all class levels. Must be in [1, 20].</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="totalLevel"/> is less than 1 or greater than 20.
        /// </exception>
        public int GetProficiencyBonus(int totalLevel)
        {
            if (totalLevel < 1 || totalLevel > 20)
                throw new ArgumentOutOfRangeException(nameof(totalLevel),
                    $"Total level must be between 1 and 20. Got: {totalLevel}.");

            // PHB table: +2 at L1–4, +3 at L5–8, +4 at L9–12, +5 at L13–16, +6 at L17–20.
            return (totalLevel - 1) / 4 + 2;
        }

        /// <summary>
        /// Returns the total bonus added to a skill check roll: ability modifier + proficiency
        /// (doubled for expertise). Does not include the exhaustion penalty — that is applied
        /// during the d20 roll in <see cref="AbilitySystem.RollCheck"/>.
        /// </summary>
        /// <param name="scores">The creature's ability scores.</param>
        /// <param name="skill">The skill being checked.</param>
        /// <param name="isProficient">Whether the creature is proficient in this skill.</param>
        /// <param name="hasExpertise">
        /// Whether the creature has Expertise (doubles the proficiency bonus).
        /// Ignored if <paramref name="isProficient"/> is false.
        /// </param>
        /// <param name="proficiencyBonus">The creature's current proficiency bonus.</param>
        public int GetSkillBonus(
            AbilityScoreSet scores,
            SkillType       skill,
            bool            isProficient,
            bool            hasExpertise,
            int             proficiencyBonus)
        {
            int abilityMod = scores.GetModifier(skill.GetAbility());
            int profBonus  = isProficient
                ? (hasExpertise ? proficiencyBonus * 2 : proficiencyBonus)
                : 0;
            return abilityMod + profBonus;
        }

        /// <summary>Returns true if <paramref name="skill"/> is in the proficiency set.</summary>
        /// <param name="proficiencies">The character's skill proficiency set.</param>
        /// <param name="skill">The skill to test.</param>
        public bool HasSkillProficiency(ISet<SkillType> proficiencies, SkillType skill)
            => proficiencies.Contains(skill);

        /// <summary>Returns true if <paramref name="category"/> is in the armor proficiency set.</summary>
        /// <param name="proficiencies">The character's armor proficiency set.</param>
        /// <param name="category">The armor category to test.</param>
        public bool HasArmorProficiency(ISet<ArmorCategory> proficiencies, ArmorCategory category)
            => proficiencies.Contains(category);

        /// <summary>Returns true if <paramref name="category"/> is in the weapon category proficiency set.</summary>
        /// <param name="proficiencies">The character's weapon category proficiency set.</param>
        /// <param name="category">The weapon category to test.</param>
        public bool HasWeaponCategoryProficiency(ISet<WeaponCategory> proficiencies, WeaponCategory category)
            => proficiencies.Contains(category);
    }
}