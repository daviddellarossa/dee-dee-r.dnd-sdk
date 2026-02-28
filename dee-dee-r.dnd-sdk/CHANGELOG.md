# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

#### Phase 6 — Character Creation Systems

- **`SavingThrowProficiencies`** added to `CharacterRecord` (`HashSet<AbilityType>`) — was missing from Phase 5; required by `CharacterFactory` to apply starting class saving throw profs.
- **`MulticlassSystem`** (`DeeDeeR.DnD.Runtime.Systems`) — sealed class, no mutable state:
  - `CalculateCombinedSpellSlots(IReadOnlyList<ClassLevel>)` — implements the D&D 2024 PHB multiclass spellcaster slot table. Full casters contribute their full level; Half casters contribute level ÷ 2 (floor); Third casters contribute level ÷ 3 (floor). The combined effective level indexes into a 20-row embedded table (rows 0–20, columns 0–9). Returns `SpellSlotState.Empty` when no class contributes to the table.
  - `ValidateMulticlassPrerequisites(CharacterRecord, ClassSO)` — checks every `AbilityPrerequisite` on `ClassSO.MulticlassPrerequisites` against the character's current ability scores; returns `false` on the first unmet requirement.
  - **Known limitation:** the combined slot formula is only fully accurate for true multiclass combinations. Single-class half/third casters (e.g. Paladin 5) yield fewer slots than their own class table. Per-class slot look-up is deferred to Phase 9 (`SpellSystem`).
- **`CharacterFactory`** (`DeeDeeR.DnD.Runtime.Systems`) — sealed class:
  - `Build(characterName, playerName, species, subspecies, background, baseScores, classLevels, chosenSkillProficiencies?)` → `(CharacterRecord, CharacterState)`. Key behaviours:
    - Ability scores: applies background ASIs on top of `baseScores` (D&D 2024 — species grants no ASIs).
    - Proficiencies: saving throws, armor, weapons, shield, tools from the primary class (index 0). Multiclass entries (index 1+) grant armor/weapon/shield profs only (no saving throws per PHB 2024).
    - Languages from `SpeciesSO.Languages` + `SubspeciesSO.AdditionalLanguages`.
    - Origin feat from `BackgroundSO.OriginFeat` added to `CharacterRecord.Feats`.
    - HP: level 1 of the primary class uses the maximum die result; all other levels use the fixed average (⌊faces ÷ 2⌋ + 1). Each level grants at least 1 HP regardless of CON modifier.
    - Hit dice: grouped by die type in `CharacterState.HitDiceAvailable`.
    - Spell slots: delegated to `MulticlassSystem.CalculateCombinedSpellSlots`.
- **`LevelUpSystem`** (`DeeDeeR.DnD.Runtime.Systems`) — sealed class:
  - `LevelUp(record, state, classToLevel, chosenSubclass?, chosenFeat?, chosenASIs?)` — mutates `CharacterRecord` and `CharacterState` in place. Key behaviours:
    - Finds an existing `ClassLevel` by class reference or creates a new one (multiclassing). New-class additions validate multiclass prerequisites first, throwing `InvalidOperationException` if not met.
    - Increments `ClassLevel.Level`.
    - Assigns `ClassLevel.Subclass` if the new level reaches `ClassSO.SubclassLevel` and no subclass has been chosen yet.
    - Adds one hit die of the class's hit die type to `HitDiceAvailable`.
    - HP gain: fixed average (⌊faces ÷ 2⌋ + 1) + CON modifier, minimum 1. Both `Current` and `Maximum` increase by this amount.
    - Applies ability score increases (each capped at 20).
    - Appends `chosenFeat` to `CharacterRecord.Feats`.
    - Recalculates `SpellSlotState` via `MulticlassSystem` after the level is applied.
    - Multiclass proficiency note: armor/weapon/shield profs applied for new class entries; saving throws are never granted when multiclassing per PHB 2024.
- **Tests** (`Tests/Editor/Systems/`):
  - `MulticlassSystemTests` — 13 cases covering full/half/third caster effective level calculation, combined table look-up, edge cases (null entries, clamped max), and `ValidateMulticlassPrerequisites` (null guards, met/unmet single/multiple prereqs).
  - `CharacterFactoryTests` — 22 cases covering argument validation, identity fields, background ASI application, proficiency assignment, language propagation, HP formula (level 1 max, multi-level average, negative CON clamping), hit dice grouping, and spell slot output.
  - `LevelUpSystemTests` — 20 cases covering argument validation, level increment, new-class addition, subclass assignment timing, HP gain (average, CON bonus, negative CON clamping), hit dice, ASI application (capped at 20), feat addition, spell slot recalculation, and multiclass prerequisite enforcement.
