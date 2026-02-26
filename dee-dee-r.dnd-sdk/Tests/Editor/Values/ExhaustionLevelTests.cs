using NUnit.Framework;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Tests.Editor.Values
{
    [TestFixture]
    public class ExhaustionLevelTests
    {
        // ── Clamping ─────────────────────────────────────────────────────────

        [Test]
        public void Constructor_ClampsValueBelowZero()
        {
            var level = new ExhaustionLevel(-1);
            Assert.AreEqual(0, level.Value);
        }

        [Test]
        public void Constructor_ClampsValueAboveSix()
        {
            var level = new ExhaustionLevel(10);
            Assert.AreEqual(6, level.Value);
        }

        [Test]
        public void Constructor_AcceptsValuesZeroToSix()
        {
            for (int i = 0; i <= 6; i++)
                Assert.AreEqual(i, new ExhaustionLevel(i).Value);
        }

        // ── D20Penalty (2024 rule: each level = -2) ──────────────────────────

        [TestCase(0, 0)]
        [TestCase(1, 2)]
        [TestCase(2, 4)]
        [TestCase(3, 6)]
        [TestCase(4, 8)]
        [TestCase(5, 10)]
        [TestCase(6, 12)]
        public void D20Penalty_IsLevelTimesTwo(int value, int expectedPenalty)
        {
            Assert.AreEqual(expectedPenalty, new ExhaustionLevel(value).D20Penalty);
        }

        // ── IsDead ───────────────────────────────────────────────────────────

        [Test]
        public void IsDead_AtLevelSix_ReturnsTrue()
        {
            Assert.IsTrue(ExhaustionLevel.Dead.IsDead);
        }

        [Test]
        public void IsDead_BelowLevelSix_ReturnsFalse()
        {
            Assert.IsFalse(new ExhaustionLevel(5).IsDead);
        }

        // ── IsExhausted ──────────────────────────────────────────────────────

        [Test]
        public void IsExhausted_AtLevelZero_ReturnsFalse()
        {
            Assert.IsFalse(ExhaustionLevel.None.IsExhausted);
        }

        [Test]
        public void IsExhausted_AboveLevelZero_ReturnsTrue()
        {
            Assert.IsTrue(new ExhaustionLevel(1).IsExhausted);
        }

        // ── Increase / Decrease ──────────────────────────────────────────────

        [Test]
        public void Increase_IncrementsValue()
        {
            var level = new ExhaustionLevel(2).Increase();
            Assert.AreEqual(3, level.Value);
        }

        [Test]
        public void Increase_ClampsAtSix()
        {
            var level = new ExhaustionLevel(6).Increase();
            Assert.AreEqual(6, level.Value);
        }

        [Test]
        public void Decrease_DecrementsValue()
        {
            var level = new ExhaustionLevel(3).Decrease();
            Assert.AreEqual(2, level.Value);
        }

        [Test]
        public void Decrease_ClampsAtZero()
        {
            var level = new ExhaustionLevel(0).Decrease();
            Assert.AreEqual(0, level.Value);
        }

        // ── Static constants ─────────────────────────────────────────────────

        [Test]
        public void None_HasValueZero()
        {
            Assert.AreEqual(0, ExhaustionLevel.None.Value);
        }

        [Test]
        public void Dead_HasValueSix()
        {
            Assert.AreEqual(6, ExhaustionLevel.Dead.Value);
        }

        // ── Comparison operators ─────────────────────────────────────────────

        [Test]
        public void LessThan_ReturnsCorrectResult()
        {
            Assert.IsTrue(new ExhaustionLevel(2) < new ExhaustionLevel(4));
            Assert.IsFalse(new ExhaustionLevel(4) < new ExhaustionLevel(2));
        }

        [Test]
        public void GreaterThan_ReturnsCorrectResult()
        {
            Assert.IsTrue(new ExhaustionLevel(5) > new ExhaustionLevel(3));
        }

        [Test]
        public void Equality_SameValue_ReturnsTrue()
        {
            Assert.IsTrue(new ExhaustionLevel(3) == new ExhaustionLevel(3));
        }
    }
}
