using NUnit.Framework;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Tests.Editor.Values
{
    [TestFixture]
    public class DeathSaveStateTests
    {
        [Test]
        public void Empty_StartsAtZeroSuccessesAndFailures()
        {
            var state = DeathSaveState.Empty;
            Assert.AreEqual(0, state.Successes);
            Assert.AreEqual(0, state.Failures);
        }

        // ── IsStabilized ─────────────────────────────────────────────────────

        [Test]
        public void IsStabilized_AtThreeSuccesses_ReturnsTrue()
        {
            var state = DeathSaveState.Empty.WithSuccess().WithSuccess().WithSuccess();
            Assert.IsTrue(state.IsStabilized);
        }

        [Test]
        public void IsStabilized_BelowThreeSuccesses_ReturnsFalse()
        {
            var state = DeathSaveState.Empty.WithSuccess().WithSuccess();
            Assert.IsFalse(state.IsStabilized);
        }

        // ── IsDead ───────────────────────────────────────────────────────────

        [Test]
        public void IsDead_AtThreeFailures_ReturnsTrue()
        {
            var state = DeathSaveState.Empty.WithFailure().WithFailure().WithFailure();
            Assert.IsTrue(state.IsDead);
        }

        [Test]
        public void IsDead_BelowThreeFailures_ReturnsFalse()
        {
            var state = DeathSaveState.Empty.WithFailure().WithFailure();
            Assert.IsFalse(state.IsDead);
        }

        // ── Immutability ─────────────────────────────────────────────────────

        [Test]
        public void WithSuccess_DoesNotMutateOriginal()
        {
            var original = DeathSaveState.Empty;
            original.WithSuccess();
            Assert.AreEqual(0, original.Successes);
        }

        [Test]
        public void WithFailure_DoesNotMutateOriginal()
        {
            var original = DeathSaveState.Empty;
            original.WithFailure();
            Assert.AreEqual(0, original.Failures);
        }

        // ── Constructor clamping ─────────────────────────────────────────────

        [Test]
        public void Constructor_NegativeValues_ClampToZero()
        {
            var state = new DeathSaveState(successes: -1, failures: -2);
            Assert.AreEqual(0, state.Successes);
            Assert.AreEqual(0, state.Failures);
        }

        // ── Counters ─────────────────────────────────────────────────────────

        [Test]
        public void WithSuccess_IncrementsSuccesses()
        {
            var state = DeathSaveState.Empty.WithSuccess();
            Assert.AreEqual(1, state.Successes);
            Assert.AreEqual(0, state.Failures);
        }

        [Test]
        public void WithFailure_IncrementsFailures()
        {
            var state = DeathSaveState.Empty.WithFailure();
            Assert.AreEqual(0, state.Successes);
            Assert.AreEqual(1, state.Failures);
        }
    }
}
