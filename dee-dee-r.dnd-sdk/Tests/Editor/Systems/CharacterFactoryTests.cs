using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.State;
using DeeDeeR.DnD.Runtime.Systems;

namespace DeeDeeR.DnD.Tests.Editor.Systems
{
    [TestFixture]
    public class CharacterFactoryTests
    {
        private CharacterFactory _factory;

        // Reusable SO assets created fresh for each test.
        private ClassSO _fighterClass;     // Non-caster, D10
        private ClassSO _wizardClass;      // Full caster, D6
        private SpeciesSO _humanSpecies;
        private BackgroundSO _soldierBackground;

        [SetUp]
        public void SetUp()
        {
            _factory = new CharacterFactory();

            _fighterClass = ScriptableObject.CreateInstance<ClassSO>();
            _fighterClass.HitDie = DieType.D10;
            _fighterClass.CasterType = CasterType.None;
            _fighterClass.SavingThrowProficiencies = new AbilityType[]
            {
                AbilityType.Strength,
                AbilityType.Constitution,
            };
            _fighterClass.ArmorProficiencies = new ArmorCategory[]
            {
                ArmorCategory.Light, ArmorCategory.Medium, ArmorCategory.Heavy,
            };
            _fighterClass.WeaponCategoryProficiencies = new WeaponCategory[]
            {
                WeaponCategory.Simple, WeaponCategory.Martial,
            };
            _fighterClass.HasShieldProficiency = true;

            _wizardClass = ScriptableObject.CreateInstance<ClassSO>();
            _wizardClass.HitDie = DieType.D6;
            _wizardClass.CasterType = CasterType.Full;
            _wizardClass.SavingThrowProficiencies = new AbilityType[]
            {
                AbilityType.Intelligence,
                AbilityType.Wisdom,
            };

            _humanSpecies = ScriptableObject.CreateInstance<SpeciesSO>();
            _humanSpecies.Languages.Add(LanguageType.Common);

            _soldierBackground = ScriptableObject.CreateInstance<BackgroundSO>();
            _soldierBackground.AbilityScoreIncreases.Add(
                new AbilityScoreIncrease { Ability = AbilityType.Strength, Amount = 2 });
            _soldierBackground.SkillProficiencies = new SkillType[]
            {
                SkillType.Athletics,
                SkillType.Intimidation,
            };
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_fighterClass);
            UnityEngine.Object.DestroyImmediate(_wizardClass);
            UnityEngine.Object.DestroyImmediate(_humanSpecies);
            UnityEngine.Object.DestroyImmediate(_soldierBackground);
        }

        // ── Argument validation ───────────────────────────────────────────────

