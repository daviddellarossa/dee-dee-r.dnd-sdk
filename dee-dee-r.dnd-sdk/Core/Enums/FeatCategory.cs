namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Feat categories introduced in D&D 2024 PHB.
    /// </summary>
    public enum FeatCategory
    {
        /// <summary>Granted by Background at character creation. No prerequisites.</summary>
        Origin       = 0,

        /// <summary>Available at level 4+ via Ability Score Improvement feature. May have prerequisites.</summary>
        General      = 1,

        /// <summary>Replaces a Fighting Style option for eligible classes.</summary>
        FightingStyle = 2,

        /// <summary>Available only at level 19+ (Epic Boon feature).</summary>
        EpicBoon     = 3
    }
}
