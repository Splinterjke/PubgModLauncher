using PubgMod.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stylet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Net;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using PubgMod.Services;
using MaterialDesignThemes.Wpf;
using System.Runtime.InteropServices;

namespace PubgMod.ViewModels
{
    public class SettingsViewModel : Screen, ITabItem
    {
        public UserData UserData { get; set; }
        public string UserKey
        {
            get
            {
                if (!UserData.IsSubscriber)
                    return Properties.Settings.Default.language == "ru-RU" ? "купить" : "purchase";
                return UserData.Userkey;
            }
        }

        public string SubTimeText
        {
            get
            {
                return Properties.Settings.Default.language == "ru-RU" ? "Подписка до:" : "Subscription ends:";
            }
        }

        public string SubTimeDate
        {
            get
            {
                return UserData.SubEndDate;
            }
        }

        public string EnterKeyText
        {
            get
            {
                return Properties.Settings.Default.language == "ru-RU" ? "Для приобретения подписки присоединитесь\nк нашему серверу Discord и свяжитесь с администрацией." : "To purchase subscription join\nour discord server and type to admins.";
            }
        }

        public string KeyHint
        {
            get
            {
                return Properties.Settings.Default.language == "ru-RU" ? "Введите ключ подписки" : "Enter subscription key";
            }
        }

