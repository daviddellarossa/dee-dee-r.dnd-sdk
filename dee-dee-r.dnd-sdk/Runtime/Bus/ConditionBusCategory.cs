using System;
using System.Collections.Generic;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Runtime.Bus.Args;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Runtime.Bus
{
    /// <summary>
    /// Bus channels for condition and exhaustion events and queries.
    /// </summary>
    public sealed class ConditionBusCategory
    {
        // ── Signals ───────────────────────────────────────────────────────────

        /// <summary>Published when a condition is applied to a character.</summary>
        public readonly Signal<ConditionChangedArgs> ConditionApplied;

        /// <summary>Published when a condition is removed from a character.</summary>
        public readonly Signal<ConditionChangedArgs> ConditionRemoved;

        /// <summary>Published when a character's exhaustion level increases or decreases.</summary>
        public readonly Signal<ExhaustionArgs>       ExhaustionChanged;

        // ── Queries ───────────────────────────────────────────────────────────

        /// <summary>Returns the addressed character's current set of active conditions.</summary>
        public readonly Query<EmptyArgs, IReadOnlyCollection<Condition>> GetConditions;

        /// <summary>Returns the addressed character's current exhaustion level (0–6).</summary>
        public readonly Query<EmptyArgs, int>                            GetExhaustionLevel;

        // ── Constructor ───────────────────────────────────────────────────────

        public ConditionBusCategory(IFrameScheduler scheduler)
        {
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            ConditionApplied   = new Signal<ConditionChangedArgs>(scheduler);
            ConditionRemoved   = new Signal<ConditionChangedArgs>(scheduler);
            ExhaustionChanged  = new Signal<ExhaustionArgs>(scheduler);
            GetConditions      = new Query<EmptyArgs, IReadOnlyCollection<Condition>>(scheduler);
            GetExhaustionLevel = new Query<EmptyArgs, int>(scheduler);
        }
    }
}