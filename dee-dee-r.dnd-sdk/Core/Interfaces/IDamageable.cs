using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Core.Interfaces
{
    /// <summary>
    /// Implemented by anything that can receive damage.
    /// The concrete implementation is responsible for applying resistances,
    /// immunities, and vulnerability before modifying state.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount, DamageType damageType);
    }
}