- **`Tests/Editor/Editor.asmdef`** updated: added `dee-dee-r.dnd-sdk.runtime` reference to allow Phase 6 tests to create `ScriptableObject` instances.

#### Design notes — Phase 6
- Systems mutate `CharacterRecord`/`CharacterState` via in-place modification (`LevelUpSystem`) or return a new pair (`CharacterFactory`). `LevelUpSystem` cannot return deep copies efficiently because the objects contain reference-type collections; this is consistent with how the surrounding system layer is expected to use them.
- `ClassSO` does not distinguish starting vs. multiclass proficiency subsets (no separate `MulticlassArmorProficiencies` field). `LevelUpSystem` applies all armor/weapon/shield profs as a simplification. If the PHB multiclass proficiency subsets need enforcement, a future field can be added to `ClassSO`.
- The multiclass spell slot limitation (single-class half/third casters) is documented on `MulticlassSystem` with a `<remarks>` XML comment.

#### Phase 5 — Runtime State Classes

- **`SpellSlotState`** (`DeeDeeR.DnD.Core.Values`) — immutable `readonly struct` tracking available spell slots at levels 1–9. Nine individual `int` fields (Slot1–Slot9), each clamped ≥ 0. Key API:
  - `GetAvailable(int level)` → int; `HasSlot(int level)` → bool
  - `WithExpended(int level)` → new state with one slot decremented (clamped to 0)
  - `WithRecovered(int level, int count)` → new state with slots added at that level
  - `Total` — sum across all levels; `Empty` — all-zero static instance
  - `ToString()` shows only non-zero levels (e.g. "1st×4 2nd×3 3rd×2")
  - `==` / `!=` / `Equals` / `GetHashCode` implemented
- **`ITemporaryEffect`** (`DeeDeeR.DnD.Core.Interfaces`) — marker interface for game-defined temporary effects stored in `CharacterState.TemporaryEffects`. The SDK holds and prunes the list; all logic is in game-provided implementations. Single member: `bool IsExpired { get; }`.
- **`ClassLevel`** (`DeeDeeR.DnD.Runtime.State`) — one entry per class in a (potentially multiclass) build: `ClassSO Class`, `SubclassSO Subclass` (null until chosen), `int Level` (1–20), `int HitDiceSpent` (reset on long rest).
- **`CharacterRecord`** (`DeeDeeR.DnD.Runtime.State`) — semi-static identity and build data:
  - Identity: `Name`, `PlayerName`, `Alignment`, personality text (plain strings — player-authored)
  - Origin: `SpeciesSO Species`, `SubspeciesSO Subspecies` (nullable), `BackgroundSO Background`
  - Build: `List<ClassLevel> ClassLevels`, `AbilityScoreSet AbilityScores` (post all-ASI)
  - Proficiencies: `HashSet<SkillType> SkillProficiencies`, `HashSet<SkillType> SkillExpertise`, `HashSet<WeaponCategory>`, `HashSet<ArmorCategory>`, `bool HasShieldProficiency`, `List<ToolSO>`
  - Other: `List<FeatSO> Feats`, `HashSet<LanguageType> Languages`
- **`CharacterState`** (`DeeDeeR.DnD.Runtime.State`) — mutable per-session state:
  - `HitPointState HitPoints`, `DeathSaveState DeathSaves`
  - `HashSet<Condition> Conditions`, `ExhaustionLevel Exhaustion`
  - `SpellSlotState SpellSlots`, `SpellSO ConcentrationSpell` (null when not concentrating)
  - `Dictionary<DieType, int> HitDiceAvailable` (per die type, for multiclass)
  - `List<ITemporaryEffect> TemporaryEffects` (`[NonSerialized]` — transient)
  - `bool Inspiration`; per-turn flags `ActionUsed`, `BonusActionUsed`, `ReactionUsed`
- **`InventoryState`** (`DeeDeeR.DnD.Runtime.State`) — carried items and equipped gear:
  - `List<OwnedItem> Items`, `Currency Currency`
  - `WeaponSO EquippedMainHand`, `WeaponSO EquippedOffHandWeapon`, `ShieldSO EquippedOffHandShield`, `ArmorSO EquippedArmor`
  - Off-hand is split into two nullable fields (weapon vs. shield) rather than a shared `ScriptableObject` reference, preserving type safety
  - `OwnedItem` — nested `[Serializable]` class: `ItemSO Item` + `int Quantity`
- **`SpellbookState`** (`DeeDeeR.DnD.Runtime.State`) — `List<SpellSO> KnownSpells` (spellbook/learning), `List<SpellSO> PreparedSpells` (daily selection or full known list for known-spell casters)

