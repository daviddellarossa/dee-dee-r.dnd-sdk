using System;
using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Core.Values
{
    /// <summary>
    /// An immutable snapshot of all six ability scores for a creature.
    /// Does not include bonuses from equipment, conditions, or spells —
    /// those are applied by AbilitySystem at query time.
    /// </summary>
    public readonly struct AbilityScoreSet
    {
        private readonly int _strength;
        private readonly int _dexterity;
        private readonly int _constitution;
        private readonly int _intelligence;
        private readonly int _wisdom;
        private readonly int _charisma;

        /// <summary>Creates an ability score set from six raw score values (typically 1–30).</summary>
        public AbilityScoreSet(int str, int dex, int con, int intel, int wis, int cha)
        {
            _strength     = str;
            _dexterity    = dex;
            _constitution = con;
            _intelligence = intel;
            _wisdom       = wis;
            _charisma     = cha;
        }

        /// <summary>Returns the raw score for the given ability.</summary>
        public int GetScore(AbilityType ability) => ability switch
        {
            AbilityType.Strength     => _strength,
            AbilityType.Dexterity    => _dexterity,
            AbilityType.Constitution => _constitution,
            AbilityType.Intelligence => _intelligence,
            AbilityType.Wisdom       => _wisdom,
            AbilityType.Charisma     => _charisma,
            _                        => throw new ArgumentOutOfRangeException(nameof(ability))
        };

        /// <summary>
        /// Standard D&D ability score modifier formula: (score - 10) / 2, rounded down.
        /// </summary>
        public static int GetModifier(int score) => (int)Math.Floor((score - 10) / 2.0);

        /// <summary>Returns the ability modifier for the given ability score.</summary>
        public int GetModifier(AbilityType ability) => GetModifier(GetScore(ability));

        /// <summary>Returns a new AbilityScoreSet with the given ability adjusted by delta.</summary>
        public AbilityScoreSet With(AbilityType ability, int newScore) => ability switch
        {
            AbilityType.Strength     => new AbilityScoreSet(newScore,    _dexterity, _constitution, _intelligence, _wisdom, _charisma),
            AbilityType.Dexterity    => new AbilityScoreSet(_strength,   newScore,   _constitution, _intelligence, _wisdom, _charisma),
            AbilityType.Constitution => new AbilityScoreSet(_strength,   _dexterity, newScore,       _intelligence, _wisdom, _charisma),
            AbilityType.Intelligence => new AbilityScoreSet(_strength,   _dexterity, _constitution,  newScore,      _wisdom, _charisma),
            AbilityType.Wisdom       => new AbilityScoreSet(_strength,   _dexterity, _constitution,  _intelligence, newScore, _charisma),
            AbilityType.Charisma     => new AbilityScoreSet(_strength,   _dexterity, _constitution,  _intelligence, _wisdom, newScore),
            _                        => throw new ArgumentOutOfRangeException(nameof(ability))
        };
    }
}
