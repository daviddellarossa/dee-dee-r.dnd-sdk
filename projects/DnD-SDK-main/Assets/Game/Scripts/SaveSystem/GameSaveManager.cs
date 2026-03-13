using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DeeDeeR.DnD.Game;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.State;
using DeeDeeR.DnD.Runtime.Systems;

namespace DeeDeeR.DnD.Game.SaveSystem
{
    /// <summary>
    /// Handles reading and writing save files using Unity's <c>JsonUtility</c>.
    /// Save files are written to <c>Application.persistentDataPath/saves/save_{slot}.json</c>.
    /// A lightweight index file (<c>saves/index.json</c>) is kept for the load-game screen.
    /// Assign a <see cref="GameDataRegistry"/> asset in the Inspector to resolve SO names on load.
    /// </summary>
    public sealed class GameSaveManager : MonoBehaviour
    {
        [SerializeField] private GameDataRegistry _registry;

        private string SaveDir  => Path.Combine(Application.persistentDataPath, "saves");
        private string IndexPath => Path.Combine(SaveDir, "index.json");

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Serialises the active character to the given slot and updates the index.
        /// </summary>
        public void Save(int slot, GameManager gm)
        {
            EnsureDirectory();

            var data = BuildSaveData(slot, gm);
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SlotPath(slot), json);

            UpdateIndex(slot, data);
            Debug.Log($"[GameSaveManager] Saved slot {slot} → {SlotPath(slot)}");
        }

