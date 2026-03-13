using UnityEngine.UIElements;
using DeeDeeR.DnD.Game;

namespace DeeDeeR.DnD.Game.Menu
{
    /// <summary>
    /// Wires the in-game pause menu buttons to save, quit, and resume actions.
    /// </summary>
    public sealed class InGameMenuController
    {
        private const int DefaultSaveSlot = 0;

        public InGameMenuController(VisualElement root, GameManager gm, MenuManager menu)
        {
            if (root == null) return;

            root.Q<Button>("btn-save")?.RegisterCallback<ClickEvent>(_ =>
                gm.SaveCurrentGame(DefaultSaveSlot));

            root.Q<Button>("btn-save-quit")?.RegisterCallback<ClickEvent>(_ =>
                gm.SaveAndQuit(DefaultSaveSlot));

            root.Q<Button>("btn-quit-nosave")?.RegisterCallback<ClickEvent>(_ =>
                gm.QuitWithoutSaving());

            root.Q<Button>("btn-resume")?.RegisterCallback<ClickEvent>(_ =>
                menu.CloseMenu());
        }
    }
}
