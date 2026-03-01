using System;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Bus.Args;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.Systems;
using UnityEngine;

namespace DeeDeeR.DnD.Runtime.Components
{
    /// <summary>
    /// Resolves combat actions for a character: turn management, attack rolls, and damage.
    /// Requires a <see cref="CharacterComponent"/> on the same GameObject.
    /// </summary>
    /// <remarks>
    /// Action economy (<c>ActionUsed</c>, <c>BonusActionUsed</c>, <c>ReactionUsed</c>) is
    /// reset by <see cref="StartTurn"/>. Extra Attack and Bonus Action attacks are the
    /// responsibility of the caller.
    /// </remarks>
    [RequireComponent(typeof(CharacterComponent))]
    [AddComponentMenu("DnD SDK/Combatant")]
    [DefaultExecutionOrder(-25)]
    public sealed class CombatantComponent : MonoBehaviour
    {
        private CharacterComponent _character;
        private readonly CombatSystem _combat = new CombatSystem();

        private void Awake()
        {
            _character = GetComponent<CharacterComponent>();
        }

        // ── Turn management ───────────────────────────────────────────────────

        /// <summary>
        /// Resets per-turn action flags and publishes
        /// <see cref="CombatBusCategory.TurnStarted"/>.
        /// Call at the beginning of this character's turn in the initiative order.
        /// </summary>
        public void StartTurn()
        {
            _character.State.ActionUsed      = false;
            _character.State.BonusActionUsed = false;
            _character.State.ReactionUsed    = false;

            DnDSdkRunner.Bus.Combat.TurnStarted.PublishBroadcast(
                new TurnArgs(_character.EndpointId));
        }

        /// <summary>
        /// Publishes <see cref="CombatBusCategory.TurnEnded"/>.
        /// Call at the end of this character's turn in the initiative order.
        /// </summary>
        public void EndTurn()
        {
            DnDSdkRunner.Bus.Combat.TurnEnded.PublishBroadcast(
                new TurnArgs(_character.EndpointId));
        }

        // ── Attack ────────────────────────────────────────────────────────────

        /// <summary>
        /// Resolves a full attack against <paramref name="target"/>: rolls the attack,
        /// optionally deals damage, and publishes the relevant bus signals.
        /// </summary>
        /// <remarks>
        /// <para>Signals published (in order):</para>
        /// <list type="bullet">
        ///   <item><see cref="CombatBusCategory.AttackMade"/> — always</item>
        ///   <item><see cref="CombatBusCategory.CriticalHit"/> — on a natural 20</item>
        ///   <item><see cref="CombatBusCategory.DamageDealt"/> — on a hit</item>
        ///   <item><see cref="CombatBusCategory.HitPointsChanged"/> — when target HP changes
        ///     (via <see cref="CharacterComponent.ApplyDamage"/>)</item>
        /// </list>
        /// <para>
        /// Resistance and immunity must be applied by the caller before this method is called
        /// or by subscribing to <see cref="CombatBusCategory.DamageDealt"/> and adjusting the
        /// amount externally. The raw rolled damage is passed directly to
        /// <see cref="CharacterComponent.ApplyDamage"/>.
        /// </para>
        /// </remarks>
        /// <param name="target">The character being attacked.</param>
        /// <param name="weapon">The weapon used.</param>
        /// <param name="advantage">Advantage state for the attack roll.</param>
        /// <param name="roller">Random number source.</param>
        /// <returns>The resolved <see cref="AttackRollResult"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public AttackRollResult PerformAttack(
            CharacterComponent target,
            WeaponSO           weapon,
            AdvantageState     advantage,
            IRollProvider      roller)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (weapon == null) throw new ArgumentNullException(nameof(weapon));
            if (roller == null) throw new ArgumentNullException(nameof(roller));

            int targetAC = _combat.CalculateArmorClass(target.Record, target.State, target.Inventory);
            var result   = _combat.RollAttack(_character.Record, _character.State, targetAC, weapon, advantage, roller);

            var bus = DnDSdkRunner.Bus;

            bus.Combat.AttackMade.PublishBroadcast(
                new AttackMadeArgs(_character.EndpointId, target.EndpointId, result, weapon));

            if (result.Critical)
                bus.Combat.CriticalHit.PublishBroadcast(
                    new CritHitArgs(_character.EndpointId, target.EndpointId));

            if (result.Hit)
            {
                int damage = _combat.RollDamage(weapon, _character.Record, result.Critical, roller);

                bus.Combat.DamageDealt.PublishBroadcast(
                    new DamageDealtArgs(_character.EndpointId, target.EndpointId, damage, weapon.DamageType));

                target.ApplyDamage(damage, weapon.DamageType);
            }

            return result;
        }
    }
}