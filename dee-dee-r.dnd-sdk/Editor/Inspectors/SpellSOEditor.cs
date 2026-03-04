using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Runtime.Data;

namespace DeeDeeR.DnD.Editor.Inspectors
{
    /// <summary>
    /// Custom inspector for <see cref="SpellSO"/> using UI Toolkit.
    /// Shows conditional fields based on <see cref="CastingTimeType"/>,
    /// <see cref="SpellRangeType"/>, and <see cref="SpellComponent"/> flags.
    /// </summary>
    [CustomEditor(typeof(SpellSO))]
    public sealed class SpellSOEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var uxml = DnDEditorUtility.LoadUxml("SpellSOEditor");
            if (uxml == null)
            {
                root.Add(new HelpBox("[DnD SDK] SpellSOEditor.uxml not found.", HelpBoxMessageType.Error));
                return root;
            }
            uxml.CloneTree(root);

            var uss = DnDEditorUtility.LoadUss("SpellSOEditor");
            if (uss != null) root.styleSheets.Add(uss);

            root.Bind(serializedObject);

            // Gather conditional fields.
            var reactionTriggerField = root.Q<PropertyField>("field-reaction-trigger");
            var rangeDistanceField   = root.Q<PropertyField>("field-range-distance");
            var selfAreaField        = root.Q<PropertyField>("field-self-area");
            var materialDescField    = root.Q<PropertyField>("field-material-desc");

            // Set initial visibility.
            RefreshAllConditionalFields(reactionTriggerField, rangeDistanceField, selfAreaField, materialDescField);

            // CastingTime → ReactionTrigger visibility.
            root.Q<PropertyField>("field-casting-time").RegisterValueChangeCallback(evt =>
            {
                bool isReaction = evt.changedProperty.enumValueIndex == (int)CastingTimeType.Reaction;
                reactionTriggerField.style.display = isReaction ? DisplayStyle.Flex : DisplayStyle.None;
            });

            // RangeType → RangeDistance / SelfAreaDescription visibility.
            root.Q<PropertyField>("field-range-type").RegisterValueChangeCallback(evt =>
            {
                bool isRanged = evt.changedProperty.enumValueIndex == (int)SpellRangeType.Ranged;
                bool isSelf   = evt.changedProperty.enumValueIndex == (int)SpellRangeType.Self;
                rangeDistanceField.style.display = isRanged ? DisplayStyle.Flex : DisplayStyle.None;
                selfAreaField.style.display      = isSelf   ? DisplayStyle.Flex : DisplayStyle.None;
            });

            // Components → MaterialDescription visibility.
            root.Q<PropertyField>("field-components").RegisterValueChangeCallback(evt =>
            {
                bool hasMaterial = (evt.changedProperty.intValue & (int)SpellComponent.Material) != 0;
                materialDescField.style.display = hasMaterial ? DisplayStyle.Flex : DisplayStyle.None;
            });

            return root;
        }

        private void RefreshAllConditionalFields(
            VisualElement reactionTriggerField,
            VisualElement rangeDistanceField,
            VisualElement selfAreaField,
            VisualElement materialDescField)
        {
            var castingTimeProp = serializedObject.FindProperty("CastingTime");
            if (castingTimeProp != null)
            {
                bool isReaction = castingTimeProp.enumValueIndex == (int)CastingTimeType.Reaction;
                reactionTriggerField.style.display = isReaction ? DisplayStyle.Flex : DisplayStyle.None;
            }

            var rangeTypeProp = serializedObject.FindProperty("RangeType");
            if (rangeTypeProp != null)
            {
                bool isRanged = rangeTypeProp.enumValueIndex == (int)SpellRangeType.Ranged;
                bool isSelf   = rangeTypeProp.enumValueIndex == (int)SpellRangeType.Self;
                rangeDistanceField.style.display = isRanged ? DisplayStyle.Flex : DisplayStyle.None;
                selfAreaField.style.display      = isSelf   ? DisplayStyle.Flex : DisplayStyle.None;
            }

            var componentsProp = serializedObject.FindProperty("Components");
            if (componentsProp != null)
            {
                bool hasMaterial = (componentsProp.intValue & (int)SpellComponent.Material) != 0;
                materialDescField.style.display = hasMaterial ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}