using System;
using UnityEngine;
using UnityEngine.Localization;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Runtime.Data
{
    /// <summary>
    /// Serializable wrapper for <see cref="Currency"/> for use in Unity ScriptableObjects.
    /// Unity cannot serialize <c>readonly struct</c> fields directly; this class stores the
    /// individual denomination values and constructs the immutable Core type on demand.
    /// </summary>
    [Serializable]
    public sealed class CurrencyData
    {
        /// <summary>Copper pieces.</summary>
        [Min(0)] public int CP;

        /// <summary>Silver pieces (1 SP = 10 CP).</summary>
        [Min(0)] public int SP;

        /// <summary>Electrum pieces (1 EP = 50 CP).</summary>
        [Min(0)] public int EP;

        /// <summary>Gold pieces (1 GP = 100 CP).</summary>
        [Min(0)] public int GP;

        /// <summary>Platinum pieces (1 PP = 1000 CP).</summary>
        [Min(0)] public int PP;

        /// <summary>Constructs an immutable <see cref="Currency"/> value from this data.</summary>
        public Currency ToValue() => new Currency(CP, SP, EP, GP, PP);
    }

    /// <summary>
    /// Serializable wrapper for <see cref="DiceExpression"/> for use in Unity ScriptableObjects.
    /// Unity cannot serialize <c>readonly struct</c> fields directly; this class stores the
    /// individual components and constructs the immutable Core type on demand.
    /// </summary>
    [Serializable]
    public sealed class DiceExpressionData
    {
        /// <summary>Number of dice to roll. Zero means flat modifier with no dice.</summary>
        [Min(0)] public int Count = 1;

        /// <summary>Type of die to roll.</summary>
        public DieType Die = DieType.D6;

        /// <summary>Flat bonus or penalty added after rolling. May be negative.</summary>
        public int Modifier;

        /// <summary>Constructs an immutable <see cref="DiceExpression"/> value from this data.</summary>
        public DiceExpression ToValue() => new DiceExpression(Count, Die, Modifier);
    }

    /// <summary>
    /// An ability score increase granted by a background or feat (e.g. +1 to Strength).
    /// D&amp;D 2024 backgrounds grant a total of +2 across one or two abilities.
    /// </summary>
    [Serializable]
    public struct AbilityScoreIncrease
    {
        /// <summary>The ability score to increase.</summary>
        public AbilityType Ability;

        /// <summary>The amount to increase the score by (typically 1 or 2).</summary>
        [Min(1)] public int Amount;
    }

    /// <summary>
    /// A class or subclass feature that becomes available at a specific character level.
    /// Used in <see cref="ClassSO.Features"/> and <see cref="SubclassSO.Features"/>.
    /// </summary>
    [Serializable]
    public struct ClassFeatureEntry
    {
        /// <summary>The class level at which this feature is gained.</summary>
        [Range(1, 20)] public int Level;

        /// <summary>The name of the feature (e.g. "Extra Attack").</summary>
        public LocalizedString Name;

        /// <summary>Rules text describing what this feature does.</summary>
        public LocalizedString Description;
    }

    /// <summary>
    /// A starting equipment grant: a specific item and the quantity given.
    /// Used in <see cref="BackgroundSO.StartingEquipment"/> and <see cref="ClassSO.StartingEquipment"/>.
    /// </summary>
    [Serializable]
    public struct ItemGrant
    {
        /// <summary>The item being granted. May be any <see cref="ItemSO"/> subtype.</summary>
        public ItemSO Item;

        /// <summary>How many of the item are granted.</summary>
        [Min(1)] public int Quantity;
    }

    /// <summary>
    /// The pool of skills a character may choose from when creating a character of a specific class,
    /// and how many choices they may make.
    /// </summary>
    [Serializable]
    public struct SkillChoiceOptions
    {
        /// <summary>The full set of skills available to choose from.</summary>
        public SkillType[] Pool;

        /// <summary>How many skills the character may select from the pool.</summary>
        [Min(1)] public int Count;
    }

    /// <summary>
    /// A minimum ability score requirement for multiclassing into a class.
    /// </summary>
    [Serializable]
    public struct AbilityPrerequisite
    {
        /// <summary>The required ability.</summary>
        public AbilityType Ability;

        /// <summary>The minimum score in that ability (e.g. 13).</summary>
        [Min(1)] public int MinScore;
    }
}