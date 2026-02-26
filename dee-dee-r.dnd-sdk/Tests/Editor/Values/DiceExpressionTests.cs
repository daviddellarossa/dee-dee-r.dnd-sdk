using NUnit.Framework;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Tests.Editor.Values
{
    [TestFixture]
    public class DiceExpressionTests
    {
        // ── ToString ─────────────────────────────────────────────────────────

        [Test]
        public void ToString_NoDice_NoModifier_ReturnsZero()
        {
            var expr = DiceExpression.Flat(0);
            Assert.AreEqual("0", expr.ToString());
        }

        [Test]
        public void ToString_FlatModifier_ReturnsModifierOnly()
        {
            var expr = DiceExpression.Flat(5);
            Assert.AreEqual("5", expr.ToString());
        }

        [Test]
        public void ToString_DiceNoModifier_ReturnsDiceOnly()
        {
            var expr = new DiceExpression(2, DieType.D6);
            Assert.AreEqual("2d6", expr.ToString());
        }

        [Test]
        public void ToString_DiceWithPositiveModifier_IncludesPlusSign()
        {
            var expr = new DiceExpression(2, DieType.D6, 3);
            Assert.AreEqual("2d6+3", expr.ToString());
        }

        [Test]
        public void ToString_DiceWithNegativeModifier_IncludesMinusSign()
        {
            var expr = new DiceExpression(1, DieType.D8, -1);
            Assert.AreEqual("1d8-1", expr.ToString());
        }

        [Test]
        public void ToString_SingleDie_FormatIsCorrect()
        {
            var expr = new DiceExpression(1, DieType.D20);
            Assert.AreEqual("1d20", expr.ToString());
        }

        // ── Average ──────────────────────────────────────────────────────────

        [Test]
        public void Average_Flat_ReturnsModifier()
        {
            var expr = DiceExpression.Flat(5);
            Assert.AreEqual(5f, expr.Average);
        }

        [Test]
        public void Average_OneDSix_IsThreePointFive()
        {
            var expr = new DiceExpression(1, DieType.D6);
            Assert.AreEqual(3.5f, expr.Average, 0.001f);
        }

        [Test]
        public void Average_TwoDSixPlusThree_IsCorrect()
        {
            // 2d6 avg = 7.0, +3 = 10.0
            var expr = new DiceExpression(2, DieType.D6, 3);
            Assert.AreEqual(10f, expr.Average, 0.001f);
        }

        [Test]
        public void Average_OneDEight_IsFourPointFive()
        {
            var expr = new DiceExpression(1, DieType.D8);
            Assert.AreEqual(4.5f, expr.Average, 0.001f);
        }

        // ── Equality ─────────────────────────────────────────────────────────

        [Test]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new DiceExpression(2, DieType.D6, 3);
            var b = new DiceExpression(2, DieType.D6, 3);
            Assert.IsTrue(a == b);
        }

        [Test]
        public void Equals_DifferentModifier_ReturnsFalse()
        {
            var a = new DiceExpression(2, DieType.D6, 3);
            var b = new DiceExpression(2, DieType.D6, 4);
            Assert.IsTrue(a != b);
        }

        // ── Flat factory ─────────────────────────────────────────────────────

        [Test]
        public void Flat_SetsCountToZero()
        {
            var expr = DiceExpression.Flat(5);
            Assert.AreEqual(0, expr.Count);
        }

        [Test]
        public void Flat_NegativeModifier_IsValid()
        {
            var expr = DiceExpression.Flat(-2);
            Assert.AreEqual("-2", expr.ToString());
        }
    }
}