#### Design notes — Phase 5
- State classes are plain C# (`[Serializable]`) — not ScriptableObjects. `[SerializeField]` is not used; fields are public for direct system access.
- `AbilityScoreSet` and `SpellSlotState` (both `readonly struct`) are used directly as fields. Unity's built-in serializer cannot handle them; JSON.NET and custom save systems can.
- `HashSet<T>` used for proficiency sets (O(1) `Contains`, matches `ProficiencySystem` API). `List<T>` used for ordered/duplicable collections.
- `TemporaryEffects` is `[NonSerialized]` — these are transient runtime objects not intended to persist across sessions. They are rebuilt when effects are re-applied on load.
- `SkillExpertise` added to `CharacterRecord` (not explicit in plan but required by `ProficiencySystem.GetSkillBonus` which already accepts an `hasExpertise` parameter).

#### Phase 4 — ScriptableObject Definitions (continued, second pass)

- All player-visible text fields across content SOs replaced with `LocalizedString` (private `[SerializeField]`, public getter property). Affected types: `AmmunitionSO.Description`, `BackgroundSO.Description`, `BackgroundSO.FlavorText`, `FeatSO.Description`, `FeatSO.PrerequisiteDescription`, `ItemSO.Description` (inherited by `ToolSO` and `AdventuringGearSO`), `ToolSO.UsageRules`, `SpeciesSO.Description`, `SubspeciesSO.Description`, `SubclassSO.Description`, `TraitSO.Description`, `SpellSO.Description`, `SpellSO.HigherLevelDescription`, `SpellSO.ReactionTrigger`, `SpellSO.SelfAreaDescription`, `SpellSO.MaterialDescription`, `ClassFeatureEntry.Name`, `ClassFeatureEntry.Description`. The `[TextArea]` attribute is removed from all of these — the `LocalizedString` inspector widget replaces it.
- Design rationale: `LocalizedString` works identically in the editor (shows a String Table picker UI) and in-game. Using it everywhere ensures the SDK ships with first-class localization support and eliminates any disparity between content SOs and companion SOs.

#### Phase 4 — ScriptableObject Definitions (continued)

- **`CastingTimeType`** (`DeeDeeR.DnD.Core.Enums`) — replaces the plain `string CastingTime` field in `SpellSO`. Values: `Action`, `BonusAction`, `Reaction`, `OneMinute`, `TenMinutes`, `OneHour`, `EightHours`, `TwentyFourHours`, `Special`. The `Reaction` case requires populating `SpellSO.ReactionTrigger` with the trigger description.
- **`SpellRangeType`** (`DeeDeeR.DnD.Core.Enums`) — replaces `string RangeType`. Values: `Self`, `Touch`, `Ranged`, `Sight`, `Unlimited`, `Special`. `Ranged` uses the existing `RangeDistance` field. `Self` spells with an emanating area (e.g. "Self (15-foot cone)") populate the new `SelfAreaDescription` field.
- **`SpellDurationType`** (`DeeDeeR.DnD.Core.Enums`) — replaces `string Duration`. Values: `Instantaneous`, `OneRound`, `OneMinute`, `TenMinutes`, `OneHour`, `EightHours`, `TwentyFourHours`, `SevenDays`, `UntilDispelled`, `UntilDispelledOrTriggered`, `Special`. Concentration is tracked separately by `SpellSO.IsConcentration`.
- **`SpellSO`** updated: `CastingTime`, `RangeType`, `Duration` changed from `string` to the three new enums; `ReactionTrigger` (string) and `SelfAreaDescription` (string) added as companion fields for the two edge cases that enums alone cannot express.

#### Phase 4 — ScriptableObject Definitions

- **`CurrencyData`** and **`DiceExpressionData`** (`DeeDeeR.DnD.Runtime.Data`) — serializable wrapper classes for the Core `readonly struct` types. Unity cannot serialize `readonly struct` fields; these classes store individual components and expose the immutable Core value via `ToValue()`. Used by all content SOs that need currency costs or dice expressions.
- **Shared serializable types** (`DeeDeeR.DnD.Runtime.Data`) in `DataHelpers.cs`:
  - `AbilityScoreIncrease` — `AbilityType + int Amount`; used by `BackgroundSO` and `FeatSO`
  - `ClassFeatureEntry` — `int Level + string Name + string Description`; used by `ClassSO` and `SubclassSO`
  - `ItemGrant` — `ItemSO reference + int Quantity`; used by `BackgroundSO` and `ClassSO`
  - `SkillChoiceOptions` — `SkillType[] Pool + int Count`; used by `ClassSO`
  - `AbilityPrerequisite` — `AbilityType + int MinScore`; used by `ClassSO.MulticlassPrerequisites`
