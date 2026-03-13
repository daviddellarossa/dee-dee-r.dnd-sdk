using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using DeeDeeR.DnD.Game;

namespace DeeDeeR.DnD.Game.Menu
{
    /// <summary>
    /// Owns the root <see cref="UIDocument"/> and routes input to the correct menu panel
    /// based on the current <see cref="GameState"/>.
    /// Assigns each panel's <see cref="VisualTreeAsset"/> in the Inspector and instantiates
    /// them into the document at startup.
    /// Subscribes to the <c>Player/Menu</c> action to toggle the in-game menu overlay.
    /// </summary>
    public sealed class MenuManager : MonoBehaviour
    {
        [SerializeField] private UIDocument   _document;
        [SerializeField] private GameManager  _gameManager;
        [SerializeField] private PlayerInput  _playerInput;

        [Header("Panel UXML assets — assign in Inspector")]
        [SerializeField] private VisualTreeAsset _mainMenuAsset;
        [SerializeField] private VisualTreeAsset _charCreationAsset;
        [SerializeField] private VisualTreeAsset _loadGameAsset;
        [SerializeField] private VisualTreeAsset _inGameMenuAsset;

        // ── Panel root elements ───────────────────────────────────────────────

        private VisualElement _mainMenuRoot;
        private VisualElement _charCreationRoot;
        private VisualElement _loadGameRoot;
        private VisualElement _inGameMenuRoot;

        // ── Controllers ───────────────────────────────────────────────────────

        private MainMenuController          _mainMenu;
        private CharacterCreationController _charCreation;
        private LoadGameController          _loadGame;
        private InGameMenuController        _inGameMenu;

        // ── Input ─────────────────────────────────────────────────────────────

        private InputAction _menuAction;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void OnEnable()
        {
            if (_playerInput != null)
            {
                _menuAction = _playerInput.actions["Menu"];
                if (_menuAction != null)
                    _menuAction.performed += OnMenuActionPerformed;
            }
        }

        private void OnDisable()
        {
            if (_menuAction != null)
                _menuAction.performed -= OnMenuActionPerformed;
        }

        private void Start()
        {
            var docRoot = _document.rootVisualElement;

            // Instantiate each panel template and add to the shared document root.
            _mainMenuRoot     = InstantiatePanel(_mainMenuAsset,     "MainMenuRoot",     docRoot);
            _charCreationRoot = InstantiatePanel(_charCreationAsset, "CharCreationRoot", docRoot);
            _loadGameRoot     = InstantiatePanel(_loadGameAsset,     "LoadGameRoot",     docRoot);
            _inGameMenuRoot   = InstantiatePanel(_inGameMenuAsset,   "InGameMenuRoot",   docRoot);

            // Wire controllers.
            _mainMenu     = new MainMenuController(_mainMenuRoot,     _gameManager, this);
            _charCreation = new CharacterCreationController(_charCreationRoot, _gameManager, this);
            _loadGame     = new LoadGameController(_loadGameRoot,     _gameManager, this);
            _inGameMenu   = new InGameMenuController(_inGameMenuRoot, _gameManager, this);

            // Start in correct state.
            ShowPanelForState(_gameManager.State);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Hides all panels then shows the one matching <paramref name="state"/>.
        /// </summary>
        public void ShowPanelForState(GameState state)
        {
            HideAll();
            switch (state)
            {
                case GameState.MainMenu:
                    Show(_mainMenuRoot);
                    break;
                case GameState.CharacterCreation:
                    Show(_charCreationRoot);
                    _charCreation?.OnShow();
                    break;
                case GameState.LoadGame:
                    Show(_loadGameRoot);
                    _loadGame?.OnShow();
                    break;
                case GameState.InGameMenu:
                    Show(_inGameMenuRoot);
                    DisablePlayerInput();
                    break;
                case GameState.InGame:
                    EnablePlayerInput();
                    break;
            }
        }

        /// <summary>Closes the in-game menu and returns to <c>InGame</c> state.</summary>
        public void CloseMenu()
        {
            _gameManager.SetState(GameState.InGame);
            ShowPanelForState(GameState.InGame);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private VisualElement InstantiatePanel(VisualTreeAsset asset, string rootName, VisualElement parent)
        {
            if (asset == null)
            {
                Debug.LogWarning($"[MenuManager] VisualTreeAsset for '{rootName}' is not assigned.");
                return null;
            }

            var instance = asset.Instantiate();
            // The UXML template wraps everything in a TemplateContainer;
            // grab the named root if present, otherwise use the container itself.
            var named = instance.Q<VisualElement>(rootName);
            var root  = named ?? instance;
            root.AddToClassList("hidden");
            parent.Add(root);
            return root;
        }

        private void HideAll()
        {
            Hide(_mainMenuRoot);
            Hide(_charCreationRoot);
            Hide(_loadGameRoot);
            Hide(_inGameMenuRoot);
        }

        private static void Hide(VisualElement el) => el?.AddToClassList("hidden");
        private static void Show(VisualElement el) => el?.RemoveFromClassList("hidden");

        private void OnMenuActionPerformed(InputAction.CallbackContext ctx)
        {
            if (_gameManager.State == GameState.InGame)
            {
                _gameManager.SetState(GameState.InGameMenu);
                ShowPanelForState(GameState.InGameMenu);
            }
            else if (_gameManager.State == GameState.InGameMenu)
            {
                CloseMenu();
            }
        }

        private void DisablePlayerInput() => _playerInput?.SwitchCurrentActionMap("UI");
        private void EnablePlayerInput()  => _playerInput?.SwitchCurrentActionMap("Player");
    }
}
