namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// Languages defined in D&D 2024 PHB.
    /// Standard languages are widely spoken; rare languages are ancient, secret, or planar.
    /// Characters know languages granted by their Species, Background, or class features.
    /// </summary>
    public enum LanguageType
    {
        // Standard languages
        Common              = 0,
        CommonSignLanguage  = 1,
        Dwarvish            = 2,
        Elvish              = 3,
        Giant               = 4,
        Gnomish             = 5,
        Goblin              = 6,
        Halfling            = 7,
        Orc                 = 8,

        // Rare languages
        Abyssal             = 9,
        Celestial           = 10,
        DeepSpeech          = 11,
        Draconic            = 12,
        Druidic             = 13,   // Secret language; Druids only
        Infernal            = 14,
        Primordial          = 15,   // Encompasses elemental dialects (Aquan, Auran, Ignan, Terran)
        Sylvan              = 16,
        ThievesCant         = 17,   // Secret language; Rogues only
        Undercommon         = 18
    }
}
