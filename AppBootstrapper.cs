using System;
using Stylet;
using StyletIoC;
using PubgMod.ViewModels;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PubgMod
{
    public class AppBootstrapper : Bootstrapper<MainViewModel>
    {
        protected override void OnStart()
        {

        }

        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Bind your own types. Concrete types are automatically self - bound.
            builder.Bind(typeof(ITabItem)).ToAllImplementations();
        }

        protected override void Configure()
        {
            // This is called after Stylet has created the IoC container, so this.Container exists, but before the
            // Root ViewModel is launched.
            // Configure your services, etc, in here
        }

        protected override void OnLaunch()
        {

        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (SettingsViewModel.IsFileRestored) return;

            var PubgPath = Properties.Settings.Default.pubgpath;
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var reshadePath = $@"{appdata}\Reshade";

            if (File.Exists($@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_xmod.pak"))
                File.Delete($@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_xmod.pak");

            if (Directory.Exists(reshadePath))
                Directory.Delete(reshadePath, true);

            var reshadeZipPath = $@"{appdata}\Reshade.zip";
            if (File.Exists(reshadeZipPath))
                File.Delete(reshadeZipPath);

            var editZipPath = $@"{appdata}\edit.zip";
            if (File.Exists(editZipPath))
                File.Delete(editZipPath);

            var driverZipPath = $@"{appdata}\Kernel.zip";
            if (File.Exists(driverZipPath))
                File.Delete(driverZipPath);


            var hiddenDriverPath = $@"{appdata}\kernel";
            if (Directory.Exists(hiddenDriverPath) && File.Exists($@"{hiddenDriverPath}\kernel.exe"))
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = $@"{hiddenDriverPath}\kernel.exe",
                    Arguments = "/unhide file all",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                var kernel = new Process() { StartInfo = processInfo };
                kernel.Start();
                kernel.WaitForExit();
            }

            var serviceInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c sc stop mobanche",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            var serviceStop = new Process() { StartInfo = serviceInfo };
            serviceStop.Start();
            serviceStop.WaitForExit();

            //if (File.Exists($@"{Environment.GetFolderPath(Environment.SpecialFolder.Windows)}\System32\Drivers\mobanche.sys"))
            //    File.Delete($@"{Environment.GetFolderPath(Environment.SpecialFolder.Windows)}\System32\Drivers\mobanche.sys");

            var systemDrive = Path.GetPathRoot(Environment.SystemDirectory);
            var editPath = $@"{systemDrive}\edit";
            if (Directory.Exists(editPath))
                Directory.Delete(editPath, true);

            var kernelPath = $@"{appdata}\kernel";
            if (Directory.Exists(kernelPath))
                Directory.Delete(kernelPath, true);

            if (Directory.Exists($@"{systemDrive}\pak\"))
                Directory.Delete($@"{systemDrive}\pak\", true);

            if (File.Exists($@"{PubgPath}\Binaries\Win64\d3d11.log"))
                File.Delete($@"{PubgPath}\Binaries\Win64\d3d11.log");

            if (Directory.Exists($@"{PubgPath}\Binaries\Win64\ReShade"))
                Directory.Delete($@"{PubgPath}\Binaries\Win64\ReShade", true);

            if (File.Exists($@"{PubgPath}\Binaries\Win64\d3d11.dll"))
                File.Delete($@"{PubgPath}\Binaries\Win64\d3d11.dll");

            if (File.Exists($@"{PubgPath}\Binaries\Win64\ReShade.fx"))
                File.Delete($@"{PubgPath}\Binaries\Win64\ReShade.fx");

            if (Directory.Exists($@"{systemDrive}\Engine"))
                Directory.Delete($@"{systemDrive}\Engine", true);

            var processKillInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c taskkill /f /im kernel.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            var kernelKill = new Process() { StartInfo = processKillInfo };
            kernelKill.Start();
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            // Called on Application.DispatcherUnhandledException
        }
    }
}
