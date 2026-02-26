using NUnit.Framework;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Tests.Editor.Values
{
    [TestFixture]
    public class AbilityScoreSetTests
    {
        private static readonly AbilityScoreSet _sample =
            new AbilityScoreSet(str: 16, dex: 14, con: 12, intel: 10, wis: 8, cha: 18);

        // ── GetScore ─────────────────────────────────────────────────────────

        [Test]
        public void GetScore_ReturnsCorrectValueForEachAbility()
        {
            Assert.AreEqual(16, _sample.GetScore(AbilityType.Strength));
            Assert.AreEqual(14, _sample.GetScore(AbilityType.Dexterity));
            Assert.AreEqual(12, _sample.GetScore(AbilityType.Constitution));
            Assert.AreEqual(10, _sample.GetScore(AbilityType.Intelligence));
            Assert.AreEqual(8,  _sample.GetScore(AbilityType.Wisdom));
            Assert.AreEqual(18, _sample.GetScore(AbilityType.Charisma));
        }

        // ── GetModifier (static) ─────────────────────────────────────────────

        // Key values from the PHB modifier table
        [TestCase(1,  -5)]
        [TestCase(2,  -4)]
        [TestCase(3,  -4)]
        [TestCase(4,  -3)]
        [TestCase(5,  -3)]
        [TestCase(8,  -1)]
        [TestCase(9,  -1)]
        [TestCase(10,  0)]
        [TestCase(11,  0)]
        [TestCase(12,  1)]
        [TestCase(13,  1)]
        [TestCase(14,  2)]
        [TestCase(15,  2)]
        [TestCase(16,  3)]
        [TestCase(17,  3)]
        [TestCase(18,  4)]
        [TestCase(19,  4)]
        [TestCase(20,  5)]
        [TestCase(30, 10)]
        public void GetModifier_Static_MatchesPHBTable(int score, int expectedModifier)
        {
            Assert.AreEqual(expectedModifier, AbilityScoreSet.GetModifier(score));
        }

        [Test]
        public void GetModifier_ByAbilityType_MatchesStaticFormula()
        {
            Assert.AreEqual(AbilityScoreSet.GetModifier(16), _sample.GetModifier(AbilityType.Strength));
            Assert.AreEqual(AbilityScoreSet.GetModifier(18), _sample.GetModifier(AbilityType.Charisma));
        }

        // Odd scores must round DOWN (floor), not toward zero
        [TestCase(9,  -1)]
        [TestCase(7,  -2)]
        [TestCase(5,  -3)]
        [TestCase(3,  -4)]
        [TestCase(1,  -5)]
        public void GetModifier_OddScoresBelowTen_RoundDown(int score, int expectedModifier)
        {
            Assert.AreEqual(expectedModifier, AbilityScoreSet.GetModifier(score));
        }

        // ── With ─────────────────────────────────────────────────────────────

        [Test]
        public void With_ReturnsNewSetWithUpdatedScore()
        {
            var updated = _sample.With(AbilityType.Strength, 20);
            Assert.AreEqual(20, updated.GetScore(AbilityType.Strength));
        }

        [Test]
        public void With_DoesNotMutateOtherScores()
        {
            var updated = _sample.With(AbilityType.Strength, 20);
            Assert.AreEqual(14, updated.GetScore(AbilityType.Dexterity));
            Assert.AreEqual(18, updated.GetScore(AbilityType.Charisma));
        }

        [Test]
        public void With_OriginalIsUnchanged()
        {
            _sample.With(AbilityType.Strength, 20);
            Assert.AreEqual(16, _sample.GetScore(AbilityType.Strength));
        }
    }
}
