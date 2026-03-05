using System.Collections.Generic;
using UnityEditor;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Runtime.Data;

namespace DeeDeeR.DnD.Editor.DataGeneration
{
    /// <summary>
    /// Generates all 16 D&amp;D 2024 PHB background assets.
    /// Each background grants +2 to one ability and +1 to another, plus two skill proficiencies.
    /// Tool proficiency and origin feat references are left null — wire up in the Unity Editor.
    /// </summary>
    internal static class PHBBackgroundGenerator
    {
        private const string Folder = PHBAssetGenerator.DataRoot + "/Backgrounds";

        // Ability shorthands
        private const AbilityType STR = AbilityType.Strength;
        private const AbilityType DEX = AbilityType.Dexterity;
        private const AbilityType CON = AbilityType.Constitution;
        private const AbilityType INT = AbilityType.Intelligence;
        private const AbilityType WIS = AbilityType.Wisdom;
        private const AbilityType CHA = AbilityType.Charisma;

        internal static void Generate()
        {
            // Background(name, +2 ability, +1 ability, skill1, skill2)
            Bg("Acolyte",    WIS, INT, SkillType.Insight,        SkillType.Religion);
            Bg("Artisan",    INT, CHA, SkillType.Investigation,  SkillType.Persuasion);
            Bg("Charlatan",  CHA, DEX, SkillType.Deception,      SkillType.SleightOfHand);
            Bg("Criminal",   DEX, INT, SkillType.Deception,      SkillType.Stealth);
            Bg("Entertainer",DEX, CHA, SkillType.Acrobatics,     SkillType.Performance);
            Bg("Farmer",     CON, WIS, SkillType.AnimalHandling, SkillType.Nature);
            Bg("Guard",      STR, WIS, SkillType.Athletics,      SkillType.Perception);
            Bg("Guide",      WIS, DEX, SkillType.Stealth,        SkillType.Survival);
            Bg("Hermit",     WIS, CON, SkillType.Medicine,       SkillType.Religion);
            Bg("Merchant",   CHA, INT, SkillType.AnimalHandling, SkillType.Persuasion);
            Bg("Noble",      CHA, INT, SkillType.History,        SkillType.Persuasion);
            Bg("Sage",       INT, WIS, SkillType.Arcana,         SkillType.History);
            Bg("Sailor",     DEX, WIS, SkillType.Acrobatics,     SkillType.Perception);
            Bg("Scribe",     INT, DEX, SkillType.Investigation,  SkillType.Perception);
            Bg("Soldier",    STR, CON, SkillType.Athletics,      SkillType.Intimidation);
            Bg("Wayfarer",   WIS, DEX, SkillType.Insight,        SkillType.Stealth);
        }

        private static void Bg(
            string name,
            AbilityType plus2, AbilityType plus1,
            SkillType skill1, SkillType skill2)
        {
            var b = PHBAssetGenerator.GetOrCreate<BackgroundSO>(Folder, name);
            b.AbilityScoreIncreases = new List<AbilityScoreIncrease>
            {
                new AbilityScoreIncrease { Ability = plus2, Amount = 2 },
                new AbilityScoreIncrease { Ability = plus1, Amount = 1 },
            };
            b.SkillProficiencies = new[] { skill1, skill2 };
            EditorUtility.SetDirty(b);
        }
    }
}
