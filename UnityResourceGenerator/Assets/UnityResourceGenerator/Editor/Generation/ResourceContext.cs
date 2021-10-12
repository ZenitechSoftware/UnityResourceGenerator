using System;

namespace UnityResourceGenerator.Editor.Generation
{
    public sealed class ResourceContext
    {
        public ResourceContext
        (
            string assetsFolder,
            string folderPath,
            string baseNamespace,
            string className,
            Action<string> info,
            Action<string> error)
        {
            AssetsFolder = assetsFolder;
            FolderPath = folderPath;
            BaseNamespace = baseNamespace;
            ClassName = className;
            Info = info;
            Error = error;
        }

        public string AssetsFolder { get; }
        public string FolderPath { get; }
        public string BaseNamespace { get; }
        public string ClassName { get; }
        public Action<string> Info { get; }
        public Action<string> Error { get; }
    }
}
