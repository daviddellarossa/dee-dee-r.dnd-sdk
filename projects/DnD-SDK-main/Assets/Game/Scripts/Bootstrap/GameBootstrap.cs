using UnityEngine;

namespace DeeDeeR.DnD.Game.Bootstrap
{
    /// <summary>
    /// Marks the Persistent scene's root hierarchy as DontDestroyOnLoad so that all managers
    /// and UI survive scene transitions. Runs before all other scripts (execution order -200).
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(transform.root.gameObject);
        }
    }
}
