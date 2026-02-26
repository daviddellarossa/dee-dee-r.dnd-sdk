namespace DeeDeeR.DnD.Core.Interfaces
{
    /// <summary>
    /// Abstracts the source of randomness for dice rolls.
    /// Inject a deterministic implementation in tests; use the Unity implementation at runtime.
    /// </summary>
    public interface IRollProvider
    {
        /// <summary>
        /// Rolls a single die with the given number of faces.
        /// Returns a value in the range [1, faces].
        /// </summary>
        int RollDie(int faces);
    }
}
