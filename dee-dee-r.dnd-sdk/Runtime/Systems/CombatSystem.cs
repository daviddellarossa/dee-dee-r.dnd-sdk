using System;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;
using DeeDeeR.DnD.Core.Values;
using DeeDeeR.DnD.Runtime.Data;
using DeeDeeR.DnD.Runtime.State;

namespace DeeDeeR.DnD.Runtime.Systems
{
    /// <summary>
    /// Resolves attack rolls, damage rolls, and armor class for combat per D&amp;D 2024 PHB rules.
    /// Stateless — create one instance and reuse it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This system resolves the mechanics of single attacks. Action economy (Extra Attack,
    /// Bonus Action attacks) is enforced by <c>CombatantComponent</c> (Phase 11).
    /// </para>
    /// <para>
    /// <b>Resistance and immunity:</b> <see cref="RollDamage"/> returns raw dice + ability modifier.
    /// The caller is responsible for applying resistance/immunity before passing the final amount
    /// to <see cref="HitPointSystem.ApplyDamage"/>.
    /// </para>
    /// <para>
    /// <b>Mastery effects:</b> Mastery consequences (Push, Sap, Topple, etc.) are not applied here.
    /// Check <see cref="WeaponMasterySystem.CanUseMastery"/> and
    /// <see cref="WeaponMasterySystem.GetMasteryEffect"/> after resolving the attack.
    /// </para>
    /// </remarks>
    public sealed class CombatSystem
    {
        // ── Attack Bonus ──────────────────────────────────────────────────────

        /// <summary>
        /// Returns the total modifier added to an attack roll with the given weapon:
        /// ability modifier + proficiency bonus (if proficient) − exhaustion penalty.
        /// </summary>
        /// <remarks>
        /// Ability selection rules (in priority order):
        /// <list type="bullet">
        ///   <item><description>Finesse: uses the higher of STR or DEX modifier.</description></item>
        ///   <item><description>Range property: always uses DEX modifier.</description></item>
        ///   <item><description>All other weapons: use STR modifier.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="record">The attacker's identity and ability scores.</param>
        /// <param name="state">The attacker's mutable state (provides exhaustion level).</param>
        /// <param name="weapon">The weapon being used.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public int GetAttackBonus(CharacterRecord record, CharacterState state, WeaponSO weapon)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (state  == null) throw new ArgumentNullException(nameof(state));
            if (weapon == null) throw new ArgumentNullException(nameof(weapon));

            var  ability    = ResolveAttackAbility(record, weapon);
            int  abilityMod = record.AbilityScores.GetModifier(ability);
            bool proficient = record.WeaponCategoryProficiencies.Contains(weapon.Category);
            int  profBonus  = proficient ? ProficiencyBonusForRecord(record) : 0;

            return abilityMod + profBonus - state.Exhaustion.D20Penalty;
        }

        // ── Attack Roll ───────────────────────────────────────────────────────

        /// <summary>
        /// Rolls an attack with the given weapon against a target AC and returns the result.
        /// </summary>
        /// <remarks>
        /// Natural 20 always hits; natural 1 always misses — regardless of modifiers or AC.
        /// For advantage/disadvantage, two d20s are rolled and the higher/lower is used;
        /// the crit flags on the returned result reflect the chosen die.
        /// </remarks>
        /// <param name="attackerRecord">The attacker's identity and ability scores.</param>
        /// <param name="attackerState">The attacker's mutable state (exhaustion, etc.).</param>
        /// <param name="targetAC">The target's armor class.</param>
        /// <param name="weapon">The weapon used in the attack.</param>
        /// <param name="advantageState">Whether to roll with advantage, disadvantage, or normally.</param>
        /// <param name="roller">Source of random die values.</param>
        /// <returns>
        /// An <see cref="AttackRollResult"/> with the roll total, hit determination, and crit flags.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="attackerRecord"/>, <paramref name="attackerState"/>,
        /// <paramref name="weapon"/>, or <paramref name="roller"/> is null.
        /// </exception>
        public AttackRollResult RollAttack(
            CharacterRecord attackerRecord,
            CharacterState  attackerState,
            int             targetAC,
            WeaponSO        weapon,
            AdvantageState  advantageState,
            IRollProvider   roller)
        {
            if (attackerRecord == null) throw new ArgumentNullException(nameof(attackerRecord));
            if (attackerState  == null) throw new ArgumentNullException(nameof(attackerState));
            if (weapon         == null) throw new ArgumentNullException(nameof(weapon));
            if (roller         == null) throw new ArgumentNullException(nameof(roller));

            int bonus = GetAttackBonus(attackerRecord, attackerState, weapon);

            RollResult chosen = Roll20WithBonus(roller, bonus, advantageState);
            return new AttackRollResult(chosen, chosen.Total >= targetAC);
        }

        // ── Damage ────────────────────────────────────────────────────────────

