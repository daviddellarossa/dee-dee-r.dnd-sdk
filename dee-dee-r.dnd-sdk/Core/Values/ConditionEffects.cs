namespace DeeDeeR.DnD.Core.Values
{
    /// <summary>
    /// An immutable descriptor of the mechanical effects imposed by a single condition.
    /// Returned by <c>ConditionSystem.GetConditionEffects</c> so that other systems can query
    /// condition consequences without hard-coding condition logic internally.
    /// </summary>
    /// <remarks>
    /// Some conditions have context-dependent effects that cannot be captured by simple flags.
    /// The following simplifications apply:
    /// <list type="bullet">
    ///   <item><description>
    ///     <b>Prone:</b> <see cref="AttackRollsAgainstHaveAdvantage"/> is <c>true</c> as a
    ///     simplification. Strictly speaking, only melee attacks gain advantage; ranged attacks
    ///     have disadvantage. The calling system is responsible for the range distinction.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Frightened:</b> <see cref="AttackRollsHaveDisadvantage"/> is <c>true</c>.
    ///     Strictly, this applies only while the source of fear is visible. The calling system
    ///     is responsible for visibility tracking.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Petrified:</b> Resistance to all damage and immunity to poison and disease are
    ///     not represented as flags (they require damage/effect type context). Those interactions
    ///     are handled by the damage and condition application logic in the calling system.
    ///   </description></item>
    /// </list>
    /// </remarks>
    public readonly struct ConditionEffects
    {
        /// <summary>The affected creature's speed is reduced to 0.</summary>
        public readonly bool SpeedReducedToZero;

        /// <summary>
        /// The affected creature is Incapacitated: it cannot take actions or reactions.
        /// </summary>
        public readonly bool Incapacitated;

        /// <summary>The affected creature cannot move (stronger than <see cref="SpeedReducedToZero"/>).</summary>
        public readonly bool CannotMove;

        /// <summary>The affected creature cannot speak (or can only speak falteringly).</summary>
        public readonly bool CannotSpeak;

        /// <summary>The affected creature's attack rolls have disadvantage.</summary>
        public readonly bool AttackRollsHaveDisadvantage;

        /// <summary>The affected creature's attack rolls have advantage (e.g. Invisible).</summary>
        public readonly bool AttackRollsHaveAdvantage;

        /// <summary>Attack rolls made against the affected creature have advantage.</summary>
        public readonly bool AttackRollsAgainstHaveAdvantage;

        /// <summary>Attack rolls made against the affected creature have disadvantage (e.g. Invisible).</summary>
        public readonly bool AttackRollsAgainstHaveDisadvantage;

        /// <summary>The affected creature automatically fails Strength saving throws.</summary>
        public readonly bool AutoFailStrengthSaves;

        /// <summary>The affected creature automatically fails Dexterity saving throws.</summary>
        public readonly bool AutoFailDexteritySaves;

        /// <summary>
        /// Melee hits against the affected creature within reach are critical hits.
        /// Applies to Paralyzed and Unconscious per D&amp;D 2024 PHB.
        /// </summary>
        public readonly bool MeleeHitsAreCritical;

        /// <summary>Creates a <see cref="ConditionEffects"/> descriptor with the given flags.</summary>
        public ConditionEffects(
            bool speedReducedToZero           = false,
            bool incapacitated                = false,
            bool cannotMove                   = false,
            bool cannotSpeak                  = false,
            bool attackRollsHaveDisadvantage  = false,
            bool attackRollsHaveAdvantage     = false,
            bool attackRollsAgainstHaveAdvantage    = false,
            bool attackRollsAgainstHaveDisadvantage = false,
            bool autoFailStrengthSaves        = false,
            bool autoFailDexteritySaves       = false,
            bool meleeHitsAreCritical         = false)
        {
            SpeedReducedToZero                = speedReducedToZero;
            Incapacitated                     = incapacitated;
            CannotMove                        = cannotMove;
            CannotSpeak                       = cannotSpeak;
            AttackRollsHaveDisadvantage       = attackRollsHaveDisadvantage;
            AttackRollsHaveAdvantage          = attackRollsHaveAdvantage;
            AttackRollsAgainstHaveAdvantage   = attackRollsAgainstHaveAdvantage;
            AttackRollsAgainstHaveDisadvantage = attackRollsAgainstHaveDisadvantage;
            AutoFailStrengthSaves             = autoFailStrengthSaves;
            AutoFailDexteritySaves            = autoFailDexteritySaves;
            MeleeHitsAreCritical              = meleeHitsAreCritical;
        }

        /// <summary>A descriptor with no effects set (used for conditions with no combat-relevant flags).</summary>
        public static readonly ConditionEffects None = new ConditionEffects();
    }
}