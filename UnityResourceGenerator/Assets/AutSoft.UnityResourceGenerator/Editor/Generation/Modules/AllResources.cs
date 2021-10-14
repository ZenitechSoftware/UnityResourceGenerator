using AutSoft.UnityResourceGenerator.Editor.Generation.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutSoft.UnityResourceGenerator.Editor.Generation.Modules
{
    public sealed class AllResources : IModuleGenerator
    {
        public string Generate(ResourceContext context) =>
            new StringBuilder()
                .AppendMultipleLines(context.Data.Select(d => Generate(context, d)))
                .ToString();

        private static string Generate(ResourceContext context, IResourceData data)
        {
            context.Info($"Started generating {data.ClassName}");

            // ReSharper disable once MissingIndent
            var classBegin =
$@"
        public static partial class {data.ClassName}
        {{
";
            // ReSharper disable once MissingIndent
            const string classEnd = "        }";

            var values = data
                .FileExtensions
                .SelectMany(ext => Directory.EnumerateFiles(context.AssetsFolder, ext, SearchOption.AllDirectories))
                .Select(filePath =>
                {
                    var (canLoad, baseFolder) = GetBaseFolder(filePath, context, data);
                    if (!canLoad) return (null, null, null);

                    var resourcePath = filePath
                        .Replace(baseFolder, string.Empty)
                        .Replace('\\', '/')
                        .Remove(0, 1);

                    resourcePath = Path.Combine
                        (
                            Path.GetDirectoryName(resourcePath) ?? string.Empty,
                            Path.GetFileNameWithoutExtension(resourcePath)
                        )
                        .Replace('\\', '/');

                    return
                    (
                        name: Path.GetFileNameWithoutExtension(filePath).Replace(" ", string.Empty),
                        path: resourcePath,
                        fileExtension: Path.GetExtension(filePath)
                    );
                })
                .Where(p => p.name != null)
                .ToArray();

            var duplicates = values.Duplicates(v => v.name).ToArray();

            if (duplicates.Length > 0)
            {
                context.Error(duplicates.Aggregate(new StringBuilder(), (sb, v) => sb.Append(v.name).Append(' ').AppendLine(v.path)).ToString());
                throw new InvalidOperationException("Found duplicate file names");
            }

            if (values.Length == 0)
            {
                LogFinished();

                return string.Empty;
            }

            var output = values
                .Aggregate(
                    new StringBuilder().Append(classBegin),
                    (sb, s) =>
                    {
                        sb.Append("            public const string ").Append(s.name).Append(" = \"").Append(s.path).AppendLine("\";");

                        if (s.fileExtension == ".unity")
                        {
                            sb.Append("            public static ").Append("void").Append(" Load").Append(s.name).Append("(LoadSceneMode mode = LoadSceneMode.Single) => SceneManager.LoadScene(").Append(s.name).AppendLine(", mode);");
                            sb.Append("            public static ").Append("AsyncOperation").Append(" LoadAsync").Append(s.name).Append("(LoadSceneMode mode = LoadSceneMode.Single) => SceneManager.LoadSceneAsync(").Append(s.name).AppendLine(", mode);");
                        }
                        else
                        {
                            sb.Append("            public static ").Append(data.DataType).Append(" Load").Append(s.name).Append("() => Resources.Load<").Append(data.DataType).Append(">(").Append(s.name).AppendLine(");");
                        }

                        return sb;
                    })
                .AppendLine(classEnd)
                .ToString();

            LogFinished();

            return output;

            void LogFinished() => context.Info($"Finished generating {data.ClassName}");
        }

        private static (bool canLoad, string baseFolder) GetBaseFolder(string filePath, ResourceContext context, IResourceData data)
        {
            if (Path.GetExtension(filePath) == ".unity") return (true, context.AssetsFolder);

            var parents = GetAllParentDirectories(filePath);
            var resourcesFolder = parents.LastOrDefault(p => p.Name == "Resources");

            return resourcesFolder is null
                ? (false, null)
                : (true, resourcesFolder.FullName);
        }

        private static IEnumerable<DirectoryInfo> GetAllParentDirectories(string directoryToScan)
        {
            var ret = new Stack<DirectoryInfo>();
            GetAllParentDirectories(new DirectoryInfo(directoryToScan), ref ret);
            return ret.ToList();
        }

        private static void GetAllParentDirectories(DirectoryInfo directoryToScan, ref Stack<DirectoryInfo> directories)
        {
            if (directoryToScan == null || directoryToScan.Name == directoryToScan.Root.Name) return;

            directories.Push(directoryToScan);
            GetAllParentDirectories(directoryToScan.Parent, ref directories);
        }
    }
}