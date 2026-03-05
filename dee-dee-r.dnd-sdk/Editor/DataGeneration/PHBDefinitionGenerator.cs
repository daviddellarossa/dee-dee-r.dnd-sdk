using UnityEditor;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Runtime.Data.Definitions;

namespace DeeDeeR.DnD.Editor.DataGeneration
{
    /// <summary>
    /// Generates one companion Definition SO asset per enum value for every
    /// enum that has a companion SO type (ability, skill, damage type, condition, etc.).
    /// LocalizedString display fields are left empty — wire up in the Unity Editor.
    /// </summary>
    internal static class PHBDefinitionGenerator
    {
        private const string DefinitionRoot = PHBAssetGenerator.DataRoot + "/Definitions";

        internal static void Generate()
        {
            PHBAssetGenerator.EnsureFolder(PHBAssetGenerator.DataRoot, "Definitions");
            PHBAssetGenerator.EnsureFolder(DefinitionRoot, "Abilities");
            PHBAssetGenerator.EnsureFolder(DefinitionRoot, "Skills");
            PHBAssetGenerator.EnsureFolder(DefinitionRoot, "DamageTypes");
            PHBAssetGenerator.EnsureFolder(DefinitionRoot, "Conditions");
            PHBAssetGenerator.EnsureFolder(DefinitionRoot, "WeaponMasteries");
            PHBAssetGenerator.EnsureFolder(DefinitionRoot, "WeaponCategories");
            PHBAssetGenerator.EnsureFolder(DefinitionRoot, "SpellSchools");
            PHBAssetGenerator.EnsureFolder(DefinitionRoot, "CurrencyTypes");
            PHBAssetGenerator.EnsureFolder(DefinitionRoot, "Languages");
            PHBAssetGenerator.EnsureFolder(DefinitionRoot, "Dice");

            GenerateAbilities();
            GenerateSkills();
            GenerateDamageTypes();
            GenerateConditions();
            GenerateWeaponMasteries();
            GenerateWeaponCategories();
            GenerateSpellSchools();
            GenerateCurrencyTypes();
            GenerateLanguages();
            GenerateDice();
        }

        // ── Abilities ─────────────────────────────────────────────────────────

        private static void GenerateAbilities()
        {
            var folder = DefinitionRoot + "/Abilities";
            Ability(folder, AbilityType.Strength);
            Ability(folder, AbilityType.Dexterity);
            Ability(folder, AbilityType.Constitution);
            Ability(folder, AbilityType.Intelligence);
            Ability(folder, AbilityType.Wisdom);
            Ability(folder, AbilityType.Charisma);
        }

        private static void Ability(string folder, AbilityType type)
        {
            var so = PHBAssetGenerator.GetOrCreate<AbilityDefinitionSO>(folder, type.ToString());
            so.Type = type;
            EditorUtility.SetDirty(so);
        }

        // ── Skills ────────────────────────────────────────────────────────────

        private static void GenerateSkills()
        {
            var folder = DefinitionRoot + "/Skills";
            Skill(folder, SkillType.Athletics);
            Skill(folder, SkillType.Acrobatics);
            Skill(folder, SkillType.SleightOfHand);
            Skill(folder, SkillType.Stealth);
            Skill(folder, SkillType.Arcana);
            Skill(folder, SkillType.History);
            Skill(folder, SkillType.Investigation);
            Skill(folder, SkillType.Nature);
            Skill(folder, SkillType.Religion);
            Skill(folder, SkillType.AnimalHandling);
            Skill(folder, SkillType.Insight);
            Skill(folder, SkillType.Medicine);
            Skill(folder, SkillType.Perception);
            Skill(folder, SkillType.Survival);
            Skill(folder, SkillType.Deception);
            Skill(folder, SkillType.Intimidation);
            Skill(folder, SkillType.Performance);
            Skill(folder, SkillType.Persuasion);
        }

