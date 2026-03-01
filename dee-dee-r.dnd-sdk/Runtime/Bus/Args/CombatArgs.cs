using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Runtime.Bus.Args
{
    /// <summary>Published when an attack roll is fully resolved (hit or miss).</summary>
    public readonly struct AttackMadeArgs
    {
        public readonly EndpointId      Attacker;
        public readonly EndpointId      Target;
        public readonly AttackRollResult Roll;
        public readonly WeaponSO        Weapon;

        public AttackMadeArgs(EndpointId attacker, EndpointId target, AttackRollResult roll, WeaponSO weapon)
        {
            Attacker = attacker;
            Target   = target;
            Roll     = roll;
            Weapon   = weapon;
        }
    }

    /// <summary>Published when raw damage is applied to a target (before HP mutation).</summary>
    public readonly struct DamageDealtArgs
    {
        public readonly EndpointId Attacker;
        public readonly EndpointId Target;
        public readonly int        Amount;
        public readonly DamageType Type;

        public DamageDealtArgs(EndpointId attacker, EndpointId target, int amount, DamageType type)
        {
            Attacker = attacker;
            Target   = target;
            Amount   = amount;
            Type     = type;
        }
    }

    /// <summary>Published whenever a character's current hit point total changes.</summary>
    public readonly struct HpChangedArgs
    {
        public readonly EndpointId Character;
        public readonly int        Previous;
        public readonly int        Current;
        public readonly int        Maximum;

        public HpChangedArgs(EndpointId character, int previous, int current, int maximum)
        {
            Character = character;
            Previous  = previous;
            Current   = current;
            Maximum   = maximum;
        }
    }

    /// <summary>Published when a character dies (HP reduced to 0 with no recovery path).</summary>
    public readonly struct CharacterDiedArgs
    {
        public readonly EndpointId Character;
        /// <summary><c>default</c> when the cause of death is environmental (no attacker endpoint).</summary>
        public readonly EndpointId Killer;

        public CharacterDiedArgs(EndpointId character, EndpointId killer = default)
        {
            Character = character;
            Killer    = killer;
        }
    }

    /// <summary>Published at the start or end of a character's turn in initiative order.</summary>
    public readonly struct TurnArgs
    {
        public readonly EndpointId Character;

        public TurnArgs(EndpointId character) => Character = character;
    }

    /// <summary>Published when an attack roll results in a natural 20.</summary>
    public readonly struct CritHitArgs
    {
        public readonly EndpointId Attacker;
        public readonly EndpointId Target;

        public CritHitArgs(EndpointId attacker, EndpointId target)
        {
            Attacker = attacker;
            Target   = target;
        }
    }

    /// <summary>Published each time a dying character makes a death saving throw.</summary>
    public readonly struct DeathSaveArgs
    {
        public readonly EndpointId    Character;
        public readonly bool          Success;
        /// <summary>
        /// <c>true</c> when the roll was a natural 1 (counts as two failures) or a natural 20
        /// (immediately stabilises the character).
        /// </summary>
        public readonly bool          IsCritical;
        public readonly DeathSaveState State;

        public DeathSaveArgs(EndpointId character, bool success, bool isCritical, DeathSaveState state)
        {
            Character  = character;
            Success    = success;
            IsCritical = isCritical;
            State      = state;
        }
    }

    /// <summary>Query args carrying the weapon for which the attack bonus is requested.</summary>
    public readonly struct GetAttackBonusArgs
    {
        public readonly WeaponSO Weapon;

        public GetAttackBonusArgs(WeaponSO weapon) => Weapon = weapon;
    }
}