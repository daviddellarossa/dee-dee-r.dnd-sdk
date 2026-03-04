using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Runtime.Data;

namespace DeeDeeR.DnD.Editor.Inspectors
{
    /// <summary>
    /// Custom inspector for <see cref="ClassSO"/> using UI Toolkit.
    /// Organises fields into collapsible foldouts and hides
    /// <c>SpellcastingAbility</c> when the class has no spellcasting.
    /// </summary>
    [CustomEditor(typeof(ClassSO))]
    public sealed class ClassSOEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var uxml = DnDEditorUtility.LoadUxml("ClassSOEditor");
            if (uxml == null)
            {
                root.Add(new HelpBox("[DnD SDK] ClassSOEditor.uxml not found.", HelpBoxMessageType.Error));
                return root;
            }
            uxml.CloneTree(root);

            var uss = DnDEditorUtility.LoadUss("ClassSOEditor");
            if (uss != null) root.styleSheets.Add(uss);

            root.Bind(serializedObject);

            // SpellcastingAbility: only relevant when CasterType is not None.
            var spellAbilityField = root.Q<PropertyField>("field-spellcasting-ability");
            var casterTypeField   = root.Q<PropertyField>("field-caster-type");

            RefreshSpellcastingVisibility(spellAbilityField);

            casterTypeField.RegisterValueChangeCallback(evt =>
            {
                bool isNone = evt.changedProperty.enumValueIndex == (int)CasterType.None;
                spellAbilityField.style.display = isNone ? DisplayStyle.None : DisplayStyle.Flex;
            });

            return root;
        }

        private void RefreshSpellcastingVisibility(VisualElement field)
        {
            var prop = serializedObject.FindProperty("CasterType");
            bool isNone = prop != null && prop.enumValueIndex == (int)CasterType.None;
            field.style.display = isNone ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}