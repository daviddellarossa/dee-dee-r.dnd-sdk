using System.Collections.Generic;
using UnityEngine;
using DeeDeeR.DnD.Runtime.Data;

namespace DeeDeeR.DnD.Game.SaveSystem
{
    /// <summary>
    /// Inspector-assigned registry of all PHB ScriptableObject assets.
    /// Provides name-based lookup so that save/load can serialise SO references as strings.
    /// Populate all list fields by dragging the generated PHB assets in from
    /// <c>Assets/DnD SDK/Data/</c> after running <c>DnD SDK → Generate PHB Assets</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "GameDataRegistry", menuName = "DnD SDK/Game Data Registry")]
    public sealed class GameDataRegistry : ScriptableObject
    {
        [SerializeField] private List<SpeciesSO>    _species    = new List<SpeciesSO>();
        [SerializeField] private List<SubspeciesSO> _subspecies = new List<SubspeciesSO>();
        [SerializeField] private List<BackgroundSO> _backgrounds = new List<BackgroundSO>();
        [SerializeField] private List<ClassSO>      _classes    = new List<ClassSO>();
        [SerializeField] private List<SubclassSO>   _subclasses = new List<SubclassSO>();
        [SerializeField] private List<WeaponSO>     _weapons    = new List<WeaponSO>();
        [SerializeField] private List<ArmorSO>      _armor      = new List<ArmorSO>();
        [SerializeField] private List<ShieldSO>     _shields    = new List<ShieldSO>();
        [SerializeField] private List<FeatSO>       _feats      = new List<FeatSO>();
        [SerializeField] private List<ToolSO>       _tools      = new List<ToolSO>();
        [SerializeField] private List<SpellSO>      _spells     = new List<SpellSO>();

        // ── Read-only list accessors ──────────────────────────────────────────

        public IReadOnlyList<SpeciesSO>    Species     => _species;
        public IReadOnlyList<SubspeciesSO> Subspecies  => _subspecies;
        public IReadOnlyList<BackgroundSO> Backgrounds => _backgrounds;
        public IReadOnlyList<ClassSO>      Classes     => _classes;
        public IReadOnlyList<SubclassSO>   Subclasses  => _subclasses;
        public IReadOnlyList<WeaponSO>     Weapons     => _weapons;
        public IReadOnlyList<ArmorSO>      Armor       => _armor;
        public IReadOnlyList<ShieldSO>     Shields     => _shields;
        public IReadOnlyList<FeatSO>       Feats       => _feats;
        public IReadOnlyList<ToolSO>       Tools       => _tools;
        public IReadOnlyList<SpellSO>      Spells      => _spells;

        // ── Name-based lookups (used by save/load) ────────────────────────────

        public SpeciesSO    FindSpecies(string assetName)     => _species.Find(x => x != null && x.name == assetName);
        public SubspeciesSO FindSubspecies(string assetName)  => _subspecies.Find(x => x != null && x.name == assetName);
        public BackgroundSO FindBackground(string assetName)  => _backgrounds.Find(x => x != null && x.name == assetName);
        public ClassSO      FindClass(string assetName)       => _classes.Find(x => x != null && x.name == assetName);
        public SubclassSO   FindSubclass(string assetName)    => _subclasses.Find(x => x != null && x.name == assetName);
        public WeaponSO     FindWeapon(string assetName)      => _weapons.Find(x => x != null && x.name == assetName);
        public ArmorSO      FindArmor(string assetName)       => _armor.Find(x => x != null && x.name == assetName);
        public ShieldSO     FindShield(string assetName)      => _shields.Find(x => x != null && x.name == assetName);
        public FeatSO       FindFeat(string assetName)        => _feats.Find(x => x != null && x.name == assetName);
        public ToolSO       FindTool(string assetName)        => _tools.Find(x => x != null && x.name == assetName);
        public SpellSO      FindSpell(string assetName)       => _spells.Find(x => x != null && x.name == assetName);
    }
}
