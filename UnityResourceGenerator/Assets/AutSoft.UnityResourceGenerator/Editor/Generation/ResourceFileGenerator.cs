using AutSoft.UnityResourceGenerator.Editor.Generation.Extensions;
using System;
using System.Linq;
using System.Text;

namespace AutSoft.UnityResourceGenerator.Editor.Generation
{
    public static class ResourceFileGenerator
    {
        public static string CreateResourceFile(ResourceContext context)
        {
            // ReSharper disable once MissingIndent
            const string fileBeginHasNamespace =
@"
namespace {0}
{
    // ReSharper disable PartialTypeWithSinglePart
    // ReSharper disable InconsistentNaming
    public static partial class {1}
    {";

            // ReSharper disable once MissingIndent
            const string fileEndHasNamespace =
@"    }
}";

            // ReSharper disable once MissingIndent
            const string fileBeginNoNamespace =
@"
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable InconsistentNaming
public static partial class {1}
{";

            const string fileEndNoNamespace = "}";

            var builder = new StringBuilder();

            var allConcreteTypes = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsGenericType && !t.IsInterface)
                .ToArray();

            builder.AppendMultipleLines(context.Usings.Select(u => $"using {u};"));

            builder.AppendLine(
                (string.IsNullOrWhiteSpace(context.BaseNamespace) ? fileBeginNoNamespace : fileBeginHasNamespace)
                .Replace("{0}", context.BaseNamespace)
                .Replace("{1}", context.ClassName));

            allConcreteTypes
                .Where(t => t.GetInterfaces().Any(i => typeof(IModuleGenerator).IsAssignableFrom(i)))
                .Select(t => (IModuleGenerator)Activator.CreateInstance(t))
                .Select(m => m.Generate(context))
                .ForEach(m => builder.AppendLine(m));

            builder.AppendLine(string.IsNullOrWhiteSpace(context.BaseNamespace) ? fileEndNoNamespace : fileEndHasNamespace);

            var fileContent = builder.ToString();

            fileContent = allConcreteTypes
                .Where(t => t.GetInterfaces().Any(i => typeof(IResourcePostProcessor).IsAssignableFrom(i)))
                .Select(t => (IResourcePostProcessor)Activator.CreateInstance(t))
                .OrderByDescending(p => p.PostProcessPriority)
                .Aggregate(fileContent, (current, processor) => processor.PostProcess(context, current));

            return fileContent;
        }
    }
}
