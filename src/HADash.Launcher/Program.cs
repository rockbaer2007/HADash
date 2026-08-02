using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace HADashLauncher
{
    internal static class Program
    {
        private const int RequiredMajorVersion = 8;
        private const string MainApplicationFileName = "HADash.App.exe";

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!WindowsDesktopRuntimeChecker.IsInstalled(RequiredMajorVersion))
            {
                MessageBox.Show(
                    "HADash benötigt die Microsoft .NET 8 Desktop Runtime (64-Bit).\n\n" +
                    "Bitte installieren Sie die passende Desktop Runtime und starten Sie HADash anschließend erneut.\n\n" +
                    "Download:\nhttps://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe",
                    "Microsoft .NET 8 fehlt",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            string applicationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MainApplicationFileName);
            if (!File.Exists(applicationPath))
            {
                MessageBox.Show(
                    "Die Programmdatei '" + MainApplicationFileName + "' wurde nicht gefunden.\n\n" +
                    "Bitte entpacken oder kopieren Sie immer den vollständigen Programmordner.",
                    "HADash kann nicht gestartet werden",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = applicationPath,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "HADash konnte nicht gestartet werden.\n\n" + ex.Message,
                    "Startfehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }

    internal static class WindowsDesktopRuntimeChecker
    {
        public static bool IsInstalled(int requiredMajorVersion)
        {
            foreach (string root in GetDotNetRoots())
            {
                string sharedFrameworkPath = Path.Combine(root, "shared", "Microsoft.WindowsDesktop.App");

                if (!Directory.Exists(sharedFrameworkPath))
                    continue;

                foreach (string directory in Directory.EnumerateDirectories(sharedFrameworkPath))
                {
                    string versionName = Path.GetFileName(directory);
                    Version version;
                    if (Version.TryParse(NormalizeVersion(versionName), out version) &&
                        version.Major == requiredMajorVersion)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static IEnumerable<string> GetDotNetRoots()
        {
            var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            AddIfPresent(roots, Environment.GetEnvironmentVariable("DOTNET_ROOT"));
            AddIfPresent(roots, Environment.GetEnvironmentVariable("DOTNET_ROOT_X64"));

            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            AddIfPresent(roots, Path.Combine(programFiles, "dotnet"));

            foreach (string root in roots)
                yield return root;
        }

        private static void AddIfPresent(ISet<string> roots, string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
                roots.Add(path.Trim());
        }

        private static string NormalizeVersion(string value)
        {
            int dashIndex = value.IndexOf('-');
            return dashIndex >= 0 ? value.Substring(0, dashIndex) : value;
        }
    }
}
