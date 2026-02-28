using System;
using NUnit.Framework;
using UnityEngine;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.State;
using DeeDeeR.DnD.Runtime.Systems;

namespace DeeDeeR.DnD.Tests.Editor.Systems
{
    [TestFixture]
    public class CombatSystemTests
    {
        private CombatSystem _system;

        // Weapons
        private WeaponSO _sword;     // melee, STR, Simple, 1d8
        private WeaponSO _dagger;    // melee, Finesse, Simple, 1d4
        private WeaponSO _shortbow;  // ranged, DEX, Simple, 1d6

        // Armor
        private ArmorSO _leatherArmor; // Light  — base 11, MaxDexBonus −1 (no cap)
        private ArmorSO _chainShirt;   // Medium — base 13, MaxDexBonus  2
        private ArmorSO _chainMail;    // Heavy  — base 16, MaxDexBonus  0

        // Shield
        private ShieldSO _shield; // +2 AC

        [SetUp]
        public void SetUp()
        {
            _system = new CombatSystem();

            _sword = ScriptableObject.CreateInstance<WeaponSO>();
            _sword.Category   = WeaponCategory.Simple;
            _sword.Properties = new WeaponProperty[0];
            _sword.DamageDice = new DiceExpressionData { Count = 1, Die = DieType.D8 };

            _dagger = ScriptableObject.CreateInstance<WeaponSO>();
            _dagger.Category   = WeaponCategory.Simple;
            _dagger.Properties = new[] { WeaponProperty.Finesse };
            _dagger.DamageDice = new DiceExpressionData { Count = 1, Die = DieType.D4 };

            _shortbow = ScriptableObject.CreateInstance<WeaponSO>();
            _shortbow.Category   = WeaponCategory.Simple;
            _shortbow.Properties = new[] { WeaponProperty.Range };
            _shortbow.DamageDice = new DiceExpressionData { Count = 1, Die = DieType.D6 };

            _leatherArmor = ScriptableObject.CreateInstance<ArmorSO>();
            _leatherArmor.Category       = ArmorCategory.Light;
            _leatherArmor.BaseArmorClass = 11;
            _leatherArmor.MaxDexBonus    = -1; // no cap

            _chainShirt = ScriptableObject.CreateInstance<ArmorSO>();
            _chainShirt.Category       = ArmorCategory.Medium;
            _chainShirt.BaseArmorClass = 13;
            _chainShirt.MaxDexBonus    = 2;

            _chainMail = ScriptableObject.CreateInstance<ArmorSO>();
            _chainMail.Category       = ArmorCategory.Heavy;
            _chainMail.BaseArmorClass = 16;
            _chainMail.MaxDexBonus    = 0;

            _shield = ScriptableObject.CreateInstance<ShieldSO>();
            _shield.AcBonus = 2;
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_sword);
            UnityEngine.Object.DestroyImmediate(_dagger);
            UnityEngine.Object.DestroyImmediate(_shortbow);
            UnityEngine.Object.DestroyImmediate(_leatherArmor);
            UnityEngine.Object.DestroyImmediate(_chainShirt);
            UnityEngine.Object.DestroyImmediate(_chainMail);
            UnityEngine.Object.DestroyImmediate(_shield);
        }

        // ── GetAttackBonus ────────────────────────────────────────────────────

