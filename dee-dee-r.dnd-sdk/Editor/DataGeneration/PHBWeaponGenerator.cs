using UnityEditor;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Runtime.Data;

namespace DeeDeeR.DnD.Editor.DataGeneration
{
    /// <summary>
    /// Generates all SRD 5.1 weapon assets (Simple and Martial).
    /// Damage, mastery, range, weight, and cost are sourced from D&amp;D 2024 PHB / SRD 5.1.
    /// </summary>
    internal static class PHBWeaponGenerator
    {
        private const string Simple  = PHBAssetGenerator.DataRoot + "/Weapons/Simple";
        private const string Martial = PHBAssetGenerator.DataRoot + "/Weapons/Martial";

        internal static void Generate()
        {
            // ── Simple Melee ─────────────────────────────────────────────────
            W(Simple, "Club",          WeaponCategory.Simple, DamageType.Bludgeoning, 1, DieType.D4,  0, 0,    WeaponMastery.Slow,   5,   0,   2.0f, gp: 0, sp: 1, props: Props(WP.Light));
            W(Simple, "Dagger",        WeaponCategory.Simple, DamageType.Piercing,    1, DieType.D4,  0, 0,    WeaponMastery.Nick,  20,  60,   1.0f, gp: 2, props: Props(WP.Finesse, WP.Light, WP.Thrown));
            W(Simple, "Greatclub",     WeaponCategory.Simple, DamageType.Bludgeoning, 1, DieType.D8,  0, 0,    WeaponMastery.Push,   5,   0,  10.0f, gp: 0, sp: 2, props: Props(WP.TwoHanded));
            W(Simple, "Handaxe",       WeaponCategory.Simple, DamageType.Slashing,    1, DieType.D6,  0, 0,    WeaponMastery.Vex,   20,  60,   2.0f, gp: 5, props: Props(WP.Light, WP.Thrown));
            W(Simple, "Javelin",       WeaponCategory.Simple, DamageType.Piercing,    1, DieType.D6,  0, 0,    WeaponMastery.Slow,  30, 120,   2.0f, gp: 0, sp: 5, props: Props(WP.Thrown));
            W(Simple, "Light Hammer",  WeaponCategory.Simple, DamageType.Bludgeoning, 1, DieType.D4,  0, 0,    WeaponMastery.Nick,  20,  60,   2.0f, gp: 2, props: Props(WP.Light, WP.Thrown));
            W(Simple, "Mace",          WeaponCategory.Simple, DamageType.Bludgeoning, 1, DieType.D6,  0, 0,    WeaponMastery.Sap,    5,   0,   4.0f, gp: 5);
            W(Simple, "Quarterstaff",  WeaponCategory.Simple, DamageType.Bludgeoning, 1, DieType.D6,  0, 0,    WeaponMastery.Topple, 5,   0,   4.0f, gp: 0, sp: 2,
                versatileCount: 1, versatileDie: DieType.D8, props: Props(WP.Versatile));
            W(Simple, "Sickle",        WeaponCategory.Simple, DamageType.Slashing,    1, DieType.D4,  0, 0,    WeaponMastery.Nick,   5,   0,   2.0f, gp: 1, props: Props(WP.Light));
            W(Simple, "Spear",         WeaponCategory.Simple, DamageType.Piercing,    1, DieType.D6,  0, 0,    WeaponMastery.Sap,   20,  60,   3.0f, gp: 1,
                versatileCount: 1, versatileDie: DieType.D8, props: Props(WP.Thrown, WP.Versatile));

            // ── Simple Ranged ─────────────────────────────────────────────────
            W(Simple, "Light Crossbow", WeaponCategory.Simple, DamageType.Piercing,    1, DieType.D8,  0, 0,    WeaponMastery.Slow,  80, 320,   5.0f, gp: 25, props: Props(WP.Ammunition, WP.Loading, WP.Range, WP.TwoHanded));
            W(Simple, "Dart",           WeaponCategory.Simple, DamageType.Piercing,    1, DieType.D4,  0, 0,    WeaponMastery.Vex,   20,  60,   0.25f, cp: 5, props: Props(WP.Finesse, WP.Thrown));
            W(Simple, "Shortbow",       WeaponCategory.Simple, DamageType.Piercing,    1, DieType.D6,  0, 0,    WeaponMastery.Vex,   80, 320,   2.0f, gp: 25, props: Props(WP.Ammunition, WP.Range, WP.TwoHanded));
            W(Simple, "Sling",          WeaponCategory.Simple, DamageType.Bludgeoning, 1, DieType.D4,  0, 0,    WeaponMastery.Slow,  30, 120,   0.0f, sp: 1, props: Props(WP.Ammunition, WP.Range));

            // ── Martial Melee ─────────────────────────────────────────────────
            W(Martial, "Battleaxe",  WeaponCategory.Martial, DamageType.Slashing,    1, DieType.D8,  0, 0, WeaponMastery.Topple,  5,   0,  4.0f, gp: 10,
                versatileCount: 1, versatileDie: DieType.D10, props: Props(WP.Versatile));
            W(Martial, "Flail",      WeaponCategory.Martial, DamageType.Bludgeoning, 1, DieType.D8,  0, 0, WeaponMastery.Sap,     5,   0,  2.0f, gp: 10);
            W(Martial, "Glaive",     WeaponCategory.Martial, DamageType.Slashing,    1, DieType.D10, 0, 0, WeaponMastery.Graze,  10,   0,  6.0f, gp: 20, props: Props(WP.Heavy, WP.Reach, WP.TwoHanded));
            W(Martial, "Greataxe",   WeaponCategory.Martial, DamageType.Slashing,    1, DieType.D12, 0, 0, WeaponMastery.Cleave,  5,   0,  7.0f, gp: 30, props: Props(WP.Heavy, WP.TwoHanded));
            W(Martial, "Greatsword", WeaponCategory.Martial, DamageType.Slashing,    2, DieType.D6,  0, 0, WeaponMastery.Graze,   5,   0,  6.0f, gp: 50, props: Props(WP.Heavy, WP.TwoHanded));
            W(Martial, "Halberd",    WeaponCategory.Martial, DamageType.Slashing,    1, DieType.D10, 0, 0, WeaponMastery.Cleave, 10,   0,  6.0f, gp: 20, props: Props(WP.Heavy, WP.Reach, WP.TwoHanded));
            W(Martial, "Lance",      WeaponCategory.Martial, DamageType.Piercing,    1, DieType.D12, 0, 0, WeaponMastery.Topple, 10,   0,  6.0f, gp: 10, props: Props(WP.Reach));
            W(Martial, "Longsword",  WeaponCategory.Martial, DamageType.Slashing,    1, DieType.D8,  0, 0, WeaponMastery.Sap,     5,   0,  3.0f, gp: 15,
                versatileCount: 1, versatileDie: DieType.D10, props: Props(WP.Versatile));
            W(Martial, "Maul",       WeaponCategory.Martial, DamageType.Bludgeoning, 2, DieType.D6,  0, 0, WeaponMastery.Topple,  5,   0, 10.0f, gp: 10, props: Props(WP.Heavy, WP.TwoHanded));
            W(Martial, "Morningstar",WeaponCategory.Martial, DamageType.Piercing,    1, DieType.D8,  0, 0, WeaponMastery.Sap,     5,   0,  4.0f, gp: 15);
            W(Martial, "Pike",       WeaponCategory.Martial, DamageType.Piercing,    1, DieType.D10, 0, 0, WeaponMastery.Push,   10,   0, 18.0f, gp:  5, props: Props(WP.Heavy, WP.Reach, WP.TwoHanded));
            W(Martial, "Rapier",     WeaponCategory.Martial, DamageType.Piercing,    1, DieType.D8,  0, 0, WeaponMastery.Vex,     5,   0,  2.0f, gp: 25, props: Props(WP.Finesse));
            W(Martial, "Scimitar",   WeaponCategory.Martial, DamageType.Slashing,    1, DieType.D6,  0, 0, WeaponMastery.Nick,    5,   0,  3.0f, gp: 25, props: Props(WP.Finesse, WP.Light));
            W(Martial, "Shortsword", WeaponCategory.Martial, DamageType.Piercing,    1, DieType.D6,  0, 0, WeaponMastery.Vex,     5,   0,  2.0f, gp: 10, props: Props(WP.Finesse, WP.Light));
            W(Martial, "Trident",    WeaponCategory.Martial, DamageType.Piercing,    1, DieType.D6,  0, 0, WeaponMastery.Topple, 20,  60,  4.0f, gp:  5,
                versatileCount: 1, versatileDie: DieType.D8, props: Props(WP.Thrown, WP.Versatile));
            W(Martial, "War Pick",   WeaponCategory.Martial, DamageType.Piercing,    1, DieType.D8,  0, 0, WeaponMastery.Slow,    5,   0,  2.0f, gp:  5);
            W(Martial, "Warhammer",  WeaponCategory.Martial, DamageType.Bludgeoning, 1, DieType.D8,  0, 0, WeaponMastery.Push,    5,   0,  2.0f, gp: 15,
                versatileCount: 1, versatileDie: DieType.D10, props: Props(WP.Versatile));
            W(Martial, "Whip",       WeaponCategory.Martial, DamageType.Slashing,    1, DieType.D4,  0, 0, WeaponMastery.Slow,   10,   0,  3.0f, gp:  2, props: Props(WP.Finesse, WP.Reach));

            // ── Martial Ranged ────────────────────────────────────────────────
            W(Martial, "Blowgun",         WeaponCategory.Martial, DamageType.Piercing, 0, DieType.D4,  1, 0, WeaponMastery.Vex,    25, 100,  1.0f, gp: 10, props: Props(WP.Ammunition, WP.Loading, WP.Range));
            W(Martial, "Hand Crossbow",   WeaponCategory.Martial, DamageType.Piercing, 1, DieType.D6,  0, 0, WeaponMastery.Vex,    30, 120,  3.0f, gp: 75, props: Props(WP.Ammunition, WP.Light, WP.Loading, WP.Range));
            W(Martial, "Heavy Crossbow",  WeaponCategory.Martial, DamageType.Piercing, 1, DieType.D10, 0, 0, WeaponMastery.Push,  100, 400, 18.0f, gp: 50, props: Props(WP.Ammunition, WP.Heavy, WP.Loading, WP.Range, WP.TwoHanded));
            W(Martial, "Longbow",         WeaponCategory.Martial, DamageType.Piercing, 1, DieType.D8,  0, 0, WeaponMastery.Slow,  150, 600,  2.0f, gp: 50, props: Props(WP.Ammunition, WP.Heavy, WP.Range, WP.TwoHanded));
            W(Martial, "Net",             WeaponCategory.Martial, DamageType.Bludgeoning, 0, DieType.D4, 0, 0, WeaponMastery.Slow, 5,  15,  3.0f, gp:  1, props: Props(WP.Thrown));
        }

