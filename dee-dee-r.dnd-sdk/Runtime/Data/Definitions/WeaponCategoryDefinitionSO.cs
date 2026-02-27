using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;

namespace DeeDeeR.DnD.Runtime.Data.Definitions
{
    /// <summary>
    /// Companion ScriptableObject for <see cref="WeaponCategory"/>.
    /// Provides localized display name and description for Simple or Martial weapons.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeaponCategoryDefinition", menuName = "DnD SDK/Definitions/Weapon Category")]
    public sealed class WeaponCategoryDefinitionSO : ScriptableObject, ILocalizable
    {
        /// <summary>The weapon category this asset describes.</summary>
        public WeaponCategory Type;

        [SerializeField] private LocalizedString _displayName;
        [SerializeField] private LocalizedString _displayDescription;

        /// <inheritdoc/>
        public string LocalizationKey => _displayName.TableEntryReference.Key;

        /// <inheritdoc/>
        public string LocalizationDescriptionKey => _displayDescription.TableEntryReference.Key;
    }
}