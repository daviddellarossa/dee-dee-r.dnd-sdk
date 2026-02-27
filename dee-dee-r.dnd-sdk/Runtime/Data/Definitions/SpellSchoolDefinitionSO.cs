using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;

namespace DeeDeeR.DnD.Runtime.Data.Definitions
{
    /// <summary>
    /// Companion ScriptableObject for <see cref="SpellSchool"/>.
    /// Provides localized display name and description for a school of magic.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSpellSchoolDefinition", menuName = "DnD SDK/Definitions/Spell School")]
    public sealed class SpellSchoolDefinitionSO : ScriptableObject, ILocalizable
    {
        /// <summary>The school of magic this asset describes.</summary>
        public SpellSchool Type;

        [SerializeField] private LocalizedString _displayName;
        [SerializeField] private LocalizedString _displayDescription;

        /// <inheritdoc/>
        public string LocalizationKey => _displayName.TableEntryReference.Key;

        /// <inheritdoc/>
        public string LocalizationDescriptionKey => _displayDescription.TableEntryReference.Key;
    }
}