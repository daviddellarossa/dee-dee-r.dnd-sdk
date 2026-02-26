namespace DeeDeeR.DnD.Core.Interfaces
{
    /// <summary>
    /// Implemented by anything that can regain hit points.
    /// </summary>
    public interface IHealable
    {
        /// <summary>Restores <paramref name="amount"/> hit points to this entity, up to its maximum.</summary>
        void Heal(int amount);
    }
}
