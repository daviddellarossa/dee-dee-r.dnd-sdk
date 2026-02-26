namespace DeeDeeR.DnD.Core.Values
{
    /// <summary>
    /// The result of an attack roll, combining the raw roll with hit determination.
    /// </summary>
    public readonly struct AttackRollResult
    {
        /// <summary>The underlying dice roll (total, crit flags).</summary>
        public readonly RollResult Roll;

        /// <summary>True if the attack total meets or exceeds the target's AC, or if it is a critical hit.</summary>
        public readonly bool Hit;

        /// <summary>True if the raw d20 was a natural 20 (always hits and deals critical damage).</summary>
        public bool Critical => Roll.IsCriticalSuccess;

        /// <summary>True if the raw d20 was a natural 1 (always misses).</summary>
        public bool Fumble => Roll.IsCriticalFail;

        /// <summary>
        /// Creates an attack roll result. Natural 20 always hits; natural 1 always misses,
        /// regardless of the <paramref name="hit"/> parameter.
        /// </summary>
        /// <param name="roll">The raw d20 roll result (with modifiers applied).</param>
        /// <param name="hit">Whether the attack total meets or exceeds the target's AC.</param>
        public AttackRollResult(RollResult roll, bool hit)
        {
            Roll = roll;
            Hit  = !roll.IsCriticalFail && (hit || roll.IsCriticalSuccess);
        }
    }
}
