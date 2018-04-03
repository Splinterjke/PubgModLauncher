using System;
using Stylet;
using StyletIoC;
using PubgMod.ViewModels;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using System.Threading.Tasks;

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
            // This is called just after the root ViewModel has been launched
            // Something like a version check that displays a dialog might be launched from here
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (SettingsViewModel.IsFileRestored) return;
            
            var pubgPath = Properties.Settings.Default.pubgpath;
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var reshadePath = $@"{appdata}\Reshade";

            if (Directory.Exists(reshadePath))
                Directory.Delete(reshadePath, true);

            var downloadedModPath = $@"{appdata}\TslGame-WindowsNoEditor_ui.pak";
            if (File.Exists(downloadedModPath))
                File.Delete(downloadedModPath);

            var modFile = $@"{pubgPath}\TslGame\Content\Paks\TslGame-WindowsNoEditor_ui.pak";
            if (File.Exists(modFile) && new FileInfo(modFile).Length < 10 * 1024 * 1024)
                File.Delete(modFile);

            var originalUIPakPath = $@"{pubgPath}\TslGame\Content\Paks\TslGame-WindowsNoEditor_erangel_lod.pak";
            var fileInfo = new FileInfo(originalUIPakPath);
            if (File.Exists(originalUIPakPath) && fileInfo.Length < 800 * 1024 * 1024 && fileInfo.Length > 100 * 1024 * 1024)
                File.Move(originalUIPakPath, $"{originalUIPakPath.Replace("TslGame-WindowsNoEditor_erangel_lod.pak", "TslGame-WindowsNoEditor_ui.pak")}");

            var originalLodPakPath = $@"{pubgPath}\TslGame\Content\TslGame-WindowsNoEditor_erangel_lod.pak";
            if (File.Exists($"{originalLodPakPath}") && !File.Exists($@"{pubgPath}\TslGame\Content\Paks\TslGame-WindowsNoEditor_erangel_lod.pak"))
                File.Move($"{originalLodPakPath}", $@"{pubgPath}\TslGame\Content\Paks\TslGame-WindowsNoEditor_erangel_lod.pak");

            if (Directory.Exists($@"{pubgPath}\TslGame\Binaries\Win64\ReShade"))
                Directory.Delete($@"{pubgPath}\TslGame\Binaries\Win64\ReShade");

            if (File.Exists($@"{pubgPath}\TslGame\Binaries\Win64\d3d11.dll"))
                File.Delete($@"{pubgPath}\TslGame\Binaries\Win64\d3d11.dll");

            if (File.Exists($@"{pubgPath}\TslGame\Binaries\Win64\ReShade.fx"))
                File.Delete($@"{pubgPath}\TslGame\Binaries\Win64\ReShade.fx");

            if (File.Exists($@"{pubgPath}\TslGame\Binaries\Win64\d3d11.log"))
                File.Delete($@"{pubgPath}\TslGame\Binaries\Win64\d3d11.log");
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            // Called on Application.DispatcherUnhandledException
        }
    }
}
