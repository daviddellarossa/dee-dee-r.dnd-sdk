using System;
using System.Collections.Generic;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.State;

namespace DeeDeeR.DnD.Runtime.Systems
{
    /// <summary>
    /// Validates multiclass prerequisites and computes combined spell slot tables for
    /// multiclass spellcasters per D&amp;D 2024 PHB rules.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Spell slot formula:</b> Full casters contribute their full level; Half casters
    /// contribute level ÷ 2 (floor); Third casters contribute level ÷ 3 (floor).
    /// The combined effective caster level indexes into the PHB multiclass spell slot table.
    /// </para>
    /// <para>
    /// <b>Known limitation — single-class half/third casters:</b> A Paladin 5 (Half caster)
    /// has an effective level of 2 via this formula, yielding fewer slots than the Paladin's own
    /// class table. This formula is only fully accurate for true multiclass combinations.
    /// Single-class spell slot look-up will be added in Phase 9 (<c>SpellSystem</c>).
    /// </para>
    /// </remarks>
    public sealed class MulticlassSystem
    {
        // PHB 2024 Multiclass Spellcaster Spell Slot Table.
        // Index 0 = effective caster level (rows 0–20); index 1 = slot level (columns 0–9, col 0 unused).
        private static readonly int[,] SlotTable = new int[21, 10]
        {
            //         0   1   2   3   4   5   6   7   8   9
            /* 0  */ { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
            /* 1  */ { 0,  2,  0,  0,  0,  0,  0,  0,  0,  0 },
            /* 2  */ { 0,  3,  0,  0,  0,  0,  0,  0,  0,  0 },
            /* 3  */ { 0,  4,  2,  0,  0,  0,  0,  0,  0,  0 },
            /* 4  */ { 0,  4,  3,  0,  0,  0,  0,  0,  0,  0 },
            /* 5  */ { 0,  4,  3,  2,  0,  0,  0,  0,  0,  0 },
            /* 6  */ { 0,  4,  3,  3,  0,  0,  0,  0,  0,  0 },
            /* 7  */ { 0,  4,  3,  3,  1,  0,  0,  0,  0,  0 },
            /* 8  */ { 0,  4,  3,  3,  2,  0,  0,  0,  0,  0 },
            /* 9  */ { 0,  4,  3,  3,  3,  1,  0,  0,  0,  0 },
            /* 10 */ { 0,  4,  3,  3,  3,  2,  0,  0,  0,  0 },
            /* 11 */ { 0,  4,  3,  3,  3,  2,  1,  0,  0,  0 },
            /* 12 */ { 0,  4,  3,  3,  3,  2,  1,  0,  0,  0 },
            /* 13 */ { 0,  4,  3,  3,  3,  2,  1,  1,  0,  0 },
            /* 14 */ { 0,  4,  3,  3,  3,  2,  1,  1,  0,  0 },
            /* 15 */ { 0,  4,  3,  3,  3,  2,  1,  1,  1,  0 },
            /* 16 */ { 0,  4,  3,  3,  3,  2,  1,  1,  1,  0 },
            /* 17 */ { 0,  4,  3,  3,  3,  2,  1,  1,  1,  1 },
            /* 18 */ { 0,  4,  3,  3,  3,  3,  1,  1,  1,  1 },
            /* 19 */ { 0,  4,  3,  3,  3,  3,  2,  1,  1,  1 },
            /* 20 */ { 0,  4,  3,  3,  3,  3,  2,  2,  1,  1 },
        };

        /// <summary>
        /// Computes the combined spell slot state for a multiclass character using the PHB 2024
        /// multiclass spell slot table.
        /// </summary>
        /// <param name="classLevels">One entry per class the character has levels in.</param>
        /// <returns>
        /// The combined spell slot state, or <see cref="SpellSlotState.Empty"/> if no class
        /// contributes to the multiclass table (e.g. all classes are <see cref="CasterType.None"/>).
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="classLevels"/> is null.</exception>
        public SpellSlotState CalculateCombinedSpellSlots(IReadOnlyList<ClassLevel> classLevels)
        {
            if (classLevels == null) throw new ArgumentNullException(nameof(classLevels));

            int effectiveLevel = 0;
            foreach (var cl in classLevels)
            {
                if (cl?.Class == null) continue;

                effectiveLevel += cl.Class.CasterType switch
                {
                    CasterType.Full  => cl.Level,
                    CasterType.Half  => cl.Level / 2,
                    CasterType.Third => cl.Level / 3,
                    _                => 0,
                };
            }

            effectiveLevel = Math.Min(effectiveLevel, 20);

            if (effectiveLevel == 0) return SpellSlotState.Empty;

            return new SpellSlotState(
                SlotTable[effectiveLevel, 1], SlotTable[effectiveLevel, 2],
                SlotTable[effectiveLevel, 3], SlotTable[effectiveLevel, 4],
                SlotTable[effectiveLevel, 5], SlotTable[effectiveLevel, 6],
                SlotTable[effectiveLevel, 7], SlotTable[effectiveLevel, 8],
                SlotTable[effectiveLevel, 9]);
        }

        /// <summary>
        /// Checks whether a character meets the ability score prerequisites to multiclass
        /// into <paramref name="newClass"/>.
        /// </summary>
        /// <param name="record">The character's current record.</param>
        /// <param name="newClass">The class the character wants to add a level in.</param>
        /// <returns>
        /// <c>true</c> if all prerequisites are met (including when the class has no prerequisites);
        /// <c>false</c> if any prerequisite score is not met.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="record"/> or <paramref name="newClass"/> is null.
        /// </exception>
        public bool ValidateMulticlassPrerequisites(CharacterRecord record, ClassSO newClass)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (newClass == null) throw new ArgumentNullException(nameof(newClass));

            foreach (var prereq in newClass.MulticlassPrerequisites)
            {
                if (record.AbilityScores.GetScore(prereq.Ability) < prereq.MinScore)
                    return false;
            }

            return true;
        }
    }
}