using NUnit.Framework;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Systems;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Tests.Editor.Systems
{
    [TestFixture]
    public class DiceRollerTests
    {
        // ── Flat expression ──────────────────────────────────────────────────

        [Test]
        public void Roll_FlatExpression_ReturnsModifierOnly()
        {
            var roller = new DiceRoller(new FakeRollProvider());
            var result = roller.Roll(DiceExpression.Flat(5));
            Assert.AreEqual(5, result.Total);
        }

        [Test]
        public void Roll_FlatExpression_NoCritFlags()
        {
            var roller = new DiceRoller(new FakeRollProvider());
            var result = roller.Roll(DiceExpression.Flat(20));
            Assert.IsFalse(result.IsCriticalSuccess);
            Assert.IsFalse(result.IsCriticalFail);
        }

        // ── Single die ───────────────────────────────────────────────────────

        [Test]
        public void Roll_SingleDieWithModifier_ReturnsDiePlusModifier()
        {
            // 1d6+3, die shows 4 → total = 7
            var roller = new DiceRoller(new FakeRollProvider(4));
            var result = roller.Roll(new DiceExpression(1, DieType.D6, modifier: 3));
            Assert.AreEqual(7, result.Total);
        }

        [Test]
        public void Roll_SingleDieNegativeModifier_SubtractsModifier()
        {
            // 1d8-2, die shows 5 → total = 3
            var roller = new DiceRoller(new FakeRollProvider(5));
            var result = roller.Roll(new DiceExpression(1, DieType.D8, modifier: -2));
            Assert.AreEqual(3, result.Total);
        }

        // ── Multiple dice ────────────────────────────────────────────────────

        [Test]
        public void Roll_MultipleDice_SumsAllDice()
        {
            // 3d6, dice show 2, 4, 6 → total = 12
            var roller = new DiceRoller(new FakeRollProvider(2, 4, 6));
            var result = roller.Roll(new DiceExpression(3, DieType.D6));
            Assert.AreEqual(12, result.Total);
        }

        [Test]
        public void Roll_MultipleDiceWithModifier_SumsDiceAndModifier()
        {
            // 2d8+2, dice show 3, 5 → total = 10
            var roller = new DiceRoller(new FakeRollProvider(3, 5));
            var result = roller.Roll(new DiceExpression(2, DieType.D8, modifier: 2));
            Assert.AreEqual(10, result.Total);
        }

        // ── Critical success (natural 20) ────────────────────────────────────

        [Test]
        public void Roll_SingleD20_Natural20_SetsCriticalSuccess()
        {
            var roller = new DiceRoller(new FakeRollProvider(20));
            var result = roller.Roll(new DiceExpression(1, DieType.D20));
            Assert.IsTrue(result.IsCriticalSuccess);
            Assert.IsFalse(result.IsCriticalFail);
        }

        [Test]
        public void Roll_SingleD20_Natural20_TotalIncludesModifier()
        {
            // 1d20+5, die shows 20 → total = 25
            var roller = new DiceRoller(new FakeRollProvider(20));
            var result = roller.Roll(new DiceExpression(1, DieType.D20, modifier: 5));
            Assert.AreEqual(25, result.Total);
            Assert.IsTrue(result.IsCriticalSuccess);
        }

        // ── Critical fail (natural 1) ─────────────────────────────────────────

        [Test]
        public void Roll_SingleD20_Natural1_SetsCriticalFail()
        {
            var roller = new DiceRoller(new FakeRollProvider(1));
            var result = roller.Roll(new DiceExpression(1, DieType.D20));
            Assert.IsTrue(result.IsCriticalFail);
            Assert.IsFalse(result.IsCriticalSuccess);
        }

        // ── Normal d20 roll ──────────────────────────────────────────────────

        [Test]
        public void Roll_SingleD20_NormalRoll_NoCritFlags()
        {
            var roller = new DiceRoller(new FakeRollProvider(10));
            var result = roller.Roll(new DiceExpression(1, DieType.D20));
            Assert.IsFalse(result.IsCriticalSuccess);
            Assert.IsFalse(result.IsCriticalFail);
        }

        // ── Crit flags require exactly one d20 ───────────────────────────────

        [Test]
        public void Roll_TwoD20AllTwenties_NoCritFlags()
        {
            // 2d20 (e.g. raw advantage pool) — caller selects; DiceRoller does not flag crits.
            var roller = new DiceRoller(new FakeRollProvider(20, 20));
            var result = roller.Roll(new DiceExpression(2, DieType.D20));
            Assert.IsFalse(result.IsCriticalSuccess);
            Assert.IsFalse(result.IsCriticalFail);
        }

        [Test]
        public void Roll_SingleD6ShowingSix_NoCritFlags()
        {
            // Only d20 can crit — a max roll on any other die is not a critical success.
            var roller = new DiceRoller(new FakeRollProvider(6));
            var result = roller.Roll(new DiceExpression(1, DieType.D6));
            Assert.IsFalse(result.IsCriticalSuccess);
        }

        // ── FakeRollProvider exhaustion guard ────────────────────────────────

        [Test]
        public void FakeRollProvider_WhenExhausted_ThrowsInvalidOperationException()
        {
            var provider = new FakeRollProvider(5); // only one result
            provider.RollDie(6);                    // consume it

            Assert.Throws<System.InvalidOperationException>(() => provider.RollDie(6));
        }
    }
}