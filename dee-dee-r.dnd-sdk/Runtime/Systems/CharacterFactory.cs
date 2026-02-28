using System;
using System.Collections.Generic;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.State;

namespace DeeDeeR.DnD.Runtime.Systems
{
    /// <summary>
    /// Constructs a fully initialised <see cref="CharacterRecord"/> and <see cref="CharacterState"/>
    /// pair from character-creation inputs, implementing the D&amp;D 2024 PHB character-creation
    /// pipeline (Phase 6).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>HP calculation:</b> The first level of the primary class (index 0 of
    /// <c>classLevels</c>) always grants the maximum hit die result. All other levels (including
    /// multiclass levels) use the fixed average: ⌊faces ÷ 2⌋ + 1. Each level adds at least 1 HP
    /// regardless of Constitution modifier.
    /// </para>
    /// <para>
    /// <b>D&amp;D 2024 background ASIs:</b> Ability score increases come from the background
    /// (total +2 across one or two abilities). Species does not grant ASIs.
    /// </para>
    /// </remarks>
    public sealed class CharacterFactory
    {
        private readonly MulticlassSystem _multiclassSystem;

        /// <summary>Creates a new <see cref="CharacterFactory"/>.</summary>
        public CharacterFactory() => _multiclassSystem = new MulticlassSystem();

