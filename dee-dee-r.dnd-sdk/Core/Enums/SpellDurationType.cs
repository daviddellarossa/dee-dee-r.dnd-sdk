namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// How long a spell's effect lasts, as defined in D&amp;D 2024 PHB.
    /// Whether the spell requires concentration is tracked separately in
    /// <c>SpellSO.IsConcentration</c>.
    /// </summary>
    public enum SpellDurationType
    {
        /// <summary>The spell's effect occurs immediately and does not persist.</summary>
        Instantaneous            = 0,

        /// <summary>The spell lasts until the end of the next round of combat.</summary>
        OneRound                 = 1,

        /// <summary>The spell lasts up to 1 minute (10 combat rounds).</summary>
        OneMinute                = 2,

        /// <summary>The spell lasts up to 10 minutes.</summary>
        TenMinutes               = 3,

        /// <summary>The spell lasts up to 1 hour.</summary>
        OneHour                  = 4,

        /// <summary>The spell lasts up to 8 hours.</summary>
        EightHours               = 5,

        /// <summary>The spell lasts up to 24 hours.</summary>
        TwentyFourHours          = 6,

        /// <summary>The spell lasts up to 7 days.</summary>
        SevenDays                = 7,

        /// <summary>The spell persists until it is magically dispelled.</summary>
        UntilDispelled           = 8,

        /// <summary>
        /// The spell persists until it is dispelled or until it is triggered and discharged
        /// (e.g. Contingency, Symbol).
        /// </summary>
        UntilDispelledOrTriggered = 9,

        /// <summary>
        /// An unusual duration not covered by the other values.
        /// See the spell description for details.
        /// </summary>
        Special                  = 10
    }
}