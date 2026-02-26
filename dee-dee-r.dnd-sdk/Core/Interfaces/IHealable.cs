namespace DeeDeeR.DnD.Core.Interfaces
{
    /// <summary>
    /// Implemented by anything that can regain hit points.
    /// </summary>
    public interface IHealable
    {
        void Heal(int amount);
    }
}
