namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Tracks accumulated sources of advantage and disadvantage for a roll.
    /// Per D&D 2024 PHB: any number of advantage sources still counts as one advantage,
    /// and advantage + disadvantage cancel out to Normal regardless of quantity.
    /// </summary>
    public enum AdvantageState
    {
        /// <summary>Roll a single d20 with no modification from advantage or disadvantage.</summary>
        Normal      = 0,

        /// <summary>Roll two d20s and take the higher result. Any number of advantage sources still yields one advantage.</summary>
        Advantage   = 1,

        /// <summary>Roll two d20s and take the lower result. Advantage and Disadvantage together cancel to Normal.</summary>
        Disadvantage = 2
    }
}
