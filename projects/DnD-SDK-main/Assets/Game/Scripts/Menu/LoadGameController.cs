using System.Collections.Generic;
using UnityEngine.UIElements;
using DeeDeeR.DnD.Game;
using DeeDeeR.DnD.Game.SaveSystem;

namespace DeeDeeR.DnD.Game.Menu
{
    /// <summary>
    /// Populates the load-game panel with save slot entries and triggers
    /// <see cref="GameManager.LoadGame"/> when a slot is selected and loaded.
    /// </summary>
    public sealed class LoadGameController
    {
        private readonly GameManager  _gm;
        private readonly MenuManager  _menu;

        private readonly ScrollView   _saveList;
        private readonly Button       _loadBtn;

        private int _selectedSlot = -1;
        private List<SaveSlotInfo> _slots;

        public LoadGameController(VisualElement root, GameManager gm, MenuManager menu)
        {
            _gm   = gm;
            _menu = menu;

            if (root == null) return;

            _saveList = root.Q<ScrollView>("save-list");
            _loadBtn  = root.Q<Button>("btn-load");

            root.Q<Button>("btn-back")?.RegisterCallback<ClickEvent>(_ =>
            {
                _gm.SetState(GameState.MainMenu);
                _menu.ShowPanelForState(GameState.MainMenu);
            });

            _loadBtn?.RegisterCallback<ClickEvent>(_ =>
            {
                if (_selectedSlot >= 0)
                    _gm.LoadGame(_selectedSlot);
            });

            if (_loadBtn != null) _loadBtn.SetEnabled(false);
        }

        /// <summary>Called by <see cref="MenuManager"/> when the load-game panel is shown.</summary>
        public void OnShow()
        {
            _selectedSlot = -1;
            if (_loadBtn != null) _loadBtn.SetEnabled(false);
            PopulateSlots();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void PopulateSlots()
        {
            if (_saveList == null) return;

            _saveList.Clear();

            _slots = _gm.SaveManager?.GetAllSlots() ?? new List<SaveSlotInfo>();

            if (_slots.Count == 0)
            {
                var empty = new Label("No saved games found.");
                empty.AddToClassList("field-label");
                _saveList.Add(empty);
                return;
            }

            foreach (var slot in _slots)
                _saveList.Add(BuildSlotEntry(slot));
        }

        private VisualElement BuildSlotEntry(SaveSlotInfo info)
        {
            var entry = new VisualElement();
            entry.AddToClassList("save-slot");
            entry.userData = info.SlotIndex;

            var nameLabel = new Label($"{info.CharacterName}  (Lv {info.Level} {info.ClassName})");
            nameLabel.AddToClassList("save-slot__name");
            var dateLabel = new Label(info.SaveDate);
            dateLabel.AddToClassList("save-slot__detail");

            entry.Add(nameLabel);
            entry.Add(dateLabel);

            entry.RegisterCallback<ClickEvent>(_ => SelectSlot(entry, info.SlotIndex));

            return entry;
        }

        private void SelectSlot(VisualElement entry, int slotIndex)
        {
            // Deselect all
            if (_saveList != null)
                foreach (var child in _saveList.Children())
                    child.RemoveFromClassList("save-slot--selected");

            entry.AddToClassList("save-slot--selected");
            _selectedSlot = slotIndex;
            if (_loadBtn != null) _loadBtn.SetEnabled(true);
        }
    }
}
