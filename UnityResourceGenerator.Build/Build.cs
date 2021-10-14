using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.MSBuild;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Nuke.Common.Tools.DocFX.DocFXTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.Unity.UnityTasks;
using static UnityHelper;

class Build : NukeBuild
{
    const string DocFxJsonPath = "Documentation/docfx.json";

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Password for Unity license")] string? UnityPassword;
    [Parameter("Email for Unity license")] string? UnityEmail;
    [Parameter("Serial for Unity license")] string? UnitySerial;

    [Parameter("Are we running in CI")] bool IsCi = false;

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
}
