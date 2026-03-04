using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Components;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.State;
using DeeDeeR.DnD.Runtime.Systems;

namespace DeeDeeR.DnD.Editor.Wizards
{
    /// <summary>
    /// Multi-step EditorWindow for creating a <see cref="CharacterComponent"/> in the scene.
    /// Collects identity, species, background, class, and ability score data, then calls
    /// <see cref="CharacterFactory"/> to build the record and state.
    /// </summary>
    public sealed class CharacterCreationWizard : EditorWindow
    {
        [MenuItem("DnD SDK/Character Creation Wizard")]
        public static void Open() => GetWindow<CharacterCreationWizard>("Character Creation Wizard");

        // ── Wizard state ──────────────────────────────────────────────────────

        private int _currentStep;
        private const int TotalSteps = 6;

        // Step 1 — Identity
        private string _characterName = "New Character";
        private string _playerName    = "";

        // Step 2 — Species
        private SpeciesSO    _species;
        private SubspeciesSO _subspecies;

        // Step 3 — Background
        private BackgroundSO _background;

        // Step 4 — Class
        private ClassSO              _classSO;
        private int                  _classLevel = 1;
        private readonly List<SkillType> _chosenSkills = new List<SkillType>();

        // Step 5 — Ability Scores
        private int _str = 10, _dex = 10, _con = 10, _int = 10, _wis = 10, _cha = 10;

        // ── Visual element references ─────────────────────────────────────────

        private Label         _stepLabel;
        private HelpBox       _identityError;
        private HelpBox       _speciesError;
        private HelpBox       _backgroundError;
        private Label         _backgroundPreview;
        private HelpBox       _classError;
        private Label         _skillCountLabel;
        private VisualElement _skillToggleContainer;
        private Label         _summaryLabel;
        private Button        _btnBack;
        private Button        _btnNext;
        private Button        _btnCreate;

        // Page roots
        private VisualElement[] _pages;

        // ── CreateGUI ─────────────────────────────────────────────────────────

        public void CreateGUI()
        {
            var uxml = DnDEditorUtility.LoadUxml("CharacterCreationWizard");
            if (uxml == null)
            {
                rootVisualElement.Add(new HelpBox(
                    "[DnD SDK] CharacterCreationWizard.uxml not found.", HelpBoxMessageType.Error));
                return;
            }
            uxml.CloneTree(rootVisualElement);

            var uss = DnDEditorUtility.LoadUss("CharacterCreationWizard");
            if (uss != null) rootVisualElement.styleSheets.Add(uss);

            CacheElements();
            WireEvents();
            ShowPage(0);
        }

        // ── Element wiring ────────────────────────────────────────────────────

