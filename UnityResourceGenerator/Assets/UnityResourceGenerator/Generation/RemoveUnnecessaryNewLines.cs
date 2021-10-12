using System.Text.RegularExpressions;

namespace UnityResourceGenerator.Generation
{
    public sealed class RemoveUnnecessaryNewLines : IResourcePostProcessor
    {
        public string PostProcess(ResourceContext context, string resourceFileContent) =>
            Regex.Replace(resourceFileContent, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);

        public int PostProcessPriority { get; } = 0;
    }
}
