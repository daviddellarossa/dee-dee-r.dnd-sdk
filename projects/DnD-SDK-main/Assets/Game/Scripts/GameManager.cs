using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DeeDeeR.DnD.Runtime.State;
using DeeDeeR.DnD.Game.SaveSystem;

namespace DeeDeeR.DnD.Game
{
    /// <summary>Tracks the current high-level game state.</summary>
    public enum GameState
    {
        MainMenu,
        CharacterCreation,
        LoadGame,
        InGame,
        InGameMenu
    }

    /// <summary>
    /// Central manager for game-level flow: scene transitions, active character data, and save/load.
    /// Lives on the Persistent scene's root (marked DontDestroyOnLoad by <c>GameBootstrap</c>).
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        private const string GameSceneName = "Game";

        [SerializeField] private GameDataRegistry _registry;
        [SerializeField] private GameSaveManager  _saveManager;

        // ── State ─────────────────────────────────────────────────────────────

        public GameState      State          { get; private set; } = GameState.MainMenu;

        // ── Registry / save manager (read by menu controllers) ────────────────
        public GameDataRegistry Registry    => _registry;
        public GameSaveManager  SaveManager => _saveManager;
        public CharacterRecord  ActiveRecord   { get; private set; }
        public CharacterState   ActiveState    { get; private set; }
        public InventoryState   ActiveInventory{ get; private set; }
        public SpellbookState   ActiveSpellbook{ get; private set; }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Starts a new game with a freshly created character.
        /// Loads Game.unity additively and transitions state to <see cref="GameState.InGame"/>.
        /// </summary>
        public void StartNewGame(CharacterRecord record, CharacterState state)
        {
            ActiveRecord    = record;
            ActiveState     = state;
            ActiveInventory = new InventoryState();
            ActiveSpellbook = new SpellbookState();

            StartCoroutine(LoadGameScene());
        }

        /// <summary>
        /// Loads a saved game from the given slot, rebuilds all character data, and enters play.
        /// </summary>
        public void LoadGame(int slotIndex)
        {
            var saveData = _saveManager.Load(slotIndex);
            if (saveData == null)
            {
                Debug.LogWarning($"[GameManager] No save data found for slot {slotIndex}.");
                return;
            }

            var (record, state, inventory, spellbook) = _saveManager.Reconstruct(saveData);
            ActiveRecord    = record;
            ActiveState     = state;
            ActiveInventory = inventory;
            ActiveSpellbook = spellbook;

            StartCoroutine(LoadGameScene());
        }

        /// <summary>Saves the current game to the specified slot without leaving play.</summary>
        public void SaveCurrentGame(int slotIndex)
        {
            if (ActiveRecord == null)
            {
                Debug.LogWarning("[GameManager] No active character to save.");
                return;
            }
            _saveManager.Save(slotIndex, this);
            Debug.Log($"[GameManager] Game saved to slot {slotIndex}.");
        }

        /// <summary>Saves the current game then returns to the main menu.</summary>
        public void SaveAndQuit(int slotIndex)
        {
            SaveCurrentGame(slotIndex);
            StartCoroutine(UnloadGameScene());
        }

        /// <summary>Returns to the main menu without saving.</summary>
        public void QuitWithoutSaving()
        {
            StartCoroutine(UnloadGameScene());
        }

        /// <summary>
        /// Transitions the state machine to a new state.
        /// Menu controllers call this to synchronise the UI.
        /// </summary>
        public void SetState(GameState newState)
        {
            State = newState;
        }

        // ── Private ───────────────────────────────────────────────────────────

        private IEnumerator LoadGameScene()
        {
            // Avoid loading the scene twice if it is already present.
            if (SceneManager.GetSceneByName(GameSceneName).isLoaded)
            {
                State = GameState.InGame;
                yield break;
            }

            var op = SceneManager.LoadSceneAsync(GameSceneName, LoadSceneMode.Additive);
            yield return op;

            State = GameState.InGame;
            Debug.Log($"[GameManager] Scene '{GameSceneName}' loaded. State = InGame.");
        }

        private IEnumerator UnloadGameScene()
        {
            var scene = SceneManager.GetSceneByName(GameSceneName);
            if (scene.isLoaded)
            {
                var op = SceneManager.UnloadSceneAsync(scene);
                yield return op;
            }

            ActiveRecord    = null;
            ActiveState     = null;
            ActiveInventory = null;
            ActiveSpellbook = null;
            State = GameState.MainMenu;

            Debug.Log("[GameManager] Returned to main menu.");
        }
    }
}