        [Test]
        public void GetAttackBonus_NullRecord_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.GetAttackBonus(null, new CharacterState(), _sword));
        }

        [Test]
        public void GetAttackBonus_NullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.GetAttackBonus(MakeRecord(str: 14), null, _sword));
        }

        [Test]
        public void GetAttackBonus_NullWeapon_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _system.GetAttackBonus(MakeRecord(str: 14), new CharacterState(), null));
        }

        [Test]
        public void GetAttackBonus_StrWeaponNoProficiency_ReturnsStrMod()
        {
            // STR 14 → mod +2; not proficient → +2.
            var bonus = _system.GetAttackBonus(MakeRecord(str: 14), new CharacterState(), _sword);
            Assert.AreEqual(2, bonus);
        }

        [Test]
        public void GetAttackBonus_ProficientWithWeapon_AddsProfBonus()
        {
            // STR 14 (+2), Level 1 → profBonus +2; total = +4.
            var record = MakeRecord(str: 14);
            record.WeaponCategoryProficiencies.Add(WeaponCategory.Simple);
            record.ClassLevels.Add(new ClassLevel { Level = 1 });
            var bonus = _system.GetAttackBonus(record, new CharacterState(), _sword);
            Assert.AreEqual(4, bonus);
        }

        [Test]
        public void GetAttackBonus_FinesseWeapon_UsesHigherModifier()
        {
            // STR 10 (0), DEX 16 (+3); Finesse → uses DEX → +3.
            var bonus = _system.GetAttackBonus(MakeRecord(str: 10, dex: 16), new CharacterState(), _dagger);
            Assert.AreEqual(3, bonus);
        }

        [Test]
        public void GetAttackBonus_FinesseWeapon_StrHigher_UsesStr()
        {
            // STR 16 (+3), DEX 10 (0); Finesse → uses STR → +3.
            var bonus = _system.GetAttackBonus(MakeRecord(str: 16, dex: 10), new CharacterState(), _dagger);
            Assert.AreEqual(3, bonus);
        }

        [Test]
        public void GetAttackBonus_RangedWeapon_UsesDex()
        {
            // STR 14 (+2), DEX 16 (+3); Range → always DEX → +3.
            var bonus = _system.GetAttackBonus(MakeRecord(str: 14, dex: 16), new CharacterState(), _shortbow);
            Assert.AreEqual(3, bonus);
        }

        [Test]
        public void GetAttackBonus_SubtractsExhaustionPenalty()
        {
            // STR 10 (0), no prof, Exhaustion 1 (penalty 2) → 0 − 2 = −2.
            var state = new CharacterState { Exhaustion = new ExhaustionLevel(1) };
            var bonus = _system.GetAttackBonus(MakeRecord(str: 10), state, _sword);
            Assert.AreEqual(-2, bonus);
        }

        // ── RollAttack ────────────────────────────────────────────────────────

        [Test]
        public void RollAttack_NullAttackerRecord_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _system.RollAttack(null, new CharacterState(), 10, _sword,
                    AdvantageState.Normal, new FakeRollProvider(10)));
        }

        [Test]
        public void RollAttack_NullAttackerState_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _system.RollAttack(MakeRecord(), null, 10, _sword,
                    AdvantageState.Normal, new FakeRollProvider(10)));
        }

        [Test]
        public void RollAttack_NullWeapon_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _system.RollAttack(MakeRecord(), new CharacterState(), 10, null,
                    AdvantageState.Normal, new FakeRollProvider(10)));
        }

        [Test]
        public void RollAttack_NullRoller_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _system.RollAttack(MakeRecord(), new CharacterState(), 10, _sword,
                    AdvantageState.Normal, null));
        }

        [Test]
        public void RollAttack_TotalMeetsAC_IsHit()
        {
            // Roll 8 + STR 0 = 8; AC 8 → hit.
            var result = _system.RollAttack(MakeRecord(str: 10), new CharacterState(), 8, _sword,
                AdvantageState.Normal, new FakeRollProvider(8));
            Assert.IsTrue(result.Hit);
        }

        [Test]
        public void RollAttack_TotalBelowAC_IsMiss()
        {
            // Roll 7 + STR 0 = 7; AC 8 → miss.
            var result = _system.RollAttack(MakeRecord(str: 10), new CharacterState(), 8, _sword,
                AdvantageState.Normal, new FakeRollProvider(7));
            Assert.IsFalse(result.Hit);
        }

        [Test]
        public void RollAttack_NaturalTwenty_AlwaysHits()
        {
            // Natural 20 always hits even against AC 100.
            var result = _system.RollAttack(MakeRecord(str: 10), new CharacterState(), 100, _sword,
                AdvantageState.Normal, new FakeRollProvider(20));
            Assert.IsTrue(result.Hit);
            Assert.IsTrue(result.Critical);
        }

        [Test]
        public void RollAttack_NaturalOne_AlwaysMisses()
        {
            // Natural 1 always misses even against AC 1 with a huge STR bonus.
            var result = _system.RollAttack(MakeRecord(str: 30), new CharacterState(), 1, _sword,
                AdvantageState.Normal, new FakeRollProvider(1));
            Assert.IsFalse(result.Hit);
            Assert.IsTrue(result.Fumble);
        }

        [Test]
        public void RollAttack_Advantage_TakesHigherRoll()
        {
            // Rolls 5 and 15; advantage → 15; STR 0 → total 15; AC 14 → hit.
            var result = _system.RollAttack(MakeRecord(str: 10), new CharacterState(), 14, _sword,
                AdvantageState.Advantage, new FakeRollProvider(5, 15));
            Assert.IsTrue(result.Hit);
            Assert.AreEqual(15, result.Roll.Total);
        }

        [Test]
        public void RollAttack_Disadvantage_TakesLowerRoll()
        {
            // Rolls 15 and 5; disadvantage → 5; STR 0 → total 5; AC 14 → miss.
            var result = _system.RollAttack(MakeRecord(str: 10), new CharacterState(), 14, _sword,
                AdvantageState.Disadvantage, new FakeRollProvider(15, 5));
            Assert.IsFalse(result.Hit);
            Assert.AreEqual(5, result.Roll.Total);
        }

        // ── RollDamage ────────────────────────────────────────────────────────

        [Test]
        public void RollDamage_NullWeapon_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _system.RollDamage(null, MakeRecord(), false, new FakeRollProvider(4)));
        }

        [Test]
        public void RollDamage_NullRecord_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _system.RollDamage(_sword, null, false, new FakeRollProvider(4)));
        }

        [Test]
        public void RollDamage_NullRoller_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _system.RollDamage(_sword, MakeRecord(), false, null));
        }

        [Test]
        public void RollDamage_NormalHit_RollsOneDie()
        {
            // 1d8, roll 6, STR 10 (mod 0) → total 6.
            var dmg = _system.RollDamage(_sword, MakeRecord(str: 10), false, new FakeRollProvider(6));
            Assert.AreEqual(6, dmg);
        }

        [Test]
        public void RollDamage_CriticalHit_DoublesDiceCount()
        {
            // 1d8 crit → 2d8; rolls 4 and 4, STR 10 (0) → total 8.
            var dmg = _system.RollDamage(_sword, MakeRecord(str: 10), true, new FakeRollProvider(4, 4));
            Assert.AreEqual(8, dmg);
        }

        [Test]
        public void RollDamage_IncludesAbilityModifier()
        {
            // 1d8, roll 5, STR 16 (+3) → 5 + 3 = 8.
            var dmg = _system.RollDamage(_sword, MakeRecord(str: 16), false, new FakeRollProvider(5));
            Assert.AreEqual(8, dmg);
        }

        [Test]
        public void RollDamage_NegativeTotalClamped_ReturnsZero()
        {
            // 1d4, roll 1, DEX 4 (mod −3); Finesse → uses max(STR, DEX) = max(-3, -3) = -3
            // total = 1 + 0 (expr mod) − 3 = −2 → clamped to 0.
            var dmg = _system.RollDamage(_dagger, MakeRecord(str: 4, dex: 4), false, new FakeRollProvider(1));
            Assert.AreEqual(0, dmg);
        }

        // ── CalculateArmorClass ───────────────────────────────────────────────

        [Test]
        public void CalculateArmorClass_NullRecord_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _system.CalculateArmorClass(null, new CharacterState(), new InventoryState()));
        }

        [Test]
        public void CalculateArmorClass_NullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _system.CalculateArmorClass(MakeRecord(), null, new InventoryState()));
        }

        [Test]
        public void CalculateArmorClass_NullInventory_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _system.CalculateArmorClass(MakeRecord(), new CharacterState(), null));
        }

        [Test]
        public void CalculateArmorClass_NoArmor_TenPlusDex()
        {
            // DEX 14 → mod +2; no armor → AC = 12.
            var ac = _system.CalculateArmorClass(MakeRecord(dex: 14), new CharacterState(), new InventoryState());
            Assert.AreEqual(12, ac);
        }

        [Test]
        public void CalculateArmorClass_LightArmor_AddsFullDex()
        {
            // Leather base 11, DEX 18 (+4), no cap (MaxDexBonus −1) → AC = 15.
            var inv = new InventoryState { EquippedArmor = _leatherArmor };
            var ac  = _system.CalculateArmorClass(MakeRecord(dex: 18), new CharacterState(), inv);
            Assert.AreEqual(15, ac);
        }

        [Test]
        public void CalculateArmorClass_MediumArmor_CapsDexAtMaxDexBonus()
        {
            // Chain shirt base 13, MaxDexBonus 2, DEX 18 (+4) → capped to +2 → AC = 15.
            var inv = new InventoryState { EquippedArmor = _chainShirt };
            var ac  = _system.CalculateArmorClass(MakeRecord(dex: 18), new CharacterState(), inv);
            Assert.AreEqual(15, ac);
        }

        [Test]
        public void CalculateArmorClass_HeavyArmor_NoDexBonus()
        {
            // Chain mail base 16, MaxDexBonus 0, DEX 18 (+4) → no DEX added → AC = 16.
            var inv = new InventoryState { EquippedArmor = _chainMail };
            var ac  = _system.CalculateArmorClass(MakeRecord(dex: 18), new CharacterState(), inv);
            Assert.AreEqual(16, ac);
        }

        [Test]
        public void CalculateArmorClass_ShieldProficient_AddsShieldBonus()
        {
            // No armor (DEX 10 → AC 10), shield +2, proficient → AC = 12.
            var record = MakeRecord(dex: 10);
            record.HasShieldProficiency = true;
            var inv = new InventoryState { EquippedOffHandShield = _shield };
            var ac  = _system.CalculateArmorClass(record, new CharacterState(), inv);
            Assert.AreEqual(12, ac);
        }

        [Test]
        public void CalculateArmorClass_ShieldNotProficient_NoShieldBonus()
        {
            // No armor (DEX 10 → AC 10), shield present, NOT proficient → AC = 10.
            var record = MakeRecord(dex: 10);
            record.HasShieldProficiency = false;
            var inv = new InventoryState { EquippedOffHandShield = _shield };
            var ac  = _system.CalculateArmorClass(record, new CharacterState(), inv);
            Assert.AreEqual(10, ac);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static CharacterRecord MakeRecord(int str = 10, int dex = 10) => new CharacterRecord
        {
            AbilityScores = new AbilityScoreSet(str, dex, 10, 10, 10, 10)
        };
    }
}