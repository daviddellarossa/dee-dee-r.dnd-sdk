using System.Collections.Generic;
using NUnit.Framework;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Systems;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Tests.Editor.Systems
{
    [TestFixture]
    public class ProficiencySystemTests
    {
        private readonly ProficiencySystem _system = new ProficiencySystem();

        // ── GetProficiencyBonus ───────────────────────────────────────────────

        [TestCase(1,  2)]
        [TestCase(4,  2)]
        [TestCase(5,  3)]
        [TestCase(8,  3)]
        [TestCase(9,  4)]
        [TestCase(12, 4)]
        [TestCase(13, 5)]
        [TestCase(16, 5)]
        [TestCase(17, 6)]
        [TestCase(20, 6)]
        public void GetProficiencyBonus_AllLevels_MatchPHBTable(int level, int expectedBonus)
        {
            Assert.AreEqual(expectedBonus, _system.GetProficiencyBonus(level));
        }

        [Test]
        public void GetProficiencyBonus_Level0_ThrowsArgumentOutOfRange()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _system.GetProficiencyBonus(0));
        }

        [Test]
        public void GetProficiencyBonus_Level21_ThrowsArgumentOutOfRange()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _system.GetProficiencyBonus(21));
        }

        // ── GetSkillBonus ─────────────────────────────────────────────────────

        [Test]
        public void GetSkillBonus_NotProficient_ReturnsAbilityModOnly()
        {
            // STR=14 → mod=+2; Athletics (STR); not proficient → bonus=+2
            var scores = new AbilityScoreSet(str: 14, dex: 10, con: 10, intel: 10, wis: 10, cha: 10);
            int bonus  = _system.GetSkillBonus(scores, SkillType.Athletics,
                isProficient: false, hasExpertise: false, proficiencyBonus: 3);
            Assert.AreEqual(2, bonus);
        }

        [Test]
        public void GetSkillBonus_Proficient_AddsBonus()
        {
            // STR=14 → mod=+2; Athletics; proficient, profBonus=3 → bonus=+5
            var scores = new AbilityScoreSet(str: 14, dex: 10, con: 10, intel: 10, wis: 10, cha: 10);
            int bonus  = _system.GetSkillBonus(scores, SkillType.Athletics,
                isProficient: true, hasExpertise: false, proficiencyBonus: 3);
            Assert.AreEqual(5, bonus);
        }

        [Test]
        public void GetSkillBonus_Expertise_DoublesBonus()
        {
            // STR=14 → mod=+2; Athletics; proficient + expertise, profBonus=3 → bonus=+2+(3×2)=+8
            var scores = new AbilityScoreSet(str: 14, dex: 10, con: 10, intel: 10, wis: 10, cha: 10);
            int bonus  = _system.GetSkillBonus(scores, SkillType.Athletics,
                isProficient: true, hasExpertise: true, proficiencyBonus: 3);
            Assert.AreEqual(8, bonus);
        }

        [Test]
        public void GetSkillBonus_ExpertiseWithoutProficiency_TreatedAsNoProficiency()
        {
            // hasExpertise has no effect if isProficient is false
            var scores = new AbilityScoreSet(str: 14, dex: 10, con: 10, intel: 10, wis: 10, cha: 10);
            int bonus  = _system.GetSkillBonus(scores, SkillType.Athletics,
                isProficient: false, hasExpertise: true, proficiencyBonus: 3);
            Assert.AreEqual(2, bonus);
        }

        [Test]
        public void GetSkillBonus_UsesCorrectGoverningAbility()
        {
            // Stealth uses DEX; DEX=16 → mod=+3; Proficient, profBonus=2 → bonus=+5
            var scores = new AbilityScoreSet(str: 10, dex: 16, con: 10, intel: 10, wis: 10, cha: 10);
            int bonus  = _system.GetSkillBonus(scores, SkillType.Stealth,
                isProficient: true, hasExpertise: false, proficiencyBonus: 2);
            Assert.AreEqual(5, bonus);
        }

        // ── HasSkillProficiency ───────────────────────────────────────────────

        [Test]
        public void HasSkillProficiency_SkillInSet_ReturnsTrue()
        {
            var profs = new HashSet<SkillType> { SkillType.Athletics, SkillType.Stealth };
            Assert.IsTrue(_system.HasSkillProficiency(profs, SkillType.Athletics));
        }

        [Test]
        public void HasSkillProficiency_SkillNotInSet_ReturnsFalse()
        {
            var profs = new HashSet<SkillType> { SkillType.Athletics };
            Assert.IsFalse(_system.HasSkillProficiency(profs, SkillType.Stealth));
        }

        // ── HasArmorProficiency ───────────────────────────────────────────────

        [Test]
        public void HasArmorProficiency_CategoryInSet_ReturnsTrue()
        {
            var profs = new HashSet<ArmorCategory> { ArmorCategory.Light, ArmorCategory.Medium };
            Assert.IsTrue(_system.HasArmorProficiency(profs, ArmorCategory.Light));
        }

        [Test]
        public void HasArmorProficiency_CategoryNotInSet_ReturnsFalse()
        {
            var profs = new HashSet<ArmorCategory> { ArmorCategory.Light };
            Assert.IsFalse(_system.HasArmorProficiency(profs, ArmorCategory.Heavy));
        }

        // ── HasWeaponCategoryProficiency ──────────────────────────────────────

        [Test]
        public void HasWeaponCategoryProficiency_CategoryInSet_ReturnsTrue()
        {
            var profs = new HashSet<WeaponCategory> { WeaponCategory.Simple, WeaponCategory.Martial };
            Assert.IsTrue(_system.HasWeaponCategoryProficiency(profs, WeaponCategory.Martial));
        }

        [Test]
        public void HasWeaponCategoryProficiency_CategoryNotInSet_ReturnsFalse()
        {
            var profs = new HashSet<WeaponCategory> { WeaponCategory.Simple };
            Assert.IsFalse(_system.HasWeaponCategoryProficiency(profs, WeaponCategory.Martial));
        }
    }
}
