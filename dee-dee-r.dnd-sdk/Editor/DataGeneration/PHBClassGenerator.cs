using System.Collections.Generic;
using UnityEditor;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Runtime.Data;

namespace DeeDeeR.DnD.Editor.DataGeneration
{
    /// <summary>
    /// Generates all 12 PHB class assets and their subclasses.
    /// Mechanical data (hit die, saving throws, proficiencies, caster type) from D&amp;D 2024 PHB.
    /// Class features (LocalizedString) are left empty — populate manually in the Unity Editor.
    /// </summary>
    internal static class PHBClassGenerator
    {
        private const string ClassFolder    = PHBAssetGenerator.DataRoot + "/Classes";
        private const string SubclassFolder = PHBAssetGenerator.DataRoot + "/Subclasses";

        // Ability shorthands
        private const AbilityType STR = AbilityType.Strength;
        private const AbilityType DEX = AbilityType.Dexterity;
        private const AbilityType CON = AbilityType.Constitution;
        private const AbilityType INT = AbilityType.Intelligence;
        private const AbilityType WIS = AbilityType.Wisdom;
        private const AbilityType CHA = AbilityType.Charisma;

        internal static void GenerateClasses()
        {
            // Barbarian ──────────────────────────────────────────────────────
            Class("Barbarian",
                hitDie:    DieType.D12,
                primary:   new[] { STR },
                saves:     new[] { STR, CON },
                armor:     new[] { ArmorCategory.Light, ArmorCategory.Medium },
                weapons:   new[] { WeaponCategory.Simple, WeaponCategory.Martial },
                shield:    true,
                caster:    CasterType.None,
                castAbility: CHA,
                subclassLv: 3,
                skillCount: 2,
                skills: new[] { SkillType.AnimalHandling, SkillType.Athletics,
                                SkillType.Intimidation,  SkillType.Nature,
                                SkillType.Perception,    SkillType.Survival },
                prereqs: Prereqs((STR, 13)));

            // Bard ────────────────────────────────────────────────────────────
            Class("Bard",
                hitDie:    DieType.D8,
                primary:   new[] { CHA },
                saves:     new[] { DEX, CHA },
                armor:     new[] { ArmorCategory.Light },
                weapons:   new[] { WeaponCategory.Simple },
                shield:    false,
                caster:    CasterType.Full,
                castAbility: CHA,
                subclassLv: 3,
                skillCount: 3,
                skills: new[] {
                    SkillType.Acrobatics, SkillType.AnimalHandling, SkillType.Arcana,
                    SkillType.Athletics, SkillType.Deception, SkillType.History,
                    SkillType.Insight, SkillType.Intimidation, SkillType.Investigation,
                    SkillType.Medicine, SkillType.Nature, SkillType.Perception,
                    SkillType.Performance, SkillType.Persuasion, SkillType.Religion,
                    SkillType.SleightOfHand, SkillType.Stealth, SkillType.Survival },
                prereqs: Prereqs((CHA, 13)));

            // Cleric ──────────────────────────────────────────────────────────
            Class("Cleric",
                hitDie:    DieType.D8,
                primary:   new[] { WIS },
                saves:     new[] { WIS, CHA },
                armor:     new[] { ArmorCategory.Light, ArmorCategory.Medium },
                weapons:   new[] { WeaponCategory.Simple },
                shield:    true,
                caster:    CasterType.Full,
                castAbility: WIS,
                subclassLv: 1,
                skillCount: 2,
                skills: new[] { SkillType.History, SkillType.Insight,
                                SkillType.Medicine, SkillType.Persuasion, SkillType.Religion },
                prereqs: Prereqs((WIS, 13)));

            // Druid ───────────────────────────────────────────────────────────
            Class("Druid",
                hitDie:    DieType.D8,
                primary:   new[] { WIS },
                saves:     new[] { INT, WIS },
                armor:     new[] { ArmorCategory.Light, ArmorCategory.Medium },
                weapons:   new[] { WeaponCategory.Simple },
                shield:    true,
                caster:    CasterType.Full,
                castAbility: WIS,
                subclassLv: 2,
                skillCount: 2,
                skills: new[] { SkillType.Arcana, SkillType.AnimalHandling,
                                SkillType.Insight, SkillType.Medicine,
                                SkillType.Nature, SkillType.Perception,
                                SkillType.Religion, SkillType.Survival },
                prereqs: Prereqs((WIS, 13)));

            // Fighter ─────────────────────────────────────────────────────────
            Class("Fighter",
                hitDie:    DieType.D10,
                primary:   new[] { STR, DEX },
                saves:     new[] { STR, CON },
                armor:     new[] { ArmorCategory.Light, ArmorCategory.Medium, ArmorCategory.Heavy },
                weapons:   new[] { WeaponCategory.Simple, WeaponCategory.Martial },
                shield:    true,
                caster:    CasterType.None,
                castAbility: INT,
                subclassLv: 3,
                skillCount: 2,
                skills: new[] { SkillType.Acrobatics, SkillType.AnimalHandling,
                                SkillType.Athletics,  SkillType.History,
                                SkillType.Insight,    SkillType.Intimidation,
                                SkillType.Perception, SkillType.Survival },
                prereqs: Prereqs((STR, 13), (DEX, 13)));

            // Monk ────────────────────────────────────────────────────────────
            Class("Monk",
                hitDie:    DieType.D8,
                primary:   new[] { STR, DEX, WIS },
                saves:     new[] { STR, DEX },
                armor:     new ArmorCategory[0],
                weapons:   new[] { WeaponCategory.Simple },
                shield:    false,
                caster:    CasterType.None,
                castAbility: WIS,
                subclassLv: 3,
                skillCount: 2,
                skills: new[] { SkillType.Acrobatics, SkillType.Athletics,
                                SkillType.History,    SkillType.Insight,
                                SkillType.Religion,   SkillType.Stealth },
                prereqs: Prereqs((DEX, 13), (WIS, 13)));

            // Paladin ─────────────────────────────────────────────────────────
            Class("Paladin",
                hitDie:    DieType.D10,
                primary:   new[] { STR, CHA },
                saves:     new[] { WIS, CHA },
                armor:     new[] { ArmorCategory.Light, ArmorCategory.Medium, ArmorCategory.Heavy },
                weapons:   new[] { WeaponCategory.Simple, WeaponCategory.Martial },
                shield:    true,
                caster:    CasterType.Half,
                castAbility: CHA,
                subclassLv: 3,
                skillCount: 2,
                skills: new[] { SkillType.Athletics,   SkillType.Insight,
                                SkillType.Intimidation, SkillType.Medicine,
                                SkillType.Persuasion,   SkillType.Religion },
                prereqs: Prereqs((STR, 13), (CHA, 13)));

            // Ranger ──────────────────────────────────────────────────────────
            Class("Ranger",
                hitDie:    DieType.D10,
                primary:   new[] { DEX, WIS },
                saves:     new[] { STR, DEX },
                armor:     new[] { ArmorCategory.Light, ArmorCategory.Medium },
                weapons:   new[] { WeaponCategory.Simple, WeaponCategory.Martial },
                shield:    true,
                caster:    CasterType.Half,
                castAbility: WIS,
                subclassLv: 3,
                skillCount: 3,
                skills: new[] { SkillType.AnimalHandling, SkillType.Athletics,
                                SkillType.Insight,        SkillType.Investigation,
                                SkillType.Nature,         SkillType.Perception,
                                SkillType.Stealth,        SkillType.Survival },
                prereqs: Prereqs((DEX, 13), (WIS, 13)));

            // Rogue ───────────────────────────────────────────────────────────
            Class("Rogue",
                hitDie:    DieType.D8,
                primary:   new[] { DEX },
                saves:     new[] { DEX, INT },
                armor:     new[] { ArmorCategory.Light },
                weapons:   new[] { WeaponCategory.Simple },
                shield:    false,
                caster:    CasterType.None,
                castAbility: INT,
                subclassLv: 3,
                skillCount: 4,
                skills: new[] {
                    SkillType.Acrobatics, SkillType.Athletics,   SkillType.Deception,
                    SkillType.Insight,    SkillType.Intimidation, SkillType.Investigation,
                    SkillType.Perception, SkillType.Performance,  SkillType.Persuasion,
                    SkillType.SleightOfHand, SkillType.Stealth },
                prereqs: Prereqs((DEX, 13)));

            // Sorcerer ────────────────────────────────────────────────────────
            Class("Sorcerer",
                hitDie:    DieType.D6,
                primary:   new[] { CHA },
                saves:     new[] { CON, CHA },
                armor:     new ArmorCategory[0],
                weapons:   new[] { WeaponCategory.Simple },
                shield:    false,
                caster:    CasterType.Full,
                castAbility: CHA,
                subclassLv: 1,
                skillCount: 2,
                skills: new[] { SkillType.Arcana,       SkillType.Deception,
                                SkillType.Insight,      SkillType.Intimidation,
                                SkillType.Persuasion,   SkillType.Religion },
                prereqs: Prereqs((CHA, 13)));

            // Warlock ─────────────────────────────────────────────────────────
            // Warlock uses Pact Magic (unique slot system), not the standard multiclass table.
            Class("Warlock",
                hitDie:    DieType.D8,
                primary:   new[] { CHA },
                saves:     new[] { WIS, CHA },
                armor:     new[] { ArmorCategory.Light },
                weapons:   new[] { WeaponCategory.Simple },
                shield:    false,
                caster:    CasterType.None,
                castAbility: CHA,
                subclassLv: 1,
                skillCount: 2,
                skills: new[] { SkillType.Arcana,       SkillType.Deception,
                                SkillType.History,      SkillType.Intimidation,
                                SkillType.Investigation, SkillType.Nature,
                                SkillType.Religion },
                prereqs: Prereqs((CHA, 13)));

            // Wizard ──────────────────────────────────────────────────────────
            Class("Wizard",
                hitDie:    DieType.D6,
                primary:   new[] { INT },
                saves:     new[] { INT, WIS },
                armor:     new ArmorCategory[0],
                weapons:   new[] { WeaponCategory.Simple },
                shield:    false,
                caster:    CasterType.Full,
                castAbility: INT,
                subclassLv: 2,
                skillCount: 2,
                skills: new[] { SkillType.Arcana,     SkillType.History,
                                SkillType.Insight,    SkillType.Investigation,
                                SkillType.Medicine,   SkillType.Religion },
                prereqs: Prereqs((INT, 13)));
        }

