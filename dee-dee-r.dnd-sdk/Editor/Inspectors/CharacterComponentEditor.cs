using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using DeeDeeR.DnD.Runtime.Components;

namespace DeeDeeR.DnD.Editor.Inspectors
{
    /// <summary>
    /// Custom inspector for <see cref="CharacterComponent"/> using UI Toolkit.
    /// Shows the endpoint ID prominently, warns when no <see cref="DnDSdkRunner"/>
    /// is present, and provides a live runtime summary in Play mode.
    /// </summary>
    [CustomEditor(typeof(CharacterComponent))]
    public sealed class CharacterComponentEditor : UnityEditor.Editor
    {
        private VisualElement _runtimePanel;
        private Label _nameLabel;
        private Label _levelLabel;
        private Label _hpLabel;
        private Label _conditionsLabel;
        private IVisualElementScheduledItem _runtimePoller;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var uxml = DnDEditorUtility.LoadUxml("CharacterComponentEditor");
            if (uxml == null)
            {
                root.Add(new HelpBox("[DnD SDK] CharacterComponentEditor.uxml not found.", HelpBoxMessageType.Error));
                return root;
            }
            uxml.CloneTree(root);

            var uss = DnDEditorUtility.LoadUss("CharacterComponentEditor");
            if (uss != null) root.styleSheets.Add(uss);

            root.Bind(serializedObject);

            // DnDSdkRunner warning.
            var runnerWarning = root.Q<HelpBox>("helpbox-no-runner");
            RefreshRunnerWarning(runnerWarning);

            // Runtime summary panel.
            _runtimePanel     = root.Q<VisualElement>("panel-runtime");
            _nameLabel        = root.Q<Label>("label-runtime-name");
            _levelLabel       = root.Q<Label>("label-runtime-level");
            _hpLabel          = root.Q<Label>("label-runtime-hp");
            _conditionsLabel  = root.Q<Label>("label-runtime-conditions");

            RefreshRuntimePanel();

            // Poll every 500 ms in play mode to keep the summary live.
            _runtimePoller = root.schedule.Execute(RefreshRuntimePanel)
                .Every(500)
                .StartingIn(500);

            return root;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void RefreshRunnerWarning(HelpBox box)
        {
            if (box == null) return;
            bool hasRunner = Object.FindObjectOfType<DnDSdkRunner>() != null;
            box.style.display = hasRunner ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void RefreshRuntimePanel()
        {
            if (_runtimePanel == null) return;

            bool isPlaying = Application.isPlaying;
            _runtimePanel.style.display = isPlaying ? DisplayStyle.Flex : DisplayStyle.None;

            if (!isPlaying) return;

            var component = (CharacterComponent)target;
            if (component == null) return;

            var record = component.Record;
            var state  = component.State;

            // Name and total level.
            int totalLevel = 0;
            var classNames = new List<string>();
            if (record.ClassLevels != null)
            {
                foreach (var cl in record.ClassLevels)
                {
                    if (cl?.Class == null) continue;
                    totalLevel += cl.Level;
                    classNames.Add($"{cl.Class.name} {cl.Level}");
                }
            }

            _nameLabel.text  = $"Character: {record.Name ?? "(unnamed)"}  |  Player: {record.PlayerName ?? "—"}";
            _levelLabel.text = totalLevel > 0
                ? $"Level {totalLevel}  ({string.Join(" / ", classNames)})"
                : "No class assigned";
            _hpLabel.text    = $"HP: {state.HitPoints.Current} / {state.HitPoints.Maximum}" +
                               (state.HitPoints.Temporary > 0 ? $"  (+{state.HitPoints.Temporary} temp)" : "");

            if (state.Conditions != null && state.Conditions.Count > 0)
            {
                var sb = new StringBuilder("Conditions: ");
                foreach (var c in state.Conditions) sb.Append(c).Append(", ");
                if (sb.Length > 2) sb.Length -= 2;
                _conditionsLabel.text = sb.ToString();
                _conditionsLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                _conditionsLabel.style.display = DisplayStyle.None;
            }
        }
    }
}