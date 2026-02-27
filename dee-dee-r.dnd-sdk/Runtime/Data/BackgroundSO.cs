using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// A character background per D&amp;D 2024 PHB rules.
    /// Backgrounds are the source of Ability Score Increases in D&amp;D 2024 — species no longer
    /// grant ASIs. Each background grants a total of +2 spread across one or two abilities,
    /// one Origin feat, two skill proficiencies, one tool proficiency, and starting equipment.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBackground", menuName = "DnD SDK/Background")]
    public sealed class BackgroundSO : ScriptableObject
    {
        [SerializeField] private LocalizedString _description = new LocalizedString();
        [SerializeField] private LocalizedString _flavorText = new LocalizedString();

        /// <summary>Flavour description of this background.</summary>
        public LocalizedString Description => _description;

        /// <summary>
        /// Ability score increases granted by this background (total +2 across one or two abilities).
        /// D&amp;D 2024 allows splitting the bonus: one entry of +2 to a single ability,
        /// or two entries of +1 each to two different abilities.
        /// </summary>
        public List<AbilityScoreIncrease> AbilityScoreIncreases = new List<AbilityScoreIncrease>();

        /// <summary>
        /// The Origin feat granted by this background.
        /// Origin feats are gained at character creation and have no level prerequisites.
        /// </summary>
        public FeatSO OriginFeat;

        /// <summary>The two skill proficiencies granted by this background.</summary>
        public SkillType[] SkillProficiencies = new SkillType[2];

        /// <summary>Tool proficiency granted by this background.</summary>
        public ToolSO ToolProficiency;

        /// <summary>Items granted as starting equipment.</summary>
        public List<ItemGrant> StartingEquipment = new List<ItemGrant>();

        /// <summary>Flavour text describing the background's personality traits, ideals, bonds, and flaws.</summary>
        public LocalizedString FlavorText => _flavorText;
    }
}