using DeeDeeR.DnD.Core.Interfaces;
using UnityEngine;

namespace DeeDeeR.DnD.Runtime.Systems
{
    /// <summary>
    /// <see cref="IRollProvider"/> implementation backed by <see cref="UnityEngine.Random"/>.
    /// Attach or inject this at runtime; swap for a deterministic provider in tests.
    /// </summary>
    public class UnityRollProvider : IRollProvider
    {
        /// <summary>
        /// Returns a random integer in the range [1, <paramref name="faces"/>] inclusive,
        /// using <see cref="Random.Range(int, int)"/>.
        /// </summary>
        public int RollDie(int faces) => Random.Range(1, faces + 1);
    }
}