        [Test]
        public void Build_NullCharacterName_ThrowsArgumentException()
        {
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 1 } };
            Assert.Throws<ArgumentException>(() =>
                _factory.Build(null, "Player", _humanSpecies, null, _soldierBackground,
                    new AbilityScoreSet(15, 13, 14, 10, 12, 8), levels));
        }

        [Test]
        public void Build_EmptyCharacterName_ThrowsArgumentException()
        {
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 1 } };
            Assert.Throws<ArgumentException>(() =>
                _factory.Build(string.Empty, "Player", _humanSpecies, null, _soldierBackground,
                    new AbilityScoreSet(15, 13, 14, 10, 12, 8), levels));
        }

        [Test]
        public void Build_NullSpecies_ThrowsArgumentNullException()
        {
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 1 } };
            Assert.Throws<ArgumentNullException>(() =>
                _factory.Build("Thorin", "Player", null, null, _soldierBackground,
                    new AbilityScoreSet(15, 13, 14, 10, 12, 8), levels));
        }

        [Test]
        public void Build_NullBackground_ThrowsArgumentNullException()
        {
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 1 } };
            Assert.Throws<ArgumentNullException>(() =>
                _factory.Build("Thorin", "Player", _humanSpecies, null, null,
                    new AbilityScoreSet(15, 13, 14, 10, 12, 8), levels));
        }

        [Test]
        public void Build_NullClassLevels_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                _factory.Build("Thorin", "Player", _humanSpecies, null, _soldierBackground,
                    new AbilityScoreSet(15, 13, 14, 10, 12, 8), null));
        }

        [Test]
        public void Build_EmptyClassLevels_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                _factory.Build("Thorin", "Player", _humanSpecies, null, _soldierBackground,
                    new AbilityScoreSet(15, 13, 14, 10, 12, 8), new List<ClassLevel>()));
        }

        [Test]
        public void Build_NullPrimaryClassEntry_ThrowsArgumentException()
        {
            var levels = new List<ClassLevel> { null };
            Assert.Throws<ArgumentException>(() =>
                _factory.Build("Thorin", "Player", _humanSpecies, null, _soldierBackground,
                    new AbilityScoreSet(15, 13, 14, 10, 12, 8), levels));
        }

        [Test]
        public void Build_PrimaryClassEntryWithNullClass_ThrowsArgumentNullException()
        {
            var levels = new List<ClassLevel> { new ClassLevel { Class = null, Level = 1 } };
            Assert.Throws<ArgumentNullException>(() =>
                _factory.Build("Thorin", "Player", _humanSpecies, null, _soldierBackground,
                    new AbilityScoreSet(15, 13, 14, 10, 12, 8), levels));
        }

        [Test]
        public void Build_PrimaryClassEntryWithZeroLevel_ThrowsArgumentException()
        {
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 0 } };
            Assert.Throws<ArgumentException>(() =>
                _factory.Build("Thorin", "Player", _humanSpecies, null, _soldierBackground,
                    new AbilityScoreSet(15, 13, 14, 10, 12, 8), levels));
        }

        // ── Identity fields ───────────────────────────────────────────────────

        [Test]
        public void Build_SetsNameAndPlayerName()
        {
            var (record, _) = BuildFighter1("Thorin", "Dave");
            Assert.AreEqual("Thorin", record.Name);
            Assert.AreEqual("Dave", record.PlayerName);
        }

        [Test]
        public void Build_NullPlayerName_StoredAsEmpty()
        {
            var (record, _) = BuildFighter1("Thorin", null);
            Assert.AreEqual(string.Empty, record.PlayerName);
        }

        [Test]
        public void Build_SetsSpeciesAndBackground()
        {
            var (record, _) = BuildFighter1();
            Assert.AreSame(_humanSpecies, record.Species);
            Assert.AreSame(_soldierBackground, record.Background);
        }

        // ── Ability scores ────────────────────────────────────────────────────

        [Test]
        public void Build_AppliesBackgroundASI_ToAbilityScores()
        {
            // Soldier gives +2 STR. Base STR=13 → final STR=15.
            var base_ = new AbilityScoreSet(str: 13, dex: 10, con: 10, intel: 10, wis: 10, cha: 10);
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 1 } };
            var (record, _) = _factory.Build("Thorin", null, _humanSpecies, null,
                _soldierBackground, base_, levels);
            Assert.AreEqual(15, record.AbilityScores.GetScore(AbilityType.Strength));
        }

        [Test]
        public void Build_BackgroundASI_CapsResultingScoreAt20()
        {
            // Base STR=19; background adds +2 → would be 21 without cap → clamped to 20.
            var base_ = new AbilityScoreSet(str: 19, dex: 10, con: 10, intel: 10, wis: 10, cha: 10);
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 1 } };
            var (record, _) = _factory.Build("Thorin", null, _humanSpecies, null,
                _soldierBackground, base_, levels);
            Assert.AreEqual(20, record.AbilityScores.GetScore(AbilityType.Strength));
        }

        [Test]
        public void Build_UnaffectedAbilityScores_AreUnchanged()
        {
            var base_ = new AbilityScoreSet(str: 13, dex: 12, con: 14, intel: 10, wis: 11, cha: 8);
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 1 } };
            var (record, _) = _factory.Build("Thorin", null, _humanSpecies, null,
                _soldierBackground, base_, levels);
            // DEX, CON, INT, WIS, CHA unchanged; STR goes 13→15.
            Assert.AreEqual(12, record.AbilityScores.GetScore(AbilityType.Dexterity));
            Assert.AreEqual(14, record.AbilityScores.GetScore(AbilityType.Constitution));
        }

        // ── Proficiencies ─────────────────────────────────────────────────────

        [Test]
        public void Build_AppliesPrimaryClassSavingThrowProficiencies()
        {
            var (record, _) = BuildFighter1();
            Assert.IsTrue(record.SavingThrowProficiencies.Contains(AbilityType.Strength));
            Assert.IsTrue(record.SavingThrowProficiencies.Contains(AbilityType.Constitution));
        }

        [Test]
        public void Build_AppliesPrimaryClassArmorProficiencies()
        {
            var (record, _) = BuildFighter1();
            Assert.IsTrue(record.ArmorProficiencies.Contains(ArmorCategory.Heavy));
        }

        [Test]
        public void Build_AppliesPrimaryClassShieldProficiency()
        {
            var (record, _) = BuildFighter1();
            Assert.IsTrue(record.HasShieldProficiency);
        }

        [Test]
        public void Build_AppliesBackgroundSkillProficiencies()
        {
            var (record, _) = BuildFighter1();
            Assert.IsTrue(record.SkillProficiencies.Contains(SkillType.Athletics));
            Assert.IsTrue(record.SkillProficiencies.Contains(SkillType.Intimidation));
        }

        [Test]
        public void Build_AppliesChosenSkillProficiencies()
        {
            var base_ = new AbilityScoreSet(15, 13, 14, 10, 12, 8);
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 1 } };
            var chosen = new[] { SkillType.Perception, SkillType.Acrobatics };
            var (record, _) = _factory.Build("Thorin", null, _humanSpecies, null,
                _soldierBackground, base_, levels, chosen);
            Assert.IsTrue(record.SkillProficiencies.Contains(SkillType.Perception));
            Assert.IsTrue(record.SkillProficiencies.Contains(SkillType.Acrobatics));
        }

        // ── Languages ─────────────────────────────────────────────────────────

        [Test]
        public void Build_AppliesSpeciesLanguages()
        {
            var (record, _) = BuildFighter1();
            Assert.IsTrue(record.Languages.Contains(LanguageType.Common));
        }

        [Test]
        public void Build_AppliesSubspeciesAdditionalLanguages()
        {
            var subspecies = ScriptableObject.CreateInstance<SubspeciesSO>();
            subspecies.AdditionalLanguages.Add(LanguageType.Elvish);
            var base_ = new AbilityScoreSet(15, 13, 14, 10, 12, 8);
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 1 } };
            try
            {
                var (record, _) = _factory.Build("Thorin", null, _humanSpecies, subspecies,
                    _soldierBackground, base_, levels);
                Assert.IsTrue(record.Languages.Contains(LanguageType.Elvish));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(subspecies);
            }
        }

        // ── Hit points ────────────────────────────────────────────────────────

        [Test]
        public void Build_Level1Fighter_ConMod0_HpEqualsMaxDie()
        {
            // Fighter D10 max = 10; CON=10 → mod=0; total HP = 10.
            var base_ = new AbilityScoreSet(15, 13, 10, 10, 12, 8);
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 1 } };
            var (_, state) = _factory.Build("Thorin", null, _humanSpecies, null,
                _soldierBackground, base_, levels);
            Assert.AreEqual(10, state.HitPoints.Maximum);
            Assert.AreEqual(10, state.HitPoints.Current);
        }

        [Test]
        public void Build_Level1Fighter_ConMod2_HpEquals12()
        {
            // Fighter D10 max = 10; CON=14 → mod=+2; total HP = 12.
            var base_ = new AbilityScoreSet(15, 13, 14, 10, 12, 8);
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 1 } };
            var (_, state) = _factory.Build("Thorin", null, _humanSpecies, null,
                _soldierBackground, base_, levels);
            // Background adds +2 STR, not CON, so CON remains 14 → mod +2.
            Assert.AreEqual(12, state.HitPoints.Maximum);
        }

        [Test]
        public void Build_Level2Fighter_ConMod0_HpIsMaxPlusAverage()
        {
            // Level 1: D10 max = 10; Level 2: D10 average = 6; total = 16.
            var base_ = new AbilityScoreSet(15, 13, 10, 10, 12, 8);
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 2 } };
            var (_, state) = _factory.Build("Thorin", null, _humanSpecies, null,
                _soldierBackground, base_, levels);
            Assert.AreEqual(16, state.HitPoints.Maximum);
        }

        [Test]
        public void Build_Fighter1Wizard1_HpIsD10MaxPlusD6Average()
        {
            // Fighter D10 level 1 max = 10; Wizard D6 level 1 average = 4; total = 14.
            // CON=10 → mod=0.
            var base_ = new AbilityScoreSet(15, 13, 10, 10, 12, 8);
            var levels = new List<ClassLevel>
            {
                new ClassLevel { Class = _fighterClass, Level = 1 },
                new ClassLevel { Class = _wizardClass,  Level = 1 },
            };
            var (_, state) = _factory.Build("Thorin", null, _humanSpecies, null,
                _soldierBackground, base_, levels);
            // Level 1 = 10 (max D10); Level 2 = 4 (avg D6) = 14 total.
            Assert.AreEqual(14, state.HitPoints.Maximum);
        }

        [Test]
        public void Build_VeryNegativeConMod_MinimumOneHpPerLevel()
        {
            // CON=3 → mod=-4. D6 average = 4. 4 + (-4) = 0, clamped to 1 per level.
            // Level 1 (max die = 6): 6 - 4 = 2.  Level 2 (average = 4): 4 - 4 = 0 → clamped to 1.
            var wizardClass = ScriptableObject.CreateInstance<ClassSO>();
            wizardClass.HitDie = DieType.D6;
            wizardClass.CasterType = CasterType.Full;
            wizardClass.SavingThrowProficiencies = System.Array.Empty<AbilityType>();
            wizardClass.ArmorProficiencies = System.Array.Empty<ArmorCategory>();
            wizardClass.WeaponCategoryProficiencies = System.Array.Empty<WeaponCategory>();

            var background = ScriptableObject.CreateInstance<BackgroundSO>();
            background.SkillProficiencies = new SkillType[2];

            try
            {
                // Use a background with no ASI so we can control CON precisely.
                var base_ = new AbilityScoreSet(10, 10, 3, 10, 10, 10);
                var levels = new List<ClassLevel>
                {
                    new ClassLevel { Class = wizardClass, Level = 2 },
                };
                var (_, state) = _factory.Build("Frail", null, _humanSpecies, null,
                    background, base_, levels);
                // Level 1 max D6 = 6 + (-4) = 2; Level 2 avg D6 = 4 + (-4) = 0 → clamped to 1.
                Assert.AreEqual(3, state.HitPoints.Maximum);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(wizardClass);
                UnityEngine.Object.DestroyImmediate(background);
            }
        }

        // ── Hit dice ──────────────────────────────────────────────────────────

        [Test]
        public void Build_SingleClass_HitDiceAvailableMatchesLevel()
        {
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 3 } };
            var base_ = new AbilityScoreSet(15, 13, 10, 10, 12, 8);
            var (_, state) = _factory.Build("Thorin", null, _humanSpecies, null,
                _soldierBackground, base_, levels);
            Assert.IsTrue(state.HitDiceAvailable.ContainsKey(DieType.D10));
            Assert.AreEqual(3, state.HitDiceAvailable[DieType.D10]);
        }

        [Test]
        public void Build_MultiClass_HitDiceAvailableGroupedByType()
        {
            var levels = new List<ClassLevel>
            {
                new ClassLevel { Class = _fighterClass, Level = 3 },
                new ClassLevel { Class = _wizardClass,  Level = 2 },
            };
            var base_ = new AbilityScoreSet(15, 13, 10, 10, 12, 8);
            var (_, state) = _factory.Build("Thorin", null, _humanSpecies, null,
                _soldierBackground, base_, levels);
            Assert.AreEqual(3, state.HitDiceAvailable[DieType.D10], "Fighter dice");
            Assert.AreEqual(2, state.HitDiceAvailable[DieType.D6],  "Wizard dice");
        }

        // ── Spell slots ───────────────────────────────────────────────────────

        [Test]
        public void Build_NonCasterClass_SpellSlotsAreEmpty()
        {
            var (_, state) = BuildFighter1();
            Assert.AreEqual(SpellSlotState.Empty, state.SpellSlots);
        }

        [Test]
        public void Build_WizardLevel3_SpellSlotsMatchTable()
        {
            // Full caster level 3 → 4,2,0,...
            var base_ = new AbilityScoreSet(10, 10, 10, 15, 12, 10);
            var wizardBackground = ScriptableObject.CreateInstance<BackgroundSO>();
            wizardBackground.SkillProficiencies = new SkillType[2];
            try
            {
                var levels = new List<ClassLevel> { new ClassLevel { Class = _wizardClass, Level = 3 } };
                var (_, state) = _factory.Build("Gandalf", null, _humanSpecies, null,
                    wizardBackground, base_, levels);
                Assert.AreEqual(4, state.SpellSlots.Slot1);
                Assert.AreEqual(2, state.SpellSlots.Slot2);
                Assert.AreEqual(0, state.SpellSlots.Slot3);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(wizardBackground);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a Fighter 1 with STR 15 / DEX 13 / CON 14 (post-background: STR 17).
        /// </summary>
        private (CharacterRecord, CharacterState) BuildFighter1(
            string name = "Thorin", string playerName = "Dave")
        {
            var base_ = new AbilityScoreSet(15, 13, 14, 10, 12, 8);
            var levels = new List<ClassLevel> { new ClassLevel { Class = _fighterClass, Level = 1 } };
            return _factory.Build(name, playerName, _humanSpecies, null, _soldierBackground, base_, levels);
        }
    }
}