        private static void Skill(string folder, SkillType type)
        {
            var so = PHBAssetGenerator.GetOrCreate<SkillDefinitionSO>(folder, type.ToString());
            so.Type = type;
            EditorUtility.SetDirty(so);
        }

        // ── Damage Types ──────────────────────────────────────────────────────

        private static void GenerateDamageTypes()
        {
            var folder = DefinitionRoot + "/DamageTypes";
            DamageTypeDef(folder, DamageType.Acid);
            DamageTypeDef(folder, DamageType.Bludgeoning);
            DamageTypeDef(folder, DamageType.Cold);
            DamageTypeDef(folder, DamageType.Fire);
            DamageTypeDef(folder, DamageType.Force);
            DamageTypeDef(folder, DamageType.Lightning);
            DamageTypeDef(folder, DamageType.Necrotic);
            DamageTypeDef(folder, DamageType.Piercing);
            DamageTypeDef(folder, DamageType.Poison);
            DamageTypeDef(folder, DamageType.Psychic);
            DamageTypeDef(folder, DamageType.Radiant);
            DamageTypeDef(folder, DamageType.Slashing);
            DamageTypeDef(folder, DamageType.Thunder);
        }

        private static void DamageTypeDef(string folder, DamageType type)
        {
            var so = PHBAssetGenerator.GetOrCreate<DamageTypeDefinitionSO>(folder, type.ToString());
            so.Type = type;
            EditorUtility.SetDirty(so);
        }

        // ── Conditions ────────────────────────────────────────────────────────

        private static void GenerateConditions()
        {
            var folder = DefinitionRoot + "/Conditions";
            ConditionDef(folder, Condition.Blinded);
            ConditionDef(folder, Condition.Charmed);
            ConditionDef(folder, Condition.Deafened);
            ConditionDef(folder, Condition.Frightened);
            ConditionDef(folder, Condition.Grappled);
            ConditionDef(folder, Condition.Incapacitated);
            ConditionDef(folder, Condition.Invisible);
            ConditionDef(folder, Condition.Paralyzed);
            ConditionDef(folder, Condition.Petrified);
            ConditionDef(folder, Condition.Poisoned);
            ConditionDef(folder, Condition.Prone);
            ConditionDef(folder, Condition.Restrained);
            ConditionDef(folder, Condition.Stunned);
            ConditionDef(folder, Condition.Unconscious);
        }

        private static void ConditionDef(string folder, Condition type)
        {
            var so = PHBAssetGenerator.GetOrCreate<ConditionDefinitionSO>(folder, type.ToString());
            so.Type = type;
            EditorUtility.SetDirty(so);
        }

        // ── Weapon Masteries ──────────────────────────────────────────────────

        private static void GenerateWeaponMasteries()
        {
            var folder = DefinitionRoot + "/WeaponMasteries";
            MasteryDef(folder, WeaponMastery.None);
            MasteryDef(folder, WeaponMastery.Cleave);
            MasteryDef(folder, WeaponMastery.Graze);
            MasteryDef(folder, WeaponMastery.Nick);
            MasteryDef(folder, WeaponMastery.Push);
            MasteryDef(folder, WeaponMastery.Sap);
            MasteryDef(folder, WeaponMastery.Slow);
            MasteryDef(folder, WeaponMastery.Topple);
            MasteryDef(folder, WeaponMastery.Vex);
        }

        private static void MasteryDef(string folder, WeaponMastery type)
        {
            var so = PHBAssetGenerator.GetOrCreate<WeaponMasteryDefinitionSO>(folder, type.ToString());
            so.Type = type;
            EditorUtility.SetDirty(so);
        }

        // ── Weapon Categories ─────────────────────────────────────────────────

        private static void GenerateWeaponCategories()
        {
            var folder = DefinitionRoot + "/WeaponCategories";
            WeaponCatDef(folder, WeaponCategory.Simple);
            WeaponCatDef(folder, WeaponCategory.Martial);
        }

        private static void WeaponCatDef(string folder, WeaponCategory type)
        {
            var so = PHBAssetGenerator.GetOrCreate<WeaponCategoryDefinitionSO>(folder, type.ToString());
            so.Type = type;
            EditorUtility.SetDirty(so);
        }

