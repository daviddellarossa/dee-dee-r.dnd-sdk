using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Bus.Args;
using DeeDeeR.DnD.Runtime.Components;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Tests.Runtime.Components
{
    /// <summary>
    /// PlayMode tests for <see cref="CharacterComponent"/>:
    /// EndpointId resolution, HP mutation, bus signal publishing, and query registration.
    /// </summary>
    [TestFixture]
    public class CharacterComponentTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created)
                if (go != null) Object.DestroyImmediate(go);
            _created.Clear();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Creates a DnDSdkRunner with a valid FrameSchedulerBehaviour.</summary>
        private void CreateRunner()
        {
            var go = new GameObject("Runner");
            _created.Add(go);
            go.SetActive(false);
            var scheduler = go.AddComponent<FrameSchedulerBehaviour>();
            var runner    = go.AddComponent<DnDSdkRunner>();
            typeof(DnDSdkRunner)
                .GetField("_scheduler", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(runner, scheduler);
            go.SetActive(true);
        }

        /// <summary>Creates a CharacterComponent with max HP already set.</summary>
        private CharacterComponent CreateCharacter(string name = "Character", int maxHp = 20)
        {
            var go = new GameObject(name);
            _created.Add(go);
            var ch = go.AddComponent<CharacterComponent>();
            ch.State.HitPoints = new HitPointState(maxHp, maxHp);
            return ch;
        }

        /// <summary>Creates a CharacterComponent whose _endpointId field is pre-set.</summary>
        private CharacterComponent CreateCharacterWithEndpointId(string endpointId, string goName = "Character")
        {
            var go = new GameObject(goName);
            _created.Add(go);
            go.SetActive(false);
            var ch = go.AddComponent<CharacterComponent>();
            typeof(CharacterComponent)
                .GetField("_endpointId", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(ch, endpointId);
            go.SetActive(true); // Awake fires.
            return ch;
        }

        // ── EndpointId ────────────────────────────────────────────────────────

        [Test]
        public void EndpointId_WhenSerializedStringEmpty_UsesGameObjectName()
        {
            var ch = CreateCharacter("Aldric");
            Assert.AreEqual("Aldric", ch.EndpointId.ToString());
        }

        [Test]
        public void EndpointId_WhenSerializedStringSet_UsesSerializedValue()
        {
            var ch = CreateCharacterWithEndpointId("player-1", goName: "SomeName");
            Assert.AreEqual("player-1", ch.EndpointId.ToString());
        }

        // ── ApplyDamage ───────────────────────────────────────────────────────

        [Test]
        public void ApplyDamage_ReducesCurrentHitPoints()
        {
            var ch = CreateCharacter(maxHp: 20);
            ch.ApplyDamage(8, DamageType.Slashing);
            Assert.AreEqual(12, ch.State.HitPoints.Current);
        }

        [Test]
        public void ApplyDamage_PublishesHitPointsChanged()
        {
            CreateRunner();
            var ch = CreateCharacter(maxHp: 20);

            HpChangedArgs? captured = null;
            DnDSdkRunner.Bus!.Combat.HitPointsChanged.Subscribe(
                new EndpointId("test-listener"),
                args => captured = args);

            ch.ApplyDamage(5, DamageType.Fire);

            Assert.IsTrue(captured.HasValue, "HitPointsChanged should have fired.");
            Assert.AreEqual(15, captured!.Value.Current);
            Assert.AreEqual(20, captured.Value.Previous);
            Assert.AreEqual(20, captured.Value.Maximum);
        }

        [Test]
        public void ApplyDamage_WhenDamageIsZero_DoesNotPublishHitPointsChanged()
        {
            CreateRunner();
            var ch = CreateCharacter(maxHp: 20);

            bool fired = false;
            DnDSdkRunner.Bus!.Combat.HitPointsChanged.Subscribe(
                new EndpointId("test-listener"),
                _ => fired = true);

            ch.ApplyDamage(0, DamageType.Cold);

            Assert.IsFalse(fired, "No signal expected when HP is unchanged.");
        }

        [Test]
        public void ApplyDamage_ExceedingMaxHp_ClampsToZero()
        {
            var ch = CreateCharacter(maxHp: 10);
            ch.ApplyDamage(999, DamageType.Necrotic);
            Assert.AreEqual(0, ch.State.HitPoints.Current);
        }

        // ── Heal ──────────────────────────────────────────────────────────────

        [Test]
        public void Heal_IncreasesCurrentHitPoints()
        {
            var ch = CreateCharacter(maxHp: 20);
            ch.ApplyDamage(10, DamageType.Bludgeoning);
            ch.Heal(4);
            Assert.AreEqual(14, ch.State.HitPoints.Current);
        }

        [Test]
        public void Heal_PublishesHitPointsChanged()
        {
            CreateRunner();
            var ch = CreateCharacter(maxHp: 20);
            ch.ApplyDamage(10, DamageType.Bludgeoning); // Drop to 10 HP.

            HpChangedArgs? captured = null;
            DnDSdkRunner.Bus!.Combat.HitPointsChanged.Subscribe(
                new EndpointId("test-listener"),
                args => captured = args);

            ch.Heal(4);

            Assert.IsTrue(captured.HasValue);
            Assert.AreEqual(14, captured!.Value.Current);
            Assert.AreEqual(10, captured.Value.Previous);
        }

        [Test]
        public void Heal_CannotExceedMaximumHitPoints()
        {
            var ch = CreateCharacter(maxHp: 20);
            ch.Heal(100);
            Assert.AreEqual(20, ch.State.HitPoints.Current);
        }

        // ── Query registration ────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator GetArmorClass_Query_RespondsAfterStartRuns()
        {
            CreateRunner();
            var ch = CreateCharacter();

            yield return null; // Let Start() run → queries registered.

            // Set DEX 10 (modifier 0) so unarmoured AC = 10 + 0 = 10.
            // Assignment is done after yield so no frame boundary can reset the struct field.
            ch.Record.AbilityScores = new AbilityScoreSet(10, 10, 10, 10, 10, 10);

            bool responded = DnDSdkRunner.Bus!.Combat.GetArmorClass.TryQueryUnicast(
                ch.EndpointId, default, out int ac);
            Assert.IsTrue(responded, "Query should have a registered handler.");
            Assert.AreEqual(10, ac, "Unarmoured AC with DEX 10 must be 10.");
        }

        [UnityTest]
        public IEnumerator QueryHandlers_AfterDestroy_AreUnregistered()
        {
            CreateRunner();
            var go = new GameObject("ToDestroy");
            _created.Add(go); // Track for TearDown safety.
            var ch = go.AddComponent<CharacterComponent>();
            ch.State.HitPoints = new HitPointState(20, 20);

            yield return null; // Let Start() run → queries registered.

            Assert.IsTrue(
                DnDSdkRunner.Bus!.Combat.GetArmorClass.HasSubscriber(ch.EndpointId),
                "Query handler should be registered before destroy.");

            _created.Remove(go);
            Object.DestroyImmediate(go);

            Assert.IsFalse(
                DnDSdkRunner.Bus!.Combat.GetArmorClass.HasSubscriber(ch.EndpointId),
                "Query handler should be unregistered after destroy.");
        }
    }
}