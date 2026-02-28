using System;
using System.Collections.Generic;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.State;

namespace DeeDeeR.DnD.Runtime.Systems
{
    /// <summary>
    /// Applies a single level gain to a character's record and state, covering both
    /// advancement within an existing class and adding a new class (multiclassing).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This system mutates <see cref="CharacterRecord"/> and <see cref="CharacterState"/>
    /// in place. Unlike most SDK systems that return new value-type snapshots, level-up
    /// modifies multiple reference-type collections on these objects — returning deep copies
    /// would be impractical and is not expected by the call-site pattern.
    /// </para>
    /// <para>
    /// <b>HP at level-up:</b> Always uses the fixed average hit die result (⌊faces ÷ 2⌋ + 1)
    /// plus the character's Constitution modifier, with a minimum of 1. Maximum die rolls are
    /// only used at character creation for the first class level (handled by
    /// <see cref="CharacterFactory"/>).
    /// </para>
    /// <para>
    /// <b>Multiclassing proficiencies:</b> When adding a brand-new class, this system applies
    /// armor, weapon, and shield proficiencies from <c>ClassSO</c>. Saving throw proficiencies
    /// are <em>not</em> added for multiclass entries per D&amp;D 2024 PHB rules. Note that the
    /// PHB specifies a subset of proficiencies per new class; <c>ClassSO</c> does not currently
    /// distinguish starting vs. multiclass proficiency lists, so all armor and weapon
    /// proficiencies from the class are applied as a simplification.
    /// </para>
    /// </remarks>
    public sealed class LevelUpSystem
    {
        private readonly MulticlassSystem _multiclassSystem;

        /// <summary>Creates a new <see cref="LevelUpSystem"/>.</summary>
        public LevelUpSystem() => _multiclassSystem = new MulticlassSystem();

        /// <summary>
        /// Applies one level gain in <paramref name="classToLevel"/> to the character.
        /// </summary>
        /// <param name="record">The character's record. Modified in place.</param>
        /// <param name="state">The character's mutable state. Modified in place.</param>
        /// <param name="classToLevel">The class to gain a level in. Must not be null.</param>
        /// <param name="chosenSubclass">
        /// The subclass to adopt. Assigned the first time <see cref="LevelUp"/> is called at or
        /// above <see cref="ClassSO.SubclassLevel"/> for this class, provided no subclass has been
        /// chosen yet. Ignored if a subclass is already set or if this argument is null.
        /// </param>
        /// <param name="chosenFeat">
        /// A feat gained at this level (e.g. from an ASI/Feat choice or Epic Boon). May be null.
        /// </param>
        /// <param name="chosenASIs">
        /// Ability score increases to apply at this level. Each entry specifies an ability and
        /// the amount to increase it by (capped at 20). May be null or empty.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="record"/>, <paramref name="state"/>, or
        /// <paramref name="classToLevel"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to add a new class that the character does not meet the
        /// multiclass prerequisites for.
        /// </exception>
        public void LevelUp(
            CharacterRecord record,
            CharacterState state,
            ClassSO classToLevel,
            SubclassSO chosenSubclass = null,
            FeatSO chosenFeat = null,
            IEnumerable<AbilityScoreIncrease> chosenASIs = null)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (classToLevel == null) throw new ArgumentNullException(nameof(classToLevel));

            // Find an existing class entry or create a new one (multiclassing).
            var classLevel = record.ClassLevels.Find(cl => cl.Class == classToLevel);
            bool isNewClass = classLevel == null;

            if (isNewClass)
            {
                // Validate multiclass prerequisites when adding a second or later class.
                if (record.ClassLevels.Count > 0 &&
                    !_multiclassSystem.ValidateMulticlassPrerequisites(record, classToLevel))
                {
                    throw new InvalidOperationException(
                        $"Character does not meet the ability score prerequisites to multiclass into {classToLevel.name}.");
                }

                classLevel = new ClassLevel { Class = classToLevel, Level = 0 };
                record.ClassLevels.Add(classLevel);

                // Apply multiclass proficiencies (armor, weapon, shield — not saving throws).
                ApplyMulticlassProficiencies(record, classToLevel);
            }

            classLevel.Level++;

            // Subclass: assign if reaching the subclass-choice level and not yet chosen.
            if (classLevel.Subclass == null && chosenSubclass != null &&
                classLevel.Level >= classToLevel.SubclassLevel)
            {
                classLevel.Subclass = chosenSubclass;
            }

            // Hit dice: gain one die of the class's hit die type.
            var die = classToLevel.HitDie;
            if (!state.HitDiceAvailable.ContainsKey(die))
                state.HitDiceAvailable[die] = 0;
            state.HitDiceAvailable[die]++;

            // HP: average hit die + CON modifier (minimum 1).
            int faces  = (int)die;
            int hpGain = Math.Max(1, faces / 2 + 1 + record.AbilityScores.GetModifier(AbilityType.Constitution));
            state.HitPoints = new HitPointState(
                state.HitPoints.Current   + hpGain,
                state.HitPoints.Maximum   + hpGain,
                state.HitPoints.Temporary);

            // Ability score increases.
            if (chosenASIs != null)
            {
                foreach (var asi in chosenASIs)
                {
                    int current = record.AbilityScores.GetScore(asi.Ability);
                    record.AbilityScores = record.AbilityScores.With(
                        asi.Ability, Math.Min(20, current + asi.Amount));
                }
            }

            // Feat.
            if (chosenFeat != null)
                record.Feats.Add(chosenFeat);

            // Recalculate combined spell slots.
            state.SpellSlots = _multiclassSystem.CalculateCombinedSpellSlots(record.ClassLevels);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static void ApplyMulticlassProficiencies(CharacterRecord record, ClassSO cls)
        {
            // Per PHB 2024: multiclassing grants a limited subset of the new class's proficiencies.
            // Saving throw proficiencies are never granted to a multiclass entry.
            if (cls.ArmorProficiencies != null)
                foreach (var armor in cls.ArmorProficiencies)
                    record.ArmorProficiencies.Add(armor);

            if (cls.WeaponCategoryProficiencies != null)
                foreach (var weapon in cls.WeaponCategoryProficiencies)
                    record.WeaponCategoryProficiencies.Add(weapon);

            if (cls.HasShieldProficiency)
                record.HasShieldProficiency = true;
        }
    }
}