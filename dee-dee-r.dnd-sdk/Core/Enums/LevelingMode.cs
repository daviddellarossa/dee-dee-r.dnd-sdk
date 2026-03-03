namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Determines how a character advances in level.
    /// </summary>
    public enum LevelingMode
    {
        /// <summary>
        /// Characters gain levels when they accumulate enough Experience Points
        /// per the standard XP thresholds in the D&amp;D 2024 PHB.
        /// </summary>
        ExperiencePoints,

        /// <summary>
        /// The Dungeon Master decides when characters advance; XP is not tracked.
        /// <c>XPSystem.IsReadyToLevelUp</c> always returns <see langword="false"/>
        /// under this mode — level-up is a narrative decision, not a calculation.
        /// </summary>
        Milestone,
    }
}