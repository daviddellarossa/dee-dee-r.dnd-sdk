using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using DeeDeeR.DnD.Game;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.State;
using DeeDeeR.DnD.Core.Systems;
using DeeDeeR.DnD.Runtime.Systems;
using DeeDeeR.DnD.Game.SaveSystem;

namespace DeeDeeR.DnD.Game.Menu
{
    /// <summary>
    /// 6-step character creation wizard.
    /// Populates UI fields from <see cref="GameDataRegistry"/>, validates each step,
    /// and on confirmation builds a character via <see cref="CharacterFactory"/>.
    /// </summary>
    public sealed class CharacterCreationController
    {
        private readonly GameManager  _gm;
        private readonly MenuManager  _menu;
        private readonly GameDataRegistry _registry;

        // ── Step panels ───────────────────────────────────────────────────────
        private readonly VisualElement[] _steps = new VisualElement[6];
        private int _currentStep;

        // ── Step 1 fields ─────────────────────────────────────────────────────
        private TextField  _charNameField;
        private TextField  _playerNameField;
        private EnumField  _alignmentField;

        // ── Step 2 fields ─────────────────────────────────────────────────────
        private DropdownField _speciesField;
        private DropdownField _subspeciesField;
        private VisualElement _subspeciesRow;

        // ── Step 3 fields ─────────────────────────────────────────────────────
        private DropdownField _backgroundField;
        private Label         _asiPreviewLabel;

        // ── Step 4 fields ─────────────────────────────────────────────────────
        private DropdownField _classField;
        private SliderInt     _levelField;
        private ScrollView    _skillsScroll;
        private readonly List<Toggle> _skillToggles = new List<Toggle>();

        // ── Step 5 fields ─────────────────────────────────────────────────────
        private IntegerField _strField, _dexField, _conField, _intField, _wisField, _chaField;

        // ── Step 6 ────────────────────────────────────────────────────────────
        private Label _summaryLabel;

        // ── Nav buttons ───────────────────────────────────────────────────────
        private Button _backBtn, _nextBtn, _confirmBtn, _cancelBtn;

        // ── Internal data ─────────────────────────────────────────────────────
        private List<string>    _speciesNames    = new List<string>();
        private List<string>    _backgroundNames = new List<string>();
        private List<string>    _classNames      = new List<string>();
        private List<SkillType> _currentSkillPool = new List<SkillType>();

        public CharacterCreationController(VisualElement root, GameManager gm, MenuManager menu)
        {
            _gm   = gm;
            _menu = menu;

            if (root == null) return;

            // Retrieve registry from gm — exposed via the GameManager's serialised field.
            // We can't access private fields directly, so GameDataRegistry must be
            // referenced through a public property or an overload. We expose it via GameManager.
            _registry = gm.Registry;

            // ── Locate step panels ────────────────────────────────────────────
            for (int i = 0; i < 6; i++)
                _steps[i] = root.Q<VisualElement>($"step-{i + 1}");

            // ── Step 1 ────────────────────────────────────────────────────────
            _charNameField   = root.Q<TextField>("field-char-name");
            _playerNameField = root.Q<TextField>("field-player-name");
            _alignmentField  = root.Q<EnumField>("field-alignment");

            // ── Step 2 ────────────────────────────────────────────────────────
            _speciesField    = root.Q<DropdownField>("field-species");
            _subspeciesField = root.Q<DropdownField>("field-subspecies");
            _subspeciesRow   = root.Q<VisualElement>("subspecies-row");

            _speciesField?.RegisterValueChangedCallback(_ => RefreshSubspecies());

            // ── Step 3 ────────────────────────────────────────────────────────
            _backgroundField = root.Q<DropdownField>("field-background");
            _asiPreviewLabel = root.Q<Label>("label-asi-preview");

            _backgroundField?.RegisterValueChangedCallback(_ => RefreshAsiPreview());

            // ── Step 4 ────────────────────────────────────────────────────────
            _classField   = root.Q<DropdownField>("field-class");
            _levelField   = root.Q<SliderInt>("field-level");
            _skillsScroll = root.Q<ScrollView>("scroll-skills");

            _classField?.RegisterValueChangedCallback(_ => RefreshSkills());

            // ── Step 5 ────────────────────────────────────────────────────────
            _strField = root.Q<IntegerField>("field-str");
            _dexField = root.Q<IntegerField>("field-dex");
            _conField = root.Q<IntegerField>("field-con");
            _intField = root.Q<IntegerField>("field-int");
            _wisField = root.Q<IntegerField>("field-wis");
            _chaField = root.Q<IntegerField>("field-cha");

            // ── Step 6 ────────────────────────────────────────────────────────
            _summaryLabel = root.Q<Label>("label-summary");
            _confirmBtn   = root.Q<Button>("btn-confirm");
            _confirmBtn?.RegisterCallback<ClickEvent>(_ => Confirm());

            // ── Nav ───────────────────────────────────────────────────────────
            _backBtn   = root.Q<Button>("btn-back");
            _nextBtn   = root.Q<Button>("btn-next");
            _cancelBtn = root.Q<Button>("btn-cancel");

            _backBtn?.RegisterCallback<ClickEvent>(_ => NavigateStep(-1));
            _nextBtn?.RegisterCallback<ClickEvent>(_ => NavigateStep(+1));
            _cancelBtn?.RegisterCallback<ClickEvent>(_ =>
            {
                _gm.SetState(GameState.MainMenu);
                _menu.ShowPanelForState(GameState.MainMenu);
            });
        }

