using System;
using System.Linq;
using System.Text;

namespace UnityResourceGenerator.Editor.Generation
{
    public static class ResourceFileGenerator
    {
        public static string CreateResourceFile(ResourceContext context)
        {
            // ReSharper disable once MissingIndent
            const string fileBegin =
@"namespace {0}
{
    // ReSharper disable PartialTypeWithSinglePart
    public static partial class {1}
    {";

            // ReSharper disable once MissingIndent
            const string fileEnd =
@"    }
}";

            var builder = new StringBuilder();

            var allConcreteTypes = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsGenericType && !t.IsInterface)
                .ToArray();

            builder.AppendLine(
                fileBegin
                    .Replace("{0}", context.BaseNamespace)
                    .Replace("{1}", context.ClassName));

            allConcreteTypes
                .Where(t => t.GetInterfaces().Any(i => typeof(IModuleGenerator).IsAssignableFrom(i)))
                .Select(t => (IModuleGenerator)Activator.CreateInstance(t))
                .Select(m => m.Generate(context))
                .ForEach(m => builder.AppendLine(m));

            builder.AppendLine(fileEnd);

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
