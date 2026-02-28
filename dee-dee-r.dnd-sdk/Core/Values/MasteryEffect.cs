namespace DeeDeeR.DnD.Core.Values
{
    /// <summary>
    /// An immutable descriptor of the mechanical effect triggered by a weapon mastery property.
    /// Returned by <c>WeaponMasterySystem.GetMasteryEffect</c> so that callers can apply
    /// mastery consequences without hard-coding mastery logic internally.
    /// </summary>
    public readonly struct MasteryEffect
    {
        /// <summary>
        /// Cleave: after hitting a creature, the character may make a free melee attack against
        /// a different adjacent creature (the extra attack adds no ability modifier to damage
        /// unless the modifier is negative).
        /// </summary>
        public readonly bool GrantsFreeAttack;

        /// <summary>
        /// Graze: even on a miss, the attack deals damage equal to the attack ability modifier
        /// (minimum 0) to the target.
        /// </summary>
        public readonly bool DealsDamageOnMiss;

        /// <summary>
        /// Nick: when using the Light property to make an off-hand attack, that attack does
        /// not consume the character's Bonus Action.
        /// </summary>
        public readonly bool OffHandAttackIsFree;

        /// <summary>Push: on a hit, the target is pushed <see cref="PushDistance"/> feet away (no save).</summary>
        public readonly bool PushesTarget;

        /// <summary>Distance in feet the target is pushed (Push mastery). Zero for all other masteries.</summary>
        public readonly int PushDistance;

        /// <summary>Sap: on a hit, the target has Disadvantage on its next attack roll before the start of your next turn.</summary>
        public readonly bool SapsTarget;

        /// <summary>Slow: on a hit, the target's speed is reduced by <see cref="SpeedReduction"/> feet until the start of your next turn.</summary>
        public readonly bool SlowsTarget;

        /// <summary>Speed reduction in feet (Slow mastery). Zero for all other masteries.</summary>
        public readonly int SpeedReduction;

        /// <summary>Topple: on a hit, the target must succeed on a CON saving throw or fall Prone.</summary>
        public readonly bool TopplesToTarget;

        /// <summary>
        /// Vex: on a hit, the attacker has Advantage on their next attack roll against the same
        /// target before the end of their current turn.
        /// </summary>
        public readonly bool GrantsAttackerAdvantage;

        /// <summary>Creates a <see cref="MasteryEffect"/> descriptor with the given flags.</summary>
        public MasteryEffect(
            bool grantsFreeAttack        = false,
            bool dealsDamageOnMiss       = false,
            bool offHandAttackIsFree     = false,
            bool pushesTarget            = false,
            int  pushDistance            = 0,
            bool sapsTarget              = false,
            bool slowsTarget             = false,
            int  speedReduction          = 0,
            bool topplesToTarget         = false,
            bool grantsAttackerAdvantage = false)
        {
            GrantsFreeAttack        = grantsFreeAttack;
            DealsDamageOnMiss       = dealsDamageOnMiss;
            OffHandAttackIsFree     = offHandAttackIsFree;
            PushesTarget            = pushesTarget;
            PushDistance            = pushDistance;
            SapsTarget              = sapsTarget;
            SlowsTarget             = slowsTarget;
            SpeedReduction          = speedReduction;
            TopplesToTarget         = topplesToTarget;
            GrantsAttackerAdvantage = grantsAttackerAdvantage;
        }

        /// <summary>A descriptor with no effects set (used for <see cref="Core.Enums.WeaponMastery.None"/>).</summary>
        public static readonly MasteryEffect None = new MasteryEffect();
    }
}