namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Tracks accumulated sources of advantage and disadvantage for a roll.
    /// Per D&D 2024 PHB: any number of advantage sources still counts as one advantage,
    /// and advantage + disadvantage cancel out to Normal regardless of quantity.
    /// </summary>
    public enum AdvantageState
    {
        Normal      = 0,
        Advantage   = 1,
        Disadvantage = 2
    }
}
