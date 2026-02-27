using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;

namespace DeeDeeR.DnD.Runtime.Data.Definitions
{
    /// <summary>
    /// Companion ScriptableObject for <see cref="DieType"/>.
    /// Provides localized display name and description for a single die type.
    /// <see cref="NumOfFaces"/> is derived from the enum value — <see cref="DieType"/> values
    /// equal their face count.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDie", menuName = "DnD SDK/Definitions/Die")]
    public sealed class DieSO : ScriptableObject, ILocalizable
    {
        /// <summary>The die type this asset describes (D4, D6, D8, etc.).</summary>
        public DieType Type;

        [SerializeField] private LocalizedString _displayName;
        [SerializeField] private LocalizedString _displayDescription;

        /// <summary>
        /// The number of faces on this die. Derived from the enum value —
        /// <see cref="DieType"/> values equal their face count.
        /// </summary>
        public int NumOfFaces => (int)Type;

        /// <inheritdoc/>
        public string LocalizationKey => _displayName.TableEntryReference.Key;

        /// <inheritdoc/>
        public string LocalizationDescriptionKey => _displayDescription.TableEntryReference.Key;
    }
}