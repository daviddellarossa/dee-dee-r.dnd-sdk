using System;
using DeeDeeR.DnD.Runtime.Bus.Args;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Runtime.Bus
{
    /// <summary>
    /// Bus channels for combat events and queries (attacks, damage, turns, death saves).
    /// </summary>
    public sealed class CombatBusCategory
    {
        // ── Signals ───────────────────────────────────────────────────────────

        /// <summary>Published when an attack roll is fully resolved (hit or miss).</summary>
        public readonly Signal<AttackMadeArgs>    AttackMade;

        /// <summary>Published when raw damage is applied to a target (before HP mutation).</summary>
        public readonly Signal<DamageDealtArgs>   DamageDealt;

        /// <summary>Published whenever a character's current hit point total changes.</summary>
        public readonly Signal<HpChangedArgs>     HitPointsChanged;

        /// <summary>Published when a character dies.</summary>
        public readonly Signal<CharacterDiedArgs> CharacterDied;

        /// <summary>Published at the beginning of a character's turn in initiative order.</summary>
        public readonly Signal<TurnArgs>          TurnStarted;

        /// <summary>Published at the end of a character's turn in initiative order.</summary>
        public readonly Signal<TurnArgs>          TurnEnded;

        /// <summary>Published when an attack roll results in a natural 20.</summary>
        public readonly Signal<CritHitArgs>       CriticalHit;

        /// <summary>Published each time a dying character makes a death saving throw.</summary>
        public readonly Signal<DeathSaveArgs>     DeathSaveMade;

        // ── Queries ───────────────────────────────────────────────────────────

        /// <summary>Returns the addressed character's current armor class.</summary>
        public readonly Query<EmptyArgs, int>          GetArmorClass;

        /// <summary>Returns the addressed character's total attack bonus for the specified weapon.</summary>
        public readonly Query<GetAttackBonusArgs, int> GetAttackBonus;

        /// <summary>Returns the addressed character's passive Perception score (10 + Perception bonus).</summary>
        public readonly Query<EmptyArgs, int>          GetPassivePerception;

        // ── Constructor ───────────────────────────────────────────────────────

        public CombatBusCategory(IFrameScheduler scheduler)
        {
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            AttackMade           = new Signal<AttackMadeArgs>(scheduler);
            DamageDealt          = new Signal<DamageDealtArgs>(scheduler);
            HitPointsChanged     = new Signal<HpChangedArgs>(scheduler);
            CharacterDied        = new Signal<CharacterDiedArgs>(scheduler);
            TurnStarted          = new Signal<TurnArgs>(scheduler);
            TurnEnded            = new Signal<TurnArgs>(scheduler);
            CriticalHit          = new Signal<CritHitArgs>(scheduler);
            DeathSaveMade        = new Signal<DeathSaveArgs>(scheduler);
            GetArmorClass        = new Query<EmptyArgs, int>(scheduler);
            GetAttackBonus       = new Query<GetAttackBonusArgs, int>(scheduler);
            GetPassivePerception = new Query<EmptyArgs, int>(scheduler);
        }
    }
}