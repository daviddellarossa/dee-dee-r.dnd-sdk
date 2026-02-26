namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Intrinsic weapon properties as defined in D&D 2024 PHB.
    /// Distinct from WeaponMastery properties, which are class-gated.
    /// </summary>
    public enum WeaponProperty
    {
        Ammunition = 0,
        Finesse    = 1,
        Heavy      = 2,
        Light      = 3,
        Loading    = 4,
        Range      = 5,
        Reach      = 6,
        Thrown     = 7,
        TwoHanded  = 8,
        Versatile  = 9
    }
}
