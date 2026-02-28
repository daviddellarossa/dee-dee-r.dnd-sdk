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
    public class LevelUpSystemTests
    {
        private LevelUpSystem _system;
        private ClassSO _fighterClass;
        private ClassSO _wizardClass;

        [SetUp]
        public void SetUp()
        {
            _system = new LevelUpSystem();

            _fighterClass = ScriptableObject.CreateInstance<ClassSO>();
            _fighterClass.HitDie  = DieType.D10;
            _fighterClass.CasterType = CasterType.None;
            _fighterClass.SubclassLevel = 3;
            _fighterClass.SavingThrowProficiencies = new AbilityType[]
            {
                AbilityType.Strength, AbilityType.Constitution,
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
            _wizardClass.HitDie  = DieType.D6;
            _wizardClass.CasterType = CasterType.Full;
            _wizardClass.SubclassLevel = 3;
            _wizardClass.SavingThrowProficiencies = new AbilityType[]
            {
                AbilityType.Intelligence, AbilityType.Wisdom,
            };
            _wizardClass.ArmorProficiencies = System.Array.Empty<ArmorCategory>();
            _wizardClass.WeaponCategoryProficiencies = System.Array.Empty<WeaponCategory>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_fighterClass);
            UnityEngine.Object.DestroyImmediate(_wizardClass);
        }

        // ── Argument validation ───────────────────────────────────────────────

        [Test]
        public void LevelUp_NullRecord_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.LevelUp(null, new CharacterState(), _fighterClass));
        }

        [Test]
        public void LevelUp_NullState_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.LevelUp(new CharacterRecord(), null, _fighterClass));
        }

        [Test]
        public void LevelUp_NullClass_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.LevelUp(new CharacterRecord(), new CharacterState(), null));
        }

        // ── Level increment ───────────────────────────────────────────────────

        [Test]
        public void LevelUp_ExistingClass_IncrementsLevel()
        {
            var (record, state) = MakeFighter2();
            _system.LevelUp(record, state, _fighterClass);
            Assert.AreEqual(3, record.ClassLevels[0].Level);
        }

        [Test]
        public void LevelUp_NewClass_AddsClassLevelEntry()
        {
            var (record, state) = MakeFighter2();
            // Fighter 2 has no Wizard entry yet; adding one should append a new ClassLevel.
            _system.LevelUp(record, state, _wizardClass);
            Assert.AreEqual(2, record.ClassLevels.Count);
            Assert.AreSame(_wizardClass, record.ClassLevels[1].Class);
            Assert.AreEqual(1, record.ClassLevels[1].Level);
        }

        // ── Subclass ──────────────────────────────────────────────────────────

        [Test]
        public void LevelUp_AtSubclassLevel_GrantsChosenSubclass()
        {
            // Fighter subclass level = 3. Level up from 2 → 3.
            var subclass = ScriptableObject.CreateInstance<SubclassSO>();
            try
            {
                var (record, state) = MakeFighter2();
                _system.LevelUp(record, state, _fighterClass, chosenSubclass: subclass);
                Assert.AreSame(subclass, record.ClassLevels[0].Subclass);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(subclass);
            }
        }

        [Test]
        public void LevelUp_BelowSubclassLevel_DoesNotGrantSubclass()
        {
            // Fighter subclass level = 3. Level up from 1 → 2 — too early for subclass.
            var subclass = ScriptableObject.CreateInstance<SubclassSO>();
            try
            {
                var (record, state) = MakeFighter1();
                _system.LevelUp(record, state, _fighterClass, chosenSubclass: subclass);
                Assert.IsNull(record.ClassLevels[0].Subclass);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(subclass);
            }
        }

        [Test]
        public void LevelUp_SubclassAlreadyChosen_DoesNotReplaceIt()
        {
            // Subclass should not be replaced if already set.
            var subclass1 = ScriptableObject.CreateInstance<SubclassSO>();
            var subclass2 = ScriptableObject.CreateInstance<SubclassSO>();
            try
            {
                var (record, state) = MakeFighter2();
                // Set subclass manually (simulating it was chosen at level 3 earlier).
                record.ClassLevels[0].Subclass = subclass1;
                // Level up to 4 and pass a different subclass — it should be ignored.
                _system.LevelUp(record, state, _fighterClass, chosenSubclass: subclass2);
                Assert.AreSame(subclass1, record.ClassLevels[0].Subclass);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(subclass1);
                UnityEngine.Object.DestroyImmediate(subclass2);
            }
        }

        // ── Hit points ────────────────────────────────────────────────────────

        [Test]
        public void LevelUp_Fighter_ConMod0_HpGainEqualsAverage()
        {
            // D10 average = 6; CON=10 → mod=0; gain = 6.
            var (record, state) = MakeFighter1(conScore: 10);
            int hpBefore = state.HitPoints.Maximum;
            _system.LevelUp(record, state, _fighterClass);
            Assert.AreEqual(hpBefore + 6, state.HitPoints.Maximum);
        }

        [Test]
        public void LevelUp_Fighter_ConMod2_HpGainEqualsAveragePlusBonus()
        {
            // D10 average = 6; CON=14 → mod=+2; gain = 8.
            var (record, state) = MakeFighter1(conScore: 14);
            int hpBefore = state.HitPoints.Maximum;
            _system.LevelUp(record, state, _fighterClass);
            Assert.AreEqual(hpBefore + 8, state.HitPoints.Maximum);
        }

        [Test]
        public void LevelUp_VeryNegativeConMod_HpGainIsAtLeastOne()
        {
            // D6 average = 4; CON=3 → mod=-4; 4 - 4 = 0 → clamped to 1.
            var (record, state) = MakeFighter1(conScore: 3);
            // Use wizard class (D6) so average is 4.
            // First need a wizard entry.
            var wizardEntry = new ClassLevel { Class = _wizardClass, Level = 1 };
            record.ClassLevels.Add(wizardEntry);
            state.HitDiceAvailable[DieType.D6] = 1;

            int hpBefore = state.HitPoints.Maximum;
            // Level up as existing wizard (level 1 → 2).
            _system.LevelUp(record, state, _wizardClass);
            Assert.GreaterOrEqual(state.HitPoints.Maximum - hpBefore, 1);
        }

        [Test]
        public void LevelUp_IncreasesCurrentHPByHpGain()
        {
            // Current HP should also increase (character gains new max immediately).
            var (record, state) = MakeFighter1(conScore: 10);
            int currentBefore = state.HitPoints.Current;
            _system.LevelUp(record, state, _fighterClass);
            Assert.AreEqual(currentBefore + 6, state.HitPoints.Current);
        }

        // ── Hit dice ──────────────────────────────────────────────────────────

        [Test]
        public void LevelUp_AddsOneHitDieOfCorrectType()
        {
            var (record, state) = MakeFighter1();
            int diceBeforeD10 = state.HitDiceAvailable.GetValueOrDefault(DieType.D10, 0);
            _system.LevelUp(record, state, _fighterClass);
            Assert.AreEqual(diceBeforeD10 + 1, state.HitDiceAvailable[DieType.D10]);
        }

        // ── Ability score increases ───────────────────────────────────────────

        [Test]
        public void LevelUp_AppliesASI_ToAbilityScore()
        {
            var (record, state) = MakeFighter1();
            // Strength starts at 15; apply +2 → should become 17.
            var asis = new[] { new AbilityScoreIncrease { Ability = AbilityType.Strength, Amount = 2 } };
            _system.LevelUp(record, state, _fighterClass, chosenASIs: asis);
            Assert.AreEqual(17, record.AbilityScores.GetScore(AbilityType.Strength));
        }

        [Test]
        public void LevelUp_ASI_CapsAt20()
        {
            var (record, state) = MakeFighter1();
            // Strength starts at 15; apply +10 → should cap at 20.
            var asis = new[] { new AbilityScoreIncrease { Ability = AbilityType.Strength, Amount = 10 } };
            _system.LevelUp(record, state, _fighterClass, chosenASIs: asis);
            Assert.AreEqual(20, record.AbilityScores.GetScore(AbilityType.Strength));
        }

        // ── Feats ─────────────────────────────────────────────────────────────

        [Test]
        public void LevelUp_AddsFeatToRecord()
        {
            var feat = ScriptableObject.CreateInstance<FeatSO>();
            try
            {
                var (record, state) = MakeFighter1();
                _system.LevelUp(record, state, _fighterClass, chosenFeat: feat);
                Assert.Contains(feat, record.Feats);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(feat);
            }
        }

        // ── Spell slots ───────────────────────────────────────────────────────

        [Test]
        public void LevelUp_WizardLevel1_SetsCorrectSpellSlots()
        {
            // After leveling up to Wizard 1 (as a multiclass from Fighter 2), effective level = 1 → 2 slots.
            var (record, state) = MakeFighter2();
            _system.LevelUp(record, state, _wizardClass);
            // Wizard is a full caster, effective level = 1 → slot table row 1 → 2,0,...
            Assert.AreEqual(2, state.SpellSlots.Slot1);
            Assert.AreEqual(0, state.SpellSlots.Slot2);
        }

        [Test]
        public void LevelUp_NonCasterLevelUp_RecalculatesSpellSlotsToEmpty()
        {
            // Slot state is always recalculated from class levels — Fighter contributes
            // nothing, so the result is Empty regardless of what was stored before.
            var (record, state) = MakeFighter1();
            state.SpellSlots = new SpellSlotState(s1: 4, s2: 3); // Simulate pre-existing slots.
            _system.LevelUp(record, state, _fighterClass);
            Assert.AreEqual(SpellSlotState.Empty, state.SpellSlots);
        }

        // ── Level caps ────────────────────────────────────────────────────────

        [Test]
        public void LevelUp_ClassAlreadyAtLevel20_ThrowsInvalidOperationException()
        {
            var (record, state) = MakeFighter1();
            record.ClassLevels[0].Level = 20;
            // total level = 20; per-class level = 20 — both guards fire (total fires first).
            Assert.Throws<InvalidOperationException>(
                () => _system.LevelUp(record, state, _fighterClass));
        }

        [Test]
        public void LevelUp_TotalLevelAlready20_ThrowsInvalidOperationException()
        {
            // Fighter 10 + Wizard 10 = total 20. Trying to level up Fighter throws.
            var (record, state) = MakeFighter1();
            record.ClassLevels[0].Level = 10;
            record.ClassLevels.Add(new ClassLevel { Class = _wizardClass, Level = 10 });
            state.HitDiceAvailable[DieType.D6] = 10;
            Assert.Throws<InvalidOperationException>(
                () => _system.LevelUp(record, state, _fighterClass));
        }

        // ── Multiclass prerequisites ──────────────────────────────────────────

        [Test]
        public void LevelUp_NewClass_PrerequisitesMet_Succeeds()
        {
            var prereqClass = ScriptableObject.CreateInstance<ClassSO>();
            prereqClass.HitDie = DieType.D8;
            prereqClass.CasterType = CasterType.None;
            prereqClass.SubclassLevel = 3;
            prereqClass.SavingThrowProficiencies = System.Array.Empty<AbilityType>();
            prereqClass.ArmorProficiencies = System.Array.Empty<ArmorCategory>();
            prereqClass.WeaponCategoryProficiencies = System.Array.Empty<WeaponCategory>();
            prereqClass.MulticlassPrerequisites.Add(
                new AbilityPrerequisite { Ability = AbilityType.Dexterity, MinScore = 13 });
            try
            {
                // DEX=14 → meets the requirement.
                var (record, state) = MakeFighter1(dexScore: 14);
                Assert.DoesNotThrow(() => _system.LevelUp(record, state, prereqClass));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prereqClass);
            }
        }

        [Test]
        public void LevelUp_NewClass_PrerequisitesUnmet_ThrowsInvalidOperationException()
        {
            var prereqClass = ScriptableObject.CreateInstance<ClassSO>();
            prereqClass.HitDie = DieType.D8;
            prereqClass.CasterType = CasterType.None;
            prereqClass.SubclassLevel = 3;
            prereqClass.SavingThrowProficiencies = System.Array.Empty<AbilityType>();
            prereqClass.ArmorProficiencies = System.Array.Empty<ArmorCategory>();
            prereqClass.WeaponCategoryProficiencies = System.Array.Empty<WeaponCategory>();
            prereqClass.MulticlassPrerequisites.Add(
                new AbilityPrerequisite { Ability = AbilityType.Dexterity, MinScore = 13 });
            try
            {
                // DEX=10 → does NOT meet the requirement.
                var (record, state) = MakeFighter1(dexScore: 10);
                Assert.Throws<InvalidOperationException>(
                    () => _system.LevelUp(record, state, prereqClass));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prereqClass);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Creates a Fighter 1 record/state with controllable ability scores.</summary>
        private (CharacterRecord, CharacterState) MakeFighter1(
            int strScore = 15,
            int dexScore = 13,
            int conScore = 10)
        {
            var scores = new AbilityScoreSet(strScore, dexScore, conScore, 10, 12, 8);
            var record = new CharacterRecord
            {
                AbilityScores = scores,
            };
            record.ClassLevels.Add(new ClassLevel { Class = _fighterClass, Level = 1 });
            record.SavingThrowProficiencies.Add(AbilityType.Strength);
            record.SavingThrowProficiencies.Add(AbilityType.Constitution);
            record.ArmorProficiencies.Add(ArmorCategory.Light);
            record.ArmorProficiencies.Add(ArmorCategory.Medium);
            record.ArmorProficiencies.Add(ArmorCategory.Heavy);
            record.WeaponCategoryProficiencies.Add(WeaponCategory.Simple);
            record.WeaponCategoryProficiencies.Add(WeaponCategory.Martial);
            record.HasShieldProficiency = true;

            // D10 max = 10 + CON modifier (min 1) for level 1.
            int conMod = AbilityScoreSet.GetModifier(conScore);
            int hp1    = Math.Max(1, 10 + conMod);
            var state = new CharacterState
            {
                HitPoints = new HitPointState(hp1, hp1),
                SpellSlots = SpellSlotState.Empty,
            };
            state.HitDiceAvailable[DieType.D10] = 1;

            return (record, state);
        }

        /// <summary>Creates a Fighter 2 record/state with default ability scores (CON=10).</summary>
        private (CharacterRecord, CharacterState) MakeFighter2()
        {
            var (record, state) = MakeFighter1();
            _system.LevelUp(record, state, _fighterClass);
            return (record, state);
        }
    }
}