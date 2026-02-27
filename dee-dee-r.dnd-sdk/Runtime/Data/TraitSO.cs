using UnityEngine;
using UnityEngine.Localization;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// A species or subspecies trait (e.g. "Darkvision", "Fey Ancestry", "Keen Senses").
    /// Traits are reusable assets — create one asset and reference it from multiple
    /// <see cref="SpeciesSO"/> or <see cref="SubspeciesSO"/> instances.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTrait", menuName = "DnD SDK/Trait")]
    public sealed class TraitSO : ScriptableObject
    {
        [SerializeField] private LocalizedString _description = new LocalizedString();

        /// <summary>Rules text describing what this trait does.</summary>
        public LocalizedString Description => _description;
    }
}