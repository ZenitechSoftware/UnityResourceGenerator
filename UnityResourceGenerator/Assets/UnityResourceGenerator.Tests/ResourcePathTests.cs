using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityResourceGenerator.Sample;

namespace UnityResourceGenerator.Tests
{
    public class ResourcePathTests
    {
        [Test]
        public void ScenesWork()
        {
            Assert.DoesNotThrow(() => SceneManager.LoadScene(ResourcePaths.Scenes.LoadSceneInitial));
            Assert.DoesNotThrow(() => SceneManager.LoadScene(ResourcePaths.Scenes.LoadSceneNext));
        }

        [Test]
        public void PrefabsWork()
        {
            var prefab = Resources.Load<GameObject>(ResourcePaths.Prefabs.Cube);
            Assert.NotNull(prefab);
        }
    }
}
