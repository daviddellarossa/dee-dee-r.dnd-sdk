using System;
using UnityEngine;
using DeeDeeR.DnD.Runtime.Data;

namespace DeeDeeR.DnD.Runtime.State
{
    /// <summary>
    /// Tracks one class entry for a character — either a single-classed character's only class,
    /// or one leg of a multiclass combination. A character has one <see cref="ClassLevel"/> per
    /// distinct class they have taken levels in.
    /// </summary>
    [Serializable]
    public sealed class ClassLevel
    {
        /// <summary>The class taken at this entry (e.g. Fighter, Wizard).</summary>
        public ClassSO Class;

        /// <summary>
        /// The subclass chosen for this class (e.g. Battle Master, Evocation).
        /// <c>null</c> until the character reaches the subclass-choice level defined in
        /// <see cref="ClassSO.SubclassLevel"/>.
        /// </summary>
        public SubclassSO Subclass;

        /// <summary>Number of levels taken in this class (1–20).</summary>
        [Range(1, 20)] public int Level;

        /// <summary>
        /// Number of hit dice of this class's hit die type spent during short rests in the
        /// current long-rest period. Reset to zero on a long rest. Used by
        /// <c>RestSystem.TakeShortRest</c> (Phase 9).
        /// </summary>
        [Min(0)] public int HitDiceSpent;
    }
}