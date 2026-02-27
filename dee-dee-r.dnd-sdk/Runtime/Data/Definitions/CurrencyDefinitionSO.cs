using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;

namespace DeeDeeR.DnD.Runtime.Data.Definitions
{
    /// <summary>
    /// Companion ScriptableObject for <see cref="CurrencyType"/>.
    /// Provides localized display name and description for a currency denomination.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCurrencyDefinition", menuName = "DnD SDK/Definitions/Currency")]
    public sealed class CurrencyDefinitionSO : ScriptableObject, ILocalizable
    {
        /// <summary>The currency denomination this asset describes.</summary>
        public CurrencyType Type;

        [SerializeField] private LocalizedString _displayName;
        [SerializeField] private LocalizedString _displayDescription;

        /// <inheritdoc/>
        public string LocalizationKey => _displayName.TableEntryReference.Key;

        /// <inheritdoc/>
        public string LocalizationDescriptionKey => _displayDescription.TableEntryReference.Key;
    }
}