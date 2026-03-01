using System.Collections.Generic;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Systems;
using DeeDeeR.DnD.Runtime.Bus.Args;
using DeeDeeR.DnD.Runtime.State;
using DeeDeeR.DnD.Runtime.Systems;
using DeeDeeR.MessageBus.Runtime.Core;
using UnityEngine;

namespace DeeDeeR.DnD.Runtime.Components
{
    /// <summary>
    /// Unity component that holds a character's <see cref="CharacterRecord"/>,
    /// <see cref="CharacterState"/>, and <see cref="InventoryState"/>, and wires them to the
    /// <see cref="DnDSdkBus"/> query channels so other systems can read character data without
    /// a direct reference.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Query handlers are registered in <c>Start</c> (after <see cref="DnDSdkRunner.Awake"/>
    /// has created the bus) and unregistered in <c>OnDestroy</c>.
    /// </para>
    /// <para>
    /// <see cref="EndpointId"/> is resolved in <c>Awake</c> from the serialized
    /// <c>_endpointId</c> field (or the GameObject name if that field is empty), making it
    /// available to sibling components (e.g. <see cref="SpellCasterComponent"/>) during their
    /// own <c>Awake</c> and <c>OnEnable</c> calls.
    /// </para>
    /// </remarks>
    [DisallowMultipleComponent]
    [AddComponentMenu("DnD SDK/Character")]
    [DefaultExecutionOrder(-50)]
    public sealed class CharacterComponent : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [SerializeField]
        [Tooltip("Unique string identifier for this character's bus endpoint. " +
                 "Defaults to the GameObject name if left empty.")]
        private string _endpointId;

        // ── Public state ──────────────────────────────────────────────────────

        /// <summary>Semi-static identity and build data (class levels, ability scores, proficiencies).</summary>
        public CharacterRecord Record = new CharacterRecord();

        /// <summary>Mutable per-session state (HP, conditions, spell slots, exhaustion, etc.).</summary>
        public CharacterState State = new CharacterState();

        /// <summary>Equipped items and currency.</summary>
        public InventoryState Inventory = new InventoryState();

        /// <summary>This character's unique identifier on the DnD SDK bus.</summary>
        public EndpointId EndpointId { get; private set; }

        // ── Systems ───────────────────────────────────────────────────────────

