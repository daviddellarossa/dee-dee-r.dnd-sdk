namespace DeeDeeR.DnD.Core.Interfaces
{
    /// <summary>
    /// Marks a type as having localizable display text.
    /// Returns String Table entry keys for use with Unity's Localization package.
    /// Implemented by companion ScriptableObjects in Runtime.Data.Definitions.
    /// </summary>
    public interface ILocalizable
    {
        /// <summary>String Table entry key for the display name.</summary>
        string LocalizationKey { get; }

        /// <summary>String Table entry key for the display description.</summary>
        string LocalizationDescriptionKey { get; }
    }
}
