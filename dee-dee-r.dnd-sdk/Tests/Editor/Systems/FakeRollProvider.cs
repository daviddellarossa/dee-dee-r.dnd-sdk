using System;
using System.Collections.Generic;
using DeeDeeR.DnD.Core.Interfaces;

namespace DeeDeeR.DnD.Tests.Editor.Systems
{
    /// <summary>
    /// Deterministic <see cref="IRollProvider"/> for unit tests.
    /// Returns values from a preset sequence in order; throws if the sequence is exhausted.
    /// </summary>
    internal class FakeRollProvider : IRollProvider
    {
        private readonly Queue<int> _results;

        /// <param name="results">Die results to return, in order, one per <see cref="RollDie"/> call.</param>
        public FakeRollProvider(params int[] results)
        {
            _results = new Queue<int>(results);
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">Thrown when the preset sequence is exhausted.</exception>
        public int RollDie(int faces)
        {
            if (_results.Count == 0)
                throw new InvalidOperationException(
                    "FakeRollProvider ran out of preset results. Add more values to the constructor.");

            return _results.Dequeue();
        }
    }
}