namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Standard polyhedral dice. Enum values equal the number of faces,
    /// which allows DiceExpression.ToString() and DiceRoller to use (int)DieType directly.
    /// </summary>
    public enum DieType
    {
        D4   = 4,
        D6   = 6,
        D8   = 8,
        D10  = 10,
        D12  = 12,
        D20  = 20,
        D100 = 100
    }
}
