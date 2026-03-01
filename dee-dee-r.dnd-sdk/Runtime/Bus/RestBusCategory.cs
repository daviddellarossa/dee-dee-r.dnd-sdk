using System.Collections.Generic;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Runtime.Bus.Args;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Runtime.Bus
{
    /// <summary>
    /// Bus channels for rest events and queries (short rest, long rest, hit dice).
    /// </summary>
    public sealed class RestBusCategory
    {
        // ── Signals ───────────────────────────────────────────────────────────

        /// <summary>Published when a character begins a short or long rest.</summary>
        public readonly Signal<RestArgs> RestStarted;

        /// <summary>Published when a character completes a short or long rest.</summary>
        public readonly Signal<RestArgs> RestCompleted;

        // ── Queries ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the addressed character's remaining hit dice, keyed by die type.
        /// </summary>
        public readonly Query<EmptyArgs, IReadOnlyDictionary<DieType, int>> GetHitDiceAvailable;

        // ── Constructor ───────────────────────────────────────────────────────

        public RestBusCategory(IFrameScheduler scheduler)
        {
            RestStarted         = new Signal<RestArgs>(scheduler);
            RestCompleted       = new Signal<RestArgs>(scheduler);
            GetHitDiceAvailable = new Query<EmptyArgs, IReadOnlyDictionary<DieType, int>>(scheduler);
        }
    }
}