namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// How long it takes to cast a spell, as defined in D&amp;D 2024 PHB.
    /// </summary>
    /// <remarks>
    /// When the value is <see cref="Reaction"/>, the spell asset must also populate
    /// <c>SpellSO.ReactionTrigger</c> with the required trigger condition
    /// (e.g. "which you take when you see a creature within 60 feet of you cast a spell").
    /// </remarks>
    public enum CastingTimeType
    {
        /// <summary>Costs the caster's Action on their turn.</summary>
        Action           = 0,

        /// <summary>Costs the caster's Bonus Action on their turn.</summary>
        BonusAction      = 1,

        /// <summary>
        /// Cast as a Reaction to a specific trigger. See <c>SpellSO.ReactionTrigger</c>
        /// for the required trigger description.
        /// </summary>
        Reaction         = 2,

        /// <summary>Requires 1 minute of uninterrupted casting (10 combat rounds).</summary>
        OneMinute        = 3,

        /// <summary>Requires 10 minutes of uninterrupted casting.</summary>
        TenMinutes       = 4,

        /// <summary>Requires 1 hour of uninterrupted casting.</summary>
        OneHour          = 5,

        /// <summary>Requires 8 hours of uninterrupted casting.</summary>
        EightHours       = 6,

        /// <summary>Requires 24 hours of uninterrupted casting.</summary>
        TwentyFourHours  = 7,

        /// <summary>
        /// Unusual casting time not covered by the other values.
        /// See the spell description for details.
        /// </summary>
        Special          = 8
    }
}