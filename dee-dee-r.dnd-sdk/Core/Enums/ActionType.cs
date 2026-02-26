namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// The action economy types available on a creature's turn (D&D 2024 PHB).
    /// </summary>
    public enum ActionType
    {
        /// <summary>The main action on a creature's turn (Attack, Cast a Spell, Dash, Dodge, etc.).</summary>
        Action              = 0,

        /// <summary>A secondary action on a creature's turn, available to certain class features and spells.</summary>
        BonusAction         = 1,

        /// <summary>An action taken in response to a trigger, outside of the creature's turn.</summary>
        Reaction            = 2,

        /// <summary>A brief interaction with one object (e.g. draw/sheath a weapon, open a door) that does not consume the Action.</summary>
        FreeObjectInteraction = 3
    }
}
