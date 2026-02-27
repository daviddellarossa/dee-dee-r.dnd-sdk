using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// A feat — an optional character advancement that provides special abilities.
    /// D&amp;D 2024 feat categories: Origin (gained at character creation via background),
    /// General (gained at certain levels), Fighting Style, and Epic Boon (level 19+).
    /// </summary>
    [CreateAssetMenu(fileName = "NewFeat", menuName = "DnD SDK/Feat")]
    public sealed class FeatSO : ScriptableObject
    {
        /// <summary>The category of this feat (Origin, General, Fighting Style, Epic Boon).</summary>
        public FeatCategory Category;

        [SerializeField] private LocalizedString _description = new LocalizedString();
        [SerializeField] private LocalizedString _prerequisiteDescription = new LocalizedString();

        /// <summary>Full rules text describing what this feat does.</summary>
        public LocalizedString Description => _description;

        /// <summary>
        /// Human-readable description of any prerequisites (e.g. "Level 4+, proficiency with
        /// martial weapons"). The prerequisite enforcement logic lives in the systems layer (Phase 6).
        /// </summary>
        public LocalizedString PrerequisiteDescription => _prerequisiteDescription;

        /// <summary>
        /// Ability score increases granted by this feat. Most General feats grant +1 to one ability.
        /// Epic Boons often grant +1 to any ability. Leave empty if this feat grants no ASI.
        /// </summary>
        public List<AbilityScoreIncrease> AbilityScoreIncreases = new List<AbilityScoreIncrease>();
    }
}