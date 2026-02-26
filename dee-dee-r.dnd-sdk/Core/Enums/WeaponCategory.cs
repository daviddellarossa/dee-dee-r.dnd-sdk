namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Weapon proficiency categories as defined in D&D 2024 PHB.
    /// Characters gain proficiency with a category (Simple, Martial) rather than
    /// with individual weapons. Used by ProficiencySystem to determine weapon proficiency.
    /// </summary>
    public enum WeaponCategory
    {
        Simple  = 0,
        Martial = 1
    }
}
