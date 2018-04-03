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

        public string ModDescription1
        {
            get
            {
                return Properties.Settings.Default.language == "ru-RU" ? "Полный набор функций" : "All features pack";
            }
        }

        public string ModDescription2
        {
            get
            {
                return Properties.Settings.Default.language == "ru-RU" ? "Семи-легит набор" : "Semi-legit features pack";
            }
        }

        public string ModDescription3
        {
            get
            {
                return Properties.Settings.Default.language == "ru-RU" ? "Легит набор Функций" : "Full legit features pack";
            }
        }

        public string UseReshadeText
        {
            get
            {
                return Properties.Settings.Default.language == "ru-RU" ? "Исп. ReShade FX" : "Use ReShade FX";
            }
        }

        public string GameProcessingText
        {
            get
            {
                if (IsMailRuVersion)
                    return Properties.Settings.Default.language == "ru-RU" ? "Все готово, запустите PUBG через MailRU лаунчер" : "Now you'd lauch PUBG via MailRU launcher";
                return Properties.Settings.Default.language == "ru-RU" ? "Идет автоматический запуск PUBG\nЕсли игра через 1 минуту не будет запущена, запустите PUBG вручную" : "Launching the PUBG procees\nWait till 1 minute or start the game manually";
            }
        }

        public string Mod1FeaturesText
        {
            get
            {
                return Properties.Settings.Default.language == "ru-RU"
                    ?
                    "Подсветка одежды людей\n" +
                    "Убрана текстура земли\n" +
                    "Убрана трава (доб. FPS)\n" +
                    "Нет отдачи\n" +
                    "Нет тряски оружия\n" +
                    "Нет дыма\n" +
                    "Нет ослепления\n" +
                    "Быстрое лечение\n" +
                    "Деревья удаляются\nдальше 1 кв\n" +
                    "Удаление лишних звуков"
                    :
                    "Players clothes glowing\n" +
                    "Removing far trees\n" +
                    "No smoke textures\n" +
                    "No flashing\n" +
                    "Fast healing\n" +
                    "Removed ground texture\n" +
                    "Removed grass (better FPS)\n" +
                    "No recoil\n" +
                    "No weapon shaking\n" +
                    "Removing extra sounds";
            }
        }

        public string Mod2FeaturesText
        {
            get
            {
                return Properties.Settings.Default.language == "ru-RU"
                    ?
                    "Подсветка одежды людей\n" +
                    "Убрана текстура земли\n" +
                    "Убрана трава (доб. FPS)\n" +
                    "Нет отдачи\n" +
                    "Нет тряски оружия\n" +
                    "Удаление лишних звуков"
                    :
                    "Players clothes glowing\n" +
                    "Removed ground texture\n" +
                    "Removed grass (better FPS)\n" +
                    "No recoil\n" +
                    "No weapon shaking\n" +
                    "Removing extra sounds";
            }
        }

        public string Mod3FeaturesText
        {
            get
            {
                return Properties.Settings.Default.language == "ru-RU"
                    ?
                    "Нет отдачи\n" +
                    "Нет тряски оружия\n" +
                    "Удаление лишних звуков"
                    :
                    "No recoil\n" +
                    "No weapon shaking\n" +
                    "Removing extra sounds";
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

        public async void ModInstallAndRunCommand(int e)
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

                if (!Directory.Exists(this.PubgPath) || !this.PubgPath.Contains("PUBG") || !Directory.Exists($@"{PubgPath}\TslGame\Binaries"))
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
                if (File.Exists(reshadeZipPath))
                    File.Delete(reshadeZipPath);
                if (File.Exists($@"{appdata}\TslGame-WindowsNoEditor_ui.pak"))
                    File.Delete($@"{appdata}\TslGame-WindowsNoEditor_ui.pak");
                var reshadePath = $@"{appdata}\Reshade";
                if (Directory.Exists(reshadePath))
                    Directory.Delete(reshadePath, true);
                string downloadModLink;
                switch (e)
                {
                    case 1:
                        downloadModLink = "http://ih943403.myihor.ru/pubg.mod/Mod1_TslGame-WindowsNoEditor_ui.pak";
                        break;
                    case 2:
                        downloadModLink = "http://ih943403.myihor.ru/pubg.mod/Mod2_TslGame-WindowsNoEditor_ui.pak";
                        break;
                    case 3:
                        downloadModLink = "http://ih943403.myihor.ru/pubg.mod/Mod3_TslGame-WindowsNoEditor_ui.pak";
                        break;
                    default:
                        Conductor.IsProcessing = false;
                        return;
                }
                var reshadeDownloadLink = "http://ih943403.myihor.ru/pubg.mod/Reshade.zip";
                using (WebClient wc = new WebClient())
                {
                    if (IsReshadeEnabled)
                        await wc.DownloadFileTaskAsync(new System.Uri(reshadeDownloadLink), reshadeZipPath);
                    await wc.DownloadFileTaskAsync(new System.Uri(downloadModLink), $@"{appdata}\TslGame-WindowsNoEditor_ui.pak");
                    //if (File.Exists($@"{PubgPath}\TslGame\Binaries\Win64\TslGame.exe"))
                    //    File.Delete($@"{PubgPath}\TslGame\Binaries\Win64\TslGame.exe");
                    //await wc.DownloadFileTaskAsync(new System.Uri("http://ih943403.myihor.ru/pubg.mod/TslGame.exe"), $@"{PubgPath}\TslGame\Binaries\Win64\TslGame.exe");
                };

                if (IsReshadeEnabled)
                    System.IO.Compression.ZipFile.ExtractToDirectory(reshadeZipPath, appdata);
                if (File.Exists(reshadeZipPath))
                    File.Delete(reshadeZipPath);

                if (File.Exists($@"{PubgPath}\TslGame\Content\TslGame-WindowsNoEditor_erangel_lod.pak"))
                    File.Delete($@"{PubgPath}\TslGame\Content\TslGame-WindowsNoEditor_erangel_lod.pak");

                var originalLodPakPath = $@"{PubgPath}\TslGame\Content\Paks\TslGame-WindowsNoEditor_erangel_lod.pak";
                if (File.Exists(originalLodPakPath) && !File.Exists($@"{PubgPath}\TslGame\Content\TslGame-WindowsNoEditor_erangel_lod.pak"))
                    File.Move(originalLodPakPath, $@"{PubgPath}\TslGame\Content\TslGame-WindowsNoEditor_erangel_lod.pak");


                var originalUIPakPath = $@"{PubgPath}\TslGame\Content\Paks\TslGame-WindowsNoEditor_ui.pak";
                if (File.Exists(originalUIPakPath) && !File.Exists($@"{PubgPath}\TslGame\Content\Paks\TslGame-WindowsNoEditor_erangel_lod.pak"))
                    File.Move(originalUIPakPath, $"{originalUIPakPath.Replace("TslGame-WindowsNoEditor_ui.pak", "TslGame-WindowsNoEditor_erangel_lod.pak")}");

                if (File.Exists($@"{appdata}\TslGame-WindowsNoEditor_ui.pak") && !File.Exists($@"{PubgPath}\TslGame\Content\Paks\TslGame-WindowsNoEditor_ui.pak"))
                    File.Move($@"{appdata}\TslGame-WindowsNoEditor_ui.pak", $@"{PubgPath}\TslGame\Content\Paks\TslGame-WindowsNoEditor_ui.pak");

                if (IsReshadeEnabled)
                {
                    if (!Directory.Exists($@"{PubgPath}\TslGame\Binaries\Win64\ReShade") && Directory.Exists($@"{reshadePath}\ReShade"))
                    {
                        SymbolicLinkSupport.DirectoryInfoExtensions.CreateSymbolicLink(new DirectoryInfo($@"{reshadePath}\ReShade"), $@"{PubgPath}\TslGame\Binaries\Win64\ReShade");
                    }

                    if (!File.Exists($@"{PubgPath}\TslGame\Binaries\Win64\d3d11.dll") && File.Exists($@"{reshadePath}\d3d11.dll"))
                    {
                        SymbolicLinkSupport.FileInfoExtensions.CreateSymbolicLink(new FileInfo($@"{reshadePath}\d3d11.dll"), $@"{PubgPath}\TslGame\Binaries\Win64\d3d11.dll");
                    }

                    if (!File.Exists($@"{PubgPath}\TslGame\Binaries\Win64\ReShade.fx") && File.Exists($@"{reshadePath}\ReShade.fx"))
                    {
                        SymbolicLinkSupport.FileInfoExtensions.CreateSymbolicLink(new FileInfo($@"{reshadePath}\ReShade.fx"), $@"{PubgPath}\TslGame\Binaries\Win64\ReShade.fx");
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

                if (Directory.Exists(reshadePath))
                    Directory.Delete(reshadePath, true);

                var downloadedModPath = $@"{appdata}\TslGame-WindowsNoEditor_ui.pak";
                if (File.Exists(downloadedModPath))
                    File.Delete(downloadedModPath);

                var modFile = $@"{PubgPath}\TslGame\Content\Paks\TslGame-WindowsNoEditor_ui.pak";
                if (File.Exists(modFile) && new FileInfo(modFile).Length < 10 * 1024 * 1024)
                    File.Delete(modFile);

                var originalUIPakPath = $@"{PubgPath}\TslGame\Content\Paks\TslGame-WindowsNoEditor_erangel_lod.pak";
                var fileInfo = new FileInfo(originalUIPakPath);
                if (File.Exists(originalUIPakPath) && fileInfo.Length < 800 * 1024 * 1024 && fileInfo.Length > 100 * 1024 * 1024)
                    File.Move(originalUIPakPath, $"{originalUIPakPath.Replace("TslGame-WindowsNoEditor_erangel_lod.pak", "TslGame-WindowsNoEditor_ui.pak")}");

                var originalLodPakPath = $@"{PubgPath}\TslGame\Content\TslGame-WindowsNoEditor_erangel_lod.pak";
                if (File.Exists($"{originalLodPakPath}") && !File.Exists($@"{PubgPath}\TslGame\Content\Paks\TslGame-WindowsNoEditor_erangel_lod.pak"))
                    File.Move($"{originalLodPakPath}", $@"{PubgPath}\TslGame\Content\Paks\TslGame-WindowsNoEditor_erangel_lod.pak");

                if (File.Exists($@"{PubgPath}\TslGame\Binaries\Win64\d3d11.log"))
                    File.Delete($@"{PubgPath}\TslGame\Binaries\Win64\d3d11.log");

                if (Directory.Exists($@"{PubgPath}\TslGame\Binaries\Win64\ReShade"))
                    Directory.Delete($@"{PubgPath}\TslGame\Binaries\Win64\ReShade");

                if (File.Exists($@"{PubgPath}\TslGame\Binaries\Win64\d3d11.dll"))
                    File.Delete($@"{PubgPath}\TslGame\Binaries\Win64\d3d11.dll");

                if (File.Exists($@"{PubgPath}\TslGame\Binaries\Win64\ReShade.fx"))
                    File.Delete($@"{PubgPath}\TslGame\Binaries\Win64\ReShade.fx");
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

        public static async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            using (Stream source = File.Open(sourcePath, FileMode.Open))
            {
                using (Stream destination = File.Create(destinationPath))
                {
                    await source.CopyToAsync(destination);
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
