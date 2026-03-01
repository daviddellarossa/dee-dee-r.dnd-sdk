using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.State;
using DeeDeeR.DnD.Runtime.Systems;

namespace DeeDeeR.DnD.Tests.Editor.Systems
{
    [TestFixture]
    public class RestSystemTests
    {
        private RestSystem _system;
        private ClassSO    _fighterClass; // D10 hit die, non-caster

        [SetUp]
        public void SetUp()
        {
            _system = new RestSystem();

            _fighterClass         = ScriptableObject.CreateInstance<ClassSO>();
            _fighterClass.HitDie  = DieType.D10;
            _fighterClass.CasterType = CasterType.None;
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_fighterClass);
        }

        // ── TakeShortRest — null guards ───────────────────────────────────────

        [Test]
        public void TakeShortRest_NullRecord_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _system.TakeShortRest(null, new CharacterState(), EmptyDice(), new FakeRollProvider()));
        }

        [Test]
        public void TakeShortRest_NullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _system.TakeShortRest(MakeRecord(), null, EmptyDice(), new FakeRollProvider()));
        }

        [Test]
        public void TakeShortRest_NullDiceToSpend_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _system.TakeShortRest(MakeRecord(), new CharacterState(), null, new FakeRollProvider()));
        }

        [Test]
        public void TakeShortRest_NullRoller_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _system.TakeShortRest(MakeRecord(), new CharacterState(), EmptyDice(), null));
        }

        // ── TakeShortRest — mechanics ─────────────────────────────────────────

        [Test]
        public void TakeShortRest_SpendOneDie_HealsRollPlusConMod()
        {
            // CON 16 → mod +3; roll 7 → heal 10; HP 5/20 → 15/20.
            var record = MakeRecord(con: 16);
            var state  = new CharacterState
            {
                HitPoints         = new HitPointState(5, 20),
                HitDiceAvailable  = new Dictionary<DieType, int> { [DieType.D10] = 3 }
            };
            _system.TakeShortRest(record, state, Dice(DieType.D10, 1), new FakeRollProvider(7));
            Assert.AreEqual(15, state.HitPoints.Current);
        }

        [Test]
        public void TakeShortRest_DeductsSpentDiceFromPool()
        {
            var record = MakeRecord();
            var state  = new CharacterState
            {
                HitPoints        = new HitPointState(10, 20),
                HitDiceAvailable = new Dictionary<DieType, int> { [DieType.D10] = 4 }
            };
            _system.TakeShortRest(record, state, Dice(DieType.D10, 2), new FakeRollProvider(5, 5));
            Assert.AreEqual(2, state.HitDiceAvailable[DieType.D10]);
        }

        [Test]
        public void TakeShortRest_HealingCappedAtMaximum()
        {
            // CON 10 → mod 0; roll 8; HP 18/20 → capped at 20.
            var record = MakeRecord(con: 10);
            var state  = new CharacterState
            {
                HitPoints        = new HitPointState(18, 20),
                HitDiceAvailable = new Dictionary<DieType, int> { [DieType.D10] = 1 }
            };
            _system.TakeShortRest(record, state, Dice(DieType.D10, 1), new FakeRollProvider(8));
            Assert.AreEqual(20, state.HitPoints.Current);
        }

        [Test]
        public void TakeShortRest_MoreDiceRequestedThanAvailable_Throws()
        {
            var state = new CharacterState
            {
                HitDiceAvailable = new Dictionary<DieType, int> { [DieType.D10] = 1 }
            };
            Assert.Throws<InvalidOperationException>(() =>
                _system.TakeShortRest(MakeRecord(), state, Dice(DieType.D10, 2), new FakeRollProvider(5)));
        }

        [Test]
        public void TakeShortRest_NegativeTotalFromLowConMod_NeverReducesHP()
        {
            // CON 4 → mod −3; roll 1 on d10 → total −2 (negative) → no healing.
            var record = MakeRecord(con: 4);
            var state  = new CharacterState
            {
                HitPoints        = new HitPointState(5, 20),
                HitDiceAvailable = new Dictionary<DieType, int> { [DieType.D10] = 1 }
            };
            _system.TakeShortRest(record, state, Dice(DieType.D10, 1), new FakeRollProvider(1));
            Assert.AreEqual(5, state.HitPoints.Current); // unchanged
        }

        [Test]
        public void TakeShortRest_EmptyDiceToSpend_HealsNothing()
        {
            var state = new CharacterState { HitPoints = new HitPointState(5, 20) };
            _system.TakeShortRest(MakeRecord(), state, EmptyDice(), new FakeRollProvider());
            Assert.AreEqual(5, state.HitPoints.Current);
        }

        [Test]
        public void TakeShortRest_HealingFromZeroHP_ResetsDeathSavesAndRemovesUnconscious()
        {
            // Character was dying (0 HP, Unconscious, 2 death-save failures).
            // Spending a hit die should restore HP and end the dying state.
            var state = new CharacterState
            {
                HitPoints        = new HitPointState(0, 20),
                DeathSaves       = new DeathSaveState().WithFailure().WithFailure(),
                HitDiceAvailable = new Dictionary<DieType, int> { [DieType.D10] = 1 }
            };
            state.Conditions.Add(Condition.Unconscious);

            _system.TakeShortRest(MakeRecord(con: 10), state, Dice(DieType.D10, 1), new FakeRollProvider(6));

            Assert.IsTrue(state.HitPoints.Current > 0);
            Assert.AreEqual(0, state.DeathSaves.Failures);
            Assert.IsFalse(state.Conditions.Contains(Condition.Unconscious));
        }

        // ── TakeLongRest — null guards ────────────────────────────────────────

        [Test]
        public void TakeLongRest_NullRecord_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _system.TakeLongRest(null, new CharacterState()));
        }

        [Test]
        public void TakeLongRest_NullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _system.TakeLongRest(MakeRecord(), null));
        }

        // ── TakeLongRest — HP ─────────────────────────────────────────────────

        [Test]
        public void TakeLongRest_RestoresHPToMaximum()
        {
            var state = new CharacterState { HitPoints = new HitPointState(3, 20) };
            _system.TakeLongRest(MakeRecord(), state);
            Assert.AreEqual(20, state.HitPoints.Current);
        }

        [Test]
        public void TakeLongRest_ClearsTemporaryHP()
        {
            var state = new CharacterState { HitPoints = new HitPointState(20, 20, temporary: 8) };
            _system.TakeLongRest(MakeRecord(), state);
            Assert.AreEqual(0, state.HitPoints.Temporary);
        }

        // ── TakeLongRest — Hit Dice ───────────────────────────────────────────

        [Test]
        public void TakeLongRest_RestoresAllHitDice()
        {
            var record = MakeRecord();
            record.ClassLevels.Add(new ClassLevel { Class = _fighterClass, Level = 4 });
            var state = new CharacterState
            {
                HitDiceAvailable = new Dictionary<DieType, int> { [DieType.D10] = 1 } // spent 3
            };
            _system.TakeLongRest(record, state);
            Assert.AreEqual(4, state.HitDiceAvailable[DieType.D10]);
        }

        [Test]
        public void TakeLongRest_NoClassLevels_HitDiceAvailableIsEmpty()
        {
            var state = new CharacterState();
            _system.TakeLongRest(MakeRecord(), state);
            Assert.AreEqual(0, state.HitDiceAvailable.Count);
        }

        // ── TakeLongRest — Spell Slots ────────────────────────────────────────

        [Test]
        public void TakeLongRest_NonCaster_SpellSlotsRemainEmpty()
        {
            var record = MakeRecord();
            record.ClassLevels.Add(new ClassLevel { Class = _fighterClass, Level = 5 });
            var state = new CharacterState(); // no slots
            _system.TakeLongRest(record, state);
            Assert.AreEqual(SpellSlotState.Empty, state.SpellSlots);
        }

        // ── TakeLongRest — Exhaustion ─────────────────────────────────────────

        [Test]
        public void TakeLongRest_ReducesExhaustionByOne()
        {
            var state = new CharacterState { Exhaustion = new ExhaustionLevel(3) };
            _system.TakeLongRest(MakeRecord(), state);
            Assert.AreEqual(2, state.Exhaustion.Value);
        }

        [Test]
        public void TakeLongRest_ExhaustionAtZero_StaysAtZero()
        {
            var state = new CharacterState { Exhaustion = ExhaustionLevel.None };
            _system.TakeLongRest(MakeRecord(), state);
            Assert.AreEqual(0, state.Exhaustion.Value);
        }

        // ── TakeLongRest — Action flags ───────────────────────────────────────

        [Test]
        public void TakeLongRest_ResetsDeathSavesAndRemovesUnconscious()
        {
            // Character was at 0 HP before resting — long rest should end the dying state.
            var state = new CharacterState
            {
                HitPoints  = new HitPointState(0, 20),
                DeathSaves = new DeathSaveState().WithFailure()
            };
            state.Conditions.Add(Condition.Unconscious);

            _system.TakeLongRest(MakeRecord(), state);

            Assert.AreEqual(0, state.DeathSaves.Failures);
            Assert.IsFalse(state.Conditions.Contains(Condition.Unconscious));
        }

        [Test]
        public void TakeLongRest_ResetsActionEconomyFlags()
        {
            var state = new CharacterState
            {
                ActionUsed      = true,
                BonusActionUsed = true,
                ReactionUsed    = true
            };
            _system.TakeLongRest(MakeRecord(), state);
            Assert.IsFalse(state.ActionUsed);
            Assert.IsFalse(state.BonusActionUsed);
            Assert.IsFalse(state.ReactionUsed);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static CharacterRecord MakeRecord(int con = 10) => new CharacterRecord
        {
            AbilityScores = new AbilityScoreSet(10, 10, con, 10, 10, 10)
        };

        private static Dictionary<DieType, int> EmptyDice() => new Dictionary<DieType, int>();

        private static Dictionary<DieType, int> Dice(DieType die, int count) =>
            new Dictionary<DieType, int> { [die] = count };
    }
}