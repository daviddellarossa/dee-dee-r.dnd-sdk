using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;

namespace DeeDeeR.DnD.Runtime.Data.Definitions
{
    /// <summary>
    /// Companion ScriptableObject for <see cref="WeaponMastery"/>.
    /// Provides localized display name and description for a weapon mastery property
    /// introduced in D&amp;D 2024.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeaponMasteryDefinition", menuName = "DnD SDK/Definitions/Weapon Mastery")]
    public sealed class WeaponMasteryDefinitionSO : ScriptableObject, ILocalizable
    {
        /// <summary>The mastery property this asset describes.</summary>
        public WeaponMastery Type;

        [SerializeField] private LocalizedString _displayName;
        [SerializeField] private LocalizedString _displayDescription;

        /// <inheritdoc/>
        public string LocalizationKey => _displayName.TableEntryReference.Key;

        /// <inheritdoc/>
        public string LocalizationDescriptionKey => _displayDescription.TableEntryReference.Key;
    }
}