        private readonly CombatSystem      _combat      = new CombatSystem();
        private readonly ProficiencySystem _proficiency = new ProficiencySystem();
        private readonly HitPointSystem    _hitPoints   = new HitPointSystem();

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            EndpointId = new EndpointId(
                string.IsNullOrWhiteSpace(_endpointId) ? gameObject.name : _endpointId);
        }

        private void Start()
        {
            RegisterQueries();
        }

        private void OnDestroy()
        {
            UnregisterQueries();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Applies damage to this character's hit points via <see cref="HitPointSystem"/> and
        /// publishes <see cref="CombatBusCategory.HitPointsChanged"/> if the HP value changes.
        /// </summary>
        /// <remarks>
        /// Resistance and immunity must be applied by the caller before passing
        /// <paramref name="amount"/> to this method.
        /// </remarks>
        /// <param name="amount">Raw damage (≥ 0; values ≤ 0 are ignored by <see cref="HitPointSystem"/>).</param>
        /// <param name="type">Damage type (for caller context; not processed here).</param>
        public void ApplyDamage(int amount, DamageType type)
        {
            int previous = State.HitPoints.Current;
            _hitPoints.ApplyDamage(State, amount, type);
            PublishHpChangedIfNeeded(previous);
        }

        /// <summary>
        /// Heals this character via <see cref="HitPointSystem"/> and publishes
        /// <see cref="CombatBusCategory.HitPointsChanged"/> if the HP value changes.
        /// </summary>
        /// <param name="amount">Healing amount (≥ 0; values ≤ 0 are ignored).</param>
        public void Heal(int amount)
        {
            int previous = State.HitPoints.Current;
            _hitPoints.Heal(State, amount);
            PublishHpChangedIfNeeded(previous);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void PublishHpChangedIfNeeded(int previous)
        {
            int current = State.HitPoints.Current;
            if (current == previous) return;

            DnDSdkRunner.Bus?.Combat.HitPointsChanged.PublishBroadcast(
                new HpChangedArgs(EndpointId, previous, current, State.HitPoints.Maximum));
        }

        private int ComputeTotalLevel()
        {
            int total = 0;
            foreach (var cl in Record.ClassLevels)
                if (cl != null) total += cl.Level;
            return total;
        }

        private int GetProficiencyBonusSafe()
        {
            int totalLevel = ComputeTotalLevel();
            return totalLevel > 0 ? _proficiency.GetProficiencyBonus(totalLevel) : 2;
        }

        // ── Query registration ────────────────────────────────────────────────

        private void RegisterQueries()
        {
            var bus = DnDSdkRunner.Bus;
            if (bus == null)
            {
                Debug.LogError(
                    $"[{nameof(CharacterComponent)}] DnDSdkRunner.Bus is null. " +
                    "Ensure a DnDSdkRunner (execution order −100) is present in the scene.", this);
                return;
            }

            var id = EndpointId;

            // ── Combat ────────────────────────────────────────────────────────
            bus.Combat.GetArmorClass.Subscribe(id,
                _ => _combat.CalculateArmorClass(Record, State, Inventory));

            bus.Combat.GetAttackBonus.Subscribe(id,
                args => _combat.GetAttackBonus(Record, State, args.Weapon));

            bus.Combat.GetPassivePerception.Subscribe(id, _ =>
            {
                int  prof  = GetProficiencyBonusSafe();
                bool isPro = Record.SkillProficiencies.Contains(SkillType.Perception);
                bool isExp = Record.SkillExpertise.Contains(SkillType.Perception);
                int  bonus = _proficiency.GetSkillBonus(Record.AbilityScores, SkillType.Perception, isPro, isExp, prof);
                return AbilitySystem.PassiveCheck(bonus);
            });

            // ── Condition ─────────────────────────────────────────────────────
            bus.Condition.GetConditions.Subscribe(id,
                _ => State.Conditions);

            bus.Condition.GetExhaustionLevel.Subscribe(id,
                _ => State.Exhaustion.Value);

            // ── Spell ─────────────────────────────────────────────────────────
            bus.Spell.GetAvailableSpellSlots.Subscribe(id,
                _ => State.SpellSlots);

            bus.Spell.GetConcentrationSpell.Subscribe(id,
                _ => State.ConcentrationSpell);

            // ── Character ─────────────────────────────────────────────────────
            bus.Character.GetAbilityModifier.Subscribe(id,
                args => Record.AbilityScores.GetModifier(args.Ability));

            bus.Character.GetSkillBonus.Subscribe(id, args =>
            {
                int  prof  = GetProficiencyBonusSafe();
                bool isPro = Record.SkillProficiencies.Contains(args.Skill);
                bool isExp = Record.SkillExpertise.Contains(args.Skill);
                return _proficiency.GetSkillBonus(Record.AbilityScores, args.Skill, isPro, isExp, prof);
            });

            bus.Character.GetProficiencyBonus.Subscribe(id,
                _ => GetProficiencyBonusSafe());

            // ── Rest ──────────────────────────────────────────────────────────
            bus.Rest.GetHitDiceAvailable.Subscribe(id,
                _ => (IReadOnlyDictionary<DieType, int>)State.HitDiceAvailable);

            // ── Inventory ─────────────────────────────────────────────────────
            bus.Inventory.GetEquippedWeapon.Subscribe(id,
                args => args.Hand == EquipHand.Main
                    ? Inventory.EquippedMainHand
                    : Inventory.EquippedOffHandWeapon);

            bus.Inventory.GetEquippedArmor.Subscribe(id,
                _ => Inventory.EquippedArmor);
        }

        private void UnregisterQueries()
        {
            var bus = DnDSdkRunner.Bus;
            if (bus == null) return;

            var id = EndpointId;

            bus.Combat.GetArmorClass.Unsubscribe(id);
            bus.Combat.GetAttackBonus.Unsubscribe(id);
            bus.Combat.GetPassivePerception.Unsubscribe(id);

            bus.Condition.GetConditions.Unsubscribe(id);
            bus.Condition.GetExhaustionLevel.Unsubscribe(id);

            bus.Spell.GetAvailableSpellSlots.Unsubscribe(id);
            bus.Spell.GetConcentrationSpell.Unsubscribe(id);

            bus.Character.GetAbilityModifier.Unsubscribe(id);
            bus.Character.GetSkillBonus.Unsubscribe(id);
            bus.Character.GetProficiencyBonus.Unsubscribe(id);

            bus.Rest.GetHitDiceAvailable.Unsubscribe(id);

            bus.Inventory.GetEquippedWeapon.Unsubscribe(id);
            bus.Inventory.GetEquippedArmor.Unsubscribe(id);
        }
    }
}