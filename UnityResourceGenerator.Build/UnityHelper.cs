using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Unity;
using Nuke.Common.Utilities.Collections;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

static class UnityHelper
{
    public static UnitySettings ConfigureCustom
    (
        this UnitySettings s,
        string projectPath,
        string unityVersion,
        string? unityPassword,
        string? unityEmail,
        string? unitySerial,
        bool isCi,
        string? buildMethod = null
    )
    {
        if (isCi)
        {
            if (string.IsNullOrWhiteSpace(unityPassword)
                || string.IsNullOrWhiteSpace(unityEmail)
                || string.IsNullOrWhiteSpace(unitySerial))
            {
                throw new InvalidOperationException("Unity credentials are not set");
            }

            s = s
                .SetPassword(unityPassword)
                .SetUsername(unityEmail)
                .SetSerial(unitySerial);
        }

        s = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? s.SetHubVersion(unityVersion)
            : s.SetProcessToolPath("/opt/unity/Editor/Unity");

        s = s
            .SetProjectPath(projectPath)
            .EnableBatchMode()
            .EnableNoGraphics()
            .EnableQuit();

        if (buildMethod is not null) s = s.SetExecuteMethod(buildMethod);

        return s;
    }

    public static async Task StopUnity()
    {
        var processes = Process.GetProcessesByName("Unity");

        if (processes.Length == 0)
        {
            Logger.Info("No Unity processes found");
        }

        processes.ForEach(p =>
        {
            Logger.Info("Killing process {0}", p.ProcessName);
            p.Kill();
        });

        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}
