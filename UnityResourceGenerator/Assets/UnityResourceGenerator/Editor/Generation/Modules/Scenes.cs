﻿using System;
using System.IO;
using System.Linq;
using System.Text;

namespace UnityResourceGenerator.Editor.Generation.Modules
{
    public sealed class Scenes : IModuleGenerator
    {
        public string Generate(ResourceContext context)
        {
            context.Info("Started generating scenes");

            // ReSharper disable once MissingIndent
            const string classBegin =
@"
        public static partial class Scenes
        {
";
            // ReSharper disable once MissingIndent
            const string classEnd = "        }";

            var values = Directory
                .EnumerateFiles(context.AssetsFolder, "*.unity", SearchOption.AllDirectories)
                .Select(filePath =>
                {
                    var resourcePath = filePath
                        .Replace(context.AssetsFolder, string.Empty)
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
                        name: Path.GetFileNameWithoutExtension(filePath),
                        path: resourcePath
                    );
                })
                .ToArray();

            var duplicates = values.Duplicates(v => v.name).ToArray();

            if (duplicates.Length > 0)
            {
                context.Error(duplicates.Aggregate(new StringBuilder(), (sb, v) => sb.Append(v.name).Append(' ').AppendLine(v.path)).ToString());
                throw new InvalidOperationException("Found duplicate file names");
            }

            var output = values
                .Aggregate(
                    new StringBuilder().Append(classBegin),
                    (sb, s) => sb
                        .Append("            public const string ")
                        .Append(s.name)
                        .Append(" = \"")
                        .Append(s.path)
                        .AppendLine("\";"))
                .AppendLine(classEnd)
                .ToString();

            context.Info("Finished generating scenes");

            return output;
        }
    }
}