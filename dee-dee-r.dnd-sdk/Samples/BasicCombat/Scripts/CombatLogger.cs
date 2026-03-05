using DeeDeeR.DnD.Runtime.Bus.Args;
using DeeDeeR.DnD.Runtime.Components;
using DeeDeeR.MessageBus.Runtime.Core;
using UnityEngine;

namespace DeeDeeR.DnD.Samples.BasicCombat
{
    /// <summary>
    /// Subscribes to DnD SDK combat bus signals and logs them to the Unity Console.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This component demonstrates the message bus as the SDK ↔ game communication seam:
    /// health bars, damage numbers, VFX, and audio triggers can all live here without
    /// importing <c>CombatSystem</c> or holding direct references to combatants.
    /// </para>
    /// <para>
    /// A fixed <see cref="EndpointId"/> (<c>"BasicCombatLogger"</c>) is used for subscription.
    /// All subscriptions are broadcast — they receive signals from every combatant.
    /// </para>
    /// </remarks>
    public sealed class CombatLogger : MonoBehaviour
    {
        private static readonly EndpointId _id = new EndpointId("BasicCombatLogger");

        private void OnEnable()
        {
            var bus = DnDSdkRunner.Bus;
            if (bus == null)
            {
                Debug.LogWarning("[CombatLogger] DnDSdkRunner.Bus is null — signals not subscribed. " +
                                 "Ensure DnDSdkRunner is active before CombatLogger enables.");
                return;
            }

            bus.Combat.TurnStarted.Subscribe(_id, OnTurnStarted);
            bus.Combat.AttackMade.Subscribe(_id, OnAttackMade);
            bus.Combat.CriticalHit.Subscribe(_id, OnCriticalHit);
            bus.Combat.DamageDealt.Subscribe(_id, OnDamageDealt);
            bus.Combat.HitPointsChanged.Subscribe(_id, OnHpChanged);
            bus.Combat.CharacterDied.Subscribe(_id, OnCharacterDied);
        }

        private void OnDisable()
        {
            var bus = DnDSdkRunner.Bus;
            if (bus == null) return;

            bus.Combat.TurnStarted.UnsubscribeAll(_id);
            bus.Combat.AttackMade.UnsubscribeAll(_id);
            bus.Combat.CriticalHit.UnsubscribeAll(_id);
            bus.Combat.DamageDealt.UnsubscribeAll(_id);
            bus.Combat.HitPointsChanged.UnsubscribeAll(_id);
            bus.Combat.CharacterDied.UnsubscribeAll(_id);
        }

        // ── Signal handlers ───────────────────────────────────────────────────

        private static void OnTurnStarted(TurnArgs args)
            => Debug.Log($"[CombatLogger] ▶ Turn started: {args.Character}");

        private static void OnAttackMade(AttackMadeArgs args)
            => Debug.Log($"[CombatLogger] ⚔ Attack: {args.Attacker} → {args.Target} " +
                         $"| d20 roll: {args.Roll.Total} | Hit: {args.Roll.Hit} " +
                         $"| Weapon: {args.Weapon.name}");

        private static void OnCriticalHit(CritHitArgs args)
            => Debug.Log($"[CombatLogger] ★ CRITICAL HIT! {args.Attacker} → {args.Target}");

        private static void OnDamageDealt(DamageDealtArgs args)
            => Debug.Log($"[CombatLogger] 💥 Damage: {args.Amount} {args.Type} " +
                         $"({args.Attacker} → {args.Target})");

        private static void OnHpChanged(HpChangedArgs args)
            => Debug.Log($"[CombatLogger] ❤ HP: {args.Character}  " +
                         $"{args.Previous} → {args.Current} / {args.Maximum}");

        private static void OnCharacterDied(CharacterDiedArgs args)
            => Debug.Log($"[CombatLogger] ✝ {args.Character} has died.");
    }
}