        // ── Helper ────────────────────────────────────────────────────────────

        // Alias for readability inside this file.
        private static WeaponProperty[] Props(params WP[] p)
        {
            var result = new WeaponProperty[p.Length];
            for (int i = 0; i < p.Length; i++) result[i] = (WeaponProperty)(int)p[i];
            return result;
        }

        private static void W(
            string folder, string name,
            WeaponCategory cat, DamageType dmgType,
            int count, DieType die, int modifier, int _unused,
            WeaponMastery mastery,
            int rangeNormal, int rangeLong,
            float weight,
            int gp = 0, int sp = 0, int cp = 0,
            WeaponProperty[] props = null,
            int versatileCount = 0, DieType versatileDie = DieType.D4)
        {
            var w = PHBAssetGenerator.GetOrCreate<WeaponSO>(folder, name);
            w.Category         = cat;
            w.DamageDice       = new DiceExpressionData { Count = count, Die = die, Modifier = modifier };
            w.DamageType       = dmgType;
            w.Properties       = props ?? new WeaponProperty[0];
            w.MasteryProperty  = mastery;
            w.RangeNormal      = rangeNormal;
            w.RangeLong        = rangeLong;
            w.Weight           = weight;
            w.Cost             = new CurrencyData { GP = gp, SP = sp, CP = cp };
            if (versatileCount > 0)
                w.VersatileDamageDice = new DiceExpressionData { Count = versatileCount, Die = versatileDie };
            EditorUtility.SetDirty(w);
        }

        // Compact alias for WeaponProperty to reduce line length.
        private enum WP
        {
            Ammunition = 0, Finesse = 1, Heavy = 2, Light = 3, Loading = 4,
            Range = 5, Reach = 6, Thrown = 7, TwoHanded = 8, Versatile = 9
        }
    }
}
