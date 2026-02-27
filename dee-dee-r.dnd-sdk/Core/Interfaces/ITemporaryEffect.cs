namespace DeeDeeR.DnD.Core.Interfaces
{
    /// <summary>
    /// Contract for a temporary effect applied to a character (e.g. a spell duration, a condition
    /// with a timer, a magic item effect). Implement this interface in game code and add instances
    /// to <c>CharacterState.TemporaryEffects</c> to track custom effects without taking a dependency
    /// on SDK internals.
    /// </summary>
    /// <remarks>
    /// The SDK never creates or interprets implementations of this interface — it only holds
    /// the list and removes expired entries. All game-specific logic lives in the implementation.
    /// </remarks>
    public interface ITemporaryEffect
    {
        /// <summary>
        /// Whether this effect has expired and should be removed from the character's effect list.
        /// The <c>ConditionSystem</c> (Phase 7) will prune expired effects each tick.
        /// </summary>
        bool IsExpired { get; }
    }
}