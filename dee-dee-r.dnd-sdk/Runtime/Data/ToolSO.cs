using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// A tool that characters can be proficient in (e.g. Thieves' Tools, Healer's Kit,
    /// Alchemist's Supplies). Proficiency allows adding the proficiency bonus to ability
    /// checks made with the tool.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTool", menuName = "DnD SDK/Items/Tool")]
    public sealed class ToolSO : ItemSO
    {
        /// <summary>
        /// The skill most closely associated with checks made using this tool.
        /// Used as the governing ability when no specific ability is called for.
        /// </summary>
        public SkillType AssociatedSkill;

        [SerializeField] private LocalizedString _usageRules = new LocalizedString();

        /// <summary>Rules text describing how proficiency with this tool applies.</summary>
        public LocalizedString UsageRules => _usageRules;
    }
}