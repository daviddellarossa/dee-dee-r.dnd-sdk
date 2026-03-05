using System.Collections;
using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;
using DeeDeeR.DnD.Runtime.Bus.Args;
using DeeDeeR.DnD.Runtime.Components;
using DeeDeeR.DnD.Runtime.Systems;
using UnityEngine;

namespace DeeDeeR.DnD.Samples.BasicCombat
{
    /// <summary>
    /// Drives a two-character turn-based combat encounter using the DnD SDK.
    /// Handles initiative, turn order, player UI hand-off, and a one-attack AI for the enemy.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>SDK contract reminders (enforced here, not by the SDK):</b>
    /// <list type="bullet">
    ///   <item><see cref="CharacterComponent.ApplyDamage"/> does not set the Unconscious condition
    ///     or publish <see cref="CombatBusCategory.CharacterDied"/> when HP reaches 0 — this
    ///     manager does both.</item>
    ///   <item><c>ActionUsed</c> / <c>BonusActionUsed</c> / <c>ReactionUsed</c> are read here
    ///     and in <see cref="PlayerActionMenu"/> to enforce the action economy; the SDK only
    ///     resets them in <see cref="CombatantComponent.StartTurn"/>.</item>
    /// </list>
    /// </para>
    /// </remarks>
    public sealed class BasicCombatManager : MonoBehaviour
    {
        [Header("Combatants")]
        [SerializeField] private CharacterComponent _playerCharacter;
        [SerializeField] private CombatantComponent _playerCombatant;
        [SerializeField] private CharacterComponent _enemyCharacter;
        [SerializeField] private CombatantComponent _enemyCombatant;

        [Header("UI")]
        [SerializeField] private PlayerActionMenu _actionMenu;

        private readonly InitiativeSystem _initiative = new InitiativeSystem();
        private readonly IRollProvider    _roller     = new UnityRollProvider();

        private void Start() => StartCombat();

        /// <summary>Starts the combat coroutine. Can be called again after combat ends.</summary>
        public void StartCombat() => StartCoroutine(CombatLoop());

        // ── Combat loop ───────────────────────────────────────────────────────

        private IEnumerator CombatLoop()
        {
            Debug.Log("[BasicCombat] ═══ Combat started! ═══");

            // Roll initiative for both combatants.
            int playerInit = _initiative.RollInitiative(
                _playerCharacter.Record, _playerCharacter.State, _roller);
            int enemyInit = _initiative.RollInitiative(
                _enemyCharacter.Record, _enemyCharacter.State, _roller);

            Debug.Log($"[BasicCombat] Initiative — " +
                      $"{_playerCharacter.Record.Name}: {playerInit}, " +
                      $"{_enemyCharacter.Record.Name}: {enemyInit}");

            // Ties go to the player.
            bool playerFirst = playerInit >= enemyInit;

            var (first,  firstChar)  = playerFirst
                ? (_playerCombatant, _playerCharacter)
                : (_enemyCombatant,  _enemyCharacter);
            var (second, secondChar) = playerFirst
                ? (_enemyCombatant,  _enemyCharacter)
                : (_playerCombatant, _playerCharacter);

            int round = 0;
            while (IsAlive(_playerCharacter) && IsAlive(_enemyCharacter))
            {
                round++;
                Debug.Log($"[BasicCombat] ── Round {round} ──");

                yield return ExecuteTurn(first, firstChar);
                if (!IsAlive(_playerCharacter) || !IsAlive(_enemyCharacter)) break;

                yield return ExecuteTurn(second, secondChar);
            }

            HandleCombatEnd();
        }

        private IEnumerator ExecuteTurn(CombatantComponent combatant, CharacterComponent self)
        {
            if (!IsAlive(self)) yield break;

            var target = GetOpponent(self);
            combatant.StartTurn();

            if (combatant == _playerCombatant)
            {
                _actionMenu.BeginPlayerTurn(_playerCombatant, _playerCharacter, _enemyCharacter);
                yield return new WaitUntil(() => _actionMenu.TurnComplete);
            }
            else
            {
                // Simple AI: attack the player with the main-hand weapon (if any).
                var weapon = self.Inventory.EquippedMainHand;
                if (weapon != null)
                    combatant.PerformAttack(target, weapon, AdvantageState.Normal, _roller);
                else
                    Debug.Log($"[BasicCombat] {self.Record.Name} has no weapon — skips attack.");

                yield return null; // pause one frame so signals propagate before EndTurn
            }

            combatant.EndTurn();
        }

        private void HandleCombatEnd()
        {
            // Identify winner and loser.
            bool playerWon = IsAlive(_playerCharacter);
            var loser  = playerWon ? _enemyCharacter  : _playerCharacter;
            var winner = playerWon ? _playerCharacter : _enemyCharacter;

            // SDK contract: game must publish CharacterDied when HP reaches 0.
            DnDSdkRunner.Bus?.Combat.CharacterDied.PublishBroadcast(
                new CharacterDiedArgs(loser.EndpointId));

            Debug.Log($"[BasicCombat] ═══ Combat over! {winner.Record.Name} wins! ═══");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private CharacterComponent GetOpponent(CharacterComponent ch)
            => ch == _playerCharacter ? _enemyCharacter : _playerCharacter;

        private static bool IsAlive(CharacterComponent ch)
            => ch != null && ch.State.HitPoints.Current > 0;
    }
}
