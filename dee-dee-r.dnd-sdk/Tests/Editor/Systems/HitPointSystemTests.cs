using System;
using NUnit.Framework;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.State;
using DeeDeeR.DnD.Runtime.Systems;

namespace DeeDeeR.DnD.Tests.Editor.Systems
{
    [TestFixture]
    public class HitPointSystemTests
    {
        private HitPointSystem _system;

        [SetUp]
        public void SetUp() => _system = new HitPointSystem();

        // ── ApplyDamage ───────────────────────────────────────────────────────

        [Test]
        public void ApplyDamage_NullState_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.ApplyDamage(null, 10, DamageType.Fire));
        }

        [Test]
        public void ApplyDamage_ReducesCurrentHP()
        {
            var state = MakeState(current: 20, maximum: 20);
            _system.ApplyDamage(state, 8, DamageType.Slashing);
            Assert.AreEqual(12, state.HitPoints.Current);
        }

        [Test]
        public void ApplyDamage_AbsorbsTempHPFirst()
        {
            var state = MakeState(current: 20, maximum: 20, temporary: 5);
            _system.ApplyDamage(state, 8, DamageType.Bludgeoning);
            // 5 temp absorbed → 3 carry-over to current HP.
            Assert.AreEqual(0,  state.HitPoints.Temporary);
            Assert.AreEqual(17, state.HitPoints.Current);
        }

        [Test]
        public void ApplyDamage_TempHPFullyAbsorbsDamage()
        {
            var state = MakeState(current: 20, maximum: 20, temporary: 10);
            _system.ApplyDamage(state, 6, DamageType.Cold);
            Assert.AreEqual(4,  state.HitPoints.Temporary);
            Assert.AreEqual(20, state.HitPoints.Current);
        }

        [Test]
        public void ApplyDamage_CannotReduceHPBelowZero()
        {
            var state = MakeState(current: 5, maximum: 20);
            _system.ApplyDamage(state, 100, DamageType.Force);
            Assert.AreEqual(0, state.HitPoints.Current);
        }

        [Test]
        public void ApplyDamage_ZeroAmount_IsIgnored()
        {
            var state = MakeState(current: 20, maximum: 20);
            _system.ApplyDamage(state, 0, DamageType.Fire);
            Assert.AreEqual(20, state.HitPoints.Current);
        }

        [Test]
        public void ApplyDamage_NegativeAmount_IsIgnored()
        {
            var state = MakeState(current: 20, maximum: 20);
            _system.ApplyDamage(state, -5, DamageType.Fire);
            Assert.AreEqual(20, state.HitPoints.Current);
        }

        // ── Heal ──────────────────────────────────────────────────────────────

        [Test]
        public void Heal_NullState_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _system.Heal(null, 10));
        }

        [Test]
        public void Heal_RestoresCurrentHP()
        {
            var state = MakeState(current: 8, maximum: 20);
            _system.Heal(state, 7);
            Assert.AreEqual(15, state.HitPoints.Current);
        }

        [Test]
        public void Heal_CapsAtMaximum()
        {
            var state = MakeState(current: 18, maximum: 20);
            _system.Heal(state, 10);
            Assert.AreEqual(20, state.HitPoints.Current);
        }

        [Test]
        public void Heal_ResetsDeathSaves()
        {
            var state = MakeState(current: 0, maximum: 20);
            state.DeathSaves = new DeathSaveState(successes: 2, failures: 1);
            _system.Heal(state, 1);
            Assert.AreEqual(DeathSaveState.Empty, state.DeathSaves);
        }

        [Test]
        public void Heal_RemovesUnconsciousCondition()
        {
            var state = MakeState(current: 0, maximum: 20);
            state.Conditions.Add(Condition.Unconscious);
            _system.Heal(state, 1);
            Assert.IsFalse(state.Conditions.Contains(Condition.Unconscious));
        }

        [Test]
        public void Heal_ZeroAmount_IsIgnored()
        {
            var state = MakeState(current: 10, maximum: 20);
            state.DeathSaves = new DeathSaveState(1, 1);
            _system.Heal(state, 0);
            Assert.AreEqual(10, state.HitPoints.Current);
            Assert.AreNotEqual(DeathSaveState.Empty, state.DeathSaves);
        }

        // ── GainTempHP ────────────────────────────────────────────────────────

        [Test]
        public void GainTempHP_NullState_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _system.GainTempHP(null, 5));
        }

        [Test]
        public void GainTempHP_SetsTemporaryHP()
        {
            var state = MakeState(current: 20, maximum: 20);
            _system.GainTempHP(state, 8);
            Assert.AreEqual(8, state.HitPoints.Temporary);
        }

        [Test]
        public void GainTempHP_DoesNotStackTakesHigher()
        {
            var state = MakeState(current: 20, maximum: 20, temporary: 5);
            _system.GainTempHP(state, 3);   // Lower — no change.
            Assert.AreEqual(5, state.HitPoints.Temporary);
        }

        [Test]
        public void GainTempHP_ReplacesWhenHigher()
        {
            var state = MakeState(current: 20, maximum: 20, temporary: 5);
            _system.GainTempHP(state, 10);  // Higher — replaces.
            Assert.AreEqual(10, state.HitPoints.Temporary);
        }

        [Test]
        public void GainTempHP_ZeroAmount_IsIgnored()
        {
            var state = MakeState(current: 20, maximum: 20, temporary: 5);
            _system.GainTempHP(state, 0);
            Assert.AreEqual(5, state.HitPoints.Temporary);
        }

        // ── RollDeathSave ─────────────────────────────────────────────────────

        [Test]
        public void RollDeathSave_NullState_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.RollDeathSave(null, new FakeRollProvider(10)));
        }

        [Test]
        public void RollDeathSave_NullRoller_ThrowsArgumentNullException()
        {
            var state = MakeState(current: 0, maximum: 20);
            Assert.Throws<ArgumentNullException>(
                () => _system.RollDeathSave(state, null));
        }

        [Test]
        public void RollDeathSave_CharacterAlive_ThrowsInvalidOperationException()
        {
            var state = MakeState(current: 5, maximum: 20);
            Assert.Throws<InvalidOperationException>(
                () => _system.RollDeathSave(state, new FakeRollProvider(10)));
        }

        [Test]
        public void RollDeathSave_Roll10OrAbove_AddsSuccess()
        {
            var state = MakeState(current: 0, maximum: 20);
            _system.RollDeathSave(state, new FakeRollProvider(10));
            Assert.AreEqual(1, state.DeathSaves.Successes);
            Assert.AreEqual(0, state.DeathSaves.Failures);
        }

        [Test]
        public void RollDeathSave_Roll19_AddsSuccess()
        {
            var state = MakeState(current: 0, maximum: 20);
            _system.RollDeathSave(state, new FakeRollProvider(19));
            Assert.AreEqual(1, state.DeathSaves.Successes);
        }

        [Test]
        public void RollDeathSave_Roll9OrBelow_AddsFailure()
        {
            var state = MakeState(current: 0, maximum: 20);
            _system.RollDeathSave(state, new FakeRollProvider(9));
            Assert.AreEqual(0, state.DeathSaves.Successes);
            Assert.AreEqual(1, state.DeathSaves.Failures);
        }

        [Test]
        public void RollDeathSave_Roll2_AddsOneFailure()
        {
            var state = MakeState(current: 0, maximum: 20);
            _system.RollDeathSave(state, new FakeRollProvider(2));
            Assert.AreEqual(1, state.DeathSaves.Failures);
        }

        [Test]
        public void RollDeathSave_NaturalOne_AddsTwoFailures()
        {
            var state = MakeState(current: 0, maximum: 20);
            _system.RollDeathSave(state, new FakeRollProvider(1));
            Assert.AreEqual(2, state.DeathSaves.Failures);
        }

        [Test]
        public void RollDeathSave_NaturalTwenty_RegainsOneHP()
        {
            var state = MakeState(current: 0, maximum: 20);
            _system.RollDeathSave(state, new FakeRollProvider(20));
            Assert.AreEqual(1, state.HitPoints.Current);
        }

        [Test]
        public void RollDeathSave_NaturalTwenty_ResetsDeathSaves()
        {
            var state = MakeState(current: 0, maximum: 20);
            state.DeathSaves = new DeathSaveState(successes: 2, failures: 1);
            _system.RollDeathSave(state, new FakeRollProvider(20));
            Assert.AreEqual(DeathSaveState.Empty, state.DeathSaves);
        }

        [Test]
        public void RollDeathSave_NaturalTwenty_RemovesUnconsciousCondition()
        {
            var state = MakeState(current: 0, maximum: 20);
            state.Conditions.Add(Condition.Unconscious);
            _system.RollDeathSave(state, new FakeRollProvider(20));
            Assert.IsFalse(state.Conditions.Contains(Condition.Unconscious));
        }

        [Test]
        public void RollDeathSave_AlreadyStabilized_ThrowsInvalidOperationException()
        {
            var state = MakeState(current: 0, maximum: 20);
            state.DeathSaves = new DeathSaveState(successes: 3, failures: 0);
            Assert.Throws<InvalidOperationException>(
                () => _system.RollDeathSave(state, new FakeRollProvider(10)));
        }

        [Test]
        public void RollDeathSave_AlreadyDead_ThrowsInvalidOperationException()
        {
            var state = MakeState(current: 0, maximum: 20);
            state.DeathSaves = new DeathSaveState(successes: 0, failures: 3);
            Assert.Throws<InvalidOperationException>(
                () => _system.RollDeathSave(state, new FakeRollProvider(20)));
        }

        [Test]
        public void RollDeathSave_ThreeSuccesses_IsStabilized()
        {
            var state = MakeState(current: 0, maximum: 20);
            _system.RollDeathSave(state, new FakeRollProvider(10));
            _system.RollDeathSave(state, new FakeRollProvider(10));
            _system.RollDeathSave(state, new FakeRollProvider(10));
            Assert.IsTrue(state.DeathSaves.IsStabilized);
        }

        [Test]
        public void RollDeathSave_ThreeFailures_IsDead()
        {
            var state = MakeState(current: 0, maximum: 20);
            _system.RollDeathSave(state, new FakeRollProvider(5));
            _system.RollDeathSave(state, new FakeRollProvider(5));
            _system.RollDeathSave(state, new FakeRollProvider(5));
            Assert.IsTrue(state.DeathSaves.IsDead);
        }

        // ── MassiveDamageCheck ────────────────────────────────────────────────

        [Test]
        public void MassiveDamageCheck_NullState_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _system.MassiveDamageCheck(null, 10));
        }

        [Test]
        public void MassiveDamageCheck_DamageEqualsHalfMax_ReturnsTrue()
        {
            // Max 20, half = 10. Damage = 10 → massive.
            var state = MakeState(current: 20, maximum: 20);
            Assert.IsTrue(_system.MassiveDamageCheck(state, 10));
        }

        [Test]
        public void MassiveDamageCheck_DamageExceedsHalfMax_ReturnsTrue()
        {
            var state = MakeState(current: 20, maximum: 20);
            Assert.IsTrue(_system.MassiveDamageCheck(state, 15));
        }

        [Test]
        public void MassiveDamageCheck_DamageBelowHalfMax_ReturnsFalse()
        {
            // Max 20, half = 10. Damage = 9 → not massive.
            var state = MakeState(current: 20, maximum: 20);
            Assert.IsFalse(_system.MassiveDamageCheck(state, 9));
        }

        [Test]
        public void MassiveDamageCheck_OddMaxHP_ThresholdRoundsUp()
        {
            // Max 21, half = 10.5 → threshold is 11 (damage must be ≥ 11).
            var state = MakeState(current: 21, maximum: 21);
            Assert.IsFalse(_system.MassiveDamageCheck(state, 10));
            Assert.IsTrue(_system.MassiveDamageCheck(state,  11));
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static CharacterState MakeState(int current, int maximum, int temporary = 0)
            => new CharacterState { HitPoints = new HitPointState(current, maximum, temporary) };
    }
}