        /// <summary>Reads and deserialises a save file for the given slot.</summary>
        public CharacterSaveData Load(int slot)
        {
            string path = SlotPath(slot);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[GameSaveManager] No save file at slot {slot} ({path}).");
                return null;
            }

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<CharacterSaveData>(json);
        }

        /// <summary>Returns metadata for all occupied slots, sorted by slot index.</summary>
        public List<SaveSlotInfo> GetAllSlots()
        {
            string path = IndexPath;
            if (!File.Exists(path))
                return new List<SaveSlotInfo>();

            string json = File.ReadAllText(path);
            var wrapper = JsonUtility.FromJson<SlotIndexWrapper>(json);
            return wrapper?.Slots ?? new List<SaveSlotInfo>();
        }

        /// <summary>Deletes a save slot file and removes it from the index.</summary>
        public void DeleteSlot(int slot)
        {
            string path = SlotPath(slot);
            if (File.Exists(path))
                File.Delete(path);

            var slots = GetAllSlots();
            slots.RemoveAll(s => s.SlotIndex == slot);
            WriteIndex(slots);
        }

        // ── Reconstruction (used by GameManager.LoadGame) ─────────────────────

        /// <summary>
        /// Reconstructs a <see cref="CharacterRecord"/> and <see cref="CharacterState"/> pair
        /// from the supplied save data via <see cref="CharacterFactory"/>.
        /// Also restores <see cref="InventoryState"/> and <see cref="SpellbookState"/>.
        /// </summary>
        public (CharacterRecord record, CharacterState state, InventoryState inventory, SpellbookState spellbook)
            Reconstruct(CharacterSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            // ── Resolve SO references ──────────────────────────────────────────
            var species    = _registry.FindSpecies(data.SpeciesName);
            var subspecies = string.IsNullOrEmpty(data.SubspeciesName)
                ? null
                : _registry.FindSubspecies(data.SubspeciesName);
            var background = _registry.FindBackground(data.BackgroundName);
            var cls        = _registry.FindClass(data.ClassName);
            var subclass   = string.IsNullOrEmpty(data.SubclassName)
                ? null
                : _registry.FindSubclass(data.SubclassName);

            if (species == null)
                Debug.LogWarning($"[GameSaveManager] Species '{data.SpeciesName}' not found in registry.");
            if (background == null)
                Debug.LogWarning($"[GameSaveManager] Background '{data.BackgroundName}' not found in registry.");
            if (cls == null)
                Debug.LogWarning($"[GameSaveManager] Class '{data.ClassName}' not found in registry.");

            // ── Base ability scores ───────────────────────────────────────────
            var scores = data.BaseAbilityScores != null && data.BaseAbilityScores.Length == 6
                ? new AbilityScoreSet(
                    data.BaseAbilityScores[0], data.BaseAbilityScores[1],
                    data.BaseAbilityScores[2], data.BaseAbilityScores[3],
                    data.BaseAbilityScores[4], data.BaseAbilityScores[5])
                : new AbilityScoreSet(10, 10, 10, 10, 10, 10);

            // ── Class levels ──────────────────────────────────────────────────
            var classLevel = new ClassLevel
            {
                Class      = cls,
                Subclass   = subclass,
                Level      = data.ClassLevel,
                HitDiceSpent = data.HitDiceSpent
            };

            // ── Skill proficiencies ───────────────────────────────────────────
            var skills = new List<SkillType>();
            if (data.ChosenSkillProficiencies != null)
            {
                foreach (var s in data.ChosenSkillProficiencies)
                {
                    if (Enum.TryParse<SkillType>(s, out var skill))
                        skills.Add(skill);
                }
            }

            // ── Build record + state via factory ──────────────────────────────
            var factory = new CharacterFactory();
            var (record, state) = factory.Build(
                data.CharacterName,
                data.PlayerName,
                species,
                subspecies,
                background,
                scores,
                new[] { classLevel },
                skills);

            // ── Apply alignment ───────────────────────────────────────────────
            if (Enum.TryParse<Alignment>(data.AlignmentName, out var alignment))
                record.Alignment = alignment;

            // ── Apply personality text ─────────────────────────────────────────
            if (data.PersonalityText != null && data.PersonalityText.Length == 4)
            {
                record.PersonalityTraits = data.PersonalityText[0];
                record.Ideals            = data.PersonalityText[1];
                record.Bonds             = data.PersonalityText[2];
                record.Flaws             = data.PersonalityText[3];
            }

            // ── Restore mutable state ─────────────────────────────────────────
            state.HitPoints  = new HitPointState(data.CurrentHp, data.MaxHp, data.TempHp);
            state.DeathSaves = new DeathSaveState(data.DeathSuccesses, data.DeathFailures);
            state.Exhaustion = new ExhaustionLevel(data.ExhaustionValue);
            state.Inspiration = data.Inspiration;

            // Conditions
            state.Conditions.Clear();
            if (data.ActiveConditions != null)
            {
                foreach (var c in data.ActiveConditions)
                {
                    if (Enum.TryParse<Condition>(c, out var condition))
                        state.Conditions.Add(condition);
                }
            }

            // Spell slots
            if (data.SpellSlots != null && data.SpellSlots.Length == 9)
            {
                state.SpellSlots = new SpellSlotState(
                    data.SpellSlots[0], data.SpellSlots[1], data.SpellSlots[2],
                    data.SpellSlots[3], data.SpellSlots[4], data.SpellSlots[5],
                    data.SpellSlots[6], data.SpellSlots[7], data.SpellSlots[8]);
            }

            // Hit dice
            state.HitDiceAvailable.Clear();
            if (data.HitDiceTypes != null && data.HitDiceCounts != null)
            {
                int len = Math.Min(data.HitDiceTypes.Length, data.HitDiceCounts.Length);
                for (int i = 0; i < len; i++)
                {
                    if (Enum.TryParse<DieType>(data.HitDiceTypes[i], out var die))
                        state.HitDiceAvailable[die] = data.HitDiceCounts[i];
                }
            }

            // Weapon masteries
            state.WeaponMasteries.Clear();
            if (data.WeaponMasteries != null)
            {
                foreach (var m in data.WeaponMasteries)
                {
                    if (Enum.TryParse<WeaponMastery>(m, out var mastery))
                        state.WeaponMasteries.Add(mastery);
                }
            }

            // ── Inventory ─────────────────────────────────────────────────────
            var inventory = new InventoryState
            {
                EquippedMainHand      = data.EquippedMainHand      != null ? _registry.FindWeapon(data.EquippedMainHand)      : null,
                EquippedOffHandWeapon = data.EquippedOffHandWeapon != null ? _registry.FindWeapon(data.EquippedOffHandWeapon) : null,
                EquippedOffHandShield = data.EquippedOffHandShield != null ? _registry.FindShield(data.EquippedOffHandShield) : null,
                EquippedArmor         = data.EquippedArmor          != null ? _registry.FindArmor(data.EquippedArmor)           : null,
                Currency              = new Currency(data.CurrencyCP, data.CurrencySP, data.CurrencyEP, data.CurrencyGP, data.CurrencyPP)
            };

            if (data.InventoryItemNames != null && data.InventoryItemQty != null)
            {
                int len = Math.Min(data.InventoryItemNames.Length, data.InventoryItemQty.Length);
                for (int i = 0; i < len; i++)
                {
                    // Items may be weapons, armor, gear, etc. — weapons are the only type in the registry.
                    // A production system would have a unified item registry; this covers the basics.
                    var weapon = _registry.FindWeapon(data.InventoryItemNames[i]);
                    if (weapon != null)
                    {
                        inventory.Items.Add(new OwnedItem { Item = weapon, Quantity = data.InventoryItemQty[i] });
                        continue;
                    }
                    var armor = _registry.FindArmor(data.InventoryItemNames[i]);
                    if (armor != null)
                    {
                        inventory.Items.Add(new OwnedItem { Item = armor, Quantity = data.InventoryItemQty[i] });
                    }
                }
            }

            // ── Spellbook ─────────────────────────────────────────────────────
            var spellbook = new SpellbookState();
            if (data.KnownSpells != null)
                foreach (var s in data.KnownSpells)
                {
                    var spell = _registry.FindSpell(s);
                    if (spell != null) spellbook.KnownSpells.Add(spell);
                }
            if (data.PreparedSpells != null)
                foreach (var s in data.PreparedSpells)
                {
                    var spell = _registry.FindSpell(s);
                    if (spell != null) spellbook.PreparedSpells.Add(spell);
                }

            return (record, state, inventory, spellbook);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private CharacterSaveData BuildSaveData(int slot, GameManager gm)
        {
            var record    = gm.ActiveRecord;
            var state     = gm.ActiveState;
            var inventory = gm.ActiveInventory;
            var spellbook = gm.ActiveSpellbook;

            var primaryClass = record.ClassLevels.Count > 0 ? record.ClassLevels[0] : null;

            // Skill proficiencies (all, background ones included — CharacterFactory will re-add background skills)
            var skills = new List<string>();
            foreach (var s in record.SkillProficiencies)
                skills.Add(s.ToString());

            // Inventory items
            var itemNames = new List<string>();
            var itemQty   = new List<int>();
            foreach (var owned in inventory.Items)
            {
                if (owned.Item != null)
                {
                    itemNames.Add(owned.Item.name);
                    itemQty.Add(owned.Quantity);
                }
            }

            // Hit dice
            var hdTypes  = new List<string>();
            var hdCounts = new List<int>();
            foreach (var kv in state.HitDiceAvailable)
            {
                hdTypes.Add(kv.Key.ToString());
                hdCounts.Add(kv.Value);
            }

            // Conditions
            var conditions = new List<string>();
            foreach (var c in state.Conditions)
                conditions.Add(c.ToString());

            // Weapon masteries
            var masteries = new List<string>();
            foreach (var m in state.WeaponMasteries)
                masteries.Add(m.ToString());

            // Spell slots
            var slots = new int[9];
            for (int i = 0; i < 9; i++)
                slots[i] = state.SpellSlots.GetAvailable(i + 1);

            // Known/prepared spells
            var knownSpells    = new List<string>();
            var preparedSpells = new List<string>();
            foreach (var s in spellbook.KnownSpells)    if (s != null) knownSpells.Add(s.name);
            foreach (var s in spellbook.PreparedSpells) if (s != null) preparedSpells.Add(s.name);

            // Reverse-engineer base ability scores: subtract background ASIs from final scores.
            // This ensures CharacterFactory.Build() re-applies the ASIs correctly on load.
            var finalScores = record.AbilityScores;
            var baseStr = finalScores.GetScore(AbilityType.Strength);
            var baseDex = finalScores.GetScore(AbilityType.Dexterity);
            var baseCon = finalScores.GetScore(AbilityType.Constitution);
            var baseInt = finalScores.GetScore(AbilityType.Intelligence);
            var baseWis = finalScores.GetScore(AbilityType.Wisdom);
            var baseCha = finalScores.GetScore(AbilityType.Charisma);

            if (record.Background != null && record.Background.AbilityScoreIncreases != null)
            {
                foreach (var asi in record.Background.AbilityScoreIncreases)
                {
                    switch (asi.Ability)
                    {
                        case AbilityType.Strength:     baseStr -= asi.Amount; break;
                        case AbilityType.Dexterity:    baseDex -= asi.Amount; break;
                        case AbilityType.Constitution: baseCon -= asi.Amount; break;
                        case AbilityType.Intelligence: baseInt -= asi.Amount; break;
                        case AbilityType.Wisdom:       baseWis -= asi.Amount; break;
                        case AbilityType.Charisma:     baseCha -= asi.Amount; break;
                    }
                }
            }

            return new CharacterSaveData
            {
                CharacterName = record.Name,
                PlayerName    = record.PlayerName,
                AlignmentName = record.Alignment.ToString(),
                SpeciesName   = record.Species   != null ? record.Species.name   : "",
                SubspeciesName = record.Subspecies != null ? record.Subspecies.name : "",
                BackgroundName = record.Background != null ? record.Background.name : "",
                ClassName      = primaryClass?.Class    != null ? primaryClass.Class.name    : "",
                SubclassName   = primaryClass?.Subclass != null ? primaryClass.Subclass.name : "",
                ClassLevel     = primaryClass?.Level ?? 1,
                HitDiceSpent   = primaryClass?.HitDiceSpent ?? 0,
                BaseAbilityScores = new[] { baseStr, baseDex, baseCon, baseInt, baseWis, baseCha },
                ChosenSkillProficiencies = skills.ToArray(),
                PersonalityText = new[]
                {
                    record.PersonalityTraits ?? "",
                    record.Ideals            ?? "",
                    record.Bonds             ?? "",
                    record.Flaws             ?? ""
                },
                CurrentHp      = state.HitPoints.Current,
                MaxHp          = state.HitPoints.Maximum,
                TempHp         = state.HitPoints.Temporary,
                DeathSuccesses = state.DeathSaves.Successes,
                DeathFailures  = state.DeathSaves.Failures,
                ActiveConditions = conditions.ToArray(),
                ExhaustionValue  = state.Exhaustion.Value,
                Inspiration      = state.Inspiration,
                SpellSlots       = slots,
                HitDiceTypes     = hdTypes.ToArray(),
                HitDiceCounts    = hdCounts.ToArray(),
                WeaponMasteries  = masteries.ToArray(),
                EquippedMainHand      = inventory.EquippedMainHand      != null ? inventory.EquippedMainHand.name      : "",
                EquippedOffHandWeapon = inventory.EquippedOffHandWeapon != null ? inventory.EquippedOffHandWeapon.name : "",
                EquippedOffHandShield = inventory.EquippedOffHandShield != null ? inventory.EquippedOffHandShield.name : "",
                EquippedArmor         = inventory.EquippedArmor          != null ? inventory.EquippedArmor.name          : "",
                InventoryItemNames    = itemNames.ToArray(),
                InventoryItemQty      = itemQty.ToArray(),
                CurrencyCP = inventory.Currency.CP,
                CurrencySP = inventory.Currency.SP,
                CurrencyEP = inventory.Currency.EP,
                CurrencyGP = inventory.Currency.GP,
                CurrencyPP = inventory.Currency.PP,
                KnownSpells    = knownSpells.ToArray(),
                PreparedSpells = preparedSpells.ToArray(),
                SceneName = "Game",
                SaveDate  = DateTime.UtcNow.ToString("o")
            };
        }

        private void UpdateIndex(int slot, CharacterSaveData data)
        {
            var slots = GetAllSlots();
            var existing = slots.Find(s => s.SlotIndex == slot);
            if (existing != null)
            {
                existing.CharacterName = data.CharacterName;
                existing.ClassName     = data.ClassName;
                existing.SaveDate      = data.SaveDate;
                existing.Level         = data.ClassLevel;
            }
            else
            {
                slots.Add(new SaveSlotInfo
                {
                    SlotIndex     = slot,
                    CharacterName = data.CharacterName,
                    ClassName     = data.ClassName,
                    SaveDate      = data.SaveDate,
                    Level         = data.ClassLevel
                });
            }
            WriteIndex(slots);
        }

        private void WriteIndex(List<SaveSlotInfo> slots)
        {
            var wrapper = new SlotIndexWrapper { Slots = slots };
            File.WriteAllText(IndexPath, JsonUtility.ToJson(wrapper, prettyPrint: true));
        }

        private string SlotPath(int slot) => Path.Combine(SaveDir, $"save_{slot}.json");

        private void EnsureDirectory()
        {
            if (!Directory.Exists(SaveDir))
                Directory.CreateDirectory(SaveDir);
        }

        // ── Wrapper for JsonUtility (which cannot serialise bare List<T>) ─────

        [Serializable]
        private sealed class SlotIndexWrapper
        {
            public List<SaveSlotInfo> Slots = new List<SaveSlotInfo>();
        }
    }
}
