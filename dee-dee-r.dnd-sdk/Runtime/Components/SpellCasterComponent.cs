using System;
using DeeDeeR.DnD.Runtime.Bus.Args;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.Systems;
using UnityEngine;

namespace DeeDeeR.DnD.Runtime.Components
{
    /// <summary>
    /// Manages spellcasting and concentration for a character.
    /// Requires a <see cref="CharacterComponent"/> on the same GameObject.
    /// </summary>
    /// <remarks>
    /// Subscribes to <see cref="CombatBusCategory.HitPointsChanged"/> while enabled so that
    /// concentration is automatically broken when this character takes damage.
    /// </remarks>
    [RequireComponent(typeof(CharacterComponent))]
    [AddComponentMenu("DnD SDK/Spell Caster")]
    [DefaultExecutionOrder(-25)]
    public sealed class SpellCasterComponent : MonoBehaviour
    {
        private CharacterComponent _character;
        private readonly SpellSystem _spells = new SpellSystem();

        private void Awake()
        {
            _character = GetComponent<CharacterComponent>();
        }

        private void OnEnable()
        {
            DnDSdkRunner.Bus?.Combat.HitPointsChanged.Subscribe(
                _character.EndpointId, OnHitPointsChanged);
        }

        private void OnDisable()
        {
            DnDSdkRunner.Bus?.Combat.HitPointsChanged.UnsubscribeAll(
                _character.EndpointId);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Attempts to cast <paramref name="spell"/> at <paramref name="slotLevel"/>.
        /// </summary>
        /// <remarks>
        /// On success:
        /// <list type="bullet">
        ///   <item>Expends the slot and publishes <see cref="SpellBusCategory.SpellSlotExpended"/>
        ///     (levelled spells only).</item>
        ///   <item>Breaks any existing concentration and begins new concentration for
        ///     concentration spells.</item>
        ///   <item>Publishes <see cref="SpellBusCategory.SpellCast"/>.</item>
        /// </list>
        /// <para>
        /// This method does not check whether the spell is known or prepared — that is the
        /// caller's responsibility via <c>SpellbookState</c>.
        /// </para>
        /// </remarks>
        /// <param name="spell">The spell to cast. Must not be null.</param>
        /// <param name="slotLevel">0 for cantrips; 1–9 for levelled spells.</param>
        /// <returns><c>true</c> if the spell was cast successfully; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="spell"/> is null.</exception>
        public bool TryCastSpell(SpellSO spell, int slotLevel)
        {
            if (spell == null) throw new ArgumentNullException(nameof(spell));

            if (!_spells.CanCastSpell(_character.State, spell, slotLevel))
                return false;

            var bus = DnDSdkRunner.Bus;

            if (slotLevel > 0)
            {
                _spells.ExpendSlot(_character.State, slotLevel);
                bus.Spell.SpellSlotExpended.PublishBroadcast(
                    new SlotArgs(_character.EndpointId, slotLevel));
            }

            if (spell.IsConcentration && _character.State.ConcentrationSpell != null)
                BreakConcentration();

            if (spell.IsConcentration)
                _spells.BeginConcentration(_character.State, spell);

            bus.Spell.SpellCast.PublishBroadcast(
                new SpellCastArgs(_character.EndpointId, spell, slotLevel));

            return true;
        }

        /// <summary>
        /// Breaks the character's current concentration (if any) and publishes
        /// <see cref="SpellBusCategory.ConcentrationBroken"/>.
        /// Safe to call when not concentrating (no-op).
        /// </summary>
        public void BreakConcentration()
        {
            var spell = _character.State.ConcentrationSpell;
            if (spell == null) return;

            _spells.BreakConcentration(_character.State);
            DnDSdkRunner.Bus.Spell.ConcentrationBroken.PublishBroadcast(
                new ConcentrationArgs(_character.EndpointId, spell));
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnHitPointsChanged(HpChangedArgs args)
        {
            // Filter: only react to damage dealt to this character.
            if (args.Character != _character.EndpointId) return;
            if (args.Current >= args.Previous) return;

            BreakConcentration();
        }
    }
}