namespace DeeDeeR.DnD.Core.Enums
{
    /// <summary>
    /// The targeting range category of a spell, as defined in D&amp;D 2024 PHB.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the value is <see cref="Ranged"/>, <c>SpellSO.RangeDistance</c> holds the distance
    /// in feet (e.g. 60, 120).
    /// </para>
    /// <para>
    /// When the value is <see cref="Self"/> and the spell emanates an area (e.g. Burning Hands:
    /// "Self (15-foot cone)"), populate <c>SpellSO.SelfAreaDescription</c> with the parenthetical
    /// text (e.g. "15-foot cone").
    /// </para>
    /// </remarks>
    public enum SpellRangeType
    {
        /// <summary>
        /// Targets the caster. May also emanate an area — see <c>SpellSO.SelfAreaDescription</c>.
        /// </summary>
        Self      = 0,

        /// <summary>The caster must touch the target. Effective range is 5 feet.</summary>
        Touch     = 1,

        /// <summary>
        /// A specific distance in feet. See <c>SpellSO.RangeDistance</c> for the value.
        /// </summary>
        Ranged    = 2,

        /// <summary>Targets any creature the caster can see, regardless of distance.</summary>
        Sight     = 3,

        /// <summary>Can target any creature on the same plane of existence.</summary>
        Unlimited = 4,

        /// <summary>
        /// An unusual range not covered by the other values.
        /// See the spell description for details.
        /// </summary>
        Special   = 5
    }
}