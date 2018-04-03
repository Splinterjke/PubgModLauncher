using PubgMod.Models;
using PubgMod.Services;
using Microsoft.Win32;
using Newtonsoft.Json;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using System.Security.Cryptography;

namespace PubgMod.ViewModels
{
    public class LoginViewModel : Screen, ITabItem
    {
        #region Properties
        public string Login { get; set; } = string.IsNullOrEmpty(Properties.Settings.Default.email) ? string.Empty : Properties.Settings.Default.email;
        public SecureString Password { get; set; }

        public string LoginButtonText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "АВТОРИЗОВАТЬСЯ";
                return "SIGN IN";
            }
        }

        public string RegisterButtonText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "РЕГИСТРАЦИЯ";
                return "SIGN UP";
            }
        }

        public string LostPasswordText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "Забыл пароль";
                return "Restore password";
            }
        }

        public string RecoveryText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "СБРО ПАРОЛЯ";
                return "PASSWORD RESET";
            }
        }

        public string AcceptRegText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "ПРИНЯТЬ";
                return "ACCEPT";
            }
        }

        public string CancelRegText
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "ОТМЕНА";
                return "CANCEL";
            }
        }

        public string LoginHint
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "Почтовый адресс";
                return "Email";
            }
        }

        public string PasswordHint
        {
            get
            {
                if (Properties.Settings.Default.language == "ru-RU")
                    return "Пароль";
                return "Password";
            }
        }

        public bool IsProcessing { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsDialogOpen { get; set; }
        public bool IsRecoveryDialogOpen { get; set; }
        #endregion

        #region Variables        
        private MainViewModel Conductor;
        #endregion

        #region Base Methods
        public LoginViewModel()
        {
            this.DisplayName = "";
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Conductor = (this.Parent as MainViewModel);
            Conductor.PropertyChanged += Conductor_PropertyChanged;
        }

        private void Conductor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsProcessing))
                IsProcessing = Conductor.IsProcessing;
        }

        public void KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && CheckAuthForm())
                LoginCommand();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }
        #endregion

        protected override void OnValidationStateChanged(IEnumerable<string> changedProperties)
        {
            base.OnValidationStateChanged(changedProperties);
            // Manual rasing notifier for properties
            this.NotifyOfPropertyChange(() => this.CanLoginCommand);
        }

        public bool CanLoginCommand
        {
            get { return CheckAuthForm(); }
        }

        private bool CheckAuthForm()
        {
            if (string.IsNullOrEmpty(Login))
                return false;
            if (Password == null || Password.Length == 0)
                return false;
            return true;
        }

        public async void OnCloseRegisterDialog(DialogClosingEventArgs eventArgs)
        {
            try
            {
                var result = (bool)eventArgs.Parameter;
                if (result)
                {
                    Conductor.IsProcessing = true;
                    var response = await HttpService.GetRegisterUserAsync(Login, Password, UniqueProvider.Value());
                    Conductor.IsProcessing = false;
                    if (response.Item1)
                    {
                        throw new Exception(response.Item2);
                    }
                    var message = Properties.Settings.Default.language == "ru-RU" ? "Регистрация завершена" : "You successfully signed up";
                    Conductor.ShowBottomSheet(message);
                }
            }
            catch (Exception ex)
            {
                Conductor.IsProcessing = false;
                Conductor.ShowBottomSheet(ex.Message);
            }
        }

        public async void OnCloseRecoveryDialog(DialogClosingEventArgs eventArgs)
        {
            try
            {
                var result = (bool)eventArgs.Parameter;
                if (result)
                {
                    Conductor.IsProcessing = true;
                    var response = await HttpService.GetRestorePasswordAsync(Login);
                    Conductor.IsProcessing = false;
                    if (response.Item1)
                    {
                        throw new Exception(response.Item2);
                    }                    
                    if (response.Item2.Contains("has been sent"))
                    {
                        var message = Properties.Settings.Default.language == "ru-RU" ? "Новый пароль отправлен на почту" : "New password has been sent to email";
                        Conductor.ShowBottomSheet(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Conductor.IsProcessing = false;
                Conductor.ShowBottomSheet(ex.Message);
            }
        }

        public void OpenRegDialogCommand()
        {
            IsDialogOpen = true;
        }

        public void OpenRecDialogCommand()
        {
            IsRecoveryDialogOpen = true;
        }

        public async void LoginCommand()
        {
            try
            {
                Conductor.IsProcessing = true;
                var response = await HttpService.GetAuthAsync(Login, Password, UniqueProvider.Value());
                Conductor.IsProcessing = false;
                if (response.Item1)
                {
                    throw new Exception(response.Item2);
                }
                var userData = JsonConvert.DeserializeObject<UserData>(response.Item2);                
                var message = Properties.Settings.Default.language == "ru-RU" ? "Вы успешно авторизовались" : "You successfully signed in";
                Properties.Settings.Default.email = Login;
                Properties.Settings.Default.Save();
                Conductor.ShowBottomSheet(message);
                Conductor.Init(userData);
            }
            catch (Exception ex)
            {
                Conductor.IsProcessing = false;
                Conductor.ShowBottomSheet(ex.Message);
            }
        }
    }
}
