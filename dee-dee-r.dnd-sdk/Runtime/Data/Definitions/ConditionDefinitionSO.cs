using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;

namespace DeeDeeR.DnD.Runtime.Data.Definitions
{
    /// <summary>
    /// Companion ScriptableObject for <see cref="Condition"/>.
    /// Provides localized display name and description for a single condition.
    /// Exhaustion is not a <see cref="Condition"/> value — it is modelled as
    /// <see cref="DeeDeeR.DnD.Core.Values.ExhaustionLevel"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "NewConditionDefinition", menuName = "DnD SDK/Definitions/Condition")]
    public sealed class ConditionDefinitionSO : ScriptableObject, ILocalizable
    {
        /// <summary>The condition this asset describes.</summary>
        public Condition Type;

        [SerializeField] private LocalizedString _displayName;
        [SerializeField] private LocalizedString _displayDescription;

        /// <inheritdoc/>
        public string LocalizationKey => _displayName.TableEntryReference.Key;

        /// <inheritdoc/>
        public string LocalizationDescriptionKey => _displayDescription.TableEntryReference.Key;
    }
}