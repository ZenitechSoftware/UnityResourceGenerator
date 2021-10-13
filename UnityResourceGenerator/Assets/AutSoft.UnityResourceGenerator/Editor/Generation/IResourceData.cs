using System.Collections.Generic;

namespace AutSoft.UnityResourceGenerator.Editor.Generation
{
    public interface IResourceData
    {
        string ClassName { get; }
        IReadOnlyList<string> FileExtensions { get; }
        bool IsResource { get; }
        string DataType { get; }
    }
}