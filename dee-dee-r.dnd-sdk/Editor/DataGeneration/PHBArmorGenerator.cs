using UnityEditor;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Runtime.Data;

namespace DeeDeeR.DnD.Editor.DataGeneration
{
    /// <summary>
    /// Generates all SRD 5.1 armor and shield assets.
    /// AC values, Dex caps, Strength requirements, and cost from D&amp;D 2024 PHB / SRD 5.1.
    /// </summary>
    internal static class PHBArmorGenerator
    {
        private const string ArmorFolder  = PHBAssetGenerator.DataRoot + "/Armor";
        private const string ShieldFolder = PHBAssetGenerator.DataRoot + "/Shields";

        internal static void Generate()
        {
            // ── Light Armor (MaxDexBonus = -1 → unlimited Dex added) ──────────
            Armor("Padded",          ArmorCategory.Light,   baseAc: 11, maxDex: -1, strReq: 0, stealth: true,  weight: 8f,  gp:   5);
            Armor("Leather",         ArmorCategory.Light,   baseAc: 11, maxDex: -1, strReq: 0, stealth: false, weight: 10f, gp:  10);
            Armor("Studded Leather", ArmorCategory.Light,   baseAc: 12, maxDex: -1, strReq: 0, stealth: false, weight: 13f, gp:  45);

            // ── Medium Armor (MaxDexBonus = 2) ────────────────────────────────
            Armor("Hide",            ArmorCategory.Medium,  baseAc: 12, maxDex: 2,  strReq: 0,  stealth: false, weight: 12f, gp:  10);
            Armor("Chain Shirt",     ArmorCategory.Medium,  baseAc: 13, maxDex: 2,  strReq: 0,  stealth: false, weight: 20f, gp:  50);
            Armor("Scale Mail",      ArmorCategory.Medium,  baseAc: 14, maxDex: 2,  strReq: 0,  stealth: true,  weight: 45f, gp:  50);
            Armor("Breastplate",     ArmorCategory.Medium,  baseAc: 14, maxDex: 2,  strReq: 0,  stealth: false, weight: 20f, gp: 400);
            Armor("Half Plate",      ArmorCategory.Medium,  baseAc: 15, maxDex: 2,  strReq: 0,  stealth: true,  weight: 40f, gp: 750);

            // ── Heavy Armor (MaxDexBonus = 0 → no Dex added) ─────────────────
            Armor("Ring Mail",       ArmorCategory.Heavy,   baseAc: 14, maxDex: 0,  strReq: 0,  stealth: true,  weight: 40f, gp:  30);
            Armor("Chain Mail",      ArmorCategory.Heavy,   baseAc: 16, maxDex: 0,  strReq: 13, stealth: true,  weight: 55f, gp:  75);
            Armor("Splint",          ArmorCategory.Heavy,   baseAc: 17, maxDex: 0,  strReq: 15, stealth: true,  weight: 60f, gp: 200);
            Armor("Plate",           ArmorCategory.Heavy,   baseAc: 18, maxDex: 0,  strReq: 15, stealth: true,  weight: 65f, gp: 1500);

            // ── Shield ────────────────────────────────────────────────────────
            Shield("Shield", acBonus: 2, strReq: 0, weight: 6f, gp: 10);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void Armor(
            string name, ArmorCategory cat,
            int baseAc, int maxDex, int strReq,
            bool stealth, float weight, int gp = 0, int sp = 0)
        {
            var a = PHBAssetGenerator.GetOrCreate<ArmorSO>(ArmorFolder, name);
            a.Category           = cat;
            a.BaseArmorClass     = baseAc;
            a.MaxDexBonus        = maxDex;
            a.StrengthRequirement = strReq;
            a.StealthDisadvantage = stealth;
            a.Weight             = weight;
            a.Cost               = new CurrencyData { GP = gp, SP = sp };
            EditorUtility.SetDirty(a);
        }

        private static void Shield(string name, int acBonus, int strReq, float weight, int gp)
        {
            var s = PHBAssetGenerator.GetOrCreate<ShieldSO>(ShieldFolder, name);
            s.AcBonus             = acBonus;
            s.StrengthRequirement = strReq;
            s.Weight              = weight;
            s.Cost                = new CurrencyData { GP = gp };
            EditorUtility.SetDirty(s);
        }
    }
}
