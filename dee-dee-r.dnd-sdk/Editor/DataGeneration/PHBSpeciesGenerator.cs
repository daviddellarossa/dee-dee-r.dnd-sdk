using System.Collections.Generic;
using UnityEditor;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Runtime.Data;

namespace DeeDeeR.DnD.Editor.DataGeneration
{
    /// <summary>
    /// Generates all 2024 PHB species assets.
    /// In D&amp;D 2024 species grant traits and movement but no Ability Score Increases (ASIs come from backgrounds).
    /// Traits and subspecies lists are left empty — populate manually or extend in a future phase.
    /// </summary>
    internal static class PHBSpeciesGenerator
    {
        private const string Folder = PHBAssetGenerator.DataRoot + "/Species";

        internal static void Generate()
        {
            // name, size, speed, darkvision, languages
            Species("Dragonborn", CreatureSize.Medium, 30, darkvision: 0,
                new[] { LanguageType.Common, LanguageType.Draconic });

            Species("Dwarf", CreatureSize.Medium, 30, darkvision: 60,
                new[] { LanguageType.Common, LanguageType.Dwarvish });

            Species("Elf", CreatureSize.Medium, 30, darkvision: 60,
                new[] { LanguageType.Common, LanguageType.Elvish });

            Species("Gnome", CreatureSize.Small, 30, darkvision: 60,
                new[] { LanguageType.Common, LanguageType.Gnomish });

            Species("Halfling", CreatureSize.Small, 30, darkvision: 0,
                new[] { LanguageType.Common, LanguageType.Halfling });

            Species("Human", CreatureSize.Medium, 30, darkvision: 0,
                new[] { LanguageType.Common });

            Species("Orc", CreatureSize.Medium, 30, darkvision: 60,
                new[] { LanguageType.Common, LanguageType.Orc });

            Species("Tiefling", CreatureSize.Medium, 30, darkvision: 60,
                new[] { LanguageType.Common, LanguageType.Infernal });
        }

        private static void Species(
            string name, CreatureSize size, int speed, int darkvision,
            LanguageType[] languages)
        {
            var s = PHBAssetGenerator.GetOrCreate<SpeciesSO>(Folder, name);
            s.Size                = size;
            s.BaseMovementSpeed   = speed;
            s.DarkvisionRange     = darkvision;
            s.Languages           = new List<LanguageType>(languages);
            EditorUtility.SetDirty(s);
        }
    }
}
