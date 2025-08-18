using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MemeArena.Tests.PlayMode
{
    public class PlayBootstrapSmokeTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Try to load Bootstrap scene if present, else just continue
            if (Application.CanStreamedLevelBeLoaded("Bootstrap"))
            {
                var op = SceneManager.LoadSceneAsync("Bootstrap", LoadSceneMode.Single);
                while (!op.isDone) yield return null;
            }
            yield break;
        }

        [UnityTest]
        public IEnumerator SceneLoads_And_CameraExists()
        {
            yield return null; // allow first frame
            var cam = Object.FindFirstObjectByType<Camera>();
            Assert.IsNotNull(cam, "Expected a Camera in the scene");
        }
    }
}