        public string AcceptKeyText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "ПРИНЯТЬ";
                return "ACCEPT";
            }
        }

        public string CancelKeyText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "ОТМЕНА";
                return "CANCEL";
            }
        }

        public string LogoutText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "ВЫХОД";
                return "LOGOUT";
            }
        }

        public string GameProcessingText
        {
            get
            {
                if (IsMailRuVersion)
                    return Properties.Settings.Default.language == "ru-RU" ? "Все готово, запустите PUBG через MailRU лаунчер" : "Now you'd lauch PUBG via MailRU launcher";
                return Properties.Settings.Default.language == "ru-RU" ? "Идет автоматический запуск PUBG\nЕсли игра через 30 секунд не будет запущена, запустите PUBG вручную" : "Launching the PUBG procees\nWait till 30 seconds or start the game manually";
            }
        }

        public string InjectErrorText
        {
            get
            {
                return Properties.Settings.Default.language == "ru-RU" ? "Произошла ошибка инъекции мода во время игры. Повторите запуск мода, кликнув на START" : "Mod injection error occurred. Try install mod again by clicking START button";
            }
        }

        public bool IsInjectError { get; set; }

        private bool noRecoil70;
        public bool NoRecoil70
        {
            get
            {
                return noRecoil70;
            }
            set
            {
                if (value)
                    NoRecoil100 = false;
                noRecoil70 = value;
            }
        }

        private bool noRecoil100;
        public bool NoRecoil100
        {
            get
            {
                return noRecoil100;
            }
            set
            {
                if (value)
                    NoRecoil70 = false;
                noRecoil100 = value;
            }
        }
        public bool ColoredBodies { get; set; }
        public bool NoGrass { get; set; }
        public bool NoBushes { get; set; }
        public bool NoTrees { get; set; }
        public bool NoGroundTexture { get; set; }
        public bool NightMode { get; set; }
        public bool NoMedBoostAnim { get; set; }
        public bool FarBallistic { get; set; }
        public bool NoFlashSmoke { get; set; }
        public bool NoExtraSounds { get; set; }
        public bool CarNoclip { get; set; }
        public bool NoLootAnim { get; set; }
        public bool TestFeature { get; set; }

        private void MoveDirectory(string sourceDirName, string destDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, true);
                file.Delete();
            }


            foreach (DirectoryInfo subdir in dirs)
            {
                // Create the subdirectory.
                string temppath = Path.Combine(destDirName, subdir.Name);

                // Copy the subdirectories.
                MoveDirectory(subdir.FullName, temppath);
            }
        }

        public string KeyInput { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsProcessing { get; set; }
        public bool IsEnterKeyDialogOpen { get; set; }
        public bool IsReshadeEnabled { get; set; } = false;
        public bool IsGameProcessing { get; set; }
        public string PubgPath { get; set; } = string.IsNullOrEmpty(Properties.Settings.Default.pubgpath) ? string.Empty : Properties.Settings.Default.pubgpath;
        public static bool IsFileRestored;
        private bool IsMailRuVersion = false;

        private MainViewModel Conductor;

        public SettingsViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            DisplayName = $"User: {UserData?.Email}";
            Conductor = (this.Parent as MainViewModel);
            Conductor.PropertyChanged += Conductor_PropertyChanged;
        }

        private void Conductor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsProcessing))
                IsProcessing = Conductor.IsProcessing;
        }

        public void LogoutCommand()
        {
            Properties.Settings.Default.pwd = string.Empty;
            Properties.Settings.Default.Save();
            Conductor.ShowLogin();
        }

        public async void ModInstallAndRunCommand()
        {
            try
            {
                IsInjectError = false;
                if (!UserData.IsSubscriber)
                {
                    var message = Properties.Settings.Default.language == "ru-RU" ? "Время подписки истекло, обновите подписку" : "Your subscription is expired, repurchase subscription";
                    Conductor.ShowBottomSheet(message);
                    return;
                }
                Conductor.IsProcessing = true;
                var response = await HttpService.GetUserDataAsync(UserData.Email, UniqueProvider.Value());
                Conductor.IsProcessing = false;
                if (response.Item1)
                {
                    Conductor.ShowBottomSheet(response.Item2);
                    return;
                }
                UserData = JsonConvert.DeserializeObject<UserData>(response.Item2);
                if (!UserData.IsSubscriber)
                {
                    var message = Properties.Settings.Default.language == "ru-RU" ? "Время подписки истекло, обновите подписку" : "Your subscription is expired, repurchase subscription";
                    Conductor.ShowBottomSheet(message);
                    return;
                }

                if (!Directory.Exists(this.PubgPath) || !Directory.Exists($@"{PubgPath}\Binaries"))
                {
                    var message = Properties.Settings.Default.language == "ru-RU" ? "Некорректный путь до PUBG" : "Path to PUBG folder is incorrect";
                    throw new Exception(message);
                }

                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    if (process.ProcessName == "TslGame")
                    {
                        var message = Properties.Settings.Default.language == "ru-RU" ? "PUBG уже запущен" : "PUBG is already running";
                        throw new Exception(message);
                    }
                }

                var steamprocess = processes.FirstOrDefault(x => x.ProcessName == "Steam");
                if (steamprocess == null)
                    IsMailRuVersion = true;
                else
                    IsMailRuVersion = false;

                if (!Directory.Exists(PubgPath))
                {
                    var message = Properties.Settings.Default.language == "ru-RU" ? "Не удалось найти TslGame" : "Couldn't find TslGame folder";
                    throw new Exception(message);
                }
                Conductor.IsProcessing = true;
                FullClean();
                Conductor.IsProcessing = true;
                IsFileRestored = false;
                await ModInstall();

                if (!IsMailRuVersion)
                    Process.Start("steam://run/578080");
                var cts = new CancellationTokenSource();
                var token = cts.Token;
                var waitForLaunchTask = Task.Run(() => { CheckForPubgLaunched(token); }, token);
                Conductor.IsProcessing = false;
                IsGameProcessing = true;
                try
                {
                    //Conductor.MinimizeWindow();
                    waitForLaunchTask.Wait();
                }
                catch (Exception ex)
                {
                    IsGameProcessing = false;
                    Conductor.IsProcessing = false;
                    Conductor.ShowBottomSheet(ex.Message);
                    cts.Cancel();
                    FullClean();
                }
            }
            catch (Exception ex)
            {
                IsGameProcessing = false;
                Conductor.IsProcessing = false;
                File.WriteAllText("log.txt", ex.ToString() + "\n\n" + ex.StackTrace);
                Conductor.ShowBottomSheet(ex.Message);
                FullClean();
            }
        }

        private async Task ModInstall()
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var systemDrive = Path.GetPathRoot(Environment.SystemDirectory);
            var reshadeZipPath = $@"{appdata}\Reshade.zip";
            var reshadePath = $@"{appdata}\Reshade";
            var editZipPath = $@"{appdata}\edit.zip";
            var editPath = $@"{systemDrive}\edit";
            var hiddenDriverPath = $@"{appdata}\kernel";
            var hiddenDriverZipPath = $@"{appdata}\kernel.zip";
            var downloadModLink = "http://ih943403.myihor.ru/pubg.mod/edit.zip";
            var reshadeDownloadLink = "http://ih943403.myihor.ru/pubg.mod/Reshade.zip";
            var hiddenDriverLink = "http://ih943403.myihor.ru/pubg.mod/kernel.zip";
            using (WebClient wc = new WebClient())
            {
                if (!File.Exists(reshadeZipPath))
                    await wc.DownloadFileTaskAsync(new System.Uri(reshadeDownloadLink), reshadeZipPath);
                if (!File.Exists(editZipPath))
                    await wc.DownloadFileTaskAsync(new System.Uri(downloadModLink), editZipPath);
                if (!File.Exists(hiddenDriverZipPath))
                    await wc.DownloadFileTaskAsync(new Uri(hiddenDriverLink), hiddenDriverZipPath);
            };

            if (File.Exists(reshadeZipPath))
                System.IO.Compression.ZipFile.ExtractToDirectory(reshadeZipPath, appdata);

            if (!Directory.Exists(editPath) && File.Exists(editZipPath))
                System.IO.Compression.ZipFile.ExtractToDirectory(editZipPath, systemDrive);

            if (!Directory.Exists(hiddenDriverPath) && File.Exists(hiddenDriverZipPath))
                System.IO.Compression.ZipFile.ExtractToDirectory(hiddenDriverZipPath, appdata);

            if (Directory.Exists(editPath))
            {
                var dirInfo = new DirectoryInfo(editPath);
                dirInfo.Attributes |= FileAttributes.Hidden;
                if (File.Exists($@"{editPath}\you.exe"))
                {
                    var fileInfo = new FileInfo($@"{editPath}\you.exe");
                    fileInfo.Attributes |= FileAttributes.Hidden;
                }
            }

            if (Directory.Exists(hiddenDriverPath))
            {
                var dirInfo = new DirectoryInfo(hiddenDriverPath);
                dirInfo.Attributes |= FileAttributes.Hidden;
                //if (File.Exists($@"{hiddenDriverPath}\kernel.exe"))
                //{
                //    var fileInfo = new FileInfo($@"{hiddenDriverPath}\kernel.exe");
                //    fileInfo.Attributes |= FileAttributes.Hidden;
                //}
                //if (File.Exists($@"{hiddenDriverPath}\kss.ini"))
                //{
                //    var fileInfo = new FileInfo($@"{hiddenDriverPath}\kss.ini");
                //    fileInfo.Attributes |= FileAttributes.Hidden;
                //}
                //if (File.Exists($@"{hiddenDriverPath}\mbc.inf"))
                //{
                //    var fileInfo = new FileInfo($@"{hiddenDriverPath}\mbc.inf");
                //    fileInfo.Attributes |= FileAttributes.Hidden;
                //}
            }

            if (File.Exists($@"{Environment.GetFolderPath(Environment.SpecialFolder.System)}\drivers\mobanche.sys"))
                File.Delete($@"{Environment.GetFolderPath(Environment.SpecialFolder.System)}\drivers\mobanche.sys");


            var driverInstallInfo = new ProcessStartInfo
            {
                FileName = $@"{hiddenDriverPath}\mbc.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            var drvInstall = new Process() { StartInfo = driverInstallInfo };
            drvInstall.Start();
            drvInstall.WaitForExit();

            var serviceInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c net start mobanche",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            var serviceStart = new Process() { StartInfo = serviceInfo };
            serviceStart.Start();

            if (File.Exists(reshadeZipPath))
                File.Delete(reshadeZipPath);

            if (File.Exists(editZipPath))
                File.Delete(editZipPath);

            if (File.Exists(hiddenDriverZipPath))
                File.Delete(hiddenDriverZipPath);

            if (Directory.Exists($@"{systemDrive}\pak\"))
                Directory.Delete($@"{systemDrive}\pak\", true);

            if (!Directory.Exists($@"{systemDrive}\pak\TslGame\Content"))
                Directory.CreateDirectory($@"{systemDrive}\pak\TslGame\Content");

            var pakInfo = new DirectoryInfo($@"{systemDrive}\pak");
            pakInfo.Attributes |= FileAttributes.Hidden;

            if (NoRecoil100)
                if (Directory.Exists($@"{editPath}\r100\Content"))
                    MoveDirectory($@"{editPath}\r100\Content", $@"{systemDrive}\pak\TslGame\Content");
            if (NoRecoil70)
                if (Directory.Exists($@"{editPath}\r70\Content"))
                    MoveDirectory($@"{editPath}\r70\Content", $@"{systemDrive}\pak\TslGame\Content");
            if (ColoredBodies)
                if (Directory.Exists($@"{editPath}\pers\Content"))
                    MoveDirectory($@"{editPath}\pers\Content", $@"{systemDrive}\pak\TslGame\Content");
            if (NoGrass)
                if (Directory.Exists($@"{editPath}\grs\Content"))
                    MoveDirectory($@"{editPath}\grs\Content", $@"{systemDrive}\pak\TslGame\Content");
            if (NoBushes)
                if (Directory.Exists($@"{editPath}\brsh\Content"))
                    MoveDirectory($@"{editPath}\brsh\Content", $@"{systemDrive}\pak\TslGame\Content");
            if (NoTrees)
                if (Directory.Exists($@"{editPath}\tree\Content"))
                    MoveDirectory($@"{editPath}\tree\Content", $@"{systemDrive}\pak\TslGame\Content");
            if (NoGroundTexture)
                if (Directory.Exists($@"{editPath}\land\Content"))
                    MoveDirectory($@"{editPath}\land\Content", $@"{systemDrive}\pak\TslGame\Content");
            if (NightMode)
                if (Directory.Exists($@"{editPath}\ngt\Content"))
                    MoveDirectory($@"{editPath}\ngt\Content", $@"{systemDrive}\pak\TslGame\Content");
            if (NoMedBoostAnim)
                if (Directory.Exists($@"{editPath}\bmed\Content"))
                    MoveDirectory($@"{editPath}\bmed\Content", $@"{systemDrive}\pak\TslGame\Content");
            if (FarBallistic)
                if (Directory.Exists($@"{editPath}\400m\Content"))
                    MoveDirectory($@"{editPath}\400m\Content", $@"{systemDrive}\pak\TslGame\Content");
            if (NoFlashSmoke)
                if (Directory.Exists($@"{editPath}\smfl\Content"))
                    MoveDirectory($@"{editPath}\smfl\Content", $@"{systemDrive}\pak\TslGame\Content");
            if (NoExtraSounds)
                if (Directory.Exists($@"{editPath}\sound\Content"))
                    MoveDirectory($@"{editPath}\sound\Content", $@"{systemDrive}\pak\TslGame\Content");
            if (CarNoclip)
                if (Directory.Exists($@"{editPath}\sprcar\Config"))
                    MoveDirectory($@"{editPath}\sprcar\", $@"{systemDrive}\pak\TslGame\");
            if (NoLootAnim)
                if (Directory.Exists($@"{editPath}\ltan\Content"))
                    MoveDirectory($@"{editPath}\ltan\Content", $@"{systemDrive}\pak\TslGame\Content");
            if (TestFeature)
                if (Directory.Exists($@"{editPath}\test\Content"))
                    MoveDirectory($@"{editPath}\test\Content", $@"{systemDrive}\pak\TslGame\Content");

            var pkInfo = new ProcessStartInfo
            {
                FileName = $@"{editPath}\you.exe",
                Arguments = $@"TslGame-WindowsNoEditor_xmod.pak -create=pk.txt -encrypt -encryptindex -aes=45DD15D6DD2DA50AEB71CE7A5284CF8EA498B2EC3D52B7E336F3EA0071CE44B3",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            var pk = new Process() { StartInfo = pkInfo };
            pk.Start();
            pk.WaitForExit();
            await Task.Delay(2000);

            if (File.Exists($@"{editPath}\TslGame-WindowsNoEditor_xmod.pak"))
            {
                File.Move($@"{editPath}\TslGame-WindowsNoEditor_xmod.pak", $@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_xmod.pak");
            }

            if (File.Exists($@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_xmod.pak"))
            {
                var xmodInfo = new FileInfo($@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_xmod.pak");
                xmodInfo.Attributes |= FileAttributes.Hidden;
            }

            if (Directory.Exists($@"{systemDrive}\pak"))
                Directory.Delete($@"{systemDrive}\pak", true);

            if (Directory.Exists($@"{systemDrive}\Engine"))
                Directory.Delete($@"{systemDrive}\Engine", true);

            if (Directory.Exists(editPath))
                Directory.Delete(editPath, true);

            if (IsReshadeEnabled)
            {
                if (!Directory.Exists($@"{PubgPath}\Binaries\Win64\ReShade") && Directory.Exists($@"{reshadePath}\ReShade"))
                {
                    SymbolicLinkSupport.DirectoryInfoExtensions.CreateSymbolicLink(new DirectoryInfo($@"{reshadePath}\ReShade"), $@"{PubgPath}\Binaries\Win64\ReShade");
                }

                if (!File.Exists($@"{PubgPath}\Binaries\Win64\d3d11.dll") && File.Exists($@"{reshadePath}\d3d11.dll"))
                {
                    SymbolicLinkSupport.FileInfoExtensions.CreateSymbolicLink(new FileInfo($@"{reshadePath}\d3d11.dll"), $@"{PubgPath}\Binaries\Win64\d3d11.dll");
                }

                if (!File.Exists($@"{PubgPath}\Binaries\Win64\ReShade.fx") && File.Exists($@"{reshadePath}\ReShade.fx"))
                {
                    SymbolicLinkSupport.FileInfoExtensions.CreateSymbolicLink(new FileInfo($@"{reshadePath}\ReShade.fx"), $@"{PubgPath}\Binaries\Win64\ReShade.fx");
                }
            }
        }

        public void FullClean()
        {
            try
            {
                Conductor.IsProcessing = true;
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

                if (File.Exists($@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_xmod.pak"))
                    File.Delete($@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_xmod.pak");

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

                IsFileRestored = true;
                Conductor.IsProcessing = false;
            }
            catch (Exception ex)
            {
                IsGameProcessing = false;
                Conductor.IsProcessing = false;
                Conductor.ShowBottomSheet(ex.Message);
            }
        }

        private bool IsGameClosed = false;
        private readonly IWindowManager windowManager;

        public void SelectSteamFolderCommand()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                EnsurePathExists = true
            };
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                if (Directory.Exists(dialog.FileName))
                {
                    PubgPath = dialog.FileName;
                    Properties.Settings.Default.pubgpath = PubgPath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private async void CheckForPubgLaunched(CancellationToken token)
        {
            bool isRunning = false;
            IsGameClosed = false;
            while (!token.IsCancellationRequested)
            {
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    if (process.ProcessName == "TslGame")
                    {
                        if (!isRunning)
                        {
                            isRunning = true;
                        }
                        else
                        {
                            Install();
                            process.WaitForExit();
                            IsGameClosed = true;
                            if (!MainViewModel.IsExitRequested)
                                FullClean();
                            token.ThrowIfCancellationRequested();
                            return;
                        }
                    }
                }
                await Task.Delay(1000);
            }
        }

        private async void Install()
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
            IsGameProcessing = false;
            await Task.Delay(TimeSpan.FromMinutes(3));
            if (IsGameClosed)
                return;
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var hiddenDriverPath = $@"{appdata}\kernel";
            if (!Directory.Exists(hiddenDriverPath) || !File.Exists($@"{hiddenDriverPath}\kernel.exe") || !File.Exists($@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_xmod.pak"))
                return;
            var processInfo = new ProcessStartInfo
            {
                FileName = $@"{hiddenDriverPath}\kernel.exe",
                Arguments = "/hide file " + $"\"{PubgPath}\\Content\\Paks\\TslGame-WindowsNoEditor_xmod.pak\""
            };
            var kernel = new Process() { StartInfo = processInfo };
            kernel.Start();
            kernel.WaitForExit();
            await Task.Delay(1500);
            if (File.Exists($@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_xmod.pak"))
            {
                var processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    if (process.ProcessName == "TslGame")
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                }
                FullClean();
                await Task.Delay(2000);
                IsInjectError = true;
            }
        }

        public void DiscordInviteCommand(int inviteLink)
        {
            if (inviteLink == 1)
                Process.Start("https://discord.gg/n9t4C2y");
            if (inviteLink == 2)
                Process.Start("https://discord.gg/xZDJNS7");
            if (inviteLink == 3)
                Process.Start("https://t.me/joinchat/AAAAAE4BvbsQwyf9YxmvAA");
        }

        public async void OnCloseEnterKeyDialog(DialogClosingEventArgs eventArgs)
        {
            try
            {
                var result = (bool)eventArgs.Parameter;
                if (result)
                {
                    Conductor.IsProcessing = true;
                    var response = await HttpService.CheckUserKeyAsync(UserData.Email, KeyInput);
                    Conductor.IsProcessing = false;
                    if (response.Item1)
                    {
                        var message = Properties.Settings.Default.language == "ru-RU" ? "Введеный ключ не существует" : "Entered subscription key doesn't exist";
                        throw new Exception(message);
                    }
                    UserData = JsonConvert.DeserializeObject<UserData>(response.Item2);
                    if (!UserData.IsSubscriber)
                    {
                        var message = Properties.Settings.Default.language == "ru-RU" ? "Введеный ключ не существует" : "Entered subscription key doesn't exist";
                        throw new Exception(message);
                    }
                }
            }
            catch (Exception ex)
            {
                IsGameProcessing = false;
                Conductor.IsProcessing = false;
                Conductor.ShowBottomSheet(ex.Message);
            }
        }

        public void OpenKeyInfoDialogCommand()
        {
            if (UserData.IsSubscriber) return;
            IsEnterKeyDialogOpen = true;
        }
    }
}
