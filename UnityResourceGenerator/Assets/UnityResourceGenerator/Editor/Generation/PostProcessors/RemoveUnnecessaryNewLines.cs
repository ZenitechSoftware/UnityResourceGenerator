using System.Text.RegularExpressions;

namespace UnityResourceGenerator.Editor.Generation.PostProcessors
{
    public sealed class RemoveUnnecessaryNewLines : IResourcePostProcessor
    {
        public string PostProcess(ResourceContext context, string resourceFileContent)
        {
            context.Info("Started removing unnecessary new lines");

            var output = Regex.Replace(resourceFileContent, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);

            context.Info("Finished removing unnecessary new lines");

            return output;
        }

        public int PostProcessPriority { get; } = 0;
    }
}
