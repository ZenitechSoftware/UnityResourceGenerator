namespace AutSoft.UnityResourceGenerator.Editor.Generation
{
    public interface IResourceData
    {
        string ClassName { get; }
        string FileExtension { get; }
        bool IsResource { get; }
    }
}