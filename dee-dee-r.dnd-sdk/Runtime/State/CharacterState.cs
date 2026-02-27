using System;
using System.Collections.Generic;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;

namespace DeeDeeR.DnD.Runtime.State
{
    /// <summary>
    /// Mutable per-session state for a character — everything that changes during play.
    /// Systems (Phase 7–9) read from and write to this class by returning new value-type
    /// snapshots that callers store back onto the relevant field.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Identity and build data live in <see cref="CharacterRecord"/>.
    /// Carried items live in <see cref="InventoryState"/>.
    /// </para>
    /// <para>
    /// Action booleans (<see cref="ActionUsed"/>, <see cref="BonusActionUsed"/>,
    /// <see cref="ReactionUsed"/>) are per-turn flags reset by <c>CombatantComponent</c> at the
    /// start of each turn (Phase 8/11).
    /// </para>
    /// </remarks>
    [Serializable]
    public sealed class CharacterState
    {
        // ── Hit Points ────────────────────────────────────────────────────────

        /// <summary>Current, maximum, and temporary hit points.</summary>
        public HitPointState HitPoints;

        /// <summary>
        /// Death saving throw successes and failures. Only relevant when
        /// <see cref="HitPointState.IsUnconscious"/> is <c>true</c>.
        /// </summary>
        public DeathSaveState DeathSaves;

        // ── Conditions & Exhaustion ───────────────────────────────────────────

        /// <summary>Active conditions on this character (Blinded, Charmed, etc.).</summary>
        public HashSet<Condition> Conditions = new HashSet<Condition>();

        /// <summary>
        /// Current exhaustion level (0 = none, 6 = dead). Each level imposes −2 to all d20
        /// tests per D&amp;D 2024 rules. See <see cref="ExhaustionLevel.D20Penalty"/>.
        /// </summary>
        public ExhaustionLevel Exhaustion;

        // ── Spell Slots ───────────────────────────────────────────────────────

        /// <summary>
        /// Available spell slots at each level (1–9). For multiclass characters this reflects
        /// the combined slot table computed by <c>MulticlassSystem</c> (Phase 9).
        /// </summary>
        public SpellSlotState SpellSlots;

        /// <summary>
        /// The spell currently being concentrated on. <c>null</c> if the character is not
        /// concentrating on any spell. Managed by <c>SpellCasterComponent</c> (Phase 11).
        /// </summary>
        public SpellSO ConcentrationSpell;

        // ── Hit Dice ──────────────────────────────────────────────────────────

        /// <summary>
        /// Available hit dice per die type. Keys are die types (e.g. D10 for Fighter,
        /// D6 for Wizard); values are how many of that die remain unspent.
        /// Spent on short rests; half recovered on a long rest per D&amp;D 2024 rules.
        /// </summary>
        public Dictionary<DieType, int> HitDiceAvailable = new Dictionary<DieType, int>();

        // ── Temporary Effects ─────────────────────────────────────────────────

        /// <summary>
        /// Game-defined temporary effects currently active on this character (spell durations,
        /// condition timers, magic item effects, etc.). The SDK holds and prunes this list but
        /// never interprets the entries — all logic lives in the <see cref="ITemporaryEffect"/>
        /// implementations provided by the game.
        /// </summary>
        [NonSerialized]
        public List<ITemporaryEffect> TemporaryEffects = new List<ITemporaryEffect>();

        // ── Inspiration ───────────────────────────────────────────────────────

        /// <summary>
        /// Whether the character has Heroic Inspiration. Allows re-rolling one die before
        /// seeing the result. Consumed on use; awarded by the DM.
        /// </summary>
        public bool Inspiration;

        // ── Turn Economy (per-turn flags) ─────────────────────────────────────

        /// <summary>Whether the character's Action has been used this turn.</summary>
        public bool ActionUsed;

        /// <summary>Whether the character's Bonus Action has been used this turn.</summary>
        public bool BonusActionUsed;

        /// <summary>Whether the character's Reaction has been used this turn.</summary>
        public bool ReactionUsed;
    }
}