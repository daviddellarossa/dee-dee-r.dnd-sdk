# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

#### Phase 1 — Core Assembly (pure C#, zero Unity dependency)

- New assembly `dee-dee-r.dnd-sdk.core` (`noEngineReferences: true`) under `Core/`
- **Enums** (`DeeDeeR.DnD.Core.Enums`):
  - `AbilityType` — six ability scores
  - `SkillType` — 18 skills (grouped by governing ability)
  - `SkillTypeExtensions` — `GetAbility(this SkillType)` and `GetSkills(this AbilityType)` rules-constant mappings; per-ability lists are static readonly (zero per-call allocation)
  - `DieType` — D4, D6, D8, D10, D12, D20, D100 (enum values equal face count)
  - `DamageType` — 13 damage types (Acid … Thunder)
  - `Condition` — 14 conditions per D&D 2024 PHB (Exhaustion handled separately via `ExhaustionLevel`)
  - `ActionType` — Action, BonusAction, Reaction, FreeObjectInteraction
  - `WeaponCategory` — Simple, Martial (used for proficiency checks)
  - `WeaponProperty` — 10 intrinsic weapon properties (Ammunition, Finesse … Versatile)
  - `WeaponMastery` — 8 mastery properties introduced in D&D 2024 (Cleave, Graze, Nick, Push, Sap, Slow, Topple, Vex) + None
  - `ArmorCategory` — Light, Medium, Heavy (Shield excluded — see ShieldSO, Phase 4)
  - `SpellSchool` — 8 schools of magic
  - `SpellComponent` — flags enum: Verbal, Somatic, Material
  - `CasterType` — None, Third, Half, Full (for multiclass combined spell slot table)
  - `FeatCategory` — Origin, General, FightingStyle, EpicBoon (D&D 2024)
  - `RestType` — Short, Long
  - `CreatureSize` — Tiny through Gargantuan
  - `Alignment` — 9 alignments + Unaligned
  - `CurrencyType` — Copper, Silver, Electrum, Gold, Platinum (values = worth in copper)
  - `AdvantageState` — Normal, Advantage, Disadvantage
  - `LanguageType` — 9 standard languages + 10 rare languages per D&D 2024 PHB
- **Value types** (`DeeDeeR.DnD.Core.Values`) — all `readonly struct`:
  - `DiceExpression` — represents dice expressions (e.g. "2d6+3"); `Flat()` factory; `Average` property
  - `AbilityScoreSet` — immutable set of six ability scores with modifier calculation; functional `With()` updater
  - `RollResult` — roll total with critical success/fail flags
  - `AttackRollResult` — wraps `RollResult`; auto-resolves hit on critical
  - `Currency` — five denominations; `TotalInCopper`; arithmetic operators
  - `HitPointState` — current/maximum/temporary HP; `WithDamage()` burns temp HP first; `WithTempHP()` does not stack (D&D 2024)
  - `DeathSaveState` — successes/failures counters; `IsStabilized`/`IsDead` flags
  - `ExhaustionLevel` — value 0–6; `D20Penalty = Value * 2` per D&D 2024 rules; comparison operators
- **Interfaces** (`DeeDeeR.DnD.Core.Interfaces`):
  - `ILocalizable` — `LocalizationKey` and `LocalizationDescriptionKey` (String Table entry keys; no Unity dep)
  - `IRollProvider` — `RollDie(int faces)`; injectable for deterministic testing
  - `IDamageable`, `IHealable`, `IConditionTarget`, `ISpellCaster`

### Changed

- `ArmorCategory` no longer contains `Shield` — shields occupy a separate equipment slot (`ShieldSO`, Phase 4) and proficiency is tracked as a dedicated `bool HasShieldProficiency` on `CharacterRecord`

### Removed

### Technical Improvements

- Established three-assembly architecture: `core` (pure C#) / `runtime` (Unity + localization) / `editor`
- Enum-based approach confirmed for PHB-defined rules types; enum + companion SO pattern adopted for localization
