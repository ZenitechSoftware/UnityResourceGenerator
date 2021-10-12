using System;
using System.IO;
using System.Linq;
using System.Text;

namespace UnityResourceGenerator.Generation
{
    public sealed class ScenesModule : IModuleGenerator
    {
        public string Generate(ResourceContext context)
        {
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
                .Select(p =>
                (
                    name: Path.GetFileNameWithoutExtension(p) + "Scene",
                    path: ResourceFileGenerator.CreateResourcePath(p, context)
                ))
                .ToArray();

            var duplicates = values.Duplicates(v => v.name).ToArray();

            if (duplicates.Length > 0)
            {
                context.Error(duplicates.Aggregate(new StringBuilder(), (sb, v) => sb.Append(v.name).Append(' ').AppendLine(v.path)).ToString());
                throw new InvalidOperationException("Found duplicate file names");
            }

            return values
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
        }
    }
}
