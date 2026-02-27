using UnityEngine;
using UnityEngine.Localization;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// Ammunition consumed by ranged weapons that have the Ammunition property
    /// (Arrows, Bolts, Sling Bullets, Needles).
    /// Referenced by <see cref="WeaponSO.RequiredAmmo"/>; quantity is tracked in
    /// <c>InventoryState</c> (Phase 5).
    /// </summary>
    [CreateAssetMenu(fileName = "NewAmmunition", menuName = "DnD SDK/Items/Ammunition")]
    public sealed class AmmunitionSO : ScriptableObject
    {
        [SerializeField] private LocalizedString _description = new LocalizedString();

        /// <summary>Short description (e.g. "Arrows for shortbows and longbows.").</summary>
        public LocalizedString Description => _description;

        /// <summary>Weight per unit in pounds.</summary>
        [Min(0f)] public float Weight;

        /// <summary>Cost per unit.</summary>
        public CurrencyData Cost = new CurrencyData();
    }
}