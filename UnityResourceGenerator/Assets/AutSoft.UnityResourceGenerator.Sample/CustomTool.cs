using AutSoft.UnityResourceGenerator.Editor;
using UnityEditor;

namespace AutSoft.UnityResourceGenerator.Sample
{
    public static class CustomTool
    {
        [MenuItem("Custom Tools / Recreate Custom Defaults")]
        public static void RecreateCustomDefaults() =>
            ResourceGeneratorSettings.GetOrCreateSettings
            (
                folderPath: "AutSoft.UnityResourceGenerator.Sample",
                baseNamespace: "AutSoft.UnityResourceGenerator.Sample",
                logInfo: true
            );
    }
}
