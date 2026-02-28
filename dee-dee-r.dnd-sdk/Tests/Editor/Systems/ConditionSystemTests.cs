using System;
using NUnit.Framework;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.State;
using DeeDeeR.DnD.Runtime.Systems;

namespace DeeDeeR.DnD.Tests.Editor.Systems
{
    [TestFixture]
    public class ConditionSystemTests
    {
        private ConditionSystem _system;

        [SetUp]
        public void SetUp() => _system = new ConditionSystem();

        // ── Apply ─────────────────────────────────────────────────────────────

        [Test]
        public void Apply_NullState_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.Apply(null, Condition.Blinded));
        }

        [Test]
        public void Apply_AddsConditionToState()
        {
            var state = new CharacterState();
            _system.Apply(state, Condition.Poisoned);
            Assert.IsTrue(state.Conditions.Contains(Condition.Poisoned));
        }

        [Test]
        public void Apply_IsIdempotent()
        {
            var state = new CharacterState();
            _system.Apply(state, Condition.Charmed);
            _system.Apply(state, Condition.Charmed);
            Assert.AreEqual(1, state.Conditions.Count);
        }

        [Test]
        public void Apply_MultipleDistinctConditions_AllPresent()
        {
            var state = new CharacterState();
            _system.Apply(state, Condition.Blinded);
            _system.Apply(state, Condition.Prone);
            _system.Apply(state, Condition.Frightened);
            Assert.AreEqual(3, state.Conditions.Count);
        }

        // ── Remove ────────────────────────────────────────────────────────────

        [Test]
        public void Remove_NullState_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.Remove(null, Condition.Blinded));
        }

        [Test]
        public void Remove_RemovesConditionFromState()
        {
            var state = new CharacterState();
            state.Conditions.Add(Condition.Grappled);
            _system.Remove(state, Condition.Grappled);
            Assert.IsFalse(state.Conditions.Contains(Condition.Grappled));
        }

        [Test]
        public void Remove_ConditionNotPresent_DoesNotThrow()
        {
            var state = new CharacterState();
            Assert.DoesNotThrow(() => _system.Remove(state, Condition.Stunned));
        }

        // ── GetD20Penalty ─────────────────────────────────────────────────────

        [Test]
        public void GetD20Penalty_NullState_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _system.GetD20Penalty(null));
        }

        [Test]
        public void GetD20Penalty_NoExhaustion_ReturnsZero()
        {
            var state = new CharacterState(); // Exhaustion defaults to level 0.
            Assert.AreEqual(0, _system.GetD20Penalty(state));
        }

        [TestCase(1, 2)]
        [TestCase(2, 4)]
        [TestCase(3, 6)]
        [TestCase(4, 8)]
        [TestCase(5, 10)]
        [TestCase(6, 12)]
        public void GetD20Penalty_ExhaustionLevel_ReturnsTwiceLevel(int level, int expectedPenalty)
        {
            var state = new CharacterState { Exhaustion = new ExhaustionLevel(level) };
            Assert.AreEqual(expectedPenalty, _system.GetD20Penalty(state));
        }

        // ── GetConditionEffects ───────────────────────────────────────────────

        [Test]
        public void GetConditionEffects_Blinded_HasCorrectFlags()
        {
            var effects = _system.GetConditionEffects(Condition.Blinded);
            Assert.IsTrue(effects.AttackRollsHaveDisadvantage,     "attack disadvantage");
            Assert.IsTrue(effects.AttackRollsAgainstHaveAdvantage, "attackers have advantage");
            Assert.IsFalse(effects.Incapacitated);
            Assert.IsFalse(effects.SpeedReducedToZero);
        }

        [Test]
        public void GetConditionEffects_Charmed_HasNoFlags()
        {
            var effects = _system.GetConditionEffects(Condition.Charmed);
            Assert.IsFalse(effects.AttackRollsHaveDisadvantage);
            Assert.IsFalse(effects.Incapacitated);
        }

        [Test]
        public void GetConditionEffects_Grappled_SpeedZeroOnly()
        {
            var effects = _system.GetConditionEffects(Condition.Grappled);
            Assert.IsTrue(effects.SpeedReducedToZero);
            Assert.IsFalse(effects.Incapacitated);
            Assert.IsFalse(effects.AttackRollsHaveDisadvantage);
        }

        [Test]
        public void GetConditionEffects_Incapacitated_CannotActOrReact()
        {
            var effects = _system.GetConditionEffects(Condition.Incapacitated);
            Assert.IsTrue(effects.Incapacitated);
        }

        [Test]
        public void GetConditionEffects_Invisible_HasCorrectFlags()
        {
            var effects = _system.GetConditionEffects(Condition.Invisible);
            Assert.IsTrue(effects.AttackRollsHaveAdvantage,                "own attacks have advantage");
            Assert.IsTrue(effects.AttackRollsAgainstHaveDisadvantage,      "attackers have disadvantage");
            Assert.IsFalse(effects.AttackRollsHaveDisadvantage);
            Assert.IsFalse(effects.AttackRollsAgainstHaveAdvantage);
        }

        [Test]
        public void GetConditionEffects_Paralyzed_HasAllCriticalFlags()
        {
            var effects = _system.GetConditionEffects(Condition.Paralyzed);
            Assert.IsTrue(effects.Incapacitated);
            Assert.IsTrue(effects.SpeedReducedToZero);
            Assert.IsTrue(effects.CannotMove);
            Assert.IsTrue(effects.CannotSpeak);
            Assert.IsTrue(effects.AttackRollsAgainstHaveAdvantage);
            Assert.IsTrue(effects.AutoFailStrengthSaves);
            Assert.IsTrue(effects.AutoFailDexteritySaves);
            Assert.IsTrue(effects.MeleeHitsAreCritical);
        }

        [Test]
        public void GetConditionEffects_Unconscious_HasAllCriticalFlags()
        {
            var effects = _system.GetConditionEffects(Condition.Unconscious);
            Assert.IsTrue(effects.Incapacitated);
            Assert.IsTrue(effects.SpeedReducedToZero);
            Assert.IsTrue(effects.CannotMove);
            Assert.IsTrue(effects.CannotSpeak);
            Assert.IsTrue(effects.AttackRollsAgainstHaveAdvantage);
            Assert.IsTrue(effects.AutoFailStrengthSaves);
            Assert.IsTrue(effects.AutoFailDexteritySaves);
            Assert.IsTrue(effects.MeleeHitsAreCritical);
        }

        [Test]
        public void GetConditionEffects_Poisoned_AttackDisadvantageOnly()
        {
            var effects = _system.GetConditionEffects(Condition.Poisoned);
            Assert.IsTrue(effects.AttackRollsHaveDisadvantage);
            Assert.IsFalse(effects.Incapacitated);
            Assert.IsFalse(effects.SpeedReducedToZero);
        }

        [Test]
        public void GetConditionEffects_Prone_AttackDisadvantageAndAttackersAdvantage()
        {
            var effects = _system.GetConditionEffects(Condition.Prone);
            Assert.IsTrue(effects.AttackRollsHaveDisadvantage);
            Assert.IsTrue(effects.AttackRollsAgainstHaveAdvantage);
            Assert.IsFalse(effects.SpeedReducedToZero);
        }

        [Test]
        public void GetConditionEffects_Restrained_SpeedZeroAndBothAttackFlags()
        {
            var effects = _system.GetConditionEffects(Condition.Restrained);
            Assert.IsTrue(effects.SpeedReducedToZero);
            Assert.IsTrue(effects.AttackRollsHaveDisadvantage);
            Assert.IsTrue(effects.AttackRollsAgainstHaveAdvantage);
        }

        [Test]
        public void GetConditionEffects_Stunned_IncapacitatedAndAutoFailSaves()
        {
            var effects = _system.GetConditionEffects(Condition.Stunned);
            Assert.IsTrue(effects.Incapacitated);
            Assert.IsTrue(effects.AutoFailStrengthSaves);
            Assert.IsTrue(effects.AutoFailDexteritySaves);
            Assert.IsTrue(effects.AttackRollsAgainstHaveAdvantage);
            Assert.IsFalse(effects.MeleeHitsAreCritical);
        }

        [Test]
        public void GetConditionEffects_AllConditions_NeverThrows()
        {
            foreach (Condition c in System.Enum.GetValues(typeof(Condition)))
                Assert.DoesNotThrow(() => _system.GetConditionEffects(c));
        }
    }
}