        /// <summary>Called by <see cref="MenuManager"/> when the character creation panel is shown.</summary>
        public void OnShow()
        {
            _currentStep = 0;
            PopulateDropdowns();
            ShowStep(0);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void PopulateDropdowns()
        {
            if (_registry == null) return;

            // Species
            _speciesNames.Clear();
            foreach (var s in _registry.Species)
                if (s != null) _speciesNames.Add(s.name);
            if (_speciesField != null) _speciesField.choices = _speciesNames;

            // Backgrounds
            _backgroundNames.Clear();
            foreach (var b in _registry.Backgrounds)
                if (b != null) _backgroundNames.Add(b.name);
            if (_backgroundField != null) _backgroundField.choices = _backgroundNames;

            // Classes
            _classNames.Clear();
            foreach (var c in _registry.Classes)
                if (c != null) _classNames.Add(c.name);
            if (_classField != null) _classField.choices = _classNames;

            RefreshSubspecies();
            RefreshAsiPreview();
        }

        private void RefreshSubspecies()
        {
            if (_speciesField == null || _subspeciesField == null || _subspeciesRow == null)
                return;

            var selected = _registry?.FindSpecies(_speciesField.value);
            if (selected == null || selected.Subspecies == null || selected.Subspecies.Count == 0)
            {
                _subspeciesRow.AddToClassList("hidden");
                return;
            }

            _subspeciesRow.RemoveFromClassList("hidden");
            var names = new List<string>();
            foreach (var sub in selected.Subspecies)
                if (sub != null) names.Add(sub.name);
            _subspeciesField.choices = names;
        }

        private void RefreshAsiPreview()
        {
            if (_asiPreviewLabel == null || _backgroundField == null) return;

            var bg = _registry?.FindBackground(_backgroundField.value);
            if (bg == null || bg.AbilityScoreIncreases == null || bg.AbilityScoreIncreases.Count == 0)
            {
                _asiPreviewLabel.text = "";
                return;
            }

            var parts = new System.Text.StringBuilder("ASIs: ");
            foreach (var asi in bg.AbilityScoreIncreases)
                parts.Append($"+{asi.Amount} {asi.Ability}  ");
            _asiPreviewLabel.text = parts.ToString().TrimEnd();
        }

        private void RefreshSkills()
        {
            if (_skillsScroll == null || _classField == null) return;

            _skillsScroll.Clear();
            _skillToggles.Clear();
            _currentSkillPool.Clear();

            var cls = _registry?.FindClass(_classField.value);
            if (cls == null) return;

            var pool  = cls.SkillChoices.Pool;
            var count = cls.SkillChoices.Count;

            if (pool == null) return;

            foreach (var skill in pool)
            {
                var toggle = new Toggle(skill.ToString());
                toggle.userData = skill;
                toggle.RegisterValueChangedCallback(evt => ClampSkillSelection(count));
                _skillsScroll.Add(toggle);
                _skillToggles.Add(toggle);
                _currentSkillPool.Add(skill);
            }
        }

        private void ClampSkillSelection(int maxCount)
        {
            int selected = 0;
            foreach (var t in _skillToggles) if (t.value) selected++;

            if (selected > maxCount)
            {
                // Un-check the last checked toggle that pushed us over the limit.
                foreach (var t in _skillToggles)
                {
                    if (t.value)
                    {
                        t.SetValueWithoutNotify(false);
                        break;
                    }
                }
            }
        }

        private void ShowStep(int index)
        {
            for (int i = 0; i < _steps.Length; i++)
                _steps[i]?.AddToClassList("hidden");

            _steps[index]?.RemoveFromClassList("hidden");

            if (_backBtn != null)   _backBtn.SetEnabled(index > 0);
            if (_nextBtn != null)   _nextBtn.SetEnabled(index < _steps.Length - 1);
            if (_confirmBtn != null) _confirmBtn.style.display = index == _steps.Length - 1
                ? DisplayStyle.Flex : DisplayStyle.None;

            if (index == _steps.Length - 1)
                BuildSummary();
        }

        private void NavigateStep(int delta)
        {
            int next = _currentStep + delta;
            if (next < 0 || next >= _steps.Length) return;
            if (delta > 0 && !ValidateCurrentStep()) return;

            _currentStep = next;
            ShowStep(_currentStep);
        }

        private bool ValidateCurrentStep()
        {
            switch (_currentStep)
            {
                case 0: // Identity
                    if (string.IsNullOrWhiteSpace(_charNameField?.value))
                    {
                        UnityEngine.Debug.LogWarning("[CharacterCreation] Character name is required.");
                        return false;
                    }
                    return true;

                case 1: // Species
                    return _speciesField?.value != null && _speciesNames.Contains(_speciesField.value);

                case 2: // Background
                    return _backgroundField?.value != null && _backgroundNames.Contains(_backgroundField.value);

                case 3: // Class
                    return _classField?.value != null && _classNames.Contains(_classField.value);

                case 4: // Ability scores
                    return ValidateAbilityScores();

                default:
                    return true;
            }
        }

        private bool ValidateAbilityScores()
        {
            var fields = new[] { _strField, _dexField, _conField, _intField, _wisField, _chaField };
            foreach (var f in fields)
            {
                if (f == null) continue;
                int val = f.value;
                if (val < 1 || val > 20)
                {
                    f.value = UnityEngine.Mathf.Clamp(val, 1, 20);
                }
            }
            return true;
        }

        private void BuildSummary()
        {
            if (_summaryLabel == null) return;

            var cls = _registry?.FindClass(_classField?.value ?? "");
            int level = _levelField?.value ?? 1;
            int str   = _strField?.value   ?? 10;
            int con   = _conField?.value   ?? 10;
            int profBonus = new ProficiencySystem().GetProficiencyBonus(level);
            int hpFaces   = cls != null ? (int)cls.HitDie : 8;
            int conMod    = AbilityScoreSet.GetModifier(con);
            int maxHp     = hpFaces + conMod + (level - 1) * (hpFaces / 2 + 1 + conMod);
            maxHp = UnityEngine.Mathf.Max(maxHp, level);

            _summaryLabel.text =
                $"Name: {_charNameField?.value}\n" +
                $"Class: {_classField?.value ?? "?"} (Level {level})\n" +
                $"Species: {_speciesField?.value ?? "?"} | Background: {_backgroundField?.value ?? "?"}\n" +
                $"Proficiency Bonus: +{profBonus} | Estimated Max HP: {maxHp}";
        }

        private void Confirm()
        {
            if (!ValidateCurrentStep()) return;

            if (_registry == null)
            {
                UnityEngine.Debug.LogError("[CharacterCreation] GameDataRegistry is not assigned.");
                return;
            }

            var species    = _registry.FindSpecies(_speciesField?.value ?? "");
            var background = _registry.FindBackground(_backgroundField?.value ?? "");
            var cls        = _registry.FindClass(_classField?.value ?? "");

            if (species == null || background == null || cls == null)
            {
                UnityEngine.Debug.LogError("[CharacterCreation] Required asset reference is null. Check registry.");
                return;
            }

            SubspeciesSO subspecies = null;
            if (_subspeciesField != null && !string.IsNullOrEmpty(_subspeciesField.value))
                subspecies = _registry.FindSubspecies(_subspeciesField.value);

            var baseScores = new AbilityScoreSet(
                _strField?.value ?? 10,
                _dexField?.value ?? 10,
                _conField?.value ?? 10,
                _intField?.value ?? 10,
                _wisField?.value ?? 10,
                _chaField?.value ?? 10);

            var classLevel = new ClassLevel
            {
                Class = cls,
                Level = _levelField?.value ?? 1
            };

            var chosenSkills = new List<SkillType>();
            foreach (var t in _skillToggles)
                if (t.value && t.userData is SkillType skill)
                    chosenSkills.Add(skill);

            var factory = new CharacterFactory();
            var (record, state) = factory.Build(
                _charNameField?.value ?? "Hero",
                _playerNameField?.value ?? "",
                species,
                subspecies,
                background,
                baseScores,
                new[] { classLevel },
                chosenSkills);

            if (Enum.TryParse<Alignment>(_alignmentField?.value?.ToString() ?? "", out var alignment))
                record.Alignment = alignment;

            record.PersonalityTraits = "";
            record.Ideals            = "";
            record.Bonds             = "";
            record.Flaws             = "";

            _gm.StartNewGame(record, state);
        }
    }
}
