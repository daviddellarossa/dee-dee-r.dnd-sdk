using System.Collections.Generic;
using UnityEditor;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Runtime.Data;

namespace DeeDeeR.DnD.Editor.DataGeneration
{
    /// <summary>
    /// Generates all SRD 5.1 spell assets (cantrips through 9th level).
    /// Mechanical fields (level, school, casting time, range, components, duration,
    /// concentration, ritual) are populated. LocalizedString description fields are left empty.
    /// SpellSO.ClassLists are populated after class assets are created.
    /// </summary>
    internal static class PHBSpellGenerator
    {
        private const string SpellRoot = PHBAssetGenerator.DataRoot + "/Spells";

        // Component flag aliases
        private const SpellComponent V   = SpellComponent.Verbal;
        private const SpellComponent So  = SpellComponent.Somatic;
        private const SpellComponent M   = SpellComponent.Material;
        private const SpellComponent VS  = SpellComponent.Verbal | SpellComponent.Somatic;
        private const SpellComponent VM  = SpellComponent.Verbal | SpellComponent.Material;
        private const SpellComponent SM  = SpellComponent.Somatic | SpellComponent.Material;
        private const SpellComponent VSM = SpellComponent.Verbal | SpellComponent.Somatic | SpellComponent.Material;

        // Casting time aliases
        private const CastingTimeType A   = CastingTimeType.Action;
        private const CastingTimeType BA  = CastingTimeType.BonusAction;
        private const CastingTimeType R   = CastingTimeType.Reaction;
        private const CastingTimeType Min = CastingTimeType.OneMinute;
        private const CastingTimeType HR  = CastingTimeType.OneHour;
        private const CastingTimeType TM  = CastingTimeType.TenMinutes;
        private const CastingTimeType H8  = CastingTimeType.EightHours;

        // Range type aliases
        private const SpellRangeType Self  = SpellRangeType.Self;
        private const SpellRangeType Touch = SpellRangeType.Touch;
        private const SpellRangeType Range = SpellRangeType.Ranged;
        private const SpellRangeType Spec  = SpellRangeType.Special;

        // Duration aliases
        private const SpellDurationType Inst  = SpellDurationType.Instantaneous;
        private const SpellDurationType R1    = SpellDurationType.OneRound;
        private const SpellDurationType D1m   = SpellDurationType.OneMinute;
        private const SpellDurationType D10m  = SpellDurationType.TenMinutes;
        private const SpellDurationType D1h   = SpellDurationType.OneHour;
        private const SpellDurationType D8h   = SpellDurationType.EightHours;
        private const SpellDurationType D24h  = SpellDurationType.TwentyFourHours;
        private const SpellDurationType D7d   = SpellDurationType.SevenDays;
        private const SpellDurationType Perm  = SpellDurationType.UntilDispelled;
        private const SpellDurationType PermT = SpellDurationType.UntilDispelledOrTriggered;
        private const SpellDurationType SpecD = SpellDurationType.Special;

        // School aliases
        private const SpellSchool Abj  = SpellSchool.Abjuration;
        private const SpellSchool Con  = SpellSchool.Conjuration;
        private const SpellSchool Div  = SpellSchool.Divination;
        private const SpellSchool Enc  = SpellSchool.Enchantment;
        private const SpellSchool Evo  = SpellSchool.Evocation;
        private const SpellSchool Ill  = SpellSchool.Illusion;
        private const SpellSchool Nec  = SpellSchool.Necromancy;
        private const SpellSchool Tra  = SpellSchool.Transmutation;

