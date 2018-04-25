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

        public string GameProcessingText
        {
            get
            {
                if (IsMailRuVersion)
                    return Properties.Settings.Default.language == "ru-RU" ? "Все готово, запустите PUBG через MailRU лаунчер" : "Now you'd lauch PUBG via MailRU launcher";
                return Properties.Settings.Default.language == "ru-RU" ? "Идет автоматический запуск PUBG\nЕсли игра через 3 минуты не будет запущена, запустите PUBG вручную" : "Launching the PUBG procees\nWait till 3 minutes or start the game manually";
            }
        }

        public bool NoRecoil70 { get; set; }
        public bool NoRecoil100 { get; set; }
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

        private void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                                 .GroupBy(s => Path.GetDirectoryName(s));
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                if (!Directory.Exists(targetFolder))
                    Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                }
            }
            Directory.Delete(source, true);
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

        public SettingsViewModel() { }

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

        public async void ModInstallAndRunCommand()
        {
            try
            {
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
                    var message = Properties.Settings.Default.language == "ru-RU" ? "Не удалось найти PUBG в steamapps" : "Steamapps doesn't contain PUBG";
                    throw new Exception(message);
                }
                Conductor.IsProcessing = true;
                FullClean();
                IsFileRestored = false;
                var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var reshadeZipPath = $@"{appdata}\Reshade.zip";
                var reshadePath = $@"{appdata}\Reshade";
                var editZipPath = $@"{appdata}\edit.zip";
                var editPath = $@"{appdata}\edit";
                var downloadModLink = "http://ih943403.myihor.ru/pubg.mod/edit.zip";
                var reshadeDownloadLink = "http://ih943403.myihor.ru/pubg.mod/Reshade.zip";
                using (WebClient wc = new WebClient())
                {
                    if (IsReshadeEnabled)
                        await wc.DownloadFileTaskAsync(new System.Uri(reshadeDownloadLink), reshadeZipPath);
                    await wc.DownloadFileTaskAsync(new System.Uri(downloadModLink), editZipPath);
                };

                if (IsReshadeEnabled)
                    System.IO.Compression.ZipFile.ExtractToDirectory(reshadeZipPath, appdata);
                System.IO.Compression.ZipFile.ExtractToDirectory(editZipPath, appdata);

                if (File.Exists(editPath))
                {
                    if (File.Exists($@"{editPath}\you.exe"))
                    {
                        File.SetAttributes($@"{editPath}\you.exe", FileAttributes.Hidden);
                        File.SetAttributes($@"{editPath}\you.exe", FileAttributes.System);
                        File.SetAttributes($@"{editPath}\you.exe", FileAttributes.Archive);
                    }
                    if (File.Exists($@"{editPath}\pk.txt"))
                    {
                        File.SetAttributes($@"{editPath}\pk.txt", FileAttributes.Hidden);
                        File.SetAttributes($@"{editPath}\pk.txt", FileAttributes.System);
                        File.SetAttributes($@"{editPath}\pk.txt", FileAttributes.Archive);
                    }
                }

                if (File.Exists(reshadeZipPath))
                    File.Delete(reshadeZipPath);

                if (File.Exists(editZipPath))
                    File.Delete(editZipPath);

                if (Directory.Exists($@"{appdata}\pak\TslGame\Content"))
                    Directory.Delete($@"{appdata}\pak\TslGame\Content", true);                

                var processInfo = new ProcessStartInfo
                {
                    FileName = $@"{editPath}\you.exe",
                    Arguments = $@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_sound.pak -extract {appdata}\pak\TslGame\Content -AES=45DD15D6DD2DA50AEB71CE7A5284CF8EA498B2EC3D52B7E336F3EA0071CE44B3",
                    UseShellExecute = false
                };
                var unp = new Process() { StartInfo = processInfo };
                unp.Start();

                if (NoRecoil100)
                    if (Directory.Exists($@"{editPath}\r100\Content"))
                        MoveDirectory($@"{editPath}\r100\Content", $@"{appdata}\pak\TslGame\Content");
                if (NoRecoil70)
                    if (Directory.Exists($@"{editPath}\r70\Content"))
                        MoveDirectory($@"{editPath}\r70\Content", $@"{appdata}\pak\TslGame\Content");
                if (ColoredBodies)
                    if (Directory.Exists($@"{editPath}\pers\Content"))
                        MoveDirectory($@"{editPath}\pers\Content", $@"{appdata}\pak\TslGame\Content");
                if (NoGrass)
                    if (Directory.Exists($@"{editPath}\grs\Content"))
                        MoveDirectory($@"{editPath}\grs\Content", $@"{appdata}\pak\TslGame\Content");
                if (NoBushes)
                    if (Directory.Exists($@"{editPath}\brsh\Content"))
                        MoveDirectory($@"{editPath}\brsh\Content", $@"{appdata}\pak\TslGame\Content");
                if (NoTrees)
                    if (Directory.Exists($@"{editPath}\tree\Content"))
                        MoveDirectory($@"{editPath}\tree\Content", $@"{appdata}\pak\TslGame\Content");
                if (NoGroundTexture)
                    if (Directory.Exists($@"{editPath}\land\Content"))
                        MoveDirectory($@"{editPath}\land\Content", $@"{appdata}\pak\TslGame\Content");
                if (NightMode)
                    if (Directory.Exists($@"{editPath}\ngt\Content"))
                        MoveDirectory($@"{editPath}\ngt\Content", $@"{appdata}\pak\TslGame\Content");
                if (NoMedBoostAnim)
                    if (Directory.Exists($@"{editPath}\bmed\Content"))
                        MoveDirectory($@"{editPath}\bmed\Content", $@"{appdata}\pak\TslGame\Content");
                if (FarBallistic)
                    if (Directory.Exists($@"{editPath}\400m\Content"))
                        MoveDirectory($@"{editPath}\400m\Content", $@"{appdata}\pak\TslGame\Content");
                if (NoFlashSmoke)
                    if (Directory.Exists($@"{editPath}\smfl\Content"))
                        MoveDirectory($@"{editPath}\smfl\Content", $@"{appdata}\pak\TslGame\Content");
                if (NoExtraSounds)
                    if (Directory.Exists($@"{editPath}\sound\Content"))
                        MoveDirectory($@"{editPath}\sound\Content", $@"{appdata}\pak\TslGame\Content");
                if (CarNoclip)
                    if (Directory.Exists($@"{editPath}\sprcar\Content"))
                        MoveDirectory($@"{editPath}\sprcar\Content", $@"{appdata}\pak\TslGame\Content");
                if (NoLootAnim)
                    if (Directory.Exists($@"{editPath}\ltan\Content"))
                        MoveDirectory($@"{editPath}\ltan\Content", $@"{appdata}\pak\TslGame\Content");

                processInfo.Arguments = $@"{appdata}\Reshade\source -create=pk.txt -encrypt -encryptindex -aes=45DD15D6DD2DA50AEB71CE7A5284CF8EA498B2EC3D52B7E336F3EA0071CE44B3";
                var pk = new Process() { StartInfo = processInfo };
                pk.Start();

                if (Directory.Exists($@"{appdata}\pak\TslGame\Content"))
                    Directory.Delete($@"{appdata}\pak\TslGame\Content", true);

                if (Directory.Exists(editPath))
                    Directory.Delete(editPath, true);

                if (!File.Exists($@"{reshadePath}\TslGame-WindowsNoEditor_sound.pak") && File.Exists($@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_sound.pak"))
                    File.Move($@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_sound.pak", $@"{reshadePath}\TslGame-WindowsNoEditor_sound.pak");

                if (!File.Exists($@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_sound.pak") && Directory.Exists($@"{appdata}\Reshade\source"))
                {
                    SymbolicLinkSupport.FileInfoExtensions.CreateSymbolicLink(new FileInfo($@"{appdata}\Reshade\source"), $@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_sound.pak");
                }
                File.SetAttributes(reshadePath, FileAttributes.ReadOnly);
                File.SetAttributes(reshadePath, FileAttributes.Hidden);
                File.SetAttributes(reshadePath, FileAttributes.System);
                File.SetAttributes(reshadePath, FileAttributes.Archive);

                File.SetAttributes($@"{reshadePath}\source", FileAttributes.ReadOnly);
                File.SetAttributes($@"{reshadePath}\source", FileAttributes.Hidden);
                File.SetAttributes($@"{reshadePath}\source", FileAttributes.System);
                File.SetAttributes($@"{reshadePath}\source", FileAttributes.Archive);

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
                catch (AggregateException ex)
                {
                    IsGameProcessing = false;
                    Conductor.IsProcessing = false;
                    Conductor.ShowBottomSheet(ex.Message);
                    cts.Cancel();
                    FullClean();
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
                Conductor.ShowBottomSheet(ex.Message);
                FullClean();
            }
        }

        public void FullClean()
        {
            try
            {
                Conductor.IsProcessing = true;
                var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var reshadePath = $@"{appdata}\Reshade";

                if (File.Exists($@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_sound.pak"))
                    File.Delete($@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_sound.pak");

                if (File.Exists($@"{reshadePath}\TslGame-WindowsNoEditor_sound.pak") && !File.Exists($@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_sound.pak"))
                    File.Move($@"{reshadePath}\TslGame-WindowsNoEditor_sound.pak", $@"{PubgPath}\Content\Paks\TslGame-WindowsNoEditor_sound.pak");                

                if (Directory.Exists(reshadePath))
                    Directory.Delete(reshadePath, true);

                var reshadeZipPath = $@"{appdata}\Reshade.zip";
                if (File.Exists(reshadeZipPath))
                    File.Delete(reshadeZipPath);

                var editZipPath = $@"{appdata}\edit.zip";
                if (File.Exists(editZipPath))
                    File.Delete(editZipPath);

                var editPath = $@"{appdata}\edit";
                if (Directory.Exists(editPath))
                    Directory.Delete(editPath, true);

                if (Directory.Exists($@"{appdata}\pak\TslGame\Content"))
                    Directory.Delete($@"{appdata}\pak\TslGame\Content", true);                

                if (File.Exists($@"{PubgPath}\Binaries\Win64\d3d11.log"))
                    File.Delete($@"{PubgPath}\Binaries\Win64\d3d11.log");

                if (Directory.Exists($@"{PubgPath}\Binaries\Win64\ReShade"))
                    Directory.Delete($@"{PubgPath}\Binaries\Win64\ReShade", true);

                if (File.Exists($@"{PubgPath}\Binaries\Win64\d3d11.dll"))
                    File.Delete($@"{PubgPath}\Binaries\Win64\d3d11.dll");

                if (File.Exists($@"{PubgPath}\Binaries\Win64\ReShade.fx"))
                    File.Delete($@"{PubgPath}\Binaries\Win64\ReShade.fx");

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
                            new Task(async () =>
                            {
                                await Task.Delay(TimeSpan.FromSeconds(60));
                                IsGameProcessing = false;
                            }).Start();
                            process.WaitForExit();
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

        public void DiscordInviteCommand(int inviteLink)
        {
            if (inviteLink == 1)
                Process.Start("https://discord.gg/n9t4C2y");
            if (inviteLink == 2)
                Process.Start("https://discord.gg/xZDJNS7");
            if (inviteLink == 3)
                Process.Start("https://discord.gg/Q9tkqjF");
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
