using System.IO;
using UnityEditor;
using UnityEngine;
using UnityResourceGenerator.Editor.Generation;

namespace UnityResourceGenerator.Editor
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

            var context = new ResourceContext
            (
                assetsFolder,
                folderPath,
                baseNamespace,
                className,
                Debug.Log,
                Debug.LogError
            );

            var fileContent = ResourceFileGenerator.CreateResourceFile(context);

            var filePath = Path.GetFullPath(Path.Combine(context.AssetsFolder, context.FolderPath, $"{context.ClassName}.cs"));

            if (File.Exists(filePath))
            {
                var old = File.ReadAllText(filePath);
                if (old == fileContent)
                {
                    context.Info("Resource file did not change");
                    return;
                }
            }

            File.WriteAllText(filePath, fileContent);
            context.Info($"Created resource file at: {filePath}");
        }
    }
}
