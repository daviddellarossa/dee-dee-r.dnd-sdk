using System;
using NUnit.Framework;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Runtime.Systems;

namespace DeeDeeR.DnD.Tests.Editor.Systems
{
    [TestFixture]
    public class XPSystemTests
    {
        private XPSystem _system;

        [SetUp]
        public void SetUp() => _system = new XPSystem();

        // ── GetXPThreshold — valid inputs ─────────────────────────────────────

        [TestCase(1,       0)]
        [TestCase(2,     300)]
        [TestCase(3,     900)]
        [TestCase(4,   2_700)]
        [TestCase(5,   6_500)]
        [TestCase(6,  14_000)]
        [TestCase(7,  23_000)]
        [TestCase(8,  34_000)]
        [TestCase(9,  48_000)]
        [TestCase(10, 64_000)]
        [TestCase(11, 85_000)]
        [TestCase(12, 100_000)]
        [TestCase(13, 120_000)]
        [TestCase(14, 140_000)]
        [TestCase(15, 165_000)]
        [TestCase(16, 195_000)]
        [TestCase(17, 225_000)]
        [TestCase(18, 265_000)]
        [TestCase(19, 305_000)]
        [TestCase(20, 355_000)]
        public void GetXPThreshold_ReturnsCorrectValue(int level, int expectedXP)
        {
            Assert.AreEqual(expectedXP, _system.GetXPThreshold(level));
        }

        // ── GetXPThreshold — invalid inputs ───────────────────────────────────

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(21)]
        public void GetXPThreshold_InvalidLevel_Throws(int level)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _system.GetXPThreshold(level));
        }

        // ── GetLevelFromXP — boundary values ──────────────────────────────────

        [TestCase(0,       1)]   // exactly level 1
        [TestCase(299,     1)]   // just below level 2
        [TestCase(300,     2)]   // exactly level 2
        [TestCase(899,     2)]   // just below level 3
        [TestCase(900,     3)]   // exactly level 3
        [TestCase(6_499,   4)]   // just below level 5
        [TestCase(6_500,   5)]   // exactly level 5
        [TestCase(354_999, 19)]  // just below level 20
        [TestCase(355_000, 20)]  // exactly level 20
        [TestCase(999_999, 20)]  // way beyond level 20 caps at 20
        public void GetLevelFromXP_ReturnsCorrectLevel(int xp, int expectedLevel)
        {
            Assert.AreEqual(expectedLevel, _system.GetLevelFromXP(xp));
        }

        [Test]
        public void GetLevelFromXP_NegativeXP_ReturnsLevelOne()
        {
            Assert.AreEqual(1, _system.GetLevelFromXP(-100));
        }

        // ── IsReadyToLevelUp — XP mode ────────────────────────────────────────

        [Test]
        public void IsReadyToLevelUp_XPMode_BelowThreshold_ReturnsFalse()
        {
            // Level 1 → 2 requires 300 XP; 299 is not enough.
            Assert.IsFalse(_system.IsReadyToLevelUp(1, 299, LevelingMode.ExperiencePoints));
        }

        [Test]
        public void IsReadyToLevelUp_XPMode_AtThreshold_ReturnsTrue()
        {
            Assert.IsTrue(_system.IsReadyToLevelUp(1, 300, LevelingMode.ExperiencePoints));
        }

        [Test]
        public void IsReadyToLevelUp_XPMode_AboveThreshold_ReturnsTrue()
        {
            Assert.IsTrue(_system.IsReadyToLevelUp(1, 1_000, LevelingMode.ExperiencePoints));
        }

        [Test]
        public void IsReadyToLevelUp_XPMode_AtMaxLevel_ReturnsFalse()
        {
            // Already level 20 — cannot level up further.
            Assert.IsFalse(_system.IsReadyToLevelUp(20, 999_999, LevelingMode.ExperiencePoints));
        }

        // ── IsReadyToLevelUp — milestone mode ────────────────────────────────

        [TestCase(1,   300)]
        [TestCase(5, 6_500)]
        [TestCase(19, 355_000)]
        public void IsReadyToLevelUp_MilestoneMode_AlwaysReturnsFalse(int level, int xp)
        {
            // Milestone leveling is a DM decision — XP is irrelevant.
            Assert.IsFalse(_system.IsReadyToLevelUp(level, xp, LevelingMode.Milestone));
        }

        // ── IsReadyToLevelUp — invalid current level ─────────────────────────

        [TestCase(0)]
        [TestCase(21)]
        public void IsReadyToLevelUp_InvalidCurrentLevel_Throws(int currentLevel)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _system.IsReadyToLevelUp(currentLevel, 0, LevelingMode.ExperiencePoints));
        }

        // ── GetXPToNextLevel ──────────────────────────────────────────────────

        [Test]
        public void GetXPToNextLevel_AtLevelOne_ReturnsXPForLevelTwo()
        {
            // 0 XP, level 1 → need 300 to reach level 2.
            Assert.AreEqual(300, _system.GetXPToNextLevel(0));
        }

        [Test]
        public void GetXPToNextLevel_PartialProgress_ReturnsRemainder()
        {
            // 500 XP: currently level 2 (threshold 300), next = level 3 at 900 → need 400 more.
            Assert.AreEqual(400, _system.GetXPToNextLevel(500));
        }

        [Test]
        public void GetXPToNextLevel_AtExactThreshold_ReturnsFullNextRange()
        {
            // 300 XP exactly: now level 2, next = level 3 at 900 → need 600 more.
            Assert.AreEqual(600, _system.GetXPToNextLevel(300));
        }

        [Test]
        public void GetXPToNextLevel_AtMaxLevel_ReturnsZero()
        {
            Assert.AreEqual(0, _system.GetXPToNextLevel(355_000));
        }

        [Test]
        public void GetXPToNextLevel_NegativeXP_TreatedAsZero()
        {
            // Same result as 0 XP.
            Assert.AreEqual(_system.GetXPToNextLevel(0), _system.GetXPToNextLevel(-999));
        }
    }
}