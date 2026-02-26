namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Types of rest a creature can take, per D&D 2024 PHB.
    /// </summary>
    public enum RestType
    {
        /// <summary>Short rest: at least 1 hour. Allows spending Hit Dice to recover HP.</summary>
        Short = 0,

        /// <summary>Long rest: at least 8 hours. Fully restores HP, resets spell slots, and recovers half of total Hit Dice.</summary>
        Long  = 1
    }
}
