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
    /// PlayMode tests for <see cref="SpellCasterComponent"/>:
    /// spell casting, slot expenditure, concentration management,
    /// and automatic concentration break on damage.
    /// </summary>
    [TestFixture]
    public class SpellCasterComponentTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();
        private SpellSO _cantrip;
        private SpellSO _level1Spell;
        private SpellSO _level1Concentration;

        [SetUp]
        public void SetUp()
        {
            // Runner — Bus must exist before SpellCasterComponent.OnEnable subscribes.
            var go = new GameObject("Runner");
            _created.Add(go);
            go.SetActive(false);
            var scheduler = go.AddComponent<FrameSchedulerBehaviour>();
            var runner    = go.AddComponent<DnDSdkRunner>();
            typeof(DnDSdkRunner)
                .GetField("_scheduler", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(runner, scheduler);
            go.SetActive(true);

            // Spell assets.
            _cantrip = ScriptableObject.CreateInstance<SpellSO>();
            _cantrip.Level = 0;

            _level1Spell = ScriptableObject.CreateInstance<SpellSO>();
            _level1Spell.Level = 1;
            _level1Spell.IsConcentration = false;

            _level1Concentration = ScriptableObject.CreateInstance<SpellSO>();
            _level1Concentration.Level = 1;
            _level1Concentration.IsConcentration = true;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created)
                if (go != null) Object.DestroyImmediate(go);
            _created.Clear();

            Object.DestroyImmediate(_cantrip);
            Object.DestroyImmediate(_level1Spell);
            Object.DestroyImmediate(_level1Concentration);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a fully configured spell-caster:
        /// CharacterComponent + SpellCasterComponent on the same GO.
        /// HP = 20/20, spell slots preset if slotLevel > 0.
        /// </summary>
        private (CharacterComponent character, SpellCasterComponent caster)
            CreateCaster(int maxHp = 20, int slot1Count = 0)
        {
            var go = new GameObject("SpellCaster");
            _created.Add(go);
            var ch  = go.AddComponent<CharacterComponent>();
            var sc  = go.AddComponent<SpellCasterComponent>();

            ch.State.HitPoints  = new HitPointState(maxHp, maxHp);
            ch.State.SpellSlots = new SpellSlotState(s1: slot1Count);

            return (ch, sc);
        }

        // ── TryCastSpell — null guard ─────────────────────────────────────────

        [Test]
        public void TryCastSpell_NullSpell_ThrowsArgumentNullException()
        {
            var (_, sc) = CreateCaster();
            Assert.Throws<System.ArgumentNullException>(() => sc.TryCastSpell(null, 0));
        }

        // ── TryCastSpell — cantrip ────────────────────────────────────────────

        [Test]
        public void TryCastSpell_Cantrip_ReturnsTrue()
        {
            var (_, sc) = CreateCaster();
            Assert.IsTrue(sc.TryCastSpell(_cantrip, 0));
        }

        [Test]
        public void TryCastSpell_Cantrip_PublishesSpellCast()
        {
            var (ch, sc) = CreateCaster();

            SpellCastArgs? captured = null;
            DnDSdkRunner.Bus!.Spell.SpellCast.Subscribe(
                new EndpointId("test-listener"),
                args => captured = args);

            sc.TryCastSpell(_cantrip, 0);

            Assert.IsTrue(captured.HasValue, "SpellCast should fire for cantrip.");
            Assert.AreEqual(0, captured!.Value.SlotLevel, "Cantrip slot level must be 0.");
            Assert.AreSame(_cantrip, captured.Value.Spell);
        }

        [Test]
        public void TryCastSpell_Cantrip_DoesNotExpendSlot()
        {
            var (ch, sc) = CreateCaster(slot1Count: 2);
            sc.TryCastSpell(_cantrip, 0);
            Assert.AreEqual(2, ch.State.SpellSlots.GetAvailable(1), "Cantrip must not consume a slot.");
        }

        // ── TryCastSpell — levelled spell ─────────────────────────────────────

        [Test]
        public void TryCastSpell_LevelledSpell_WhenNoSlotAvailable_ReturnsFalse()
        {
            var (_, sc) = CreateCaster(slot1Count: 0);
            Assert.IsFalse(sc.TryCastSpell(_level1Spell, 1));
        }

        [Test]
        public void TryCastSpell_LevelledSpell_ExpendsSlot()
        {
            var (ch, sc) = CreateCaster(slot1Count: 3);
            sc.TryCastSpell(_level1Spell, 1);
            Assert.AreEqual(2, ch.State.SpellSlots.GetAvailable(1));
        }

        [Test]
        public void TryCastSpell_LevelledSpell_PublishesSpellSlotExpended()
        {
            var (_, sc) = CreateCaster(slot1Count: 2);

            SlotArgs? captured = null;
            DnDSdkRunner.Bus!.Spell.SpellSlotExpended.Subscribe(
                new EndpointId("test-listener"),
                args => captured = args);

            sc.TryCastSpell(_level1Spell, 1);

            Assert.IsTrue(captured.HasValue, "SpellSlotExpended should fire for levelled spell.");
            Assert.AreEqual(1, captured!.Value.SlotLevel);
        }

        [Test]
        public void TryCastSpell_LevelledSpell_PublishesSpellCast()
        {
            var (_, sc) = CreateCaster(slot1Count: 2);

            SpellCastArgs? captured = null;
            DnDSdkRunner.Bus!.Spell.SpellCast.Subscribe(
                new EndpointId("test-listener"),
                args => captured = args);

            sc.TryCastSpell(_level1Spell, 1);

            Assert.IsTrue(captured.HasValue);
            Assert.AreEqual(1, captured!.Value.SlotLevel);
            Assert.AreSame(_level1Spell, captured.Value.Spell);
        }

        // ── Concentration ─────────────────────────────────────────────────────

        [Test]
        public void TryCastSpell_ConcentrationSpell_SetsConcentrationOnState()
        {
            var (ch, sc) = CreateCaster(slot1Count: 2);
            sc.TryCastSpell(_level1Concentration, 1);
            Assert.AreSame(_level1Concentration, ch.State.ConcentrationSpell);
        }

        [Test]
        public void TryCastSpell_NewConcentration_BreaksExistingConcentration()
        {
            var (ch, sc) = CreateCaster(slot1Count: 4);

            // Cast first concentration spell.
            sc.TryCastSpell(_level1Concentration, 1);
            Assert.AreSame(_level1Concentration, ch.State.ConcentrationSpell, "Precondition.");

            // Cast a second concentration spell — first must be broken.
            var secondSpell = ScriptableObject.CreateInstance<SpellSO>();
            secondSpell.Level = 1;
            secondSpell.IsConcentration = true;

            bool concentrationBroken = false;
            DnDSdkRunner.Bus!.Spell.ConcentrationBroken.Subscribe(
                new EndpointId("test-listener"),
                _ => concentrationBroken = true);

            sc.TryCastSpell(secondSpell, 1);

            Assert.IsTrue(concentrationBroken, "ConcentrationBroken should fire when overwriting concentration.");
            Assert.AreSame(secondSpell, ch.State.ConcentrationSpell, "New spell must be the active concentration.");

            Object.DestroyImmediate(secondSpell);
        }

        [Test]
        public void BreakConcentration_WhenNotConcentrating_IsNoOp()
        {
            var (ch, sc) = CreateCaster();
            Assert.IsNull(ch.State.ConcentrationSpell, "Precondition: not concentrating.");
            Assert.DoesNotThrow(() => sc.BreakConcentration());
        }

        [Test]
        public void BreakConcentration_WhenConcentrating_ClearsConcentrationSpell()
        {
            var (ch, sc) = CreateCaster(slot1Count: 2);
            sc.TryCastSpell(_level1Concentration, 1);
            Assert.IsNotNull(ch.State.ConcentrationSpell, "Precondition: must be concentrating.");

            sc.BreakConcentration();
            Assert.IsNull(ch.State.ConcentrationSpell);
        }

        [Test]
        public void BreakConcentration_WhenConcentrating_PublishesConcentrationBroken()
        {
            var (_, sc) = CreateCaster(slot1Count: 2);
            sc.TryCastSpell(_level1Concentration, 1);

            ConcentrationArgs? captured = null;
            DnDSdkRunner.Bus!.Spell.ConcentrationBroken.Subscribe(
                new EndpointId("test-listener"),
                args => captured = args);

            sc.BreakConcentration();

            Assert.IsTrue(captured.HasValue, "ConcentrationBroken should fire.");
            Assert.AreSame(_level1Concentration, captured!.Value.Spell);
        }

        // ── Auto-break on damage ──────────────────────────────────────────────

        [Test]
        public void ApplyDamage_WhenConcentrating_BreaksConcentration()
        {
            var (ch, sc) = CreateCaster(maxHp: 20, slot1Count: 2);
            sc.TryCastSpell(_level1Concentration, 1);
            Assert.IsNotNull(ch.State.ConcentrationSpell, "Precondition: must be concentrating.");

            // ApplyDamage publishes HitPointsChanged (immediate), which SpellCasterComponent
            // subscribes to in OnEnable. It auto-breaks concentration when HP decreases.
            ch.ApplyDamage(5, DamageType.Fire);

            Assert.IsNull(ch.State.ConcentrationSpell,
                "Concentration should break automatically when the caster takes damage.");
        }

        [Test]
        public void Heal_WhenConcentrating_DoesNotBreakConcentration()
        {
            var (ch, sc) = CreateCaster(maxHp: 20, slot1Count: 2);
            ch.ApplyDamage(5, DamageType.Fire); // Drop HP so healing has room.
            sc.TryCastSpell(_level1Concentration, 1);
            Assert.IsNotNull(ch.State.ConcentrationSpell, "Precondition: concentrating.");

            ch.Heal(3); // HP increases — must NOT break concentration.

            Assert.IsNotNull(ch.State.ConcentrationSpell,
                "Concentration must not break on healing.");
        }
    }
}