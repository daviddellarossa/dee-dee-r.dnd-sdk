namespace DeeDeeR.DnD.Core.Values
{
    /// <summary>
    /// An immutable snapshot of a creature's death saving throw progress.
    /// Three successes = stabilized; three failures = dead.
    /// Resets when the creature regains any HP.
    /// </summary>
    public readonly struct DeathSaveState
    {
        public readonly int Successes;
        public readonly int Failures;

        public bool IsStabilized => Successes >= 3;
        public bool IsDead       => Failures  >= 3;

        public DeathSaveState(int successes, int failures)
        {
            Successes = successes;
            Failures  = failures;
        }

        public DeathSaveState WithSuccess() => new DeathSaveState(Successes + 1, Failures);
        public DeathSaveState WithFailure() => new DeathSaveState(Successes, Failures + 1);

        /// <summary>Resets death save progress (e.g. on regaining HP or completing a rest).</summary>
        public static readonly DeathSaveState Empty = new DeathSaveState(0, 0);

        public override string ToString() => $"Death Saves: {Successes}✓ / {Failures}✗";
    }
}
