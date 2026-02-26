using NUnit.Framework;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Systems;
using DeeDeeR.DnD.Core.Values;

namespace DeeDeeR.DnD.Tests.Editor.Systems
{
    [TestFixture]
    public class AbilitySystemTests
    {
        // Helper: build an AbilitySystem with a FakeRollProvider preloaded with the given results.
        private static AbilitySystem Make(params int[] dieResults)
        {
            var roller = new DiceRoller(new FakeRollProvider(dieResults));
            return new AbilitySystem(roller);
        }

        // ── RollCheck — Normal ────────────────────────────────────────────────

        [Test]
        public void RollCheck_Normal_ReturnsDiePlusModifier()
        {
            // die=10, modifier=3 → total=13
            var result = Make(10).RollCheck(totalModifier: 3, AdvantageState.Normal);
            Assert.AreEqual(13, result.Total);
        }

        [Test]
        public void RollCheck_Normal_ConsumesOnlyOneDie()
        {
            // If a second die were consumed this would throw (FakeRollProvider exhausted).
            Assert.DoesNotThrow(() => Make(8).RollCheck(0, AdvantageState.Normal));
        }

        [Test]
        public void RollCheck_Normal_NegativeModifier_SubtractsFromTotal()
        {
            // die=10, modifier=-3 → total=7
            var result = Make(10).RollCheck(totalModifier: -3, AdvantageState.Normal);
            Assert.AreEqual(7, result.Total);
        }

        // ── RollCheck — Advantage ─────────────────────────────────────────────

        [Test]
        public void RollCheck_Advantage_FirstHigher_ReturnsFirst()
        {
            var result = Make(15, 8).RollCheck(0, AdvantageState.Advantage);
            Assert.AreEqual(15, result.Total);
        }

        [Test]
        public void RollCheck_Advantage_SecondHigher_ReturnsSecond()
        {
            var result = Make(8, 15).RollCheck(0, AdvantageState.Advantage);
            Assert.AreEqual(15, result.Total);
        }

        [Test]
        public void RollCheck_Advantage_Natural20OnFirstDie_PreservesCritFlag()
        {
            // First=20 (crit), second=8 → advantage picks first.
            var result = Make(20, 8).RollCheck(0, AdvantageState.Advantage);
            Assert.IsTrue(result.IsCriticalSuccess);
        }

        [Test]
        public void RollCheck_Advantage_Natural20OnSecondDie_PreservesCritFlag()
        {
            // First=8, second=20 (crit) → advantage picks second.
            var result = Make(8, 20).RollCheck(0, AdvantageState.Advantage);
            Assert.IsTrue(result.IsCriticalSuccess);
        }

        // ── RollCheck — Disadvantage ──────────────────────────────────────────

        [Test]
        public void RollCheck_Disadvantage_FirstLower_ReturnsFirst()
        {
            var result = Make(5, 14).RollCheck(0, AdvantageState.Disadvantage);
            Assert.AreEqual(5, result.Total);
        }

        [Test]
        public void RollCheck_Disadvantage_SecondLower_ReturnsSecond()
        {
            var result = Make(14, 5).RollCheck(0, AdvantageState.Disadvantage);
            Assert.AreEqual(5, result.Total);
        }

        [Test]
        public void RollCheck_Disadvantage_Natural20OnHigherDie_DiscardedNoCritFlag()
        {
            // First=20 (crit), second=8 → disadvantage picks second (8 < 20), no crit.
            var result = Make(20, 8).RollCheck(0, AdvantageState.Disadvantage);
            Assert.IsFalse(result.IsCriticalSuccess);
            Assert.AreEqual(8, result.Total);
        }

        [Test]
        public void RollCheck_Disadvantage_BothNatural20_CritFlagSet()
        {
            // Both=20 → the result is a natural 20 regardless of which is picked.
            var result = Make(20, 20).RollCheck(0, AdvantageState.Disadvantage);
            Assert.IsTrue(result.IsCriticalSuccess);
        }

        // ── PassiveCheck ──────────────────────────────────────────────────────

        [Test]
        public void PassiveCheck_ReturnsTenPlusModifier()
        {
            Assert.AreEqual(15, AbilitySystem.PassiveCheck(5));
        }

        [Test]
        public void PassiveCheck_ZeroModifier_ReturnsTen()
        {
            Assert.AreEqual(10, AbilitySystem.PassiveCheck(0));
        }

        [Test]
        public void PassiveCheck_NegativeModifier_BelowTen()
        {
            Assert.AreEqual(8, AbilitySystem.PassiveCheck(-2));
        }

        // ── Modifier composition (documents expected call-site patterns) ──────

        [Test]
        public void RollCheck_RawAbilityCheck_ModifierComputedFromScores()
        {
            // Pattern: raw ability check = ability modifier − exhaustion penalty.
            // STR=16 → mod=+3; Exhaustion 1 → penalty=2; die=10 → total=10+3-2=11
            var scores    = new AbilityScoreSet(str: 16, dex: 10, con: 10, intel: 10, wis: 10, cha: 10);
            var exhaustion = new ExhaustionLevel(1);
            int modifier  = scores.GetModifier(AbilityType.Strength) - exhaustion.D20Penalty;

            var result = Make(10).RollCheck(modifier, AdvantageState.Normal);
            Assert.AreEqual(11, result.Total);
        }

        [Test]
        public void RollCheck_SavingThrow_ModifierIncludesProficiency()
        {
            // Pattern: saving throw = ability modifier + proficiency bonus (if proficient) − exhaustion.
            // STR=14 → mod=+2; proficient, profBonus=3; no exhaustion; die=10 → total=15
            var scores   = new AbilityScoreSet(str: 14, dex: 10, con: 10, intel: 10, wis: 10, cha: 10);
            bool isProficient  = true;
            int  profBonus     = 3;
            int  modifier      = scores.GetModifier(AbilityType.Strength)
                                 + (isProficient ? profBonus : 0)
                                 - ExhaustionLevel.None.D20Penalty;

            var result = Make(10).RollCheck(modifier, AdvantageState.Normal);
            Assert.AreEqual(15, result.Total);
        }
    }
}