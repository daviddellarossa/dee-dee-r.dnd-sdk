namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Intrinsic weapon properties as defined in D&D 2024 PHB.
    /// Distinct from WeaponMastery properties, which are class-gated.
    /// </summary>
    public enum WeaponProperty
    {
        /// <summary>Weapon requires ammunition (arrows, bolts, etc.). Ammunition is recovered after battle.</summary>
        Ammunition = 0,

        /// <summary>Attack rolls can use the higher of STR or DEX modifier.</summary>
        Finesse    = 1,

        /// <summary>Requires STR 13+ to use without penalty. Small creatures have disadvantage.</summary>
        Heavy      = 2,

        /// <summary>When wielded with another Light weapon, the other can be used for an off-hand attack as a Bonus Action.</summary>
        Light      = 3,

        /// <summary>Only one attack per action (no Extra Attack). Must be reloaded between attacks.</summary>
        Loading    = 4,

        /// <summary>Weapon has normal and long range (in feet). Attacks beyond normal range have disadvantage.</summary>
        Range      = 5,

        /// <summary>Melee weapon with extended reach; adds 5 feet to the wielder's reach.</summary>
        Reach      = 6,

        /// <summary>Can be hurled at a target within the listed range. Uses the weapon's damage die (not an improvised throw).</summary>
        Thrown     = 7,

        /// <summary>Requires two hands to attack. Cannot be used with a shield in the other hand.</summary>
        TwoHanded  = 8,

        /// <summary>Can be used one-handed (listed damage die) or two-handed (alternate die in parentheses).</summary>
        Versatile  = 9
    }
}
