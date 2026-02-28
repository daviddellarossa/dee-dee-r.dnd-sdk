using System;
using NUnit.Framework;
using UnityEngine;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.State;
using DeeDeeR.DnD.Runtime.Systems;

namespace DeeDeeR.DnD.Tests.Editor.Systems
{
    [TestFixture]
    public class WeaponMasterySystemTests
    {
        private WeaponMasterySystem _system;
        private WeaponSO            _weapon;

        [SetUp]
        public void SetUp()
        {
            _system = new WeaponMasterySystem();
            _weapon = ScriptableObject.CreateInstance<WeaponSO>();
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(_weapon);

        // ── CanUseMastery ─────────────────────────────────────────────────────

        [Test]
        public void CanUseMastery_NullState_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _system.CanUseMastery(null, _weapon));
        }

        [Test]
        public void CanUseMastery_NullWeapon_ReturnsFalse()
        {
            Assert.IsFalse(_system.CanUseMastery(new CharacterState(), null));
        }

        [Test]
        public void CanUseMastery_WeaponHasNoMastery_ReturnsFalse()
        {
            _weapon.MasteryProperty = WeaponMastery.None;
            var state = new CharacterState();
            state.WeaponMasteries.Add(WeaponMastery.Push); // has mastery, but weapon has None
            Assert.IsFalse(_system.CanUseMastery(state, _weapon));
        }

        [Test]
        public void CanUseMastery_MasteryNotInState_ReturnsFalse()
        {
            _weapon.MasteryProperty = WeaponMastery.Push;
            Assert.IsFalse(_system.CanUseMastery(new CharacterState(), _weapon));
        }

        [Test]
        public void CanUseMastery_MasteryPresentInState_ReturnsTrue()
        {
            _weapon.MasteryProperty = WeaponMastery.Topple;
            var state = new CharacterState();
            state.WeaponMasteries.Add(WeaponMastery.Topple);
            Assert.IsTrue(_system.CanUseMastery(state, _weapon));
        }

        // ── GetMasteryEffect ──────────────────────────────────────────────────

        [Test]
        public void GetMasteryEffect_Cleave_GrantsFreeAttack()
        {
            var effect = _system.GetMasteryEffect(WeaponMastery.Cleave);
            Assert.IsTrue(effect.GrantsFreeAttack);
            Assert.IsFalse(effect.DealsDamageOnMiss);
        }

        [Test]
        public void GetMasteryEffect_Graze_DealsDamageOnMiss()
        {
            var effect = _system.GetMasteryEffect(WeaponMastery.Graze);
            Assert.IsTrue(effect.DealsDamageOnMiss);
            Assert.IsFalse(effect.GrantsFreeAttack);
        }

        [Test]
        public void GetMasteryEffect_Nick_OffHandAttackIsFree()
        {
            var effect = _system.GetMasteryEffect(WeaponMastery.Nick);
            Assert.IsTrue(effect.OffHandAttackIsFree);
        }

        [Test]
        public void GetMasteryEffect_Push_PushesTargetTenFeet()
        {
            var effect = _system.GetMasteryEffect(WeaponMastery.Push);
            Assert.IsTrue(effect.PushesTarget);
            Assert.AreEqual(10, effect.PushDistance);
        }

        [Test]
        public void GetMasteryEffect_Sap_SapsTarget()
        {
            var effect = _system.GetMasteryEffect(WeaponMastery.Sap);
            Assert.IsTrue(effect.SapsTarget);
        }

        [Test]
        public void GetMasteryEffect_Slow_SlowsTargetTenFeet()
        {
            var effect = _system.GetMasteryEffect(WeaponMastery.Slow);
            Assert.IsTrue(effect.SlowsTarget);
            Assert.AreEqual(10, effect.SpeedReduction);
        }

        [Test]
        public void GetMasteryEffect_Topple_TopplesToTarget()
        {
            var effect = _system.GetMasteryEffect(WeaponMastery.Topple);
            Assert.IsTrue(effect.TopplesToTarget);
        }

        [Test]
        public void GetMasteryEffect_Vex_GrantsAttackerAdvantage()
        {
            var effect = _system.GetMasteryEffect(WeaponMastery.Vex);
            Assert.IsTrue(effect.GrantsAttackerAdvantage);
        }

        [Test]
        public void GetMasteryEffect_None_HasNoFlags()
        {
            var effect = _system.GetMasteryEffect(WeaponMastery.None);
            Assert.IsFalse(effect.GrantsFreeAttack);
            Assert.IsFalse(effect.DealsDamageOnMiss);
            Assert.IsFalse(effect.OffHandAttackIsFree);
            Assert.IsFalse(effect.PushesTarget);
            Assert.IsFalse(effect.SapsTarget);
            Assert.IsFalse(effect.SlowsTarget);
            Assert.IsFalse(effect.TopplesToTarget);
            Assert.IsFalse(effect.GrantsAttackerAdvantage);
        }
    }
}