        /// <summary>
        /// Builds a character from the supplied creation inputs.
        /// </summary>
        /// <param name="characterName">The character's name. Must be non-null and non-empty.</param>
        /// <param name="playerName">The player's name. May be null or empty.</param>
        /// <param name="species">The character's species. Must not be null.</param>
        /// <param name="subspecies">The character's subspecies, or <c>null</c> if none.</param>
        /// <param name="background">The character's background. Must not be null.</param>
        /// <param name="baseScores">
        /// Ability scores before background ASIs are applied (the raw rolled/assigned values).
        /// </param>
        /// <param name="classLevels">
        /// One entry per class in the character's build. Must have at least one entry. The first
        /// entry is treated as the primary class (determines level-1 max HP and starting
        /// proficiencies). Later entries represent multiclassed levels.
        /// </param>
        /// <param name="chosenSkillProficiencies">
        /// Skills chosen from the primary class's skill pool. May be null or empty. Background
        /// skill proficiencies are applied separately from <paramref name="background"/>.
        /// </param>
        /// <returns>
        /// A tuple of a fully initialised <see cref="CharacterRecord"/> (identity and build data)
        /// and <see cref="CharacterState"/> (mutable session state, HP at maximum, all slots full).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="characterName"/> is null/empty or
        /// <paramref name="classLevels"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="species"/> or <paramref name="background"/> is null.
        /// </exception>
        public (CharacterRecord record, CharacterState state) Build(
            string characterName,
            string playerName,
            SpeciesSO species,
            SubspeciesSO subspecies,
            BackgroundSO background,
            AbilityScoreSet baseScores,
            IReadOnlyList<ClassLevel> classLevels,
            IEnumerable<SkillType> chosenSkillProficiencies = null)
        {
            if (string.IsNullOrEmpty(characterName))
                throw new ArgumentException("Character name must not be null or empty.", nameof(characterName));
            if (species == null) throw new ArgumentNullException(nameof(species));
            if (background == null) throw new ArgumentNullException(nameof(background));
            if (classLevels == null || classLevels.Count == 0)
                throw new ArgumentException("At least one class level is required.", nameof(classLevels));
            if (classLevels[0] == null)
                throw new ArgumentException("Primary class entry (index 0) must not be null.", nameof(classLevels));
            if (classLevels[0].Class == null)
                throw new ArgumentNullException(nameof(classLevels), "Primary class entry must have a non-null Class.");
            if (classLevels[0].Level <= 0)
                throw new ArgumentException("Primary class entry must have a positive Level.", nameof(classLevels));

            var record = BuildRecord(characterName, playerName, species, subspecies, background,
                baseScores, classLevels, chosenSkillProficiencies);

            var state = BuildState(record, classLevels);

            return (record, state);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private CharacterRecord BuildRecord(
            string characterName,
            string playerName,
            SpeciesSO species,
            SubspeciesSO subspecies,
            BackgroundSO background,
            AbilityScoreSet baseScores,
            IReadOnlyList<ClassLevel> classLevels,
            IEnumerable<SkillType> chosenSkillProficiencies)
        {
            var record = new CharacterRecord
            {
                Name       = characterName,
                PlayerName = playerName ?? string.Empty,
                Species    = species,
                Subspecies = subspecies,
                Background = background,
            };

            // Ability scores: start from base scores, then apply background ASIs (D&D 2024),
            // clamping each resulting score to the standard maximum (20).
            var scores = baseScores;
            foreach (var asi in background.AbilityScoreIncreases)
                scores = scores.With(asi.Ability, Math.Min(20, scores.GetScore(asi.Ability) + asi.Amount));
            record.AbilityScores = scores;

            // Class levels list.
            record.ClassLevels = new List<ClassLevel>(classLevels);

            // Proficiencies from the primary class (full starting proficiency set).
            ApplyClassProficiencies(record, classLevels[0].Class, isPrimary: true);

            // Proficiencies from additional classes (multiclass — no saving throws per PHB 2024).
            for (int i = 1; i < classLevels.Count; i++)
            {
                if (classLevels[i]?.Class != null)
                    ApplyClassProficiencies(record, classLevels[i].Class, isPrimary: false);
            }

            // Skill proficiencies from background.
            foreach (var skill in background.SkillProficiencies)
                record.SkillProficiencies.Add(skill);

            // Skill proficiencies chosen from primary class pool.
            if (chosenSkillProficiencies != null)
                foreach (var skill in chosenSkillProficiencies)
                    record.SkillProficiencies.Add(skill);

            // Tool proficiency from background.
            if (background.ToolProficiency != null)
                record.ToolProficiencies.Add(background.ToolProficiency);

            // Languages from species and subspecies.
            foreach (var lang in species.Languages)
                record.Languages.Add(lang);
            if (subspecies != null)
                foreach (var lang in subspecies.AdditionalLanguages)
                    record.Languages.Add(lang);

            // Origin feat from background.
            if (background.OriginFeat != null)
                record.Feats.Add(background.OriginFeat);

            return record;
        }

        private static void ApplyClassProficiencies(CharacterRecord record, ClassSO cls, bool isPrimary)
        {
            // Saving throws: only granted by the primary (starting) class.
            if (isPrimary && cls.SavingThrowProficiencies != null)
                foreach (var save in cls.SavingThrowProficiencies)
                    record.SavingThrowProficiencies.Add(save);

            if (cls.ArmorProficiencies != null)
                foreach (var armor in cls.ArmorProficiencies)
                    record.ArmorProficiencies.Add(armor);

            if (cls.WeaponCategoryProficiencies != null)
                foreach (var weapon in cls.WeaponCategoryProficiencies)
                    record.WeaponCategoryProficiencies.Add(weapon);

            if (cls.HasShieldProficiency)
                record.HasShieldProficiency = true;

            // Tool proficiencies are only granted by the primary (starting) class.
            // ClassSO has no separate multiclass proficiency subset, so applying the full
            // tool list for multiclass entries would over-grant. Some classes do grant a
            // specific tool on multiclassing (e.g. Bard: instrument, Rogue: thieves' tools),
            // but that granularity requires a future ClassSO field.
            if (isPrimary && cls.ToolProficiencies != null)
                foreach (var tool in cls.ToolProficiencies)
                    record.ToolProficiencies.Add(tool);
        }

        private CharacterState BuildState(CharacterRecord record, IReadOnlyList<ClassLevel> classLevels)
        {
            var state = new CharacterState();

            // Hit dice available (one die per class level, grouped by die type).
            foreach (var cl in classLevels)
            {
                if (cl?.Class == null) continue;
                var die = cl.Class.HitDie;
                if (!state.HitDiceAvailable.ContainsKey(die))
                    state.HitDiceAvailable[die] = 0;
                state.HitDiceAvailable[die] += cl.Level;
            }

            // Hit points: max die at level 1 (primary class), average at all other levels.
            int totalHp = CalculateTotalHp(classLevels, record.AbilityScores);
            state.HitPoints = new HitPointState(totalHp, totalHp);

            // Spell slots (multiclass combined table).
            state.SpellSlots = _multiclassSystem.CalculateCombinedSpellSlots(classLevels);

            return state;
        }

        private static int CalculateTotalHp(IReadOnlyList<ClassLevel> classLevels, AbilityScoreSet scores)
        {
            int conMod = scores.GetModifier(AbilityType.Constitution);
            int totalHp = 0;
            bool isFirstLevel = true;

            foreach (var cl in classLevels)
            {
                if (cl?.Class == null) continue;

                int faces = (int)cl.Class.HitDie;
                int average = faces / 2 + 1;

                for (int i = 0; i < cl.Level; i++)
                {
                    // Level 1 of the primary class always rolls max.
                    int roll = isFirstLevel ? faces : average;
                    isFirstLevel = false;
                    totalHp += Math.Max(1, roll + conMod);
                }
            }

            return totalHp;
        }
    }
}