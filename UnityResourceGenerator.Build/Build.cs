using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities.Collections;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Nuke.Common.Tools.DocFX.DocFXTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.Unity.UnityTasks;
using static UnityHelper;

class Build : NukeBuild
{
    const string DocFxJsonPath = "Documentation/docfx.json";

    public static int Main() => Execute<Build>(x => x.Compile);

    [PathExecutable] readonly Tool Gh = default!;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Password for Unity license")] string? UnityPassword;
    [Parameter("Email for Unity license")] string? UnityEmail;
    [Parameter("Serial for Unity license")] string? UnitySerial;

    [Parameter("Are we running in CI")] bool IsCi = false;

    string CurrentVersion { get; set; } = default!;
    bool IsNewestVersion { get; set; }

    static AbsolutePath UnityProjectPath => RootDirectory / "UnityResourceGenerator";
    static AbsolutePath UnitySolution => UnityProjectPath / "UnityResourceGenerator.sln";

    static string UnityVersion =>
        Regex.Match
            (
                File.ReadAllLines(Path.Combine(UnityProjectPath, "ProjectSettings", "ProjectVersion.txt"))[0],
                @"(\d+.\d+.\d+.*)"
            )
            .Value;

    Target Restore => _ => _
        .Executes(() =>
            MSBuild(s => s
                .SetTargetPath(UnitySolution)
                .SetTargets("Restore")));

    Target Compile => _ => _
        .DependsOn(Restore)
        .DependsOn(GenerateUnitySolution)
        .Executes(() =>
            MSBuild(s => s
                .SetTargetPath(UnitySolution)
                .SetTargets("Rebuild")
                .SetConfiguration(Configuration)
                .SetMaxCpuCount(Environment.ProcessorCount)
                .SetNodeReuse(IsLocalBuild)));

    Target GenerateUnitySolution => _ => _
        .OnlyWhenDynamic(() => IsCi)
        .Triggers(ReturnLicense)
        .Executes(async () =>
        {
            async Task GenerateSolution()
            {
                await StopUnity();

                Unity(s => s
                    .ConfigureCustom
                    (
                        UnityProjectPath,
                        UnityVersion,
                        UnityPassword,
                        UnityEmail,
                        UnitySerial,
                        IsCi,
                        "AutSoft.UnityResourceGenerator.Sample.BuildHelper.RegenerateProjectFiles"
                    ));
            }

            await GenerateSolution();

            // Changing editor preferences are only applied after restart
            if (IsCi) await GenerateSolution();
        });

    Target ReturnLicense => _ => _
        .OnlyWhenDynamic(() => IsCi)
        .AssuredAfterFailure()
        .Executes(async () =>
        {
            await StopUnity();

            Unity(s => s
                .ConfigureCustom
                (
                    UnityProjectPath,
                    UnityVersion,
                    UnityPassword,
                    UnityEmail,
                    UnitySerial,
                    IsCi
                )
                .SetProcessArgumentConfigurator(a => a.Add("-returnlicense")));
        });

    Target CreateMetadata => _ => _
        .DependsOn(Compile)
        .Executes(() => DocFX($"metadata {DocFxJsonPath}"));

    Target BuildDocs => _ => _
        .DependsOn(CreateMetadata)
        .Executes(() => DocFX($"build {DocFxJsonPath}"));

    Target ServeDocs => _ => _
        .DependsOn(BuildDocs)
        .Executes(() => DocFX($"{DocFxJsonPath} --serve"));

    Target CreateGithubRelease => _ => _
        .OnlyWhenDynamic(() => IsNewestVersion)
        .OnlyWhenDynamic(() => IsCi)
        .Executes(() =>
        {
            var version = CurrentVersion;

            var notes = File.ReadAllLines(RootDirectory / "CHANGELOG.md")
                .Skip(1)
                .TakeUntil(string.IsNullOrWhiteSpace)
                .Aggregate(new StringBuilder(), (sb, l) => sb.AppendLine(l))
                .ToString();

            Gh($"release create {version} -t {version} -n \"{notes}\"");
        });


    protected override void OnBuildInitialized()
    {
        bool GetIsNewestVersion()
        {
            var currentVersion = new Version(CurrentVersion);

            GitLogger = (_, s) => Logger.Info(s);

            Git("fetch --tags");

            var maxPublishedVersion = Git("tag")
                .Select(o => new Version(o.Text))
                .OrderBy(v => v)
                .LastOrDefault();

            return currentVersion.CompareTo(maxPublishedVersion) > 0;
        }

        string GetCurrentVersion()
        {
            var packagePath = UnityProjectPath / "Assets" / "AutSoft.UnityResourceGenerator" / "package.json";

            if (!File.Exists(packagePath)) throw new InvalidOperationException($"package.json does not exist at path: {packagePath}");

            var jsonContent = File.ReadAllText(packagePath);
            var package = JsonSerializer.Deserialize<PackageJson>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (package?.Version is null) throw new InvalidOperationException($"Cloud not deserialize package.json:{Environment.NewLine}{jsonContent}");

            return package.Version;
        }

        CurrentVersion = GetCurrentVersion();
        IsNewestVersion = GetIsNewestVersion();

        base.OnBuildInitialized();
    }

    sealed class PackageJson
    {
        public PackageJson(string version) => Version = version;

        public string Version { get; }
    }
}
