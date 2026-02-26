namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Weapon Mastery properties introduced in D&D 2024 PHB.
    /// A character must have the relevant class feature to use a weapon's mastery property.
    /// </summary>
    public enum WeaponMastery
    {
        /// <summary>No mastery property (non-mastery weapons, or property not yet unlocked).</summary>
        None   = 0,

        /// <summary>On a hit, attack a second creature adjacent to the first (no roll needed for the extra attack).</summary>
        Cleave = 1,

        /// <summary>On a miss, still deal damage equal to the ability modifier to the target.</summary>
        Graze  = 2,

        /// <summary>When used with the Light property off-hand attack, it costs no Bonus Action.</summary>
        Nick   = 3,

        /// <summary>On a hit, push the target 10 feet away (no save).</summary>
        Push   = 4,

        /// <summary>On a hit, target has Disadvantage on its next attack roll before the start of your next turn.</summary>
        Sap    = 5,

        /// <summary>On a hit, target's Speed is reduced by 10 feet until the start of your next turn.</summary>
        Slow   = 6,

        /// <summary>On a hit, target must succeed on a CON save or be knocked Prone.</summary>
        Topple = 7,

        /// <summary>On a hit, you have Advantage on your next attack roll against the same target before the end of your turn.</summary>
        Vex    = 8
    }
}