- **10 companion ScriptableObjects** (`DeeDeeR.DnD.Runtime.Data.Definitions`) — one per enum that needs UI display; each implements `ILocalizable` and exposes its enum type plus two `LocalizedString` fields:
  - `AbilityDefinitionSO` — `AbilityType`
  - `SkillDefinitionSO` — `SkillType`; derives `LinkedAbility` from `SkillTypeExtensions.GetAbility()` (not serialized)
  - `DieSO` — `DieType`; derives `NumOfFaces` from enum value
  - `DamageTypeDefinitionSO` — `DamageType`
  - `ConditionDefinitionSO` — `Condition`
  - `WeaponCategoryDefinitionSO` — `WeaponCategory`
  - `WeaponMasteryDefinitionSO` — `WeaponMastery`
  - `SpellSchoolDefinitionSO` — `SpellSchool`
  - `CurrencyDefinitionSO` — `CurrencyType`
  - `LanguageDefinitionSO` — `LanguageType`
- **15 content ScriptableObjects** (`DeeDeeR.DnD.Runtime.Data`) — pure data assets, no game logic:
  - `TraitSO` — reusable species/subspecies trait with description text
  - `ItemSO` — base item (description, weight, cost); subclassed by ToolSO and AdventuringGearSO
  - `ToolSO : ItemSO` — adds `AssociatedSkill (SkillType)` and usage rules text
  - `AdventuringGearSO : ItemSO` — semantic subtype tag (rope, torch, etc.)
  - `AmmunitionSO` — ammunition for ranged weapons; referenced by `WeaponSO.RequiredAmmo`
  - `ArmorSO` — Light/Medium/Heavy armour: `BaseArmorClass`, `MaxDexBonus` (−1 = no cap), `StrengthRequirement`, `StealthDisadvantage`
  - `ShieldSO` — shield slot (separate from ArmorCategory); `AcBonus` (default +2)
  - `WeaponSO` — weapon: category, damage dice/type, properties array, mastery property, normal/long range, optional ammo reference, optional versatile dice, weight, cost
  - `SpeciesSO` — species: size, movement speed, darkvision range, traits, languages, subspecies list. **No ASI** (D&amp;D 2024 rule — backgrounds grant ASI)
  - `SubspeciesSO` — parent species reference, additional traits and languages
  - `FeatSO` — category, description, prerequisite description (text), ability score increases list
  - `BackgroundSO` — ASI list (+2 total), origin feat, two skill proficiencies, tool proficiency, starting equipment grants, flavour text
  - `ClassSO` — hit die, primary abilities, saving throw proficiencies, armour/weapon/tool proficiencies, shield proficiency flag, skill choices, caster type, spellcasting ability, multiclass prerequisites, subclass level, feature list, starting equipment
  - `SubclassSO` — parent class, description, expanded spell list, granted weapon masteries, feature list
  - `SpellSO` — level (0–9), school, casting time (string), range type/distance, components (flags), material description, duration (string), concentration/ritual flags, description, higher-level description, class lists

#### Technical Improvements (Phase 4)

- Added `"Unity.Localization"` to `dee-dee-r.dnd-sdk.runtime.asmdef` references — companion SOs depend on `LocalizedString` from `com.unity.localization`
- Added `"com.unity.localization": "1.5.2"` to `DnD-SDK-main/Packages/manifest.json`
- Design rationale — companion SOs vs content SOs: companion SOs (`*DefinitionSO`) implement `ILocalizable` and are **display-only** — systems never use them. Content SOs hold all rule-relevant data that systems will read in Phase 5+. The split keeps Core free of Unity dependencies.
- Design rationale — `CurrencyData` / `DiceExpressionData`: `readonly struct` is the correct immutable value type for game logic, but Unity's serialization cannot handle `readonly` fields. Wrapper classes bridge the gap: the inspector edits the components; calling code reads the immutable Core value via `ToValue()` / `DamageExpression` etc.

#### Phase 3 — AbilitySystem + ProficiencySystem

- **`AbilitySystem`** (`DeeDeeR.DnD.Core.Systems`) — single entry point for all d20 rolls:
  - `RollCheck(int totalModifier, AdvantageState)` — the only roll method; caller computes the full modifier (see class xmldoc for per-check-type formulas). For advantage/disadvantage, two `1d20` rolls are made and the appropriate result is returned with accurate crit flags.
  - `static PassiveCheck(int totalModifier)` → `10 + modifier`, no roll
  - Design principle: `AbilitySystem` owns dice mechanics only. Modifier computation (ability mod, proficiency, exhaustion penalty) is always the caller's responsibility, ensuring a uniform API regardless of check type (raw ability check, skill check, or saving throw).
