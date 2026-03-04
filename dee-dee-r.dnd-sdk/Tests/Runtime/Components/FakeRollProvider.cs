using DeeDeeR.DnD.Core.Interfaces;

namespace DeeDeeR.DnD.Tests.Runtime.Components
{
    /// <summary>
    /// Deterministic roll provider for PlayMode component tests.
    /// Always returns <c>fixedValue</c> clamped to the die's valid face range.
    /// </summary>
    internal sealed class FakeRollProvider : IRollProvider
    {
        private readonly int _fixedValue;

        public FakeRollProvider(int fixedValue) => _fixedValue = fixedValue;

        public int RollDie(int faces) => System.Math.Clamp(_fixedValue, 1, faces);
    }
}