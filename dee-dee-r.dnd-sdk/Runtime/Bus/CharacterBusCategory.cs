using DeeDeeR.DnD.Runtime.Bus.Args;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Runtime.Bus
{
    /// <summary>
    /// Bus channels for character progression events and queries (levels, ability scores, feats).
    /// </summary>
    public sealed class CharacterBusCategory
    {
        // ── Signals ───────────────────────────────────────────────────────────

        /// <summary>Published when a character gains a level in any class.</summary>
        public readonly Signal<LevelUpArgs>      LeveledUp;

        /// <summary>Published when one of a character's ability scores is permanently changed.</summary>
        public readonly Signal<AbilityScoreArgs> AbilityScoreChanged;

        /// <summary>Published when a character is granted a feat.</summary>
        public readonly Signal<FeatArgs>         FeatGranted;

        // ── Queries ───────────────────────────────────────────────────────────

        /// <summary>Returns the addressed character's modifier for the specified ability.</summary>
        public readonly Query<GetAbilityModifierArgs, int> GetAbilityModifier;

        /// <summary>Returns the addressed character's total bonus for the specified skill.</summary>
        public readonly Query<GetSkillBonusArgs, int>      GetSkillBonus;

        /// <summary>Returns the addressed character's proficiency bonus.</summary>
        public readonly Query<EmptyArgs, int>              GetProficiencyBonus;

        // ── Constructor ───────────────────────────────────────────────────────

        public CharacterBusCategory(IFrameScheduler scheduler)
        {
            LeveledUp           = new Signal<LevelUpArgs>(scheduler);
            AbilityScoreChanged = new Signal<AbilityScoreArgs>(scheduler);
            FeatGranted         = new Signal<FeatArgs>(scheduler);
            GetAbilityModifier  = new Query<GetAbilityModifierArgs, int>(scheduler);
            GetSkillBonus       = new Query<GetSkillBonusArgs, int>(scheduler);
            GetProficiencyBonus = new Query<EmptyArgs, int>(scheduler);
        }
    }
}