        private void CacheElements()
        {
            _stepLabel  = rootVisualElement.Q<Label>("label-step");
            _pages      = new[]
            {
                rootVisualElement.Q<VisualElement>("page-identity"),
                rootVisualElement.Q<VisualElement>("page-species"),
                rootVisualElement.Q<VisualElement>("page-background"),
                rootVisualElement.Q<VisualElement>("page-class"),
                rootVisualElement.Q<VisualElement>("page-scores"),
                rootVisualElement.Q<VisualElement>("page-summary"),
            };

            _identityError        = rootVisualElement.Q<HelpBox>("helpbox-identity-error");
            _speciesError         = rootVisualElement.Q<HelpBox>("helpbox-species-error");
            _backgroundError      = rootVisualElement.Q<HelpBox>("helpbox-background-error");
            _backgroundPreview    = rootVisualElement.Q<Label>("label-background-preview");
            _classError           = rootVisualElement.Q<HelpBox>("helpbox-class-error");
            _skillCountLabel      = rootVisualElement.Q<Label>("label-skill-count");
            _skillToggleContainer = rootVisualElement.Q<VisualElement>("container-skill-toggles");
            _summaryLabel         = rootVisualElement.Q<Label>("label-summary");

            _btnBack   = rootVisualElement.Q<Button>("btn-back");
            _btnNext   = rootVisualElement.Q<Button>("btn-next");
            _btnCreate = rootVisualElement.Q<Button>("btn-create");

            // Bind identity fields.
            rootVisualElement.Q<TextField>("field-char-name").RegisterValueChangedCallback(
                evt => _characterName = evt.newValue);
            rootVisualElement.Q<TextField>("field-player-name").RegisterValueChangedCallback(
                evt => _playerName = evt.newValue);

            // Bind species fields.
            var speciesField = rootVisualElement.Q<ObjectField>("field-species");
            speciesField.objectType = typeof(SpeciesSO);
            speciesField.RegisterValueChangedCallback(evt =>
            {
                _species    = evt.newValue as SpeciesSO;
                _subspecies = null;
                var subsField = rootVisualElement.Q<ObjectField>("field-subspecies");
                subsField.SetValueWithoutNotify(null);
                subsField.style.display = _species?.Subspecies?.Count > 0
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            });

            var subspeciesField = rootVisualElement.Q<ObjectField>("field-subspecies");
            subspeciesField.objectType = typeof(SubspeciesSO);
            subspeciesField.RegisterValueChangedCallback(evt => _subspecies = evt.newValue as SubspeciesSO);
            subspeciesField.style.display = DisplayStyle.None;

            // Bind background field.
            var bgField = rootVisualElement.Q<ObjectField>("field-background");
            bgField.objectType = typeof(BackgroundSO);
            bgField.RegisterValueChangedCallback(evt =>
            {
                _background = evt.newValue as BackgroundSO;
                RefreshBackgroundPreview();
            });

            // Bind class field.
            var classField = rootVisualElement.Q<ObjectField>("field-class");
            classField.objectType = typeof(ClassSO);
            classField.RegisterValueChangedCallback(evt =>
            {
                _classSO = evt.newValue as ClassSO;
                _chosenSkills.Clear();
                RebuildSkillToggles();
            });

            rootVisualElement.Q<IntegerField>("field-class-level").RegisterValueChangedCallback(
                evt => _classLevel = Mathf.Clamp(evt.newValue, 1, 20));

            // Bind score fields.
            rootVisualElement.Q<IntegerField>("field-str").RegisterValueChangedCallback(evt => _str = evt.newValue);
            rootVisualElement.Q<IntegerField>("field-dex").RegisterValueChangedCallback(evt => _dex = evt.newValue);
            rootVisualElement.Q<IntegerField>("field-con").RegisterValueChangedCallback(evt => _con = evt.newValue);
            rootVisualElement.Q<IntegerField>("field-int").RegisterValueChangedCallback(evt => _int = evt.newValue);
            rootVisualElement.Q<IntegerField>("field-wis").RegisterValueChangedCallback(evt => _wis = evt.newValue);
            rootVisualElement.Q<IntegerField>("field-cha").RegisterValueChangedCallback(evt => _cha = evt.newValue);
        }

        private void WireEvents()
        {
            _btnBack.clicked   += OnBack;
            _btnNext.clicked   += OnNext;
            _btnCreate.clicked += OnCreate;
        }

        // ── Navigation ────────────────────────────────────────────────────────

        private void OnBack()
        {
            if (_currentStep > 0) ShowPage(_currentStep - 1);
        }

        private void OnNext()
        {
            if (!ValidateCurrentStep()) return;
            if (_currentStep < TotalSteps - 1)
            {
                if (_currentStep == TotalSteps - 2) BuildSummary();
                ShowPage(_currentStep + 1);
            }
        }