        internal static void Generate()
        {
            // S(name, level, school, castTime, rangeType, rangeFt, components, duration, concentration, ritual, classes...)
            // ── Cantrips (level 0) ────────────────────────────────────────────
            string lvl0 = SpellRoot + "/0 Cantrips";
            S(lvl0, "Acid Splash",       0, Con, A,  Range, 60,  VS,  Inst,  false, false, "Sorcerer","Wizard");
            S(lvl0, "Blade Ward",        0, Abj, A,  Self,  0,   VS,  R1,    false, false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl0, "Chill Touch",       0, Nec, A,  Range, 60,  VS,  R1,    false, false, "Sorcerer","Warlock","Wizard");
            S(lvl0, "Dancing Lights",    0, Ill, A,  Range, 120, VSM, D1m,   true,  false, "Bard","Sorcerer","Wizard");
            S(lvl0, "Druidcraft",        0, Tra, A,  Range, 30,  VS,  Inst,  false, false, "Druid");
            S(lvl0, "Eldritch Blast",    0, Evo, A,  Range, 120, VS,  Inst,  false, false, "Warlock");
            S(lvl0, "Fire Bolt",         0, Evo, A,  Range, 120, VS,  Inst,  false, false, "Sorcerer","Wizard");
            S(lvl0, "Friends",           0, Enc, A,  Self,  0,   SM,  D1m,   true,  false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl0, "Guidance",          0, Div, A,  Touch, 0,   VS,  D1m,   true,  false, "Cleric","Druid");
            S(lvl0, "Light",             0, Evo, A,  Touch, 0,   VM,  D1h,   false, false, "Bard","Cleric","Sorcerer","Wizard");
            S(lvl0, "Mage Hand",         0, Con, A,  Range, 30,  VS,  D1m,   false, false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl0, "Mending",           0, Tra, Min, Touch, 0,  VSM, Inst,  false, false, "Bard","Cleric","Druid","Sorcerer","Wizard");
            S(lvl0, "Message",           0, Tra, A,  Range, 120, VSM, R1,    false, false, "Bard","Sorcerer","Wizard");
            S(lvl0, "Minor Illusion",    0, Ill, A,  Range, 30,  SM,  D1m,   false, false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl0, "Poison Spray",      0, Con, A,  Range, 30,  VS,  Inst,  false, false, "Druid","Sorcerer","Warlock","Wizard");
            S(lvl0, "Prestidigitation",  0, Tra, A,  Range, 10,  VS,  D1h,   false, false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl0, "Produce Flame",     0, Con, A,  Self,  0,   VS,  D10m,  false, false, "Druid");
            S(lvl0, "Ray of Frost",      0, Evo, A,  Range, 60,  VS,  Inst,  false, false, "Sorcerer","Wizard");
            S(lvl0, "Resistance",        0, Abj, A,  Touch, 0,   VSM, D1m,   true,  false, "Cleric","Druid");
            S(lvl0, "Sacred Flame",      0, Evo, A,  Range, 60,  VS,  Inst,  false, false, "Cleric");
            S(lvl0, "Shillelagh",        0, Tra, BA, Touch, 0,   VSM, D1m,   false, false, "Druid");
            S(lvl0, "Shocking Grasp",    0, Evo, A,  Touch, 0,   VS,  Inst,  false, false, "Sorcerer","Wizard");
            S(lvl0, "Spare the Dying",   0, Nec, A,  Range, 15,  VS,  Inst,  false, false, "Cleric");
            S(lvl0, "Thaumaturgy",       0, Tra, A,  Range, 30,  V,   D1m,   false, false, "Cleric");
            S(lvl0, "Toll the Dead",     0, Nec, A,  Range, 60,  VS,  Inst,  false, false, "Cleric","Warlock","Wizard");
            S(lvl0, "True Strike",       0, Div, A,  Range, 30,  So,  R1,    false, false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl0, "Vicious Mockery",   0, Enc, A,  Range, 60,  V,   Inst,  false, false, "Bard");
            S(lvl0, "Word of Radiance",  0, Evo, A,  Self,  0,   VM,  Inst,  false, false, "Cleric");

            // ── 1st Level ─────────────────────────────────────────────────────
            string lvl1 = SpellRoot + "/1";
            S(lvl1, "Alarm",                      1, Abj, Min, Range, 30,  VSM, D8h,   false, true,  "Ranger","Wizard");
            S(lvl1, "Animal Friendship",           1, Enc, A,  Range, 30,  VSM, D24h,  false, false, "Bard","Druid","Ranger");
            S(lvl1, "Bane",                        1, Enc, A,  Range, 30,  VSM, D1m,   true,  false, "Bard","Cleric");
            S(lvl1, "Bless",                       1, Enc, A,  Range, 30,  VSM, D1m,   true,  false, "Cleric","Paladin");
            S(lvl1, "Burning Hands",               1, Evo, A,  Self,  0,   VS,  Inst,  false, false, "Sorcerer","Wizard");
            S(lvl1, "Charm Person",                1, Enc, A,  Range, 30,  VS,  D1h,   false, false, "Bard","Druid","Sorcerer","Warlock","Wizard");
            S(lvl1, "Color Spray",                 1, Ill, A,  Self,  0,   VSM, R1,    false, false, "Sorcerer","Wizard");
            S(lvl1, "Command",                     1, Enc, A,  Range, 60,  V,   R1,    false, false, "Cleric","Paladin");
            S(lvl1, "Comprehend Languages",        1, Div, A,  Self,  0,   VSM, D1h,   false, true,  "Bard","Sorcerer","Warlock","Wizard");
            S(lvl1, "Create or Destroy Water",     1, Tra, A,  Range, 30,  VSM, Inst,  false, false, "Cleric","Druid");
            S(lvl1, "Cure Wounds",                 1, Evo, A,  Touch, 0,   VS,  Inst,  false, false, "Bard","Cleric","Druid","Paladin","Ranger");
            S(lvl1, "Detect Evil and Good",        1, Div, A,  Self,  0,   VS,  D10m,  true,  false, "Cleric","Paladin");
            S(lvl1, "Detect Magic",                1, Div, A,  Self,  0,   VS,  D10m,  true,  true,  "Bard","Cleric","Druid","Paladin","Ranger","Sorcerer","Wizard");
            S(lvl1, "Detect Poison and Disease",   1, Div, A,  Self,  0,   VSM, D10m,  true,  true,  "Cleric","Druid","Paladin","Ranger");
            S(lvl1, "Disguise Self",               1, Ill, A,  Self,  0,   VS,  D1h,   false, false, "Bard","Sorcerer","Wizard");
            S(lvl1, "Divine Favor",                1, Evo, BA, Self,  0,   VS,  D1m,   true,  false, "Paladin");
            S(lvl1, "Entangle",                    1, Con, A,  Range, 90,  VS,  D1m,   true,  false, "Druid");
            S(lvl1, "Expeditious Retreat",         1, Tra, BA, Self,  0,   VS,  D10m,  true,  false, "Sorcerer","Warlock","Wizard");
            S(lvl1, "Faerie Fire",                 1, Evo, A,  Range, 60,  V,   D1m,   true,  false, "Bard","Druid");
            S(lvl1, "False Life",                  1, Nec, A,  Self,  0,   VSM, D1h,   false, false, "Sorcerer","Wizard");
            S(lvl1, "Feather Fall",                1, Tra, R,  Range, 60,  VM,  D1m,   false, false, "Bard","Sorcerer","Wizard");
            S(lvl1, "Find Familiar",               1, Con, HR, Range, 10,  VSM, Inst,  false, true,  "Wizard");
            S(lvl1, "Fog Cloud",                   1, Con, A,  Range, 120, VS,  D1h,   true,  false, "Druid","Ranger","Sorcerer","Wizard");
            S(lvl1, "Goodberry",                   1, Tra, A,  Touch, 0,   VSM, Inst,  false, false, "Druid","Ranger");
            S(lvl1, "Grease",                      1, Con, A,  Range, 60,  VSM, D1m,   false, false, "Wizard");
            S(lvl1, "Guiding Bolt",                1, Evo, A,  Range, 120, VS,  R1,    false, false, "Cleric");
            S(lvl1, "Healing Word",                1, Evo, BA, Range, 60,  V,   Inst,  false, false, "Bard","Cleric","Druid");
            S(lvl1, "Hellish Rebuke",              1, Evo, R,  Range, 60,  VS,  Inst,  false, false, "Warlock");
            S(lvl1, "Heroism",                     1, Enc, A,  Touch, 0,   VS,  D1m,   true,  false, "Bard","Paladin");
            S(lvl1, "Hideous Laughter",            1, Enc, A,  Range, 30,  VSM, D1m,   true,  false, "Bard","Wizard");
            S(lvl1, "Hunter's Mark",               1, Div, BA, Range, 90,  V,   D1h,   true,  false, "Ranger");
            S(lvl1, "Identify",                    1, Div, Min, Touch, 0,  VSM, Inst,  false, true,  "Bard","Wizard");
            S(lvl1, "Illusory Script",             1, Ill, Min, Touch, 0,  SM,  D10d,  false, true,  "Bard","Warlock","Wizard");
            S(lvl1, "Inflict Wounds",              1, Nec, A,  Touch, 0,   VS,  Inst,  false, false, "Cleric");
            S(lvl1, "Jump",                        1, Tra, A,  Touch, 0,   VSM, D1m,   false, false, "Druid","Ranger","Sorcerer","Wizard");
            S(lvl1, "Longstrider",                 1, Tra, A,  Touch, 0,   VSM, D1h,   false, false, "Bard","Druid","Ranger","Wizard");
            S(lvl1, "Mage Armor",                  1, Abj, A,  Touch, 0,   VSM, D8h,   false, false, "Sorcerer","Wizard");
            S(lvl1, "Magic Missile",               1, Evo, A,  Range, 120, VS,  Inst,  false, false, "Sorcerer","Wizard");
            S(lvl1, "Protection from Evil and Good", 1, Abj, A, Touch, 0, VSM, D10m, true, false, "Cleric","Druid","Paladin","Warlock","Wizard");
            S(lvl1, "Purify Food and Drink",       1, Tra, A,  Range, 10,  VS,  Inst,  false, true,  "Cleric","Druid","Paladin");
            S(lvl1, "Ray of Sickness",             1, Nec, A,  Range, 60,  VS,  Inst,  false, false, "Sorcerer","Wizard");
            S(lvl1, "Shield",                      1, Abj, R,  Self,  0,   VS,  R1,    false, false, "Sorcerer","Wizard");
            S(lvl1, "Shield of Faith",             1, Abj, BA, Range, 60,  VSM, D10m,  true,  false, "Cleric","Paladin");
            S(lvl1, "Silent Image",                1, Ill, A,  Range, 60,  VSM, D10m,  true,  false, "Bard","Sorcerer","Wizard");
            S(lvl1, "Sleep",                       1, Enc, A,  Range, 90,  VSM, D1m,   false, false, "Bard","Sorcerer","Wizard");
            S(lvl1, "Speak with Animals",          1, Div, A,  Self,  0,   VS,  D10m,  false, true,  "Bard","Druid","Ranger");
            S(lvl1, "Thunderwave",                 1, Evo, A,  Self,  0,   VS,  Inst,  false, false, "Bard","Druid","Sorcerer","Wizard");
            S(lvl1, "Unseen Servant",              1, Con, A,  Range, 60,  VSM, D1h,   false, true,  "Bard","Warlock","Wizard");
            S(lvl1, "Witch Bolt",                  1, Evo, A,  Range, 30,  VS,  D1m,   true,  false, "Sorcerer","Warlock","Wizard");
            S(lvl1, "Wrathful Smite",              1, Evo, BA, Self,  0,   V,   D1m,   true,  false, "Paladin");

            // ── 2nd Level ─────────────────────────────────────────────────────
            string lvl2 = SpellRoot + "/2";
            S(lvl2, "Aid",                     2, Abj, A,  Range, 30,  VSM, D8h,   false, false, "Cleric","Paladin","Ranger");
            S(lvl2, "Alter Self",              2, Tra, A,  Self,  0,   VS,  D1h,   true,  false, "Sorcerer","Wizard");
            S(lvl2, "Animal Messenger",        2, Enc, A,  Range, 30,  VSM, D24h,  false, true,  "Bard","Druid","Ranger");
            S(lvl2, "Arcane Lock",             2, Abj, A,  Touch, 0,   VSM, Perm,  false, false, "Wizard");
            S(lvl2, "Augury",                  2, Div, Min, Self, 0,   VSM, Inst,  false, true,  "Cleric","Druid");
            S(lvl2, "Barkskin",                2, Tra, A,  Touch, 0,   VS,  D1h,   false, false, "Druid","Ranger");
            S(lvl2, "Blindness Deafness",      2, Nec, A,  Range, 30,  V,   D1m,   false, false, "Bard","Cleric","Sorcerer","Wizard");
            S(lvl2, "Blur",                    2, Ill, A,  Self,  0,   V,   D1m,   true,  false, "Sorcerer","Wizard");
            S(lvl2, "Branding Smite",          2, Evo, BA, Self,  0,   V,   D1m,   true,  false, "Paladin");
            S(lvl2, "Calm Emotions",           2, Enc, A,  Range, 60,  VS,  D1m,   true,  false, "Bard","Cleric");
            S(lvl2, "Cloud of Daggers",        2, Con, A,  Range, 60,  VSM, D1m,   true,  false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl2, "Continual Flame",         2, Evo, A,  Touch, 0,   VSM, Perm,  false, false, "Cleric","Wizard");
            S(lvl2, "Cordon of Arrows",        2, Tra, A,  Touch, 0,   VSM, D8h,   false, false, "Ranger");
            S(lvl2, "Crown of Madness",        2, Enc, A,  Range, 120, VS,  D1m,   true,  false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl2, "Darkness",                2, Evo, A,  Range, 60,  VM,  D10m,  true,  false, "Sorcerer","Warlock","Wizard");
            S(lvl2, "Darkvision",              2, Tra, A,  Touch, 0,   VSM, D8h,   false, false, "Druid","Ranger","Sorcerer","Wizard");
            S(lvl2, "Detect Thoughts",         2, Div, A,  Self,  0,   VS,  D1m,   true,  false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl2, "Dragon's Breath",         2, Tra, BA, Touch, 0,   VSM, D1m,   true,  false, "Sorcerer","Wizard");
            S(lvl2, "Enhance Ability",         2, Tra, A,  Touch, 0,   VSM, D1h,   true,  false, "Bard","Cleric","Druid","Ranger","Sorcerer","Wizard");
            S(lvl2, "Enlarge Reduce",          2, Tra, A,  Range, 30,  VSM, D1m,   true,  false, "Sorcerer","Wizard");
            S(lvl2, "Enthrall",                2, Enc, A,  Range, 60,  VS,  D1m,   false, false, "Bard","Warlock");
            S(lvl2, "Find Steed",              2, Con, TM, Self,  0,   VS,  Inst,  false, false, "Paladin");
            S(lvl2, "Find Traps",              2, Div, A,  Range, 120, VS,  Inst,  false, false, "Cleric","Druid","Ranger");
            S(lvl2, "Flame Blade",             2, Evo, BA, Self,  0,   VS,  D10m,  true,  false, "Druid");
            S(lvl2, "Flaming Sphere",          2, Con, A,  Range, 60,  VSM, D1m,   true,  false, "Druid","Wizard");
            S(lvl2, "Gentle Repose",           2, Nec, A,  Touch, 0,   VSM, D10d,  false, true,  "Cleric","Wizard");
            S(lvl2, "Gust of Wind",            2, Evo, A,  Self,  0,   VSM, D1m,   true,  false, "Druid","Sorcerer","Wizard");
            S(lvl2, "Heat Metal",              2, Tra, A,  Range, 60,  VSM, D1m,   true,  false, "Bard","Druid");
            S(lvl2, "Hold Person",             2, Enc, A,  Range, 60,  VSM, D1m,   true,  false, "Bard","Cleric","Druid","Sorcerer","Warlock","Wizard");
            S(lvl2, "Invisibility",            2, Ill, A,  Touch, 0,   VSM, D1h,   true,  false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl2, "Knock",                   2, Tra, A,  Range, 60,  V,   Inst,  false, false, "Bard","Sorcerer","Wizard");
            S(lvl2, "Lesser Restoration",      2, Abj, A,  Touch, 0,   VS,  Inst,  false, false, "Bard","Cleric","Druid","Paladin","Ranger");
            S(lvl2, "Levitate",                2, Tra, A,  Range, 60,  VSM, D10m,  true,  false, "Sorcerer","Wizard");
            S(lvl2, "Locate Animals or Plants",2, Div, A,  Self,  0,   VSM, Inst,  false, true,  "Bard","Druid","Ranger");
            S(lvl2, "Locate Object",           2, Div, A,  Self,  0,   VSM, D10m,  true,  false, "Bard","Cleric","Druid","Paladin","Ranger","Wizard");
            S(lvl2, "Magic Mouth",             2, Ill, Min, Range, 30, VSM, Perm,  false, true,  "Bard","Wizard");
            S(lvl2, "Magic Weapon",            2, Tra, BA, Touch, 0,   VS,  D1h,   true,  false, "Paladin","Wizard");
            S(lvl2, "Misty Step",              2, Con, BA, Self,  0,   V,   Inst,  false, false, "Sorcerer","Warlock","Wizard");
            S(lvl2, "Moonbeam",                2, Evo, A,  Range, 120, VSM, D1m,   true,  false, "Druid");
            S(lvl2, "Nystul's Magic Aura",     2, Ill, A,  Touch, 0,   VSM, D24h,  false, false, "Wizard");
            S(lvl2, "Pass without Trace",      2, Abj, A,  Self,  0,   VSM, D1h,   true,  false, "Druid","Ranger");
            S(lvl2, "Phantasmal Force",        2, Ill, A,  Range, 60,  VSM, D1m,   true,  false, "Bard","Sorcerer","Wizard");
            S(lvl2, "Prayer of Healing",       2, Evo, TM, Range, 30,  V,   Inst,  false, false, "Cleric");
            S(lvl2, "Protection from Poison",  2, Abj, A,  Touch, 0,   VS,  D1h,   false, false, "Cleric","Druid","Paladin","Ranger");
            S(lvl2, "Ray of Enfeeblement",     2, Nec, A,  Range, 60,  VS,  D1m,   true,  false, "Warlock","Wizard");
            S(lvl2, "Rope Trick",              2, Tra, A,  Touch, 0,   VSM, D1h,   false, false, "Wizard");
            S(lvl2, "See Invisibility",        2, Div, A,  Self,  0,   VSM, D1h,   false, false, "Bard","Sorcerer","Wizard");
            S(lvl2, "Shatter",                 2, Evo, A,  Range, 60,  VSM, Inst,  false, false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl2, "Silence",                 2, Ill, A,  Range, 120, VS,  D10m,  true,  true,  "Bard","Cleric","Ranger");
            S(lvl2, "Spider Climb",            2, Tra, A,  Touch, 0,   VSM, D1h,   true,  false, "Sorcerer","Warlock","Wizard");
            S(lvl2, "Spiritual Weapon",        2, Evo, BA, Range, 60,  VS,  D1m,   false, false, "Cleric");
            S(lvl2, "Suggestion",              2, Enc, A,  Range, 30,  VM,  D8h,   true,  false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl2, "Warding Bond",            2, Abj, A,  Touch, 0,   VSM, D1h,   false, false, "Cleric","Paladin");
            S(lvl2, "Web",                     2, Con, A,  Range, 60,  VSM, D1h,   true,  false, "Sorcerer","Wizard");
            S(lvl2, "Zone of Truth",           2, Enc, A,  Range, 60,  VS,  D10m,  false, false, "Bard","Cleric","Paladin");

            // ── 3rd Level ─────────────────────────────────────────────────────
            string lvl3 = SpellRoot + "/3";
            S(lvl3, "Animate Dead",            3, Nec, Min, Range, 10,  VSM, Inst,  false, false, "Cleric","Wizard");
            S(lvl3, "Aura of Vitality",        3, Evo, A,  Self,  0,   V,   D1m,   true,  false, "Paladin");
            S(lvl3, "Beacon of Hope",          3, Abj, A,  Range, 30,  VS,  D1m,   true,  false, "Cleric");
            S(lvl3, "Bestow Curse",            3, Nec, A,  Touch, 0,   VS,  D1m,   true,  false, "Bard","Cleric","Wizard");
            S(lvl3, "Blink",                   3, Tra, A,  Self,  0,   VS,  D1m,   false, false, "Sorcerer","Wizard");
            S(lvl3, "Call Lightning",          3, Con, A,  Range, 120, VS,  D10m,  true,  false, "Druid");
            S(lvl3, "Clairvoyance",            3, Div, TM, Spec,  0,   VSM, D10m,  true,  false, "Bard","Cleric","Sorcerer","Wizard");
            S(lvl3, "Conjure Animals",         3, Con, A,  Range, 60,  VS,  D1h,   true,  false, "Druid","Ranger");
            S(lvl3, "Conjure Barrage",         3, Con, A,  Self,  0,   VS,  Inst,  false, false, "Ranger");
            S(lvl3, "Counterspell",            3, Abj, R,  Range, 60,  So,  Inst,  false, false, "Sorcerer","Warlock","Wizard");
            S(lvl3, "Create Food and Water",   3, Con, A,  Range, 30,  VS,  Inst,  false, false, "Cleric","Paladin");
            S(lvl3, "Crusader's Mantle",       3, Evo, A,  Self,  0,   V,   D1m,   true,  false, "Paladin");
            S(lvl3, "Daylight",                3, Evo, A,  Range, 60,  VS,  D1h,   false, false, "Cleric","Druid","Paladin","Ranger","Sorcerer");
            S(lvl3, "Dispel Magic",            3, Abj, A,  Range, 120, VS,  Inst,  false, false, "Bard","Cleric","Druid","Paladin","Sorcerer","Warlock","Wizard");
            S(lvl3, "Elemental Weapon",        3, Tra, A,  Touch, 0,   VS,  D1h,   true,  false, "Paladin");
            S(lvl3, "Fear",                    3, Ill, A,  Self,  0,   VSM, D1m,   true,  false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl3, "Feign Death",             3, Nec, A,  Touch, 0,   VSM, D1h,   false, true,  "Bard","Cleric","Druid","Wizard");
            S(lvl3, "Fireball",                3, Evo, A,  Range, 150, VSM, Inst,  false, false, "Sorcerer","Wizard");
            S(lvl3, "Fly",                     3, Tra, A,  Touch, 0,   VSM, D10m,  true,  false, "Sorcerer","Warlock","Wizard");
            S(lvl3, "Gaseous Form",            3, Tra, A,  Touch, 0,   VSM, D1h,   true,  false, "Sorcerer","Warlock","Wizard");
            S(lvl3, "Glyph of Warding",        3, Abj, HR, Touch, 0,   VSM, Perm,  false, false, "Bard","Cleric","Wizard");
            S(lvl3, "Haste",                   3, Tra, A,  Range, 30,  VSM, D1m,   true,  false, "Sorcerer","Wizard");
            S(lvl3, "Hunger of Hadar",         3, Con, A,  Range, 150, VSM, D1m,   true,  false, "Warlock");
            S(lvl3, "Hypnotic Pattern",        3, Ill, A,  Range, 120, SM,  D1m,   true,  false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl3, "Lightning Bolt",          3, Evo, A,  Self,  0,   VSM, Inst,  false, false, "Sorcerer","Wizard");
            S(lvl3, "Magic Circle",            3, Abj, Min, Range, 10, VSM, D1h,   false, false, "Cleric","Paladin","Warlock","Wizard");
            S(lvl3, "Mass Healing Word",       3, Evo, BA, Range, 60,  V,   Inst,  false, false, "Bard","Cleric");
            S(lvl3, "Meld into Stone",         3, Tra, A,  Touch, 0,   VS,  D8h,   false, true,  "Cleric","Druid");
            S(lvl3, "Nondetection",            3, Div, A,  Touch, 0,   VSM, D8h,   false, false, "Bard","Ranger","Wizard");
            S(lvl3, "Phantom Steed",           3, Ill, Min, Self, 0,   VS,  D1h,   false, true,  "Wizard");
            S(lvl3, "Plant Growth",            3, Tra, A,  Range, 150, VS,  Inst,  false, false, "Bard","Druid","Ranger");
            S(lvl3, "Protection from Energy",  3, Abj, A,  Touch, 0,   VS,  D1h,   true,  false, "Cleric","Druid","Ranger","Sorcerer","Wizard");
            S(lvl3, "Remove Curse",            3, Abj, A,  Touch, 0,   VS,  Inst,  false, false, "Cleric","Paladin","Warlock","Wizard");
            S(lvl3, "Revivify",                3, Nec, A,  Touch, 0,   VSM, Inst,  false, false, "Cleric","Paladin","Ranger");
            S(lvl3, "Sending",                 3, Evo, A,  Spec,  0,   VSM, R1,    false, false, "Bard","Cleric","Wizard");
            S(lvl3, "Sleet Storm",             3, Con, A,  Range, 150, VSM, D1m,   true,  false, "Druid","Sorcerer","Wizard");
            S(lvl3, "Slow",                    3, Tra, A,  Range, 120, VSM, D1m,   true,  false, "Sorcerer","Wizard");
            S(lvl3, "Speak with Dead",         3, Nec, A,  Range, 10,  VSM, D10m,  false, false, "Bard","Cleric","Wizard");
            S(lvl3, "Speak with Plants",       3, Tra, A,  Self,  0,   VS,  D10m,  false, false, "Bard","Druid","Ranger");
            S(lvl3, "Spirit Guardians",        3, Con, A,  Self,  0,   VSM, D10m,  true,  false, "Cleric");
            S(lvl3, "Stinking Cloud",          3, Con, A,  Range, 90,  VSM, D1m,   true,  false, "Bard","Sorcerer","Wizard");
            S(lvl3, "Tiny Hut",                3, Evo, Min, Self, 0,   VSM, D8h,   false, true,  "Bard","Wizard");
            S(lvl3, "Tongues",                 3, Div, A,  Touch, 0,   VM,  D1h,   false, false, "Bard","Cleric","Sorcerer","Warlock","Wizard");
            S(lvl3, "Vampiric Touch",          3, Nec, A,  Self,  0,   VS,  D1m,   true,  false, "Warlock","Wizard");
            S(lvl3, "Water Breathing",         3, Tra, A,  Range, 30,  VSM, D24h,  false, true,  "Druid","Ranger","Sorcerer","Wizard");
            S(lvl3, "Water Walk",              3, Tra, A,  Range, 30,  VSM, D1h,   false, true,  "Cleric","Druid","Ranger","Sorcerer");
            S(lvl3, "Wind Wall",               3, Evo, A,  Range, 120, VSM, D1m,   true,  false, "Druid","Ranger");

            // ── 4th Level ─────────────────────────────────────────────────────
            string lvl4 = SpellRoot + "/4";
            S(lvl4, "Arcane Eye",              4, Div, A,  Range, 30,  VSM, D1h,   true,  false, "Wizard");
            S(lvl4, "Aura of Life",            4, Abj, A,  Self,  0,   V,   D10m,  true,  false, "Paladin");
            S(lvl4, "Aura of Purity",          4, Abj, A,  Self,  0,   V,   D10m,  true,  false, "Paladin");
            S(lvl4, "Banishment",              4, Abj, A,  Range, 60,  VSM, D1m,   true,  false, "Cleric","Paladin","Sorcerer","Warlock","Wizard");
            S(lvl4, "Blight",                  4, Nec, A,  Range, 30,  VS,  Inst,  false, false, "Druid","Sorcerer","Warlock","Wizard");
            S(lvl4, "Compulsion",              4, Enc, A,  Range, 30,  VS,  D1m,   true,  false, "Bard");
            S(lvl4, "Confusion",               4, Enc, A,  Range, 90,  VSM, D1m,   true,  false, "Bard","Druid","Sorcerer","Wizard");
            S(lvl4, "Conjure Minor Elementals",4, Con, Min, Range, 90, VS,  D1h,   true,  false, "Druid","Wizard");
            S(lvl4, "Conjure Woodland Beings", 4, Con, A,  Range, 60,  VSM, D1h,   true,  false, "Druid","Ranger");
            S(lvl4, "Control Water",           4, Tra, A,  Range, 300, VSM, D10m,  true,  false, "Cleric","Druid","Wizard");
            S(lvl4, "Death Ward",              4, Abj, A,  Touch, 0,   VS,  D8h,   false, false, "Cleric","Paladin");
            S(lvl4, "Dimension Door",          4, Con, A,  Range, 500, V,   Inst,  false, false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl4, "Divination",              4, Div, A,  Self,  0,   VSM, Inst,  false, true,  "Cleric","Druid","Wizard");
            S(lvl4, "Dominate Beast",          4, Enc, A,  Range, 60,  VS,  D1m,   true,  false, "Druid","Ranger","Sorcerer");
            S(lvl4, "Fabricate",               4, Tra, TM, Range, 120, VS,  Inst,  false, false, "Wizard");
            S(lvl4, "Fire Shield",             4, Evo, A,  Self,  0,   VSM, D10m,  false, false, "Wizard");
            S(lvl4, "Freedom of Movement",     4, Abj, A,  Touch, 0,   VSM, D1h,   false, false, "Bard","Cleric","Druid","Ranger");
            S(lvl4, "Giant Insect",            4, Tra, A,  Range, 30,  VS,  D10m,  true,  false, "Druid");
            S(lvl4, "Grasping Vine",           4, Con, BA, Range, 30,  VS,  D1m,   true,  false, "Druid","Ranger");
            S(lvl4, "Greater Invisibility",    4, Ill, A,  Touch, 0,   VS,  D1m,   true,  false, "Bard","Sorcerer","Wizard");
            S(lvl4, "Guardian of Faith",       4, Con, A,  Range, 30,  V,   D8h,   false, false, "Cleric");
            S(lvl4, "Hallucinatory Terrain",   4, Ill, TM, Range, 300, VSM, D24h,  false, false, "Bard","Druid","Warlock","Wizard");
            S(lvl4, "Ice Storm",               4, Evo, A,  Range, 300, VSM, Inst,  false, false, "Druid","Sorcerer","Wizard");
            S(lvl4, "Leomund's Secret Chest",  4, Con, A,  Touch, 0,   VSM, Inst,  false, false, "Wizard");
            S(lvl4, "Locate Creature",         4, Div, A,  Self,  0,   VSM, D1h,   true,  false, "Bard","Cleric","Druid","Paladin","Ranger","Wizard");
            S(lvl4, "Mordenkainen's Faithful Hound", 4, Con, A, Range, 30, VSM, D8h, false, false, "Wizard");
            S(lvl4, "Mordenkainen's Private Sanctum", 4, Abj, TM, Range, 120, VSM, D24h, false, false, "Wizard");
            S(lvl4, "Otiluke's Resilient Sphere", 4, Evo, A, Range, 30, VSM, D1m, true, false, "Wizard");
            S(lvl4, "Phantasmal Killer",       4, Ill, A,  Range, 120, VS,  D1m,   true,  false, "Wizard");
            S(lvl4, "Polymorph",               4, Tra, A,  Range, 60,  VSM, D1h,   true,  false, "Bard","Druid","Sorcerer","Wizard");
            S(lvl4, "Staggering Smite",        4, Evo, BA, Self,  0,   V,   D1m,   true,  false, "Paladin");
            S(lvl4, "Stone Shape",             4, Tra, A,  Touch, 0,   VSM, Inst,  false, false, "Cleric","Druid","Wizard");
            S(lvl4, "Stoneskin",               4, Abj, A,  Touch, 0,   VSM, D1h,   true,  false, "Druid","Ranger","Sorcerer","Wizard");
            S(lvl4, "Wall of Fire",            4, Evo, A,  Range, 120, VSM, D1m,   true,  false, "Druid","Sorcerer","Wizard");

            // ── 5th Level ─────────────────────────────────────────────────────
            string lvl5 = SpellRoot + "/5";
            S(lvl5, "Antilife Shell",          5, Abj, A,  Self,  0,   VS,  D1h,   true,  false, "Druid");
            S(lvl5, "Arcane Hand",             5, Evo, A,  Range, 120, VSM, D1m,   true,  false, "Wizard");
            S(lvl5, "Awaken",                  5, Tra, H8, Touch, 0,   VSM, Inst,  false, false, "Bard","Druid");
            S(lvl5, "Banishing Smite",         5, Abj, BA, Self,  0,   V,   D1m,   true,  false, "Paladin");
            S(lvl5, "Circle of Power",         5, Abj, A,  Self,  0,   VS,  D10m,  true,  false, "Paladin");
            S(lvl5, "Cloudkill",               5, Con, A,  Range, 120, VS,  D10m,  true,  false, "Sorcerer","Wizard");
            S(lvl5, "Commune",                 5, Div, Min, Self, 0,   VSM, D1m,   false, true,  "Cleric");
            S(lvl5, "Commune with Nature",     5, Div, Min, Self, 0,   VS,  Inst,  false, true,  "Druid","Ranger");
            S(lvl5, "Conjure Elemental",       5, Con, Min, Range, 90, VSM, D1h,   true,  false, "Druid","Wizard");
            S(lvl5, "Conjure Volley",          5, Con, A,  Range, 150, VSM, Inst,  false, false, "Ranger");
            S(lvl5, "Contact Other Plane",     5, Div, Min, Self, 0,   V,   D1m,   false, true,  "Warlock","Wizard");
            S(lvl5, "Contagion",               5, Nec, A,  Touch, 0,   VS,  D7d,   false, false, "Cleric","Druid");
            S(lvl5, "Creation",                5, Ill, Min, Range, 30, VSM, SpecD, false, false, "Sorcerer","Wizard");
            S(lvl5, "Destructive Wave",        5, Evo, A,  Self,  0,   V,   Inst,  false, false, "Paladin");
            S(lvl5, "Dispel Evil and Good",    5, Abj, A,  Self,  0,   VSM, D1m,   true,  false, "Cleric","Paladin");
            S(lvl5, "Dominate Person",         5, Enc, A,  Range, 60,  VS,  D1m,   true,  false, "Bard","Sorcerer","Wizard");
            S(lvl5, "Dream",                   5, Ill, Min, Spec,  0,   VSM, D8h,   false, false, "Bard","Warlock","Wizard");
            S(lvl5, "Flame Strike",            5, Evo, A,  Range, 60,  VSM, Inst,  false, false, "Cleric");
            S(lvl5, "Geas",                    5, Enc, Min, Range, 60, V,   D30d,  false, false, "Bard","Cleric","Druid","Paladin","Wizard");
            S(lvl5, "Greater Restoration",     5, Abj, A,  Touch, 0,   VSM, Inst,  false, false, "Bard","Cleric","Druid");
            S(lvl5, "Hold Monster",            5, Enc, A,  Range, 90,  VSM, D1m,   true,  false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl5, "Insect Plague",           5, Con, A,  Range, 300, VSM, D10m,  true,  false, "Cleric","Druid","Sorcerer");
            S(lvl5, "Legend Lore",             5, Div, TM, Self,  0,   VSM, Inst,  false, false, "Bard","Cleric","Wizard");
            S(lvl5, "Mass Cure Wounds",        5, Evo, A,  Range, 60,  VS,  Inst,  false, false, "Bard","Cleric","Druid");
            S(lvl5, "Mislead",                 5, Ill, A,  Self,  0,   So,  D1h,   true,  false, "Bard","Wizard");
            S(lvl5, "Modify Memory",           5, Enc, A,  Range, 30,  VS,  D1m,   true,  false, "Bard","Wizard");
            S(lvl5, "Passwall",                5, Tra, A,  Range, 30,  VSM, D1h,   false, false, "Wizard");
            S(lvl5, "Planar Binding",          5, Abj, HR, Range, 60,  VSM, D24h,  false, false, "Bard","Cleric","Druid","Wizard");
            S(lvl5, "Raise Dead",              5, Nec, HR, Touch, 0,   VSM, Inst,  false, false, "Bard","Cleric","Paladin");
            S(lvl5, "Rary's Telepathic Bond",  5, Div, A,  Range, 30,  VSM, D1h,   false, true,  "Wizard");
            S(lvl5, "Reincarnate",             5, Tra, HR, Touch, 0,   VSM, Inst,  false, false, "Druid");
            S(lvl5, "Scrying",                 5, Div, TM, Self,  0,   VSM, D10m,  true,  false, "Bard","Cleric","Druid","Warlock","Wizard");
            S(lvl5, "Seeming",                 5, Ill, A,  Range, 30,  VS,  D8h,   false, false, "Bard","Sorcerer","Wizard");
            S(lvl5, "Swift Quiver",            5, Tra, BA, Touch, 0,   VSM, D1m,   true,  false, "Ranger");
            S(lvl5, "Telekinesis",             5, Tra, A,  Range, 60,  VS,  D10m,  true,  false, "Sorcerer","Wizard");
            S(lvl5, "Teleportation Circle",    5, Con, Min, Range, 10, VSM, R1,    false, false, "Bard","Sorcerer","Wizard");
            S(lvl5, "Tree Stride",             5, Con, A,  Self,  0,   VS,  D1m,   true,  false, "Druid","Ranger");
            S(lvl5, "Wall of Force",           5, Evo, A,  Range, 120, VSM, D10m,  true,  false, "Wizard");
            S(lvl5, "Wall of Stone",           5, Evo, A,  Range, 120, VSM, D10m,  true,  false, "Druid","Sorcerer","Wizard");
            S(lvl5, "Wrath of Nature",         5, Evo, A,  Range, 120, VS,  D1m,   true,  false, "Druid","Ranger");

            // ── 6th Level ─────────────────────────────────────────────────────
            string lvl6 = SpellRoot + "/6";
            S(lvl6, "Arcane Gate",             6, Con, A,  Range, 500, VS,  D10m,  true,  false, "Sorcerer","Warlock","Wizard");
            S(lvl6, "Blade Barrier",           6, Evo, A,  Range, 90,  VS,  D10m,  true,  false, "Cleric");
            S(lvl6, "Chain Lightning",         6, Evo, A,  Range, 150, VSM, Inst,  false, false, "Sorcerer","Wizard");
            S(lvl6, "Circle of Death",         6, Nec, A,  Range, 150, VSM, Inst,  false, false, "Sorcerer","Warlock","Wizard");
            S(lvl6, "Conjure Fey",             6, Con, Min, Range, 90, VS,  D1h,   true,  false, "Druid","Warlock");
            S(lvl6, "Contingency",             6, Evo, TM, Self,  0,   VSM, D10d,  false, false, "Wizard");
            S(lvl6, "Create Undead",           6, Nec, Min, Range, 10, VSM, Inst,  false, false, "Cleric","Warlock","Wizard");
            S(lvl6, "Disintegrate",            6, Tra, A,  Range, 60,  VSM, Inst,  false, false, "Sorcerer","Wizard");
            S(lvl6, "Drawmij's Instant Summons",6,Con, Min, Touch, 0,  VSM, Perm,  false, true,  "Wizard");
            S(lvl6, "Eyebite",                 6, Nec, A,  Self,  0,   VS,  D1m,   true,  false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl6, "Find the Path",           6, Div, Min, Self, 0,   VSM, D24h,  true,  false, "Bard","Cleric","Druid");
            S(lvl6, "Flesh to Stone",          6, Tra, A,  Range, 60,  VSM, D10m,  true,  false, "Warlock","Wizard");
            S(lvl6, "Forbiddance",             6, Abj, TM, Touch, 0,   VSM, D30d,  false, true,  "Cleric");
            S(lvl6, "Globe of Invulnerability",6, Abj, A,  Self,  0,   VSM, D1m,   true,  false, "Sorcerer","Wizard");
            S(lvl6, "Guards and Wards",        6, Abj, TM, Touch, 0,   VSM, D24h,  false, false, "Bard","Wizard");
            S(lvl6, "Harm",                    6, Nec, A,  Range, 60,  VS,  Inst,  false, false, "Cleric");
            S(lvl6, "Heal",                    6, Evo, A,  Range, 60,  VS,  Inst,  false, false, "Cleric","Druid");
            S(lvl6, "Heroes' Feast",           6, Con, TM, Range, 30,  VSM, Inst,  false, false, "Cleric","Druid");
            S(lvl6, "Investiture of Flame",    6, Tra, A,  Self,  0,   VS,  D10m,  true,  false, "Druid","Sorcerer","Warlock","Wizard");
            S(lvl6, "Investiture of Ice",      6, Tra, A,  Self,  0,   VS,  D10m,  true,  false, "Druid","Sorcerer","Warlock","Wizard");
            S(lvl6, "Investiture of Stone",    6, Tra, A,  Self,  0,   VS,  D10m,  true,  false, "Druid","Sorcerer","Warlock","Wizard");
            S(lvl6, "Investiture of Wind",     6, Tra, A,  Self,  0,   VS,  D10m,  true,  false, "Druid","Sorcerer","Warlock","Wizard");
            S(lvl6, "Mass Suggestion",         6, Enc, A,  Range, 60,  VM,  D24h,  false, false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl6, "Move Earth",              6, Tra, A,  Range, 120, VSM, D2h,   true,  false, "Druid","Sorcerer","Wizard");
            S(lvl6, "Otiluke's Freezing Sphere",6,Evo, A,  Range, 300, VSM, Inst,  false, false, "Wizard");
            S(lvl6, "Otto's Irresistible Dance",6,Enc, A,  Range, 30,  V,   D1m,   true,  false, "Bard","Wizard");
            S(lvl6, "Planar Ally",             6, Con, TM, Self,  0,   VS,  Inst,  false, false, "Cleric");
            S(lvl6, "Programmed Illusion",     6, Ill, A,  Range, 120, VSM, Perm,  false, false, "Bard","Wizard");
            S(lvl6, "Sunbeam",                 6, Evo, A,  Self,  0,   VSM, D1m,   true,  false, "Cleric","Druid","Sorcerer","Wizard");
            S(lvl6, "Transport via Plants",    6, Con, A,  Touch, 0,   VS,  R1,    false, false, "Druid");
            S(lvl6, "True Seeing",             6, Div, A,  Touch, 0,   VSM, D1h,   false, false, "Bard","Cleric","Sorcerer","Warlock","Wizard");
            S(lvl6, "Wall of Ice",             6, Evo, A,  Range, 120, VSM, D10m,  true,  false, "Wizard");
            S(lvl6, "Wall of Thorns",          6, Con, A,  Range, 120, VSM, D10m,  true,  false, "Druid");
            S(lvl6, "Wind Walk",               6, Tra, Min, Touch, 0,  VSM, D8h,   false, false, "Druid");
            S(lvl6, "Word of Recall",          6, Con, A,  Range, 5,   V,   Inst,  false, false, "Cleric");

            // ── 7th Level ─────────────────────────────────────────────────────
            string lvl7 = SpellRoot + "/7";
            S(lvl7, "Conjure Celestial",       7, Con, Min, Range, 90, VS,  D1h,   true,  false, "Cleric");
            S(lvl7, "Delayed Blast Fireball",  7, Evo, A,  Range, 150, VSM, D1m,   true,  false, "Sorcerer","Wizard");
            S(lvl7, "Divine Word",             7, Evo, BA, Range, 30,  V,   Inst,  false, false, "Cleric");
            S(lvl7, "Etherealness",            7, Tra, A,  Self,  0,   VS,  D8h,   false, false, "Bard","Cleric","Sorcerer","Warlock","Wizard");
            S(lvl7, "Finger of Death",         7, Nec, A,  Range, 60,  VS,  Inst,  false, false, "Sorcerer","Warlock","Wizard");
            S(lvl7, "Fire Storm",              7, Evo, A,  Range, 150, VS,  Inst,  false, false, "Cleric","Druid","Sorcerer");
            S(lvl7, "Forcecage",               7, Evo, A,  Range, 100, VSM, D1h,   false, false, "Bard","Warlock","Wizard");
            S(lvl7, "Mirage Arcane",           7, Ill, TM, Self,  0,   VS,  D10d,  false, false, "Bard","Druid","Wizard");
            S(lvl7, "Mordenkainen's Magnificent Mansion", 7, Con, Min, Range, 300, VSM, D24h, false, false, "Bard","Wizard");
            S(lvl7, "Mordenkainen's Sword",    7, Evo, A,  Range, 60,  VSM, D1m,   true,  false, "Bard","Wizard");
            S(lvl7, "Plane Shift",             7, Con, A,  Touch, 0,   VSM, Inst,  false, false, "Cleric","Druid","Sorcerer","Warlock","Wizard");
            S(lvl7, "Prismatic Spray",         7, Evo, A,  Self,  0,   VS,  Inst,  false, false, "Sorcerer","Wizard");
            S(lvl7, "Project Image",           7, Ill, A,  Range, 500, VSM, D24h,  true,  false, "Bard","Wizard");
            S(lvl7, "Regenerate",              7, Tra, Min, Touch, 0,  VSM, D1h,   false, false, "Bard","Cleric","Druid");
            S(lvl7, "Resurrection",            7, Nec, HR, Touch, 0,   VSM, Inst,  false, false, "Bard","Cleric");
            S(lvl7, "Reverse Gravity",         7, Tra, A,  Range, 100, VSM, D1m,   true,  false, "Druid","Sorcerer","Wizard");
            S(lvl7, "Sequester",               7, Tra, A,  Touch, 0,   VSM, Perm,  false, false, "Wizard");
            S(lvl7, "Simulacrum",              7, Ill, H8, Touch, 0,   VSM, Perm,  false, false, "Wizard");
            S(lvl7, "Symbol",                  7, Abj, Min, Touch, 0,  VSM, PermT, false, false, "Bard","Cleric","Wizard");
            S(lvl7, "Teleport",                7, Con, A,  Self,  0,   V,   Inst,  false, false, "Bard","Sorcerer","Wizard");

            // ── 8th Level ─────────────────────────────────────────────────────
            string lvl8 = SpellRoot + "/8";
            S(lvl8, "Antimagic Field",         8, Abj, A,  Self,  0,   VSM, D1h,   true,  false, "Cleric","Wizard");
            S(lvl8, "Antipathy Sympathy",      8, Enc, H8, Range, 60,  VSM, D10d,  false, false, "Bard","Druid","Wizard");
            S(lvl8, "Clone",                   8, Nec, HR, Touch, 0,   VSM, Inst,  false, false, "Wizard");
            S(lvl8, "Control Weather",         8, Tra, TM, Self,  0,   VSM, D8h,   true,  false, "Cleric","Druid","Wizard");
            S(lvl8, "Demiplane",               8, Con, A,  Range, 60,  So,  D1h,   false, false, "Warlock","Wizard");
            S(lvl8, "Dominate Monster",        8, Enc, A,  Range, 60,  VS,  D1h,   true,  false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl8, "Earthquake",              8, Evo, A,  Range, 500, VSM, D1m,   true,  false, "Cleric","Druid","Sorcerer");
            S(lvl8, "Feeblemind",              8, Enc, A,  Range, 150, VSM, Inst,  false, false, "Bard","Druid","Warlock","Wizard");
            S(lvl8, "Glibness",                8, Tra, A,  Self,  0,   V,   D1h,   false, false, "Bard","Warlock");
            S(lvl8, "Holy Aura",               8, Abj, A,  Self,  0,   VSM, D1m,   true,  false, "Cleric");
            S(lvl8, "Incendiary Cloud",        8, Con, A,  Range, 150, VS,  D1m,   true,  false, "Sorcerer","Wizard");
            S(lvl8, "Maze",                    8, Con, A,  Range, 60,  VS,  D10m,  true,  false, "Wizard");
            S(lvl8, "Mind Blank",              8, Abj, A,  Touch, 0,   VS,  D24h,  false, false, "Bard","Wizard");
            S(lvl8, "Power Word Stun",         8, Enc, A,  Range, 60,  V,   SpecD, false, false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl8, "Sunburst",                8, Evo, A,  Range, 150, VSM, Inst,  false, false, "Cleric","Druid","Sorcerer","Wizard");
            S(lvl8, "Tsunami",                 8, Con, Min, Spec,  0,  VS,  D6R,   true,  false, "Druid");

            // ── 9th Level ─────────────────────────────────────────────────────
            string lvl9 = SpellRoot + "/9";
            S(lvl9, "Astral Projection",       9, Nec, HR, Range, 10,  VSM, SpecD, false, false, "Cleric","Warlock","Wizard");
            S(lvl9, "Foresight",               9, Div, Min, Touch, 0,  VSM, D8h,   false, false, "Bard","Druid","Warlock","Wizard");
            S(lvl9, "Gate",                    9, Con, A,  Range, 60,  VSM, D1m,   true,  false, "Cleric","Sorcerer","Warlock","Wizard");
            S(lvl9, "Imprisonment",            9, Abj, Min, Range, 30, VSM, Perm,  false, false, "Warlock","Wizard");
            S(lvl9, "Mass Heal",               9, Evo, A,  Range, 60,  VS,  Inst,  false, false, "Cleric");
            S(lvl9, "Meteor Swarm",            9, Evo, A,  Range, 1000,VS,  Inst,  false, false, "Sorcerer","Wizard");
            S(lvl9, "Power Word Kill",         9, Enc, A,  Range, 60,  V,   Inst,  false, false, "Bard","Sorcerer","Warlock","Wizard");
            S(lvl9, "Prismatic Wall",          9, Abj, A,  Range, 60,  VS,  D10m,  false, false, "Wizard");
            S(lvl9, "Shapechange",             9, Tra, A,  Self,  0,   VSM, D1h,   true,  false, "Druid","Wizard");
            S(lvl9, "Storm of Vengeance",      9, Con, A,  Spec,  0,   VS,  D1m,   true,  false, "Druid");
            S(lvl9, "Time Stop",               9, Tra, A,  Self,  0,   V,   SpecD, false, false, "Sorcerer","Wizard");
            S(lvl9, "True Polymorph",          9, Tra, A,  Range, 30,  VSM, D1h,   true,  false, "Bard","Warlock","Wizard");
            S(lvl9, "True Resurrection",       9, Nec, HR, Touch, 0,   VSM, Inst,  false, false, "Cleric","Druid");
            S(lvl9, "Weird",                   9, Ill, A,  Range, 120, VS,  D1m,   true,  false, "Warlock","Wizard");
            S(lvl9, "Wish",                    9, Con, A,  Self,  0,   V,   Inst,  false, false, "Sorcerer","Wizard");
        }

