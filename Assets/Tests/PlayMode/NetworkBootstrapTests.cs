using NUnit.Framework;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;

namespace MemeArena.Tests.PlayMode
{
    public class NetworkBootstrapTests
    {
        [UnityTest]
        public IEnumerator NetworkManager_Singleton_Exists_When_Starting()
        {
            yield return null;
            var nm = Object.FindFirstObjectByType<NetworkManager>();
            Assert.IsNotNull(nm, "Expected a NetworkManager in test scene. If missing, ensure Bootstrap scene is added or DevelopmentAuditor creates one.");
        }
    }
}
