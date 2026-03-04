using UnityEditor;
using UnityEngine.UIElements;

namespace DeeDeeR.DnD.Editor
{
    /// <summary>Shared helpers for loading UI Toolkit assets in DnD SDK editor tools.</summary>
    internal static class DnDEditorUtility
    {
        /// <summary>
        /// Finds and loads a <see cref="VisualTreeAsset"/> (UXML) by file name (without extension).
        /// Returns <see langword="null"/> if no matching asset is found.
        /// </summary>
        internal static VisualTreeAsset LoadUxml(string assetName)
        {
            var guids = AssetDatabase.FindAssets($"{assetName} t:VisualTreeAsset");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        /// <summary>
        /// Finds and loads a <see cref="StyleSheet"/> (USS) by file name (without extension).
        /// Returns <see langword="null"/> if no matching asset is found.
        /// </summary>
        internal static StyleSheet LoadUss(string assetName)
        {
            var guids = AssetDatabase.FindAssets($"{assetName} t:StyleSheet");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}