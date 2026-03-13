using System;

namespace DeeDeeR.DnD.Game.SaveSystem
{
    /// <summary>
    /// Flat, JSON-safe DTO that captures everything needed to reconstruct a character.
    /// SO references are stored as Unity asset names; enums are stored as strings.
    /// Reconstructed via <c>CharacterFactory.Build()</c> for the record portion, then
    /// mutable state fields are restored directly.
    /// </summary>
    [Serializable]
    public sealed class CharacterSaveData
    {
        // ── Record params (passed to CharacterFactory.Build) ──────────────────

        public string CharacterName;
        public string PlayerName;
        public string AlignmentName;

        public string SpeciesName;
        public string SubspeciesName;   // empty string if none
        public string BackgroundName;

        /// <summary>Primary class name.</summary>
        public string ClassName;
        /// <summary>Primary class subclass name (empty if not yet chosen).</summary>
        public string SubclassName;
        public int ClassLevel;
        public int HitDiceSpent;

        /// <summary>
        /// Base ability scores BEFORE background ASIs [STR, DEX, CON, INT, WIS, CHA].
        /// These are the raw values passed to CharacterFactory.Build(); the factory re-applies
        /// background ASIs during reconstruction.
        /// </summary>
        public int[] BaseAbilityScores;

        public string[] ChosenSkillProficiencies;

        /// <summary>[traits, ideals, bonds, flaws]</summary>
        public string[] PersonalityText;

        // ── Mutable state ─────────────────────────────────────────────────────

        public int CurrentHp;
        public int MaxHp;
        public int TempHp;

        public int DeathSuccesses;
        public int DeathFailures;

        public string[] ActiveConditions;
        public int ExhaustionValue;

        public bool Inspiration;

        /// <summary>Available spell slots at levels 1–9 (index 0 = level 1).</summary>
        public int[] SpellSlots;

        public string[] HitDiceTypes;
        public int[]    HitDiceCounts;

        public string[] WeaponMasteries;

        // ── Inventory ─────────────────────────────────────────────────────────

        public string EquippedMainHand;
        public string EquippedOffHandWeapon;
        public string EquippedOffHandShield;
        public string EquippedArmor;

        public string[] InventoryItemNames;
        public int[]    InventoryItemQty;

        public int CurrencyCP;
        public int CurrencySP;
        public int CurrencyEP;
        public int CurrencyGP;
        public int CurrencyPP;

        // ── Spellbook ─────────────────────────────────────────────────────────

        public string[] KnownSpells;
        public string[] PreparedSpells;

        // ── Meta ──────────────────────────────────────────────────────────────

        /// <summary>The additive game scene name to load when resuming this save.</summary>
        public string SceneName;

        /// <summary>ISO 8601 timestamp of when the save was written.</summary>
        public string SaveDate;
    }
}
