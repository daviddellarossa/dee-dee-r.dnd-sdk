namespace DeeDeeR.DnD.Core.Values
{
    /// <summary>
    /// The result of a dice roll. For d20 checks, IsCriticalSuccess and IsCriticalFail
    /// reflect a natural 20 or natural 1 respectively before any modifiers.
    /// </summary>
    public readonly struct RollResult
    {
        /// <summary>Final total including all modifiers.</summary>
        public readonly int  Total;

        /// <summary>True if the raw d20 showed a natural 20.</summary>
        public readonly bool IsCriticalSuccess;

        /// <summary>True if the raw d20 showed a natural 1.</summary>
        public readonly bool IsCriticalFail;

        public RollResult(int total, bool isCriticalSuccess = false, bool isCriticalFail = false)
        {
            Total             = total;
            IsCriticalSuccess = isCriticalSuccess;
            IsCriticalFail    = isCriticalFail;
        }
    }
}
