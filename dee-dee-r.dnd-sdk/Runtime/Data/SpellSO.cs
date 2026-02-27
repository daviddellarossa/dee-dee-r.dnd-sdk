using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// A spell definition per D&amp;D 2024 PHB rules.
    /// Spell slots, preparation, and concentration are managed at runtime by the systems layer
    /// (Phase 9). This SO is a pure data asset.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSpell", menuName = "DnD SDK/Spell")]
    public sealed class SpellSO : ScriptableObject
    {
        /// <summary>Spell level (0 = cantrip, 1–9 = spell level).</summary>
        [Range(0, 9)] public int Level;

        /// <summary>School of magic.</summary>
        public SpellSchool School;

        // ── Casting ───────────────────────────────────────────────────────────

        /// <summary>How long it takes to cast this spell.</summary>
        public CastingTimeType CastingTime = CastingTimeType.Action;

        [SerializeField] private LocalizedString _reactionTrigger = new LocalizedString();

        /// <summary>
        /// The trigger condition required to cast this spell as a Reaction.
        /// Only populated when <see cref="CastingTime"/> is <see cref="CastingTimeType.Reaction"/>
        /// (e.g. "which you take when you see a creature within 60 feet of you cast a spell").
        /// </summary>
        public LocalizedString ReactionTrigger => _reactionTrigger;

        // ── Range ─────────────────────────────────────────────────────────────

        /// <summary>The targeting range category of this spell.</summary>
        public SpellRangeType RangeType = SpellRangeType.Ranged;

        /// <summary>
        /// Range distance in feet. Only relevant when <see cref="RangeType"/> is
        /// <see cref="SpellRangeType.Ranged"/>.
        /// </summary>
        [Min(0)] public int RangeDistance;

        [SerializeField] private LocalizedString _selfAreaDescription = new LocalizedString();

        /// <summary>
        /// Parenthetical area description for self-targeting spells that also produce an area
        /// (e.g. "15-foot cone" for Burning Hands, "60-foot line" for Lightning Bolt).
        /// Only relevant when <see cref="RangeType"/> is <see cref="SpellRangeType.Self"/>.
        /// Leave empty for spells that affect only the caster with no emanating area.
        /// </summary>
        public LocalizedString SelfAreaDescription => _selfAreaDescription;

        // ── Components ────────────────────────────────────────────────────────

        /// <summary>Required casting components (Verbal, Somatic, Material — flags enum).</summary>
        public SpellComponent Components;

        [SerializeField] private LocalizedString _materialDescription = new LocalizedString();

        /// <summary>
        /// Description of the material component(s), if any.
        /// Only relevant when <see cref="Components"/> includes <see cref="SpellComponent.Material"/>.
        /// </summary>
        public LocalizedString MaterialDescription => _materialDescription;

        // ── Duration ──────────────────────────────────────────────────────────

        /// <summary>How long the spell's effect lasts.</summary>
        public SpellDurationType Duration = SpellDurationType.Instantaneous;

        /// <summary>
        /// Whether this spell requires concentration to maintain. When the caster loses
        /// concentration (takes damage, is incapacitated, etc.) the spell ends immediately.
        /// </summary>
        public bool IsConcentration;

        /// <summary>Whether this spell can be cast as a ritual (takes 10 extra minutes, consumes no spell slot).</summary>
        public bool IsRitual;

        // ── Description ───────────────────────────────────────────────────────

        [SerializeField] private LocalizedString _description = new LocalizedString();
        [SerializeField] private LocalizedString _higherLevelDescription = new LocalizedString();

        /// <summary>Full rules text for this spell at its base level.</summary>
        public LocalizedString Description => _description;

        /// <summary>
        /// Additional rules text describing the spell's effect when cast using a higher-level slot.
        /// Leave empty for spells that do not scale with slot level.
        /// </summary>
        public LocalizedString HigherLevelDescription => _higherLevelDescription;

        // ── Class Lists ───────────────────────────────────────────────────────

        /// <summary>
        /// Classes for which this spell appears on the spell list.
        /// Subclass-specific spells are also listed here against the parent class.
        /// </summary>
        public List<ClassSO> ClassLists = new List<ClassSO>();
    }
}
