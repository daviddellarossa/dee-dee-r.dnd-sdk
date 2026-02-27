using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;

namespace DeeDeeR.DnD.Runtime.Data.Definitions
{
    /// <summary>
    /// Companion ScriptableObject for <see cref="DamageType"/>.
    /// Provides localized display name and description for a single damage type.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDamageTypeDefinition", menuName = "DnD SDK/Definitions/Damage Type")]
    public sealed class DamageTypeDefinitionSO : ScriptableObject, ILocalizable
    {
        /// <summary>The damage type this asset describes.</summary>
        public DamageType Type;

        [SerializeField] private LocalizedString _displayName;
        [SerializeField] private LocalizedString _displayDescription;

        /// <inheritdoc/>
        public string LocalizationKey => _displayName.TableEntryReference.Key;

        /// <inheritdoc/>
        public string LocalizationDescriptionKey => _displayDescription.TableEntryReference.Key;
    }
}