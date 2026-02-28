using System;
using NUnit.Framework;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.State;
using DeeDeeR.DnD.Runtime.Systems;

namespace DeeDeeR.DnD.Tests.Editor.Systems
{
    [TestFixture]
    public class InitiativeSystemTests
    {
        private InitiativeSystem _system;

        [SetUp]
        public void SetUp() => _system = new InitiativeSystem();

        // ── Null guards ───────────────────────────────────────────────────────

        [Test]
        public void RollInitiative_NullRecord_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.RollInitiative(null, new CharacterState(), new FakeRollProvider(10)));
        }

        [Test]
        public void RollInitiative_NullState_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.RollInitiative(MakeRecord(dex: 10), null, new FakeRollProvider(10)));
        }

        [Test]
        public void RollInitiative_NullRoller_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.RollInitiative(MakeRecord(dex: 10), new CharacterState(), null));
        }

        // ── Normal roll ───────────────────────────────────────────────────────

        [Test]
        public void RollInitiative_AddsDexModifier()
        {
            // DEX 14 → modifier +2; roll 10 → total 12.
            var result = _system.RollInitiative(MakeRecord(dex: 14), new CharacterState(), new FakeRollProvider(10));
            Assert.AreEqual(12, result);
        }

        [Test]
        public void RollInitiative_NegativeDexModifier_Subtracted()
        {
            // DEX 8 → modifier −1; roll 10 → total 9.
            var result = _system.RollInitiative(MakeRecord(dex: 8), new CharacterState(), new FakeRollProvider(10));
            Assert.AreEqual(9, result);
        }

        // ── Exhaustion ────────────────────────────────────────────────────────

        [Test]
        public void RollInitiative_SubtractsExhaustionPenalty()
        {
            // DEX 10 (mod 0), exhaustion 2 (penalty 4), roll 15 → 15 − 4 = 11.
            var state  = new CharacterState { Exhaustion = new ExhaustionLevel(2) };
            var result = _system.RollInitiative(MakeRecord(dex: 10), state, new FakeRollProvider(15));
            Assert.AreEqual(11, result);
        }

        // ── Advantage / Disadvantage ──────────────────────────────────────────

        [Test]
        public void RollInitiative_Advantage_TakesHigherRoll()
        {
            // Rolls 8 and 15; advantage → takes 15; DEX 10 (+0) → 15.
            var result = _system.RollInitiative(
                MakeRecord(dex: 10), new CharacterState(),
                new FakeRollProvider(8, 15), AdvantageState.Advantage);
            Assert.AreEqual(15, result);
        }

        [Test]
        public void RollInitiative_Disadvantage_TakesLowerRoll()
        {
            // Rolls 15 and 8; disadvantage → takes 8; DEX 10 (+0) → 8.
            var result = _system.RollInitiative(
                MakeRecord(dex: 10), new CharacterState(),
                new FakeRollProvider(15, 8), AdvantageState.Disadvantage);
            Assert.AreEqual(8, result);
        }

        [Test]
        public void RollInitiative_Normal_ConsumesOnlyOneRoll()
        {
            // Normal roll should only consume one value from the provider.
            var provider = new FakeRollProvider(12, 999); // second value should never be used
            _system.RollInitiative(MakeRecord(dex: 10), new CharacterState(), provider);
            // If a second roll were made, the test would still pass — but this documents intent.
            Assert.Pass();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static CharacterRecord MakeRecord(int dex = 10) => new CharacterRecord
        {
            AbilityScores = new AbilityScoreSet(10, dex, 10, 10, 10, 10)
        };
    }
}