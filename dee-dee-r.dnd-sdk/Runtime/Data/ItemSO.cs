using UnityEngine;
using UnityEngine.Localization;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// Base ScriptableObject for any item that can be carried or granted as starting equipment.
    /// The item's display name is the asset name (<c>this.name</c> from <see cref="UnityEngine.Object"/>).
    /// Subclassed by <see cref="ToolSO"/> and <see cref="AdventuringGearSO"/>.
    /// Weapons, armour, and ammunition use their own SOs and do not extend this class.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "DnD SDK/Items/Generic Item")]
    public class ItemSO : ScriptableObject
    {
        [SerializeField] private LocalizedString _description = new LocalizedString();

        /// <summary>Short description of what this item is or does.</summary>
        public LocalizedString Description => _description;

        /// <summary>Weight in pounds.</summary>
        [Min(0f)] public float Weight;

        /// <summary>Market cost of one unit of this item.</summary>
        public CurrencyData Cost = new CurrencyData();
    }
}