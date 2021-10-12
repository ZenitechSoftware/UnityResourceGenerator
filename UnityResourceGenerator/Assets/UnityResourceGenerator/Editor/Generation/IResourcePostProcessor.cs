namespace UnityResourceGenerator.Editor.Generation
{
    public interface IResourcePostProcessor
    {
        int PostProcessPriority { get; }
        string PostProcess(ResourceContext context, string resourceFileContent);
    }
}