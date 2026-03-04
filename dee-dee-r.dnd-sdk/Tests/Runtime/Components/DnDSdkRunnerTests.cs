using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DeeDeeR.DnD.Runtime.Components;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Tests.Runtime.Components
{
    /// <summary>
    /// PlayMode tests for <see cref="DnDSdkRunner"/>: Bus lifecycle and guard conditions.
    /// </summary>
    [TestFixture]
    public class DnDSdkRunnerTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created)
                if (go != null) Object.DestroyImmediate(go);
            _created.Clear();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private GameObject CreateRunner()
        {
            var go = new GameObject("Runner");
            _created.Add(go);
            go.SetActive(false);
            var scheduler = go.AddComponent<FrameSchedulerBehaviour>();
            var runner    = go.AddComponent<DnDSdkRunner>();
            typeof(DnDSdkRunner)
                .GetField("_scheduler", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(runner, scheduler);
            go.SetActive(true); // Awake fires here — Bus is created.
            return go;
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Test]
        public void Awake_WithValidScheduler_CreatesBus()
        {
            CreateRunner();
            Assert.IsNotNull(DnDSdkRunner.Bus);
        }

        [Test]
        public void Awake_WithNullScheduler_LogsException_AndBusRemainsNull()
        {
            // _scheduler is never set so Awake throws InvalidOperationException,
            // which Unity logs as an exception rather than propagating it.
            var go = new GameObject("BadRunner");
            _created.Add(go);
            go.SetActive(false);
            go.AddComponent<DnDSdkRunner>(); // _scheduler not set.

            LogAssert.Expect(LogType.Exception, new Regex("Scheduler reference is null"));
            go.SetActive(true);

            Assert.IsNull(DnDSdkRunner.Bus);
        }

        [Test]
        public void OnDestroy_ClearsBusStaticReference()
        {
            var go = CreateRunner();
            Assert.IsNotNull(DnDSdkRunner.Bus, "Precondition: Bus must be set.");

            Object.DestroyImmediate(go);
            _created.Remove(go); // Already destroyed — skip in TearDown.

            Assert.IsNull(DnDSdkRunner.Bus);
        }

        [Test]
        public void SecondRunner_WhenBusAlreadyActive_LogsError_AndPreservesOriginalBus()
        {
            CreateRunner();
            var originalBus = DnDSdkRunner.Bus;

            // Second runner in the same scene should log an error and leave the bus unchanged.
            LogAssert.Expect(LogType.Error, new Regex("A DnDSdkBus is already active"));
            CreateRunner();

            Assert.AreSame(originalBus, DnDSdkRunner.Bus);
        }
    }
}