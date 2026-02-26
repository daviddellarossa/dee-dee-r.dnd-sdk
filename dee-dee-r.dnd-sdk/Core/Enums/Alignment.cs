namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Character alignment along the law/chaos and good/evil axes, as defined in D&D 2024 PHB.
    /// Unaligned is used for creatures that have no moral or ethical framework (most beasts).
    /// </summary>
    public enum Alignment
    {
        LawfulGood     = 0,
        NeutralGood    = 1,
        ChaoticGood    = 2,
        LawfulNeutral  = 3,
        TrueNeutral    = 4,
        ChaoticNeutral = 5,
        LawfulEvil     = 6,
        NeutralEvil    = 7,
        ChaoticEvil    = 8,
        Unaligned      = 9
    }
}
