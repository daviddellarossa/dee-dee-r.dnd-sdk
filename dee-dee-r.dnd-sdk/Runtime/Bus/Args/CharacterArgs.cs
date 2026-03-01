using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Runtime.Bus.Args
{
    /// <summary>Published when a character gains a level in any class.</summary>
    public readonly struct LevelUpArgs
    {
        public readonly EndpointId Character;
        public readonly ClassSO    Class;
        public readonly int        NewLevel;

        public LevelUpArgs(EndpointId character, ClassSO @class, int newLevel)
        {
            Character = character;
            Class     = @class;
            NewLevel  = newLevel;
        }
    }

    /// <summary>Published when one of a character's six ability scores is permanently changed.</summary>
    public readonly struct AbilityScoreArgs
    {
        public readonly EndpointId Character;
        public readonly AbilityType Ability;
        public readonly int         Previous;
        public readonly int         Current;

        public AbilityScoreArgs(EndpointId character, AbilityType ability, int previous, int current)
        {
            Character = character;
            Ability   = ability;
            Previous  = previous;
            Current   = current;
        }
    }

    /// <summary>Published when a character is granted a feat (level-up, background, or class feature).</summary>
    public readonly struct FeatArgs
    {
        public readonly EndpointId Character;
        public readonly FeatSO     Feat;

        public FeatArgs(EndpointId character, FeatSO feat)
        {
            Character = character;
            Feat      = feat;
        }
    }

    /// <summary>Query args carrying the ability type whose modifier is requested.</summary>
    public readonly struct GetAbilityModifierArgs
    {
        public readonly AbilityType Ability;

        public GetAbilityModifierArgs(AbilityType ability) => Ability = ability;
    }

    /// <summary>Query args carrying the skill type whose total bonus is requested.</summary>
    public readonly struct GetSkillBonusArgs
    {
        public readonly SkillType Skill;

        public GetSkillBonusArgs(SkillType skill) => Skill = skill;
    }
}