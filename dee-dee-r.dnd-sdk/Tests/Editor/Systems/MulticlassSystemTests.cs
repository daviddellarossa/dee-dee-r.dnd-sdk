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
    public class MulticlassSystemTests
    {
        private MulticlassSystem _system;
        private ClassSO _fullCasterClass;
        private ClassSO _halfCasterClass;
        private ClassSO _thirdCasterClass;
        private ClassSO _nonCasterClass;

        [SetUp]
        public void SetUp()
        {
            _system = new MulticlassSystem();

            _fullCasterClass  = ScriptableObject.CreateInstance<ClassSO>();
            _halfCasterClass  = ScriptableObject.CreateInstance<ClassSO>();
            _thirdCasterClass = ScriptableObject.CreateInstance<ClassSO>();
            _nonCasterClass   = ScriptableObject.CreateInstance<ClassSO>();

            _fullCasterClass.CasterType  = CasterType.Full;
            _halfCasterClass.CasterType  = CasterType.Half;
            _thirdCasterClass.CasterType = CasterType.Third;
            _nonCasterClass.CasterType   = CasterType.None;
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_fullCasterClass);
            UnityEngine.Object.DestroyImmediate(_halfCasterClass);
            UnityEngine.Object.DestroyImmediate(_thirdCasterClass);
            UnityEngine.Object.DestroyImmediate(_nonCasterClass);
        }

        // ── CalculateCombinedSpellSlots ───────────────────────────────────────

        [Test]
        public void CalculateCombinedSpellSlots_NullList_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.CalculateCombinedSpellSlots(null));
        }

        [Test]
        public void CalculateCombinedSpellSlots_EmptyList_ReturnsEmpty()
        {
            var result = _system.CalculateCombinedSpellSlots(new List<ClassLevel>());
            Assert.AreEqual(SpellSlotState.Empty, result);
        }

        [Test]
        public void CalculateCombinedSpellSlots_NonCasterOnly_ReturnsEmpty()
        {
            var levels = new List<ClassLevel>
            {
                new ClassLevel { Class = _nonCasterClass, Level = 5 }
            };
            Assert.AreEqual(SpellSlotState.Empty, _system.CalculateCombinedSpellSlots(levels));
        }

        [Test]
        public void CalculateCombinedSpellSlots_FullCasterLevel5_ReturnsLevel5Table()
        {
            // Effective level = 5 → table row 5 → 4,3,2,0,0,0,0,0,0
            var levels = new List<ClassLevel>
            {
                new ClassLevel { Class = _fullCasterClass, Level = 5 }
            };
            var result = _system.CalculateCombinedSpellSlots(levels);
            Assert.AreEqual(4, result.Slot1, "1st-level slots");
            Assert.AreEqual(3, result.Slot2, "2nd-level slots");
            Assert.AreEqual(2, result.Slot3, "3rd-level slots");
            Assert.AreEqual(0, result.Slot4, "4th-level slots");
        }

        [Test]
        public void CalculateCombinedSpellSlots_HalfCasterLevel3_EffectiveLevelIs1()
        {
            // Half caster level 3 → effective = floor(3/2) = 1 → table row 1 → 2,0,0,...
            var levels = new List<ClassLevel>
            {
                new ClassLevel { Class = _halfCasterClass, Level = 3 }
            };
            var result = _system.CalculateCombinedSpellSlots(levels);
            Assert.AreEqual(2, result.Slot1, "1st-level slots");
            Assert.AreEqual(0, result.Slot2, "2nd-level slots");
        }

        [Test]
        public void CalculateCombinedSpellSlots_ThirdCasterLevel3_EffectiveLevelIs1()
        {
            // Third caster level 3 → effective = floor(3/3) = 1 → table row 1 → 2,0,...
            var levels = new List<ClassLevel>
            {
                new ClassLevel { Class = _thirdCasterClass, Level = 3 }
            };
            var result = _system.CalculateCombinedSpellSlots(levels);
            Assert.AreEqual(2, result.Slot1, "1st-level slots");
            Assert.AreEqual(0, result.Slot2, "2nd-level slots");
        }

        [Test]
        public void CalculateCombinedSpellSlots_FullLevel3_HalfLevel4_CombinesCorrectly()
        {
            // Full 3 (eff=3) + Half 4 (eff=floor(4/2)=2) → total eff=5 → 4,3,2,0,...
            var levels = new List<ClassLevel>
            {
                new ClassLevel { Class = _fullCasterClass, Level = 3 },
                new ClassLevel { Class = _halfCasterClass, Level = 4 },
            };
            var result = _system.CalculateCombinedSpellSlots(levels);
            Assert.AreEqual(4, result.Slot1, "1st-level slots");
            Assert.AreEqual(3, result.Slot2, "2nd-level slots");
            Assert.AreEqual(2, result.Slot3, "3rd-level slots");
            Assert.AreEqual(0, result.Slot4, "4th-level slots");
        }

        [Test]
        public void CalculateCombinedSpellSlots_FullLevel5_Wizard3_NonCaster2_CombinesFullAndFull()
        {
            // Two full casters: level 5 + level 3 → effective = 8 → 4,3,3,2,0,...
            var secondFullCaster = ScriptableObject.CreateInstance<ClassSO>();
            secondFullCaster.CasterType = CasterType.Full;
            try
            {
                var levels = new List<ClassLevel>
                {
                    new ClassLevel { Class = _fullCasterClass,   Level = 5 },
                    new ClassLevel { Class = secondFullCaster,    Level = 3 },
                    new ClassLevel { Class = _nonCasterClass,    Level = 2 },
                };
                var result = _system.CalculateCombinedSpellSlots(levels);
                Assert.AreEqual(4, result.Slot1, "1st-level slots");
                Assert.AreEqual(3, result.Slot2, "2nd-level slots");
                Assert.AreEqual(3, result.Slot3, "3rd-level slots");
                Assert.AreEqual(2, result.Slot4, "4th-level slots");
                Assert.AreEqual(0, result.Slot5, "5th-level slots");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(secondFullCaster);
            }
        }

        [Test]
        public void CalculateCombinedSpellSlots_MaxLevel20_ReturnsRow20()
        {
            // Effective level 20 → 4,3,3,3,3,2,2,1,1
            var levels = new List<ClassLevel>
            {
                new ClassLevel { Class = _fullCasterClass, Level = 20 }
            };
            var result = _system.CalculateCombinedSpellSlots(levels);
            Assert.AreEqual(4, result.Slot1);
            Assert.AreEqual(3, result.Slot2);
            Assert.AreEqual(3, result.Slot3);
            Assert.AreEqual(3, result.Slot4);
            Assert.AreEqual(3, result.Slot5);
            Assert.AreEqual(2, result.Slot6);
            Assert.AreEqual(2, result.Slot7);
            Assert.AreEqual(1, result.Slot8);
            Assert.AreEqual(1, result.Slot9);
        }

        [Test]
        public void CalculateCombinedSpellSlots_NullClassInEntry_SkipsEntry()
        {
            // Entry with null Class should be silently skipped.
            var levels = new List<ClassLevel>
            {
                new ClassLevel { Class = null, Level = 5 },
                new ClassLevel { Class = _fullCasterClass, Level = 3 },
            };
            // Effective level = 0 + 3 = 3 → 4,2,0,...
            var result = _system.CalculateCombinedSpellSlots(levels);
            Assert.AreEqual(4, result.Slot1);
            Assert.AreEqual(2, result.Slot2);
        }

        // ── ValidateMulticlassPrerequisites ───────────────────────────────────

        [Test]
        public void ValidateMulticlassPrerequisites_NullRecord_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.ValidateMulticlassPrerequisites(null, _nonCasterClass));
        }

        [Test]
        public void ValidateMulticlassPrerequisites_NullClass_ThrowsArgumentNullException()
        {
            var record = new CharacterRecord
            {
                AbilityScores = new AbilityScoreSet(10, 10, 10, 10, 10, 10)
            };
            Assert.Throws<ArgumentNullException>(
                () => _system.ValidateMulticlassPrerequisites(record, null));
        }

        [Test]
        public void ValidateMulticlassPrerequisites_NoPrerequisites_ReturnsTrue()
        {
            var cls = ScriptableObject.CreateInstance<ClassSO>();
            // cls.MulticlassPrerequisites is an empty list by default.
            var record = new CharacterRecord
            {
                AbilityScores = new AbilityScoreSet(8, 8, 8, 8, 8, 8)
            };
            try
            {
                Assert.IsTrue(_system.ValidateMulticlassPrerequisites(record, cls));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(cls);
            }
        }

        [Test]
        public void ValidateMulticlassPrerequisites_AllPrerequisitesMet_ReturnsTrue()
        {
            var cls = ScriptableObject.CreateInstance<ClassSO>();
            cls.MulticlassPrerequisites.Add(new AbilityPrerequisite
            {
                Ability = AbilityType.Strength,
                MinScore = 13,
            });
            // STR=15 → meets the 13 requirement.
            var record = new CharacterRecord
            {
                AbilityScores = new AbilityScoreSet(str: 15, dex: 10, con: 10, intel: 10, wis: 10, cha: 10)
            };
            try
            {
                Assert.IsTrue(_system.ValidateMulticlassPrerequisites(record, cls));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(cls);
            }
        }

        [Test]
        public void ValidateMulticlassPrerequisites_OnePrerequisiteUnmet_ReturnsFalse()
        {
            var cls = ScriptableObject.CreateInstance<ClassSO>();
            cls.MulticlassPrerequisites.Add(new AbilityPrerequisite
            {
                Ability = AbilityType.Intelligence,
                MinScore = 13,
            });
            // INT=10 → does not meet the 13 requirement.
            var record = new CharacterRecord
            {
                AbilityScores = new AbilityScoreSet(10, 10, 10, 10, 10, 10)
            };
            try
            {
                Assert.IsFalse(_system.ValidateMulticlassPrerequisites(record, cls));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(cls);
            }
        }

        [Test]
        public void ValidateMulticlassPrerequisites_MultiplePrerequisites_AllMustPass()
        {
            var cls = ScriptableObject.CreateInstance<ClassSO>();
            cls.MulticlassPrerequisites.Add(new AbilityPrerequisite { Ability = AbilityType.Strength, MinScore = 13 });
            cls.MulticlassPrerequisites.Add(new AbilityPrerequisite { Ability = AbilityType.Dexterity, MinScore = 13 });
            // STR=15 OK, DEX=10 fails.
            var record = new CharacterRecord
            {
                AbilityScores = new AbilityScoreSet(str: 15, dex: 10, con: 10, intel: 10, wis: 10, cha: 10)
            };
            try
            {
                Assert.IsFalse(_system.ValidateMulticlassPrerequisites(record, cls));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(cls);
            }
        }
    }
}