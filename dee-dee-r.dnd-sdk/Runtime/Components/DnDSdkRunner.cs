using System;
using DeeDeeR.DnD.Runtime.Bus;
using DeeDeeR.MessageBus.Runtime.Core;
using UnityEngine;

namespace DeeDeeR.DnD.Runtime.Components
{
    /// <summary>
    /// Scene-level owner of the <see cref="DnDSdkBus"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Place one on a root GameObject in each scene that uses the DnD SDK and assign a
    /// <see cref="FrameSchedulerBehaviour"/> in the Inspector. The bus is created in
    /// <c>Awake</c> (execution order −100) so that all <see cref="CharacterComponent"/>
    /// instances can register their query handlers in <c>Start</c>.
    /// </para>
    /// <para>
    /// <see cref="Bus"/> is a static accessor to the current scene's bus instance —
    /// it is not a singleton. The reference is set to <c>null</c> in <c>OnDestroy</c>
    /// so stale references are surfaced immediately.
    /// </para>
    /// </remarks>
    [DisallowMultipleComponent]
    [AddComponentMenu("DnD SDK/DnD SDK Runner")]
    [DefaultExecutionOrder(-100)]
    public sealed class DnDSdkRunner : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("FrameSchedulerBehaviour that drives deferred message delivery. Required.")]
        private FrameSchedulerBehaviour _scheduler;

        /// <summary>The DnD SDK bus for the current scene context.</summary>
        public static DnDSdkBus Bus { get; private set; }

        private void Awake()
        {
            if (_scheduler == null)
                throw new InvalidOperationException(
                    $"[{nameof(DnDSdkRunner)}] Scheduler reference is null. " +
                    "Assign a FrameSchedulerBehaviour in the Inspector.");

            Bus = new DnDSdkBus(_scheduler);
        }

        private void OnDestroy()
        {
            Bus = null;
        }
    }
}