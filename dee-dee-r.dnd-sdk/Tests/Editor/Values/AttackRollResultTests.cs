using NUnit.Framework;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Tests.Editor.Values
{
    [TestFixture]
    public class AttackRollResultTests
    {
        // ── Hit determination ────────────────────────────────────────────────

        [Test]
        public void Hit_WhenRollMeetsAC_ReturnsTrue()
        {
            var roll   = new RollResult(total: 15);
            var result = new AttackRollResult(roll, hit: true);
            Assert.IsTrue(result.Hit);
        }

        [Test]
        public void Hit_WhenRollMissesAC_ReturnsFalse()
        {
            var roll   = new RollResult(total: 8);
            var result = new AttackRollResult(roll, hit: false);
            Assert.IsFalse(result.Hit);
        }

        // ── Critical hit (natural 20) always hits ────────────────────────────

        [Test]
        public void Hit_OnCriticalSuccess_IsTrueRegardlessOfHitParameter()
        {
            var roll   = new RollResult(total: 5, isCriticalSuccess: true);
            var result = new AttackRollResult(roll, hit: false); // explicitly passed false
            Assert.IsTrue(result.Hit);  // overridden by crit
        }

        [Test]
        public void Critical_OnNaturalTwenty_ReturnsTrue()
        {
            var roll   = new RollResult(total: 25, isCriticalSuccess: true);
            var result = new AttackRollResult(roll, hit: true);
            Assert.IsTrue(result.Critical);
        }

        [Test]
        public void Critical_OnNormalHit_ReturnsFalse()
        {
            var roll   = new RollResult(total: 18);
            var result = new AttackRollResult(roll, hit: true);
            Assert.IsFalse(result.Critical);
        }

        // ── Fumble (natural 1) always misses ─────────────────────────────────

        [Test]
        public void Fumble_OnNaturalOne_ReturnsTrue()
        {
            var roll   = new RollResult(total: 1, isCriticalFail: true);
            var result = new AttackRollResult(roll, hit: false);
            Assert.IsTrue(result.Fumble);
        }

        [Test]
        public void Fumble_OnNormalMiss_ReturnsFalse()
        {
            var roll   = new RollResult(total: 5);
            var result = new AttackRollResult(roll, hit: false);
            Assert.IsFalse(result.Fumble);
        }
    }
}
