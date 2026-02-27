using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;

namespace DeeDeeR.DnD.Runtime.Data.Definitions
{
    /// <summary>
    /// Companion ScriptableObject for <see cref="LanguageType"/>.
    /// Provides localized display name and description for a language (standard or rare).
    /// </summary>
    [CreateAssetMenu(fileName = "NewLanguageDefinition", menuName = "DnD SDK/Definitions/Language")]
    public sealed class LanguageDefinitionSO : ScriptableObject, ILocalizable
    {
        /// <summary>The language this asset describes.</summary>
        public LanguageType Type;

        [SerializeField] private LocalizedString _displayName;
        [SerializeField] private LocalizedString _displayDescription;

        /// <inheritdoc/>
        public string LocalizationKey => _displayName.TableEntryReference.Key;

        /// <inheritdoc/>
        public string LocalizationDescriptionKey => _displayDescription.TableEntryReference.Key;
    }
}