using AutSoft.UnityResourceGenerator.Sample;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutSoft.UnityResourceGenerator.Tests
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

        [Test]
        public void MaterialsWork()
        {
            var cubeMaterial = Resources.Load<Material>(ResourcePaths.Materials.Cube);
            Assert.NotNull(cubeMaterial);

            var cubeAltMaterial = Resources.Load<Material>(ResourcePaths.Materials.CubeAlt);
            Assert.NotNull(cubeAltMaterial);
        }
    }
}
