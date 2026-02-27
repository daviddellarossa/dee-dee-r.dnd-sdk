using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;

namespace DeeDeeR.DnD.Runtime.Data.Definitions
{
    /// <summary>
    /// Companion ScriptableObject for <see cref="AbilityType"/>.
    /// Provides localized display name and description for a single ability score.
    /// Create one asset per ability (Strength, Dexterity, …) and assign them in project settings.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbilityDefinition", menuName = "DnD SDK/Definitions/Ability")]
    public sealed class AbilityDefinitionSO : ScriptableObject, ILocalizable
    {
        /// <summary>The ability this asset describes.</summary>
        public AbilityType Type;

        [SerializeField] private LocalizedString _displayName;
        [SerializeField] private LocalizedString _displayDescription;

        /// <inheritdoc/>
        public string LocalizationKey => _displayName.TableEntryReference.Key;

        /// <inheritdoc/>
        public string LocalizationDescriptionKey => _displayDescription.TableEntryReference.Key;
    }
}