        /// <summary>
        /// Rolls weapon damage dice and adds the attack ability modifier.
        /// On a critical hit, the number of dice is doubled (2024 PHB: roll each die twice);
        /// the flat expression modifier and ability modifier are each added only once.
        /// The result is clamped to a minimum of 0.
        /// </summary>
        /// <param name="weapon">The weapon whose damage expression is rolled.</param>
        /// <param name="attackerRecord">The attacker's ability scores (for ability modifier).</param>
        /// <param name="isCritical">Whether the attack was a critical hit.</param>
        /// <param name="roller">Source of random die values.</param>
        /// <returns>Total damage (minimum 0).</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="weapon"/>, <paramref name="attackerRecord"/>, or
        /// <paramref name="roller"/> is null.
        /// </exception>
        public int RollDamage(
            WeaponSO        weapon,
            CharacterRecord attackerRecord,
            bool            isCritical,
            IRollProvider   roller)
        {
            if (weapon         == null) throw new ArgumentNullException(nameof(weapon));
            if (attackerRecord == null) throw new ArgumentNullException(nameof(attackerRecord));
            if (roller         == null) throw new ArgumentNullException(nameof(roller));

            DiceExpression expr  = weapon.DamageExpression;
            int            faces = (int)expr.Die;
            int            count = isCritical ? expr.Count * 2 : expr.Count;

            int diceTotal = 0;
            for (int i = 0; i < count; i++)
                diceTotal += roller.RollDie(faces);

            var ability    = ResolveAttackAbility(attackerRecord, weapon);
            int abilityMod = attackerRecord.AbilityScores.GetModifier(ability);

            return Math.Max(0, diceTotal + expr.Modifier + abilityMod);
        }

        // ── Armor Class ───────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the character's Armor Class from their equipped armor, shield, and
        /// Dexterity modifier per D&amp;D 2024 PHB rules.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        ///   <listheader><term>Armor</term><description>AC formula</description></listheader>
        ///   <item><term>None (unarmored)</term><description>10 + DEX modifier</description></item>
        ///   <item><term>Light</term><description>Base AC + full DEX modifier (<see cref="ArmorSO.MaxDexBonus"/> = −1)</description></item>
        ///   <item><term>Medium</term><description>Base AC + min(DEX modifier, <see cref="ArmorSO.MaxDexBonus"/>)</description></item>
        ///   <item><term>Heavy</term><description>Base AC only (<see cref="ArmorSO.MaxDexBonus"/> = 0)</description></item>
        /// </list>
        /// A <see cref="ShieldSO"/> in the off hand adds its <see cref="ShieldSO.AcBonus"/> only
        /// when the character has <see cref="CharacterRecord.HasShieldProficiency"/>.
        /// </remarks>
        /// <param name="record">The character's build data (shield proficiency check).</param>
        /// <param name="state">
        /// The character's session state. Reserved for future temporary AC modifiers; currently unused.
        /// </param>
        /// <param name="inventory">The character's equipped items.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public int CalculateArmorClass(
            CharacterRecord record,
            CharacterState  state,
            InventoryState  inventory)
        {
            if (record    == null) throw new ArgumentNullException(nameof(record));
            if (state     == null) throw new ArgumentNullException(nameof(state));
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));

            int dexMod = record.AbilityScores.GetModifier(AbilityType.Dexterity);
            int ac;

            ArmorSO armor = inventory.EquippedArmor;
            if (armor == null)
            {
                ac = 10 + dexMod;
            }
            else
            {
                int effectiveDex = armor.MaxDexBonus < 0
                    ? dexMod
                    : Math.Min(dexMod, armor.MaxDexBonus);
                ac = armor.BaseArmorClass + effectiveDex;
            }

            if (inventory.EquippedOffHandShield != null && record.HasShieldProficiency)
                ac += inventory.EquippedOffHandShield.AcBonus;

            return ac;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static AbilityType ResolveAttackAbility(CharacterRecord record, WeaponSO weapon)
        {
            bool isFinesse = HasProperty(weapon, WeaponProperty.Finesse);
            bool isRanged  = HasProperty(weapon, WeaponProperty.Range);

            if (isFinesse)
            {
                return record.AbilityScores.GetModifier(AbilityType.Strength) >=
                       record.AbilityScores.GetModifier(AbilityType.Dexterity)
                    ? AbilityType.Strength
                    : AbilityType.Dexterity;
            }

            return isRanged ? AbilityType.Dexterity : AbilityType.Strength;
        }

        private static bool HasProperty(WeaponSO weapon, WeaponProperty property)
            => weapon.Properties != null && Array.IndexOf(weapon.Properties, property) >= 0;

        private static int ProficiencyBonusForRecord(CharacterRecord record)
        {
            int totalLevel = 0;
            foreach (var cl in record.ClassLevels)
                if (cl != null) totalLevel += cl.Level;

            return (Math.Max(1, totalLevel) - 1) / 4 + 2;
        }

        private static RollResult Roll20WithBonus(IRollProvider roller, int bonus, AdvantageState advantage)
        {
            int  roll1          = roller.RollDie(20);
            bool isCritSuccess1 = roll1 == 20;
            bool isCritFail1    = roll1 == 1;

            if (advantage == AdvantageState.Normal)
                return new RollResult(roll1 + bonus, isCritSuccess1, isCritFail1);

            int  roll2          = roller.RollDie(20);
            bool isCritSuccess2 = roll2 == 20;
            bool isCritFail2    = roll2 == 1;

            bool useFirst = advantage == AdvantageState.Advantage
                ? (roll1 + bonus) >= (roll2 + bonus)  // totals equal when bonus is same; prefer first
                : (roll1 + bonus) <= (roll2 + bonus);

            return useFirst
                ? new RollResult(roll1 + bonus, isCritSuccess1, isCritFail1)
                : new RollResult(roll2 + bonus, isCritSuccess2, isCritFail2);
        }
    }
}