using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.DnD.Core.Interfaces;
using DeeDeeR.DnD.Runtime.Components;
using DeeDeeR.DnD.Runtime.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace DeeDeeR.DnD.Samples.BasicCombat
{
    /// <summary>
    /// Simple action menu for the player's turn.
    /// Shows an <b>Attack</b> button and an <b>End Turn</b> button.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="BasicCombatManager"/> calls <see cref="BeginPlayerTurn"/> at the start of the
    /// player's turn and then waits on <see cref="TurnComplete"/> via <c>WaitUntil</c>.
    /// </para>
    /// <para>
    /// <b>Action economy:</b> The Attack button is disabled once <c>ActionUsed</c> is set.
    /// Setting <c>ActionUsed = true</c> is this menu's responsibility — the SDK only reads the
    /// flag inside <see cref="CombatantComponent.StartTurn"/> to reset it.
    /// </para>
    /// </remarks>
    public sealed class PlayerActionMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button     _attackButton;
        [SerializeField] private Button     _endTurnButton;

        private CombatantComponent _combatant;
        private CharacterComponent _character;
        private CharacterComponent _target;

        private readonly IRollProvider _roller = new UnityRollProvider();

        /// <summary>
        /// <c>true</c> once the player has pressed End Turn (or their action is spent and
        /// they clicked End Turn). Polled by <see cref="BasicCombatManager"/> via
        /// <c>WaitUntil</c>.
        /// </summary>
        public bool TurnComplete { get; private set; }

        private void Awake()
        {
            _attackButton.onClick.AddListener(OnAttack);
            _endTurnButton.onClick.AddListener(OnEndTurn);
            _panel.SetActive(false);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Called by <see cref="BasicCombatManager"/> to activate the panel for the player's turn.
        /// </summary>
        public void BeginPlayerTurn(
            CombatantComponent combatant,
            CharacterComponent character,
            CharacterComponent target)
        {
            _combatant   = combatant;
            _character   = character;
            _target      = target;
            TurnComplete = false;
            _panel.SetActive(true);
            RefreshButtons();
        }

        // ── Button handlers ───────────────────────────────────────────────────

        private void OnAttack()
        {
            if (_character.State.ActionUsed)
                return;

            var weapon = _character.Inventory.EquippedMainHand;
            if (weapon == null)
            {
                Debug.Log("[BasicCombat] No weapon equipped — cannot attack.");
                return;
            }

            // PerformAttack publishes AttackMade / DamageDealt / HitPointsChanged via the bus.
            _combatant.PerformAttack(_target, weapon, AdvantageState.Normal, _roller);

            // Game is responsible for marking the action as used.
            _character.State.ActionUsed = true;
            RefreshButtons();
        }

        private void OnEndTurn()
        {
            _panel.SetActive(false);
            TurnComplete = true;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void RefreshButtons()
        {
            _attackButton.interactable = !_character.State.ActionUsed;
        }
    }
}