        // ── Helper ────────────────────────────────────────────────────────────

        // Duration constants for unusual durations not in the enum
        // (used as comments only — mapped to Spec or the closest value)
        private const SpellDurationType D10d = SpellDurationType.Special;  // 10 days
        private const SpellDurationType D30d = SpellDurationType.Special;  // 30 days
        private const SpellDurationType D2h  = SpellDurationType.Special;  // 2 hours (approx 1h)
        private const SpellDurationType D6R  = SpellDurationType.Special;  // 6 rounds
        private const SpellDurationType D30R = SpellDurationType.Special;  // 30 days (Forbiddance)

        private static void S(
            string folder, string name,
            int level, SpellSchool school,
            CastingTimeType castTime, SpellRangeType rangeType, int rangeFt,
            SpellComponent components,
            SpellDurationType duration, bool concentration, bool ritual,
            params string[] classNames)
        {
            var spell = PHBAssetGenerator.GetOrCreate<SpellSO>(folder, name);
            spell.Level         = level;
            spell.School        = school;
            spell.CastingTime   = castTime;
            spell.RangeType     = rangeType;
            spell.RangeDistance = rangeFt;
            spell.Components    = components;
            spell.Duration      = duration;
            spell.IsConcentration = concentration;
            spell.IsRitual      = ritual;

            // Wire class lists from already-created class assets.
            string classFolder = PHBAssetGenerator.DataRoot + "/Classes";
            var classList = new List<ClassSO>();
            foreach (var cn in classNames)
            {
                var cls = PHBAssetGenerator.Load<ClassSO>(classFolder, cn);
                if (cls != null) classList.Add(cls);
            }
            spell.ClassLists = classList;

            EditorUtility.SetDirty(spell);
        }
    }
}
