using NUnit.Framework;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Tests.Editor.Values
{
    [TestFixture]
    public class HitPointStateTests
    {
        // ── Construction ────────────────────────────────────────────────────────

        [Test]
        public void Constructor_ClampsCurrentBelowZero()
        {
            var hp = new HitPointState(-5, 10);
            Assert.AreEqual(0, hp.Current);
        }

        [Test]
        public void Constructor_ClampsTemporaryBelowZero()
        {
            var hp = new HitPointState(10, 10, -3);
            Assert.AreEqual(0, hp.Temporary);
        }

        // ── IsAlive / IsUnconscious ──────────────────────────────────────────

        [Test]
        public void IsAlive_WhenCurrentAboveZero_ReturnsTrue()
        {
            var hp = new HitPointState(1, 10);
            Assert.IsTrue(hp.IsAlive);
        }

        [Test]
        public void IsUnconscious_WhenCurrentIsZero_ReturnsTrue()
        {
            var hp = new HitPointState(0, 10);
            Assert.IsTrue(hp.IsUnconscious);
        }

        // ── WithDamage ───────────────────────────────────────────────────────

        [Test]
        public void WithDamage_ReducesCurrentHP()
        {
            var hp = new HitPointState(10, 10);
            var result = hp.WithDamage(4);
            Assert.AreEqual(6, result.Current);
        }

        [Test]
        public void WithDamage_BurnsTempHPFirst()
        {
            var hp = new HitPointState(10, 10, temporary: 5);
            var result = hp.WithDamage(3);
            Assert.AreEqual(2, result.Temporary);
            Assert.AreEqual(10, result.Current); // current HP untouched
        }

        [Test]
        public void WithDamage_BurnsThroughTempAndIntoCurrentHP()
        {
            var hp = new HitPointState(10, 10, temporary: 3);
            var result = hp.WithDamage(5); // 3 absorbed by temp, 2 into current
            Assert.AreEqual(0, result.Temporary);
            Assert.AreEqual(8, result.Current);
        }

        [Test]
        public void WithDamage_NeverGoesBelowZero()
        {
            var hp = new HitPointState(2, 10);
            var result = hp.WithDamage(100);
            Assert.AreEqual(0, result.Current);
        }

        [Test]
        public void WithDamage_ZeroAmount_ReturnsSameValues()
        {
            var hp = new HitPointState(8, 10, 2);
            var result = hp.WithDamage(0);
            Assert.AreEqual(8, result.Current);
            Assert.AreEqual(2, result.Temporary);
        }

        [Test]
        public void WithDamage_DoesNotChangeMaximum()
        {
            var hp = new HitPointState(10, 10);
            var result = hp.WithDamage(4);
            Assert.AreEqual(10, result.Maximum);
        }

        // ── WithHeal ─────────────────────────────────────────────────────────

        [Test]
        public void WithHeal_IncreasesCurrentHP()
        {
            var hp = new HitPointState(4, 10);
            var result = hp.WithHeal(3);
            Assert.AreEqual(7, result.Current);
        }

        [Test]
        public void WithHeal_CapsAtMaximum()
        {
            var hp = new HitPointState(8, 10);
            var result = hp.WithHeal(10);
            Assert.AreEqual(10, result.Current);
        }

        [Test]
        public void WithHeal_DoesNotAffectTempHP()
        {
            var hp = new HitPointState(5, 10, temporary: 3);
            var result = hp.WithHeal(2);
            Assert.AreEqual(3, result.Temporary);
        }

        [Test]
        public void WithHeal_ZeroAmount_ReturnsSameValues()
        {
            var hp = new HitPointState(5, 10);
            var result = hp.WithHeal(0);
            Assert.AreEqual(5, result.Current);
        }

        // ── WithTempHP (2024: no stacking) ───────────────────────────────────

        [Test]
        public void WithTempHP_SetsTemporaryHP()
        {
            var hp = new HitPointState(10, 10);
            var result = hp.WithTempHP(5);
            Assert.AreEqual(5, result.Temporary);
        }

        [Test]
        public void WithTempHP_DoesNotStack_KeepsHigher()
        {
            var hp = new HitPointState(10, 10, temporary: 8);
            var result = hp.WithTempHP(5); // lower value — should not replace
            Assert.AreEqual(8, result.Temporary);
        }

        [Test]
        public void WithTempHP_ReplacesWithHigherValue()
        {
            var hp = new HitPointState(10, 10, temporary: 3);
            var result = hp.WithTempHP(10); // higher value — replaces
            Assert.AreEqual(10, result.Temporary);
        }

        [Test]
        public void WithTempHP_DoesNotAffectCurrentHP()
        {
            var hp = new HitPointState(7, 10);
            var result = hp.WithTempHP(5);
            Assert.AreEqual(7, result.Current);
        }

        // ── WithMaximum ──────────────────────────────────────────────────────

        [Test]
        public void WithMaximum_UpdatesMaximum()
        {
            var hp = new HitPointState(10, 10);
            var result = hp.WithMaximum(15);
            Assert.AreEqual(15, result.Maximum);
        }

        [Test]
        public void WithMaximum_ClampsCurrentToNewMax_WhenNewMaxLower()
        {
            var hp = new HitPointState(10, 10);
            var result = hp.WithMaximum(6);
            Assert.AreEqual(6, result.Current);
        }

        [Test]
        public void WithMaximum_KeepsCurrentHP_WhenCurrentBelowNewMax()
        {
            var hp = new HitPointState(5, 10);
            var result = hp.WithMaximum(20);
            Assert.AreEqual(5, result.Current);
        }
    }
}
