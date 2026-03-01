using System;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Runtime.Bus
{
    /// <summary>
    /// Top-level message bus container for the DnD SDK.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Create one instance per scene context via <c>DnDSdkRunner</c> (Phase 11) and pass it to
    /// any component that needs to publish signals or register query handlers.
    /// </para>
    /// <para>
    /// This class is intentionally <b>not</b> a static singleton. Keeping it instance-based
    /// allows multiple isolated contexts in the same process (e.g. unit tests, split-screen)
    /// and simplifies dependency injection.
    /// </para>
    /// </remarks>
    public sealed class DnDSdkBus
    {
        /// <summary>Attack rolls, damage, turns, critical hits, and death saves.</summary>
        public readonly CombatBusCategory    Combat;

        /// <summary>Condition application/removal and exhaustion changes.</summary>
        public readonly ConditionBusCategory Condition;

        /// <summary>Spellcasting, concentration, slot expenditure, and spell learning.</summary>
        public readonly SpellBusCategory     Spell;

        /// <summary>Level-up events, ability score changes, and feat grants.</summary>
        public readonly CharacterBusCategory Character;

        /// <summary>Short and long rest events and hit-dice availability.</summary>
        public readonly RestBusCategory      Rest;

        /// <summary>Inventory mutations (equip, add, remove) and equipped-item queries.</summary>
        public readonly InventoryBusCategory Inventory;

        /// <param name="scheduler">
        /// Frame scheduler used by all signals and queries for deferred delivery.
        /// Typically supplied by <c>FrameSchedulerBehaviour</c> from the message-bus package.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="scheduler"/> is null.</exception>
        public DnDSdkBus(IFrameScheduler scheduler)
        {
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));

            Combat    = new CombatBusCategory(scheduler);
            Condition = new ConditionBusCategory(scheduler);
            Spell     = new SpellBusCategory(scheduler);
            Character = new CharacterBusCategory(scheduler);
            Rest      = new RestBusCategory(scheduler);
            Inventory = new InventoryBusCategory(scheduler);
        }
    }
}