using System;

namespace DeeDeeR.DnD.Core.Values
{
    /// <summary>
    /// An immutable snapshot of a creature's death saving throw progress.
    /// Three successes = stabilized; three failures = dead.
    /// Resets when the creature regains any HP.
    /// </summary>
    public readonly struct DeathSaveState
    {
        /// <summary>Number of successful death saving throws made (0–3).</summary>
        public readonly int Successes;

        /// <summary>Number of failed death saving throws made (0–3).</summary>
        public readonly int Failures;

        /// <summary>True when three or more successes have been accumulated.</summary>
        public bool IsStabilized => Successes >= 3;

        /// <summary>True when three or more failures have been accumulated.</summary>
        public bool IsDead       => Failures  >= 3;

        /// <summary>Creates a death save state. Both values are clamped to zero minimum.</summary>
        /// <param name="successes">Number of successes (clamped to [0, ∞)).</param>
        /// <param name="failures">Number of failures (clamped to [0, ∞)).</param>
        public DeathSaveState(int successes, int failures)
        {
            Successes = Math.Max(0, successes);
            Failures  = Math.Max(0, failures);
        }

        /// <summary>Returns a new state with one additional success.</summary>
        public DeathSaveState WithSuccess() => new DeathSaveState(Successes + 1, Failures);

        /// <summary>Returns a new state with one additional failure.</summary>
        public DeathSaveState WithFailure() => new DeathSaveState(Successes, Failures + 1);

        /// <summary>Resets death save progress (e.g. on regaining HP or completing a rest).</summary>
        public static readonly DeathSaveState Empty = new DeathSaveState(0, 0);

        /// <summary>Returns a display string such as "Death Saves: 2✓ / 1✗".</summary>
        public override string ToString() => $"Death Saves: {Successes}✓ / {Failures}✗";
    }
}
