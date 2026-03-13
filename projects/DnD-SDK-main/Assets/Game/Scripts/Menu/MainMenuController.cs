using UnityEngine.UIElements;
using DeeDeeR.DnD.Game;

namespace DeeDeeR.DnD.Game.Menu
{
    /// <summary>
    /// Wires the main menu buttons to <see cref="GameManager"/> state transitions.
    /// </summary>
    public sealed class MainMenuController
    {
        private readonly GameManager  _gm;
        private readonly MenuManager  _menu;

        public MainMenuController(VisualElement root, GameManager gm, MenuManager menu)
        {
            _gm   = gm;
            _menu = menu;

            if (root == null) return;

            root.Q<Button>("btn-new-game")?.RegisterCallback<ClickEvent>(_ =>
            {
                _gm.SetState(GameState.CharacterCreation);
                _menu.ShowPanelForState(GameState.CharacterCreation);
            });

            root.Q<Button>("btn-load-game")?.RegisterCallback<ClickEvent>(_ =>
            {
                _gm.SetState(GameState.LoadGame);
                _menu.ShowPanelForState(GameState.LoadGame);
            });
        }
    }
}
