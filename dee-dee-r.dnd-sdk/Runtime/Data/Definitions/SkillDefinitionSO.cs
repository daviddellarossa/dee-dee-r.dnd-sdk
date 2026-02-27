using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;

namespace DeeDeeR.DnD.Runtime.Data.Definitions
{
    /// <summary>
    /// Companion ScriptableObject for <see cref="SkillType"/>.
    /// Provides localized display name and description for a single skill.
    /// <see cref="LinkedAbility"/> is derived from <see cref="SkillTypeExtensions.GetAbility"/> —
    /// it is not a serialized field and cannot be overridden.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkillDefinition", menuName = "DnD SDK/Definitions/Skill")]
    public sealed class SkillDefinitionSO : ScriptableObject, ILocalizable
    {
        /// <summary>The skill this asset describes.</summary>
        public SkillType Type;

        [SerializeField] private LocalizedString _displayName;
        [SerializeField] private LocalizedString _displayDescription;

        /// <summary>
        /// The ability score that governs this skill, derived from PHB rules via
        /// <see cref="SkillTypeExtensions.GetAbility"/>. Read-only; not serialized.
        /// </summary>
        public AbilityType LinkedAbility => Type.GetAbility();

        /// <inheritdoc/>
        public string LocalizationKey => _displayName.TableEntryReference.Key;

        /// <inheritdoc/>
        public string LocalizationDescriptionKey => _displayDescription.TableEntryReference.Key;
    }
}