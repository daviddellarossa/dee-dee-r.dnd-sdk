using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Bus.Args;
using DeeDeeR.DnD.Runtime.Components;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Tests.Runtime.Components
{
    /// <summary>
    /// PlayMode tests for <see cref="CombatantComponent"/>:
    /// turn management, bus signals, and attack resolution.
    /// </summary>
    [TestFixture]
    public class CombatantComponentTests
    {
        private readonly List<GameObject> _createdGos  = new List<GameObject>();
        private readonly List<Object>     _createdSOs  = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            // Create runner so Bus is available for all tests.
            var go = new GameObject("Runner");
            _createdGos.Add(go);
            go.SetActive(false);
            var scheduler = go.AddComponent<FrameSchedulerBehaviour>();
            var runner    = go.AddComponent<DnDSdkRunner>();
            typeof(DnDSdkRunner)
                .GetField("_scheduler", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(runner, scheduler);
            go.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _createdGos)
                if (go != null) Object.DestroyImmediate(go);
            _createdGos.Clear();

            foreach (var so in _createdSOs)
                if (so != null) Object.DestroyImmediate(so);
            _createdSOs.Clear();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Creates a combatant with CharacterComponent + CombatantComponent.</summary>
        private (CharacterComponent character, CombatantComponent combatant)
            CreateCombatant(string name = "Fighter", int maxHp = 20, int strScore = 16)
        {
            var go = new GameObject(name);
            _createdGos.Add(go);
            var ch  = go.AddComponent<CharacterComponent>();
            var cbt = go.AddComponent<CombatantComponent>();

            ch.State.HitPoints      = new HitPointState(maxHp, maxHp);
            ch.Record.AbilityScores = new AbilityScoreSet(strScore, 14, 15, 10, 12, 8);

            // Pre-set flags as used so StartTurn tests can verify the reset.
            ch.State.ActionUsed      = true;
            ch.State.BonusActionUsed = true;
            ch.State.ReactionUsed    = true;

            return (ch, cbt);
        }

        private WeaponSO CreateLongsword()
        {
            var w = ScriptableObject.CreateInstance<WeaponSO>();
            _createdSOs.Add(w);
            w.Category         = WeaponCategory.Martial;
            w.DamageDice.Count = 1;
            w.DamageDice.Die   = DieType.D8;
            w.DamageType       = DamageType.Slashing;
            w.Properties       = System.Array.Empty<WeaponProperty>();
            return w;
        }

        // ── StartTurn ─────────────────────────────────────────────────────────

        [Test]
        public void StartTurn_ResetsAllActionFlags()
        {
            var (ch, cbt) = CreateCombatant();
            cbt.StartTurn();

            Assert.IsFalse(ch.State.ActionUsed,      "ActionUsed should be reset.");
            Assert.IsFalse(ch.State.BonusActionUsed, "BonusActionUsed should be reset.");
            Assert.IsFalse(ch.State.ReactionUsed,    "ReactionUsed should be reset.");
        }

        [Test]
        public void StartTurn_PublishesTurnStarted()
        {
            var (ch, cbt) = CreateCombatant();

            TurnArgs? captured = null;
            DnDSdkRunner.Bus!.Combat.TurnStarted.Subscribe(
                new EndpointId("test-listener"),
                args => captured = args);

            cbt.StartTurn();

            Assert.IsTrue(captured.HasValue, "TurnStarted signal should fire.");
            Assert.AreEqual(ch.EndpointId, captured!.Value.Character);
        }

        // ── EndTurn ───────────────────────────────────────────────────────────

        [Test]
        public void EndTurn_PublishesTurnEnded()
        {
            var (ch, cbt) = CreateCombatant();

            TurnArgs? captured = null;
            DnDSdkRunner.Bus!.Combat.TurnEnded.Subscribe(
                new EndpointId("test-listener"),
                args => captured = args);

            cbt.EndTurn();

            Assert.IsTrue(captured.HasValue, "TurnEnded signal should fire.");
            Assert.AreEqual(ch.EndpointId, captured!.Value.Character);
        }

        // ── PerformAttack ─────────────────────────────────────────────────────

        [Test]
        public void PerformAttack_WhenHit_ReducesTargetHitPoints()
        {
            var (_, attacker) = CreateCombatant("Attacker", maxHp: 20, strScore: 16);
            var (targetCh, _) = CreateCombatant("Target",   maxHp: 20);
            var weapon        = CreateLongsword();

            // FakeRollProvider(20): d20 = 20 (nat-20 → crit hit), damage dice roll max.
            var result = attacker.PerformAttack(targetCh, weapon, AdvantageState.Normal, new FakeRollProvider(20));

            Assert.IsTrue(result.Hit,      "Expected a hit with nat-20.");
            Assert.IsTrue(result.Critical, "Expected a critical hit with nat-20.");
            Assert.Less(targetCh.State.HitPoints.Current, 20, "Target HP should have decreased.");
        }

        [Test]
        public void PerformAttack_WhenMiss_DoesNotChangeTargetHitPoints()
        {
            var (_, attacker) = CreateCombatant("Attacker", maxHp: 20, strScore: 16);
            var (targetCh, _) = CreateCombatant("Target",   maxHp: 20);
            var weapon        = CreateLongsword();

            // FakeRollProvider(1): d20 = 1 (nat-1 → always misses).
            var result = attacker.PerformAttack(targetCh, weapon, AdvantageState.Normal, new FakeRollProvider(1));

            Assert.IsFalse(result.Hit,   "Expected a miss with nat-1.");
            Assert.IsTrue(result.Fumble, "Expected a fumble with nat-1.");
            Assert.AreEqual(20, targetCh.State.HitPoints.Current, "Target HP must be unchanged on a miss.");
        }

        [Test]
        public void PerformAttack_PublishesAttackMade()
        {
            var (_, attacker) = CreateCombatant("Attacker");
            var (targetCh, _) = CreateCombatant("Target", maxHp: 20);
            var weapon        = CreateLongsword();

            AttackMadeArgs? captured = null;
            DnDSdkRunner.Bus!.Combat.AttackMade.Subscribe(
                new EndpointId("test-listener"),
                args => captured = args);

            attacker.PerformAttack(targetCh, weapon, AdvantageState.Normal, new FakeRollProvider(10));

            Assert.IsTrue(captured.HasValue, "AttackMade signal should always fire.");
        }

        [Test]
        public void PerformAttack_CriticalHit_PublishesCriticalHitSignal()
        {
            var (_, attacker) = CreateCombatant("Attacker");
            var (targetCh, _) = CreateCombatant("Target", maxHp: 20);
            var weapon        = CreateLongsword();

            bool critFired = false;
            DnDSdkRunner.Bus!.Combat.CriticalHit.Subscribe(
                new EndpointId("test-listener"),
                _ => critFired = true);

            attacker.PerformAttack(targetCh, weapon, AdvantageState.Normal, new FakeRollProvider(20));

            Assert.IsTrue(critFired, "CriticalHit signal should fire on nat-20.");
        }

        [Test]
        public void PerformAttack_NullTarget_ThrowsArgumentNullException()
        {
            var (_, attacker) = CreateCombatant("Attacker");
            var weapon        = CreateLongsword();

            Assert.Throws<System.ArgumentNullException>(
                () => attacker.PerformAttack(null, weapon, AdvantageState.Normal, new FakeRollProvider(10)));
        }
    }
}