        // ── Spell Schools ─────────────────────────────────────────────────────

        private static void GenerateSpellSchools()
        {
            var folder = DefinitionRoot + "/SpellSchools";
            SchoolDef(folder, SpellSchool.Abjuration);
            SchoolDef(folder, SpellSchool.Conjuration);
            SchoolDef(folder, SpellSchool.Divination);
            SchoolDef(folder, SpellSchool.Enchantment);
            SchoolDef(folder, SpellSchool.Evocation);
            SchoolDef(folder, SpellSchool.Illusion);
            SchoolDef(folder, SpellSchool.Necromancy);
            SchoolDef(folder, SpellSchool.Transmutation);
        }

        private static void SchoolDef(string folder, SpellSchool type)
        {
            var so = PHBAssetGenerator.GetOrCreate<SpellSchoolDefinitionSO>(folder, type.ToString());
            so.Type = type;
            EditorUtility.SetDirty(so);
        }

        // ── Currency Types ────────────────────────────────────────────────────

        private static void GenerateCurrencyTypes()
        {
            var folder = DefinitionRoot + "/CurrencyTypes";
            CurrencyDef(folder, CurrencyType.Copper);
            CurrencyDef(folder, CurrencyType.Silver);
            CurrencyDef(folder, CurrencyType.Electrum);
            CurrencyDef(folder, CurrencyType.Gold);
            CurrencyDef(folder, CurrencyType.Platinum);
        }

        private static void CurrencyDef(string folder, CurrencyType type)
        {
            var so = PHBAssetGenerator.GetOrCreate<CurrencyDefinitionSO>(folder, type.ToString());
            so.Type = type;
            EditorUtility.SetDirty(so);
        }

        // ── Languages ─────────────────────────────────────────────────────────

        private static void GenerateLanguages()
        {
            var folder = DefinitionRoot + "/Languages";
            LangDef(folder, LanguageType.Common);
            LangDef(folder, LanguageType.CommonSignLanguage);
            LangDef(folder, LanguageType.Dwarvish);
            LangDef(folder, LanguageType.Elvish);
            LangDef(folder, LanguageType.Giant);
            LangDef(folder, LanguageType.Gnomish);
            LangDef(folder, LanguageType.Goblin);
            LangDef(folder, LanguageType.Halfling);
            LangDef(folder, LanguageType.Orc);
            LangDef(folder, LanguageType.Abyssal);
            LangDef(folder, LanguageType.Celestial);
            LangDef(folder, LanguageType.DeepSpeech);
            LangDef(folder, LanguageType.Draconic);
            LangDef(folder, LanguageType.Druidic);
            LangDef(folder, LanguageType.Infernal);
            LangDef(folder, LanguageType.Primordial);
            LangDef(folder, LanguageType.Sylvan);
            LangDef(folder, LanguageType.ThievesCant);
            LangDef(folder, LanguageType.Undercommon);
        }

        private static void LangDef(string folder, LanguageType type)
        {
            var so = PHBAssetGenerator.GetOrCreate<LanguageDefinitionSO>(folder, type.ToString());
            so.Type = type;
            EditorUtility.SetDirty(so);
        }

        // ── Dice ──────────────────────────────────────────────────────────────

        private static void GenerateDice()
        {
            var folder = DefinitionRoot + "/Dice";
            DieDef(folder, DieType.D4);
            DieDef(folder, DieType.D6);
            DieDef(folder, DieType.D8);
            DieDef(folder, DieType.D10);
            DieDef(folder, DieType.D12);
            DieDef(folder, DieType.D20);
            DieDef(folder, DieType.D100);
        }

        private static void DieDef(string folder, DieType type)
        {
            var so = PHBAssetGenerator.GetOrCreate<DieSO>(folder, type.ToString());
            so.Type = type;
            EditorUtility.SetDirty(so);
        }
    }
}
