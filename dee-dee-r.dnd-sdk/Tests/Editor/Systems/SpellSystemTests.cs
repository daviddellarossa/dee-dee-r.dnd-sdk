using System;
using NUnit.Framework;
using UnityEngine;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.State;
using DeeDeeR.DnD.Runtime.Systems;

namespace DeeDeeR.DnD.Tests.Editor.Systems
{
    [TestFixture]
    public class SpellSystemTests
    {
        private SpellSystem _system;
        private SpellSO     _cantrip;    // Level 0
        private SpellSO     _level1Spell;
        private SpellSO     _level3Spell;

        [SetUp]
        public void SetUp()
        {
            _system = new SpellSystem();

            _cantrip = ScriptableObject.CreateInstance<SpellSO>();
            _cantrip.Level = 0;

            _level1Spell = ScriptableObject.CreateInstance<SpellSO>();
            _level1Spell.Level = 1;

            _level3Spell = ScriptableObject.CreateInstance<SpellSO>();
            _level3Spell.Level = 3;
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_cantrip);
            UnityEngine.Object.DestroyImmediate(_level1Spell);
            UnityEngine.Object.DestroyImmediate(_level3Spell);
        }

        // ── CanCastSpell — null guards ────────────────────────────────────────

        [Test]
        public void CanCastSpell_NullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _system.CanCastSpell(null, _level1Spell, 1));
        }

        [Test]
        public void CanCastSpell_NullSpell_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _system.CanCastSpell(new CharacterState(), null, 1));
        }

        [Test]
        public void CanCastSpell_SlotLevelNegative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _system.CanCastSpell(new CharacterState(), _level1Spell, -1));
        }

        [Test]
        public void CanCastSpell_SlotLevelTen_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _system.CanCastSpell(new CharacterState(), _level1Spell, 10));
        }

        // ── CanCastSpell — cantrips ────────────────────────────────────────────

        [Test]
        public void CanCastSpell_Cantrip_SlotZero_ReturnsTrue()
        {
            Assert.IsTrue(_system.CanCastSpell(new CharacterState(), _cantrip, 0));
        }

        [Test]
        public void CanCastSpell_Cantrip_SlotNonZero_ReturnsFalse()
        {
            // Cantrips never consume slots; slotLevel must be 0.
            var state = new CharacterState { SpellSlots = new SpellSlotState(s1: 4) };
            Assert.IsFalse(_system.CanCastSpell(state, _cantrip, 1));
        }

        // ── CanCastSpell — levelled spells ─────────────────────────────────────

        [Test]
        public void CanCastSpell_LevelledSpell_SlotZero_ReturnsFalse()
        {
            // Can't cast a levelled spell without a slot (slotLevel 0).
            var state = new CharacterState { SpellSlots = new SpellSlotState(s1: 4) };
            Assert.IsFalse(_system.CanCastSpell(state, _level1Spell, 0));
        }

        [Test]
        public void CanCastSpell_SlotBelowSpellLevel_ReturnsFalse()
        {
            // Level 3 spell in a level-2 slot — invalid.
            var state = new CharacterState { SpellSlots = new SpellSlotState(s2: 3) };
            Assert.IsFalse(_system.CanCastSpell(state, _level3Spell, 2));
        }

        [Test]
        public void CanCastSpell_HasMatchingSlot_ReturnsTrue()
        {
            var state = new CharacterState { SpellSlots = new SpellSlotState(s1: 4) };
            Assert.IsTrue(_system.CanCastSpell(state, _level1Spell, 1));
        }

        [Test]
        public void CanCastSpell_NoSlotAtLevel_ReturnsFalse()
        {
            // No level-1 slot available.
            Assert.IsFalse(_system.CanCastSpell(new CharacterState(), _level1Spell, 1));
        }

        [Test]
        public void CanCastSpell_Upcast_SlotAboveSpellLevel_ReturnsTrue()
        {
            // Level 1 spell upcasted in a level-3 slot.
            var state = new CharacterState { SpellSlots = new SpellSlotState(s3: 2) };
            Assert.IsTrue(_system.CanCastSpell(state, _level1Spell, 3));
        }

        // ── ExpendSlot ────────────────────────────────────────────────────────

        [Test]
        public void ExpendSlot_NullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _system.ExpendSlot(null, 1));
        }

        [Test]
        public void ExpendSlot_SlotLevelZero_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _system.ExpendSlot(new CharacterState(), 0));
        }

        [Test]
        public void ExpendSlot_SlotLevelTen_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _system.ExpendSlot(new CharacterState(), 10));
        }

        [Test]
        public void ExpendSlot_NoSlotAvailable_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _system.ExpendSlot(new CharacterState(), 1));
        }

        [Test]
        public void ExpendSlot_HasSlot_DecreasesSlotCount()
        {
            var state = new CharacterState { SpellSlots = new SpellSlotState(s2: 3) };
            _system.ExpendSlot(state, 2);
            Assert.AreEqual(2, state.SpellSlots.GetAvailable(2));
        }

        [Test]
        public void ExpendSlot_HasOneSlot_LeavesZero()
        {
            var state = new CharacterState { SpellSlots = new SpellSlotState(s1: 1) };
            _system.ExpendSlot(state, 1);
            Assert.IsFalse(state.SpellSlots.HasSlot(1));
        }

        // ── BeginConcentration ────────────────────────────────────────────────

        [Test]
        public void BeginConcentration_NullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _system.BeginConcentration(null, _level1Spell));
        }

        [Test]
        public void BeginConcentration_NullSpell_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _system.BeginConcentration(new CharacterState(), null));
        }

        [Test]
        public void BeginConcentration_SetsConcentrationSpell()
        {
            var state = new CharacterState();
            _system.BeginConcentration(state, _level1Spell);
            Assert.AreSame(_level1Spell, state.ConcentrationSpell);
        }

        [Test]
        public void BeginConcentration_OverwritesExistingConcentration()
        {
            var state = new CharacterState { ConcentrationSpell = _level1Spell };
            _system.BeginConcentration(state, _level3Spell);
            Assert.AreSame(_level3Spell, state.ConcentrationSpell);
        }

        // ── BreakConcentration ────────────────────────────────────────────────

        [Test]
        public void BreakConcentration_NullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _system.BreakConcentration(null));
        }

        [Test]
        public void BreakConcentration_ClearsConcentrationSpell()
        {
            var state = new CharacterState { ConcentrationSpell = _level1Spell };
            _system.BreakConcentration(state);
            Assert.IsNull(state.ConcentrationSpell);
        }

        [Test]
        public void BreakConcentration_WhenNotConcentrating_IsIdempotent()
        {
            var state = new CharacterState();
            Assert.DoesNotThrow(() => _system.BreakConcentration(state));
            Assert.IsNull(state.ConcentrationSpell);
        }
    }
}