        // Second pass — must run after GenerateClasses() so parent class assets exist.
        internal static void GenerateSubclasses()
        {
            // Barbarian Primal Paths
            Sub("Berserker",         "Barbarian");
            Sub("Wild Heart",        "Barbarian");
            Sub("World Tree",        "Barbarian");
            Sub("Zealot",            "Barbarian");

            // Bard Colleges
            Sub("College of Dance",  "Bard");
            Sub("College of Glamour","Bard");
            Sub("College of Lore",   "Bard");
            Sub("College of Valor",  "Bard");

            // Cleric Divine Orders / Domains
            Sub("Life Domain",       "Cleric");
            Sub("Light Domain",      "Cleric");
            Sub("Trickery Domain",   "Cleric");
            Sub("War Domain",        "Cleric");

            // Druid Circles
            Sub("Circle of the Land","Druid");
            Sub("Circle of the Moon","Druid");
            Sub("Circle of the Sea", "Druid");
            Sub("Circle of Stars",   "Druid");

            // Fighter Martial Archetypes
            Sub("Battle Master",     "Fighter");
            Sub("Champion",          "Fighter");
            Sub("Eldritch Knight",   "Fighter");
            Sub("Psi Warrior",       "Fighter");

            // Monk Monastic Traditions
            Sub("Warrior of the Elements", "Monk");
            Sub("Warrior of the Open Hand","Monk");
            Sub("Warrior of Shadow",       "Monk");
            Sub("Warrior of Mercy",        "Monk");

            // Paladin Sacred Oaths
            Sub("Oath of Ancients", "Paladin");
            Sub("Oath of Devotion", "Paladin");
            Sub("Oath of Glory",    "Paladin");
            Sub("Oath of Vengeance","Paladin");

            // Ranger Archetypes
            Sub("Beast Master",     "Ranger");
            Sub("Fey Wanderer",     "Ranger");
            Sub("Gloom Stalker",    "Ranger");
            Sub("Hunter",           "Ranger");

            // Rogue Archetypes
            Sub("Arcane Trickster", "Rogue");
            Sub("Assassin",         "Rogue");
            Sub("Soulknife",        "Rogue");
            Sub("Thief",            "Rogue");

            // Sorcerer Origins
            Sub("Aberrant Mind",      "Sorcerer");
            Sub("Clockwork Soul",     "Sorcerer");
            Sub("Draconic Bloodline", "Sorcerer");
            Sub("Wild Magic",         "Sorcerer");

            // Warlock Patrons
            Sub("Archfey",           "Warlock");
            Sub("Celestial",         "Warlock");
            Sub("Fiend",             "Warlock");
            Sub("Great Old One",     "Warlock");

            // Wizard Arcane Traditions
            Sub("Abjurer",    "Wizard");
            Sub("Diviner",    "Wizard");
            Sub("Evoker",     "Wizard");
            Sub("Illusionist","Wizard");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void Class(
            string name,
            DieType hitDie,
            AbilityType[] primary,
            AbilityType[] saves,
            ArmorCategory[] armor,
            WeaponCategory[] weapons,
            bool shield,
            CasterType caster,
            AbilityType castAbility,
            int subclassLv,
            int skillCount,
            SkillType[] skills,
            List<AbilityPrerequisite> prereqs)
        {
            var c = PHBAssetGenerator.GetOrCreate<ClassSO>(ClassFolder, name);
            c.HitDie                      = hitDie;
            c.PrimaryAbilities            = primary;
            c.SavingThrowProficiencies    = saves;
            c.ArmorProficiencies          = armor;
            c.WeaponCategoryProficiencies = weapons;
            c.HasShieldProficiency        = shield;
            c.CasterType                  = caster;
            c.SpellcastingAbility         = castAbility;
            c.SubclassLevel               = subclassLv;
            c.SkillChoices                = new SkillChoiceOptions { Pool = skills, Count = skillCount };
            c.MulticlassPrerequisites     = prereqs;
            EditorUtility.SetDirty(c);
        }

        private static void Sub(string subName, string parentName)
        {
            var parent = PHBAssetGenerator.Load<ClassSO>(ClassFolder, parentName);
            if (parent == null)
            {
                UnityEngine.Debug.LogWarning($"[PHBClassGenerator] Parent class '{parentName}' not found for subclass '{subName}'.");
                return;
            }
            var s = PHBAssetGenerator.GetOrCreate<SubclassSO>(SubclassFolder, subName);
            s.ParentClass = parent;
            EditorUtility.SetDirty(s);
        }

        private static List<AbilityPrerequisite> Prereqs(params (AbilityType Ability, int Score)[] pairs)
        {
            var list = new List<AbilityPrerequisite>();
            foreach (var (a, score) in pairs)
                list.Add(new AbilityPrerequisite { Ability = a, MinScore = score });
            return list;
        }
    }
}
