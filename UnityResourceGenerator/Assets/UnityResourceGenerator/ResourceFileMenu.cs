using UnityEditor;
using UnityEngine;

namespace UnityResourceGenerator
{
    public static class ResourceFileMenu
    {
        [MenuItem("Tools / Generate Resources")]
        public static void GenerateResources()
        {
            const string baseNamespace = "UnityResourceGenerator.Sample";
            const string className = "ResourcePaths";
            const string folderPath = "UnityResourceGenerator.Sample";
            var assetsFolder = Application.dataPath;

            ResourceFileGenerator.CreateResourceFile
            (
                new ResourceContext
                (
                    assetsFolder,
                    folderPath,
                    baseNamespace,
                    className,
                    Debug.Log,
                    Debug.LogError
                )
            );
        }
    }
}
