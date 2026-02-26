using NUnit.Framework;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Tests.Editor.Values
{
    [TestFixture]
    public class CurrencyTests
    {
        // ── TotalInCopper ────────────────────────────────────────────────────

        [Test]
        public void TotalInCopper_CopperOnly_ReturnsDirectValue()
        {
            Assert.AreEqual(5, new Currency(cp: 5).TotalInCopper);
        }

        [Test]
        public void TotalInCopper_SilverIstenCopper()
        {
            Assert.AreEqual(10, new Currency(sp: 1).TotalInCopper);
        }

        [Test]
        public void TotalInCopper_ElectrumIsFiftyCopper()
        {
            Assert.AreEqual(50, new Currency(ep: 1).TotalInCopper);
        }

        [Test]
        public void TotalInCopper_GoldIsOneHundredCopper()
        {
            Assert.AreEqual(100, new Currency(gp: 1).TotalInCopper);
        }

        [Test]
        public void TotalInCopper_PlatinumIsOneThousandCopper()
        {
            Assert.AreEqual(1000, new Currency(pp: 1).TotalInCopper);
        }

        [Test]
        public void TotalInCopper_MixedDenominations_SumsCorrectly()
        {
            // 1pp=1000, 2gp=200, 3ep=150, 4sp=40, 5cp=5 → 1395
            var c = new Currency(cp: 5, sp: 4, ep: 3, gp: 2, pp: 1);
            Assert.AreEqual(1395, c.TotalInCopper);
        }

        [Test]
        public void Zero_HasTotalInCopperOfZero()
        {
            Assert.AreEqual(0, Currency.Zero.TotalInCopper);
        }

        // ── FromGold ─────────────────────────────────────────────────────────

        [Test]
        public void FromGold_SetsGoldFieldOnly()
        {
            var c = Currency.FromGold(10);
            Assert.AreEqual(10, c.GP);
            Assert.AreEqual(0, c.SP);
            Assert.AreEqual(0, c.CP);
        }

        // ── Addition ─────────────────────────────────────────────────────────

        [Test]
        public void Addition_SumsEachDenomination()
        {
            var a = new Currency(cp: 3, gp: 2);
            var b = new Currency(cp: 2, gp: 5);
            var result = a + b;
            Assert.AreEqual(5,  result.CP);
            Assert.AreEqual(7,  result.GP);
        }

        // ── Subtraction ──────────────────────────────────────────────────────

        [Test]
        public void Subtraction_SubtractsEachDenomination()
        {
            var a = new Currency(gp: 10, sp: 5);
            var b = new Currency(gp: 3,  sp: 2);
            var result = a - b;
            Assert.AreEqual(7, result.GP);
            Assert.AreEqual(3, result.SP);
        }

        // ── Equality ─────────────────────────────────────────────────────────

        [Test]
        public void Equality_SameValues_ReturnsTrue()
        {
            var a = new Currency(cp: 1, sp: 2, ep: 3, gp: 4, pp: 5);
            var b = new Currency(cp: 1, sp: 2, ep: 3, gp: 4, pp: 5);
            Assert.IsTrue(a == b);
        }

        [Test]
        public void Equality_DifferentValues_ReturnsFalse()
        {
            var a = Currency.FromGold(5);
            var b = Currency.FromGold(6);
            Assert.IsTrue(a != b);
        }
    }
}
