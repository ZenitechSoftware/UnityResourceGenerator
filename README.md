# Unity Resource Generator [![openupm](https://img.shields.io/npm/v/com.autsoft.unityresourcegenerator?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.autsoft.unityresourcegenerator/) [![GitHub tag (latest SemVer)](https://img.shields.io/github/v/release/AutSoft/UnityResourceGenerator?style=flat)](https://github.com/AutSoft/UnityResourceGenerator/releases/latest) [![Publish](https://github.com/AutSoft/UnityResourceGenerator/actions/workflows/publish.yml/badge.svg)](https://github.com/AutSoft/UnityResourceGenerator/actions/workflows/publish.yml) [![Publish To GitHub Pages](https://github.com/AutSoft/UnityResourceGenerator/actions/workflows/publish-to-gh-pages.yml/badge.svg)](https://github.com/AutSoft/UnityResourceGenerator/actions/workflows/publish-to-gh-pages.yml)

Automatically generate a helper class for `Resources.Load` in Unity with the press of a button.

![Generate Button](~/images/intro/GenerateButton.png)

With this folder structure:

```
Assets/
├─ Resources/
│  ├─ Coin.prefab
│  ├─ Coin.mp3
├─ Scenes/
│  ├─ CoinRain.unity
```

The following helper class is generated:

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sample
{
    // ReSharper disable PartialTypeWithSinglePart
    public static partial class ResourcePaths
    {

        public static partial class Scenes
        {
            public const string CoinRain = "Scenes/CoinRain";

            public static void LoadCoinRain(LoadSceneMode mode = LoadSceneMode.Single) =>
                SceneManager.LoadScene(CoinRain, mode);

            public static AsyncOperation LoadAsyncCoinRain(LoadSceneMode mode = LoadSceneMode.Single) =>
                SceneManager.LoadSceneAsync(CoinRain, mode);
        }

        public static partial class Prefabs
        {
            public const string Coin = "Coin";
            public static GameObject LoadCube() => Resources.Load<GameObject>(Coin);
        }

        public static partial class AudioClips
        {
            public const string Coin = "Coin";
            public static AudioClip LoadCoin() => Resources.Load<AudioClip>(Coin);
        }
    }
}
```

## Installation

Either use the `.unitypackage` provided in the releases or use OpenUPM

```
openupm add com.autsoft.unityresourcegenerator
```

## Documentation

For the complete documentation, visit the [website](https://autsoft.github.io/UnityResourceGenerator/).
