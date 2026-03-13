using System;

namespace DeeDeeR.DnD.Game.SaveSystem
{
    /// <summary>
    /// Lightweight save-slot metadata displayed on the load-game screen.
    /// Populated from the save index file without loading the full save data.
    /// </summary>
    [Serializable]
    public sealed class SaveSlotInfo
    {
        public int    SlotIndex;
        public string CharacterName;
        public string ClassName;
        public string SaveDate;
        public int    Level;
    }
}