- **`ProficiencySystem`** (`DeeDeeR.DnD.Core.Systems`) — pure computations, no dependencies:
  - `GetProficiencyBonus(int totalLevel)` → PHB 2024 table (+2 at L1–4, +3 at L5–8 … +6 at L17–20); throws `ArgumentOutOfRangeException` outside [1, 20]
  - `GetSkillBonus(AbilityScoreSet, SkillType, bool isProficient, bool hasExpertise, int profBonus)` → ability modifier + proficiency (doubled for expertise); exhaustion not included
  - `HasSkillProficiency`, `HasArmorProficiency`, `HasWeaponCategoryProficiency` — `ISet<T>` membership checks
- **30 EditMode test methods** (39 total test case executions) across two new test classes:
  - `AbilitySystemTests` (16) — normal/advantage/disadvantage roll picking, crit flag preservation and discard, passive check, and two composite tests documenting the raw-ability-check and saving-throw modifier patterns
  - `ProficiencySystemTests` (14 methods, 23 test case executions) — full PHB bonus table via `[TestCase]`, expertise edge cases, governing-ability routing for `GetSkillBonus`, proficiency set membership checks, out-of-range level guard

#### Phase 2 — DiceRoller

- **`DiceRoller`** (`DeeDeeR.DnD.Core.Systems`) — stateless class that rolls a `DiceExpression` via an injected `IRollProvider` and returns a `RollResult`
  - Critical success / critical fail flags set only for a single d20 roll (the standard check die)
  - Multi-die expressions (2d20 for advantage pool, damage dice) return no crit flags — the calling system selects and interprets results
- **`UnityRollProvider`** (`DeeDeeR.DnD.Runtime.Systems`) — `IRollProvider` backed by `UnityEngine.Random.Range`; swap for `FakeRollProvider` in tests
- **`FakeRollProvider`** (test assembly) — deterministic queue-based `IRollProvider` for unit tests; throws `InvalidOperationException` when the preset sequence is exhausted
- **11 EditMode unit tests** (`DiceRollerTests`):
  - Flat expression returns modifier with no crit flags
  - Single die + positive/negative modifier
  - Multiple dice sum correctly
  - Natural 20 on a single d20 sets `IsCriticalSuccess`
  - Natural 1 on a single d20 sets `IsCriticalFail`
  - Normal d20 roll sets neither flag
  - 2d20 (advantage pool) sets no crit flags even on all-20s
  - Non-d20 max roll sets no crit flags
  - `FakeRollProvider` throws when exhausted
- Expanded `dee-dee-r.dnd-sdk.runtime.asmdef` with `rootNamespace`, `references` (`dee-dee-r.dnd-sdk.core`), and full field set

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
- Added 84 EditMode unit tests for all Core value types (`dee-dee-r.dnd-sdk.tests.editor`):
  - `HitPointStateTests` (18) — damage/heal/temp HP logic, clamping, 2024 no-stack rule
  - `AbilityScoreSetTests` (11) — full PHB modifier table, odd-score floor rounding, immutable `With()`
  - `DiceExpressionTests` (12) — all `ToString()` formats, `Average` calculation, equality
  - `ExhaustionLevelTests` (16) — 0–6 clamping, `D20Penalty = Level × 2`, comparison operators
  - `CurrencyTests` (15) — denomination conversion, mixed totals, arithmetic operators, subtraction clamping, constructor clamping
  - `DeathSaveStateTests` (9) — stabilization/death thresholds, immutability, constructor clamping
  - `AttackRollResultTests` (8) — crit auto-hit, fumble, normal hit/miss, natural-1 forced miss
- Added `"testables": ["dee-dee-r.dnd-sdk"]` to `DnD-SDK-main/Packages/manifest.json` — tests visible in Unity Test Runner under EditMode tab
- Added XML documentation to all public members of all Core enums, value types, and interfaces
- **Bug fixes** applied to Core value types:
  - `AttackRollResult`: natural 1 now always forces a miss regardless of the `hit` parameter
  - `Currency`: constructor clamps all denominations to zero (no negative values); `TotalInCopper` now returns `long` to avoid 32-bit overflow; `ToString()` now lists all non-zero denominations (PP, GP, EP, SP, CP) instead of only GP/SP/CP
  - `DeathSaveState`: constructor now clamps `Successes` and `Failures` to zero minimum
  - `IRollProvider`: removed stale unused `using` directives
