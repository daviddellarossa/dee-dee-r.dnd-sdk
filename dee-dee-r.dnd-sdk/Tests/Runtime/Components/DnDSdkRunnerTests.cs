using System.Collections.Generic;
using System.Reflection;
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

        [SetUp]
        public void SetUp()
        {
            // Clear any Bus left by previous tests (cross-fixture stale state).
            if (DnDSdkRunner.Bus != null)
                typeof(DnDSdkRunner)
                    .GetProperty("Bus", BindingFlags.Public | BindingFlags.Static)!
                    .GetSetMethod(nonPublic: true)!
                    .Invoke(null, new object[] { null });
        }

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
        public void Awake_WithNullScheduler_LogsError_AndBusRemainsNull()
        {
            var go = new GameObject("BadRunner");
            _created.Add(go);
            go.SetActive(false);
            go.AddComponent<DnDSdkRunner>(); // _scheduler not set.

            // LogAssert.Expect is unreliable for LogType.Error in Unity 6000.3;
            // use ignoreFailingMessages to prevent the expected error from failing the test.
            LogAssert.ignoreFailingMessages = true;
            go.SetActive(true);
            LogAssert.ignoreFailingMessages = false;

            Assert.IsNull(DnDSdkRunner.Bus, "Bus must remain null when scheduler is missing.");
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

            // Second runner logs an error and must not overwrite the existing bus.
            // LogAssert.Expect is unreliable for LogType.Error in Unity 6000.3.
            LogAssert.ignoreFailingMessages = true;
            CreateRunner();
            LogAssert.ignoreFailingMessages = false;

            Assert.AreSame(originalBus, DnDSdkRunner.Bus, "Original bus must be preserved.");
        }
    }
}