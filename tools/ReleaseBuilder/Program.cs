using System.IO.Compression;
using System.Xml.Linq;

namespace HADash.ReleaseBuilder;

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            string repoRoot = FindRepositoryRoot(AppContext.BaseDirectory);
            string configuration = GetOption(args, "--configuration") ?? "Release";
            string runtime = GetOption(args, "--runtime") ?? "win-x64";
            string? versionOverride = GetOption(args, "--version");
            bool skipBuild = args.Contains("--skip-build", StringComparer.OrdinalIgnoreCase);

            string version = versionOverride ?? ReadVersion(Path.Combine(repoRoot, "Directory.Build.props"));
            string artifacts = Path.Combine(repoRoot, "artifacts");
            string staging = Path.Combine(artifacts, "staging", $"HADash-v{version}-Portable");
            string packages = Path.Combine(artifacts, "packages");
            string zipPath = Path.Combine(packages, $"HADash-v{version}-Portable-{runtime}.zip");

            RecreateDirectory(staging);
            Directory.CreateDirectory(packages);

            if (!skipBuild)
            {
                RunDotNet(repoRoot, $"publish src/HADash.App/HADash.App.csproj -c {configuration} -r {runtime} --self-contained false -o \"{Path.Combine(staging, "app")}\"");
                RunDotNet(repoRoot, $"build src/HADash.Launcher/HADash.Launcher.csproj -c {configuration}");
                CopyLauncher(repoRoot, configuration, staging);
            }
            else
            {
                CopyExistingBuild(repoRoot, configuration, runtime, staging);
            }

            FlattenApplication(staging);
            CreatePortableFolders(staging);
            CopyIfExists(Path.Combine(repoRoot, "README.md"), Path.Combine(staging, "README.md"));
            CopyIfExists(Path.Combine(repoRoot, "CHANGELOG.md"), Path.Combine(staging, "CHANGELOG.md"));
            CopyIfExists(Path.Combine(repoRoot, "LICENSE"), Path.Combine(staging, "LICENSE.txt"));
            CopyDirectory(Path.Combine(repoRoot, "examples"), Path.Combine(staging, "examples"));

            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipFile.CreateFromDirectory(staging, zipPath, CompressionLevel.Optimal, false);

            Console.WriteLine($"Portable package created: {zipPath}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static string FindRepositoryRoot(string start)
    {
        DirectoryInfo? current = new(start);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Directory.Build.props"))) return current.FullName;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException("Repository root not found.");
    }

    private static string ReadVersion(string propsPath)
    {
        XDocument doc = XDocument.Load(propsPath);
        return doc.Descendants("Version").FirstOrDefault()?.Value
            ?? throw new InvalidDataException("Version is missing in Directory.Build.props.");
    }

    private static string? GetOption(string[] args, string name)
    {
        int index = Array.FindIndex(args, a => a.Equals(name, StringComparison.OrdinalIgnoreCase));
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }

    private static void RunDotNet(string workingDirectory, string arguments)
    {
        using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        }) ?? throw new InvalidOperationException("dotnet could not be started.");

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) Console.WriteLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) Console.Error.WriteLine(e.Data); };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        if (process.ExitCode != 0) throw new InvalidOperationException($"dotnet command failed: {arguments}");
    }

    private static void CopyLauncher(string repoRoot, string configuration, string staging)
    {
        string launcherDir = Path.Combine(repoRoot, "src", "HADash.Launcher", "bin", configuration, "net48");
        CopyDirectory(launcherDir, staging);
    }

    private static void CopyExistingBuild(string repoRoot, string configuration, string runtime, string staging)
    {
        string appDir = Path.Combine(repoRoot, "src", "HADash.App", "bin", configuration, "net8.0-windows", runtime, "publish");
        string launcherDir = Path.Combine(repoRoot, "src", "HADash.Launcher", "bin", configuration, "net48");
        CopyDirectory(appDir, Path.Combine(staging, "app"));
        CopyDirectory(launcherDir, staging);
    }

    private static void FlattenApplication(string staging)
    {
        string app = Path.Combine(staging, "app");
        if (!Directory.Exists(app)) return;
        foreach (string file in Directory.GetFiles(app, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(app, file);
            string destination = Path.Combine(staging, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, true);
        }
        Directory.Delete(app, true);
    }

    private static void CreatePortableFolders(string staging)
    {
        foreach (string name in new[] { "config", "logs", "backups", "temp" })
            Directory.CreateDirectory(Path.Combine(staging, name));

        string config = Path.Combine(staging, "config", "user.config");
        if (!File.Exists(config))
            File.WriteAllText(config, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<AppSettings />\n");
    }

    private static void CopyIfExists(string source, string destination)
    {
        if (File.Exists(source)) File.Copy(source, destination, true);
    }

    private static void CopyDirectory(string source, string destination)
    {
        if (!Directory.Exists(source)) return;
        foreach (string directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            string target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, true);
        }
    }

    private static void RecreateDirectory(string path)
    {
        if (Directory.Exists(path)) Directory.Delete(path, true);
        Directory.CreateDirectory(path);
    }
}
