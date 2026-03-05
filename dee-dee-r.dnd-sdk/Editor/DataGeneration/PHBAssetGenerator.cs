using UnityEditor;
using UnityEngine;

namespace DeeDeeR.DnD.Editor.DataGeneration
{
    /// <summary>
    /// Entry point for the PHB data asset generator.
    /// Menu: <b>DnD SDK → Generate PHB Assets</b>.
    /// Creates all PHB ScriptableObject .asset files under <c>Assets/DnD SDK/Data/</c>.
    /// Content drawn from SRD 5.1 (CC-BY-4.0) with D&amp;D 2024 PHB mechanics.
    /// Existing assets are updated in place; nothing is deleted.
    /// </summary>
    public static class PHBAssetGenerator
    {
        internal const string DataRoot = "Assets/DnD SDK/Data";

        [MenuItem("DnD SDK/Generate PHB Assets")]
        public static void GenerateAll()
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                EnsureFolders();

                PHBDefinitionGenerator.Generate();
                PHBWeaponGenerator.Generate();
                PHBArmorGenerator.Generate();
                PHBClassGenerator.GenerateClasses();
                PHBSpeciesGenerator.Generate();
                PHBBackgroundGenerator.Generate();
                PHBSpellGenerator.Generate();

                // Second pass: link subclasses to parent class assets.
                PHBClassGenerator.GenerateSubclasses();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[PHBAssetGenerator] Done — all PHB assets written to " + DataRoot);
            }
        }

        // ── Folder helpers ────────────────────────────────────────────────────

        private static void EnsureFolders()
        {
            EnsureFolder("Assets",          "DnD SDK");
            EnsureFolder("Assets/DnD SDK",  "Data");
            EnsureFolder(DataRoot,          "Weapons");
            EnsureFolder(DataRoot + "/Weapons", "Simple");
            EnsureFolder(DataRoot + "/Weapons", "Martial");
            EnsureFolder(DataRoot, "Armor");
            EnsureFolder(DataRoot, "Shields");
            EnsureFolder(DataRoot, "Classes");
            EnsureFolder(DataRoot, "Subclasses");
            EnsureFolder(DataRoot, "Species");
            EnsureFolder(DataRoot, "Backgrounds");
            EnsureFolder(DataRoot, "Spells");
            for (int i = 0; i <= 9; i++)
                EnsureFolder(DataRoot + "/Spells", i == 0 ? "0 Cantrips" : i.ToString());
        }

        internal static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        // ── Asset creation helper ─────────────────────────────────────────────

        /// <summary>
        /// Loads an existing asset at <c>folder/name.asset</c> or creates a new one.
        /// Call <see cref="EditorUtility.SetDirty"/> after setting fields.
        /// </summary>
        internal static T GetOrCreate<T>(string folder, string name) where T : ScriptableObject
        {
            string path = folder + "/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        /// <summary>Loads an already-created asset by path. Returns null if not found.</summary>
        internal static T Load<T>(string folder, string name) where T : ScriptableObject
            => AssetDatabase.LoadAssetAtPath<T>(folder + "/" + name + ".asset");
    }
}
