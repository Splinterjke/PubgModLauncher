using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stylet;
using System.Windows;
using PubgMod.Services;
using System.Globalization;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Net;
using Microsoft.Win32;
using System.IO;
using Newtonsoft.Json;
using PubgMod.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Reflection;
using System.Security.Cryptography;
using System.Diagnostics;
using MaterialDesignThemes.Wpf;
using System.ComponentModel;

namespace PubgMod.ViewModels
{
    public class MainViewModel : Conductor<ITabItem>.Collection.OneActive
    {

        private AppSettings appSettings;
        public AppSettings AppSettings
        {
            get { return this.appSettings; }
            set { SetAndNotify(ref this.appSettings, value); }
        }

        private IScreen bottomSheetView;
        public IScreen BottomSheetViewModel
        {
            get { return this.bottomSheetView; }
            private set { this.SetAndNotify(ref this.bottomSheetView, value); }
        }

        public static bool IsExitRequested;
        public bool IsExitDialogOpen { get; set; }
        public bool IsUpdateDialogOpen { get; set; }
        public bool IsChangelogDialogOpen { get; set; }
        public string ProcessingText { get; set; } = "Загрузка";

        public string ChangelogTitleText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "ЧТО НОВОГО:";
                return "CHANGELOG:";
            }
        }

        public string ChangelogText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "- Исправлена ошибка проверки HWID (снова)";
                return "- Fixed HWID validation (again)";
            }
        }

        public string OnExitText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "Закрытие программы приведет к завершению процесса PUBG.";
                return "Closing the launcher causes terminating PUBG process.";
            }
        }

        public string OnUpdateText
        {
            get
            {
                if (AppSettings != null && AppSettings.IsFrozen)
                {
                    if (Properties.Settings.Default.language == "ru-RU")
                        return "Моды на данный момент обнаруживаются античитом Battle Eye.\nЛанучер временно заморожен администрацией.";
                    return "Mods are currenly detected by Battle Eye anticheat.\nThe launcher is temporarily frozen by adminstrator.";
                }
                if (Properties.Settings.Default.language == "ru-RU")
                    return "Версия лаунчера устарела. Установите обновление, скачав последнюю версию программы.";
                return "The launcher version is outdate. Install last launcher update.";
            }
        }

        public string DownloadText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "СКАЧАТЬ";
                return "DOWNLOAD";
            }
        }

        public string AcceptExitText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "ПРИНЯТЬ";
                return "ACCEPT";
            }
        }

        public string CancelExitText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "ОТМЕНА";
                return "CANCEL";
            }
        }

        private bool isProcessing;
        public bool IsProcessing
        {
            get { return this.isProcessing; }
            set { SetAndNotify(ref this.isProcessing, value); }
        }

        public bool IsAppFrozen { get; set; }
        public bool UpdateIsReady { get; set; }

        public MainViewModel()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.language))
            {
                if (CultureInfo.CurrentUICulture.Name == "ru-RU")
                    Properties.Settings.Default.language = "ru-RU";
                else Properties.Settings.Default.language = "en-EN";
                Properties.Settings.Default.Save();
            }

            var loginView = new LoginViewModel();
            var settingsView = new SettingsViewModel();
            BottomSheetViewModel = new BottomSheetViewModel();

            DisplayName = "PUBG MOD";            
            Items.Add(settingsView);
            Items.Add(loginView);
        }

        protected override async void OnViewLoaded()
        {
            base.OnViewLoaded();
            try
            {
                IsProcessing = true;
                await Task.Run(() => UniqueProvider.Value());
                var sha1 = "";
                using (var md5 = new SHA1CryptoServiceProvider())
                {
                    using (var fs = new FileStream(Assembly.GetExecutingAssembly().Location, FileMode.Open, FileAccess.Read))
                    {
                        var byteArray = md5.ComputeHash(fs);
                        sha1 = BitConverter.ToString(byteArray).Replace("-", string.Empty);
                    }
                }
                var result = await HttpService.GetAppSettingsAsync();
                IsProcessing = false;
                if (result.Item1)
                {
                    ShowBottomSheet(result.Item2);
                    return;
                }
                AppSettings = JsonConvert.DeserializeObject<AppSettings>(result.Item2);
                if (AppSettings.IsFrozen)
                {
                    IsUpdateDialogOpen = true;
                    return;
                }
                if (AppSettings.Apphash.ToUpper() != sha1)
                {
                    UpdateIsReady = true;
                    IsUpdateDialogOpen = true;
                    return;
                }
                if (Properties.Settings.Default.apphash != sha1)
                {
                    IsChangelogDialogOpen = true;
                    Properties.Settings.Default.apphash = sha1;
                    Properties.Settings.Default.Save();
                }
                ShowLogin();
            }
            catch (Exception ex)
            {
                IsExitRequested = false;
                ShowBottomSheet(ex.Message);
            }
        }

        public void ShowLogin()
        {
            (Items[0] as SettingsViewModel).IsEnabled = false;
            (Items[1] as LoginViewModel).IsEnabled = true;
            this.ActiveItem = Items[1];
            var tabAnim = (this.View as Window).FindResource("TabAnimation") as Storyboard;
            tabAnim.Begin();
        }

        public void Init(UserData userdata)
        {
            try
            {
                if (userdata == null)
                    return;

                if (UpdateIsReady)
                {
                    Items.Remove(Items[0]);
                    return;
                }
                (Items[1] as LoginViewModel).IsEnabled = false;
                (Items[0] as SettingsViewModel).IsEnabled = true;
                (Items[0] as SettingsViewModel).UserData = userdata;
                this.ActiveItem = Items[0];
                var tabAnim = (this.View as Window).FindResource("TabAnimation") as Storyboard;
                tabAnim.Begin();
            }
            catch (Exception ex)
            {
                ShowBottomSheet(ex.Message);
                ShowLogin();
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            ((this.View as Window).FindName("DragableGrid") as Grid).MouseLeftButtonDown += View_MouseLeftButtonDown;
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            ((this.View as Window).FindName("DragableGrid") as Grid).MouseLeftButtonDown -= View_MouseLeftButtonDown;
        }

        private void View_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            (this.View as Window).DragMove();
        }

        public void ShowBottomSheet(string message)
        {
            (BottomSheetViewModel as BottomSheetViewModel).IsDisplayed = true;
            (BottomSheetViewModel as BottomSheetViewModel).Message = message;
        }

        public void HideBottomSheet()
        {
            (BottomSheetViewModel as BottomSheetViewModel).IsDisplayed = false;
        }

        public void MinimizeWindow()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        public void CloseWindow()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName == "TslGame")
                {
                    IsExitDialogOpen = true;
                    return;
                }
            }
            Application.Current.Shutdown();
        }

        public async void OnCloseExitDialog(DialogClosingEventArgs eventArgs)
        {
            try
            {
                var result = (bool)eventArgs.Parameter;
                Process[] processes;
                if (result)
                {
                    IsExitRequested = true;
                    processes = Process.GetProcesses();
                    foreach (Process process in processes)
                    {
                        if (process.ProcessName == "TslGame")
                        {
                            process.Kill();
                            process.WaitForExit();
                        }
                    }
                    (Items[0] as SettingsViewModel).FullClean();
                    await Task.Delay(2000);
                    Application.Current.Shutdown();
                    return;
                }
                processes = Process.GetProcesses();
            }
            catch (Exception ex)
            {
                IsExitRequested = false;
                ShowBottomSheet(ex.Message);
            }
        }

        public void DownloadUpdateCommand()
        {
            Process.Start(AppSettings.UpdateLink);
        }

        public void OnCloseUpdateDialog()
        {
            SettingsViewModel.IsFileRestored = true;
            Application.Current.Shutdown();
        }

        public void RepeatPreview(object sender, System.Windows.RoutedEventArgs e)
        {
            (sender as MediaElement).Position = new TimeSpan(0, 0, 1);
        }
    }
}