        private void ShowPage(int index)
        {
            _currentStep = index;
            for (int i = 0; i < _pages.Length; i++)
                _pages[i].style.display = i == index ? DisplayStyle.Flex : DisplayStyle.None;

            _stepLabel.text = $"Step {index + 1} of {TotalSteps}";
            _btnBack.style.display   = index > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _btnNext.style.display   = index < TotalSteps - 1 ? DisplayStyle.Flex : DisplayStyle.None;
            _btnCreate.style.display = index == TotalSteps - 1 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // ── Validation ────────────────────────────────────────────────────────

        private bool ValidateCurrentStep()
        {
            switch (_currentStep)
            {
                case 0: return ValidateIdentity();
                case 1: return ValidateSpecies();
                case 2: return ValidateBackground();
                case 3: return ValidateClass();
                default: return true;
            }
        }

        private bool ValidateIdentity()
        {
            if (string.IsNullOrWhiteSpace(_characterName))
            {
                ShowError(_identityError, "Character name is required.");
                return false;
            }
            HideError(_identityError);
            return true;
        }

        private bool ValidateSpecies()
        {
            if (_species == null)
            {
                ShowError(_speciesError, "Please select a species.");
                return false;
            }
            HideError(_speciesError);
            return true;
        }

        private bool ValidateBackground()
        {
            if (_background == null)
            {
                ShowError(_backgroundError, "Please select a background.");
                return false;
            }
            HideError(_backgroundError);
            return true;
        }

        private bool ValidateClass()
        {
            if (_classSO == null)
            {
                ShowError(_classError, "Please select a class.");
                return false;
            }
            if (_classLevel < 1 || _classLevel > 20)
            {
                ShowError(_classError, "Level must be between 1 and 20.");
                return false;
            }
            int maxSkills = _classSO.SkillChoices.Count;
            if (_chosenSkills.Count > maxSkills)
            {
                ShowError(_classError, $"You may choose at most {maxSkills} skills.");
                return false;
            }
            HideError(_classError);
            return true;
        }

        private static void ShowError(HelpBox box, string msg)
        {
            if (box == null) return;
            box.text = msg;
            box.style.display = DisplayStyle.Flex;
        }

        private static void HideError(HelpBox box)
        {
            if (box != null) box.style.display = DisplayStyle.None;
        }

        // ── Background preview ────────────────────────────────────────────────

        private void RefreshBackgroundPreview()
        {
            if (_backgroundPreview == null || _background == null)
            {
                if (_backgroundPreview != null) _backgroundPreview.text = "";
                return;
            }

            var sb = new StringBuilder();
            if (_background.AbilityScoreIncreases != null)
            {
                sb.Append("ASIs: ");
                foreach (var asi in _background.AbilityScoreIncreases)
                    sb.Append($"{asi.Ability} +{asi.Amount}  ");
                sb.AppendLine();
            }
            if (_background.SkillProficiencies != null && _background.SkillProficiencies.Length > 0)
            {
                sb.Append("Skills: ");
                sb.AppendJoin(", ", _background.SkillProficiencies);
                sb.AppendLine();
            }
            if (_background.OriginFeat != null)
                sb.AppendLine($"Origin Feat: {_background.OriginFeat.name}");

            _backgroundPreview.text = sb.ToString().TrimEnd();
        }

        // ── Skill toggles ─────────────────────────────────────────────────────

        private void RebuildSkillToggles()
        {
            _skillToggleContainer.Clear();
            _chosenSkills.Clear();

            if (_classSO == null || _classSO.SkillChoices.Pool == null || _classSO.SkillChoices.Pool.Length == 0)
            {
                _skillCountLabel.text = "";
                var titleLabel = rootVisualElement.Q<Label>("label-skill-pool-title");
                if (titleLabel != null) titleLabel.style.display = DisplayStyle.None;
                return;
            }

            var titleLabel2 = rootVisualElement.Q<Label>("label-skill-pool-title");
            if (titleLabel2 != null) titleLabel2.style.display = DisplayStyle.Flex;

            int maxChoices = _classSO.SkillChoices.Count;
            UpdateSkillCountLabel(maxChoices);

            foreach (var skill in _classSO.SkillChoices.Pool)
            {
                var toggle = new Toggle(skill.ToString());
                var capturedSkill = skill;
                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        if (_chosenSkills.Count >= maxChoices)
                        {
                            toggle.SetValueWithoutNotify(false);
                            return;
                        }
                        _chosenSkills.Add(capturedSkill);
                    }
                    else
                    {
                        _chosenSkills.Remove(capturedSkill);
                    }
                    UpdateSkillCountLabel(maxChoices);
                });
                _skillToggleContainer.Add(toggle);
            }
        }

        private void UpdateSkillCountLabel(int maxChoices)
        {
            if (_skillCountLabel != null)
                _skillCountLabel.text = $"Chosen: {_chosenSkills.Count} / {maxChoices}";
        }

        // ── Summary ───────────────────────────────────────────────────────────

        private void BuildSummary()
        {
            if (_summaryLabel == null) return;

            var scores = new AbilityScoreSet(_str, _dex, _con, _int, _wis, _cha);
            var classLevel = new ClassLevel { Class = _classSO, Level = _classLevel };

            int computedHp = 0;
            try
            {
                var factory = new CharacterFactory();
                var (record, state) = factory.Build(
                    _characterName, _playerName,
                    _species, _subspecies, _background,
                    scores,
                    new List<ClassLevel> { classLevel },
                    _chosenSkills);
                computedHp = state.HitPoints.Maximum;
            }
            catch { /* summary is best-effort */ }

            var sb = new StringBuilder();
            sb.AppendLine($"Name:        {_characterName}");
            sb.AppendLine($"Player:      {(_playerName.Length > 0 ? _playerName : "—")}");
            sb.AppendLine($"Species:     {(_species != null ? _species.name : "—")}");
            if (_subspecies != null) sb.AppendLine($"Subspecies:  {_subspecies.name}");
            sb.AppendLine($"Background:  {(_background != null ? _background.name : "—")}");
            sb.AppendLine($"Class:       {(_classSO != null ? _classSO.name : "—")} (Level {_classLevel})");
            sb.AppendLine($"Scores:      STR {_str}  DEX {_dex}  CON {_con}  INT {_int}  WIS {_wis}  CHA {_cha}");
            if (computedHp > 0) sb.AppendLine($"Starting HP: {computedHp}");

            _summaryLabel.text = sb.ToString().TrimEnd();
        }

        // ── Create ────────────────────────────────────────────────────────────

        private void OnCreate()
        {
            if (!ValidateCurrentStep()) return;

            var scores     = new AbilityScoreSet(_str, _dex, _con, _int, _wis, _cha);
            var classLevel = new ClassLevel { Class = _classSO, Level = _classLevel };

            CharacterRecord record;
            CharacterState  state;
            try
            {
                var factory = new CharacterFactory();
                (record, state) = factory.Build(
                    _characterName, _playerName,
                    _species, _subspecies, _background,
                    scores,
                    new List<ClassLevel> { classLevel },
                    _chosenSkills);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DnD SDK] CharacterCreationWizard failed to build character: {ex.Message}");
                return;
            }

            // Create GameObject and attach CharacterComponent.
            var go = new GameObject(_characterName);
            Undo.RegisterCreatedObjectUndo(go, "Create Character");

            var component = Undo.AddComponent<CharacterComponent>(go);
            component.Record    = record;
            component.State     = state;
            component.Inventory = new InventoryState();
            EditorUtility.SetDirty(component);

            Selection.activeGameObject = go;

            Debug.Log(
                $"[DnD SDK] Character '{_characterName}' created — " +
                $"Level {_classLevel} {_classSO?.name}, HP {state.HitPoints.Maximum}. " +
                "Note: AbilityScoreSet is not serialized by Unity's built-in serializer; " +
                "scores will reset on scene reload without a custom save system.");

            Close();
        }
    }
}