using PubgMod.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubgMod.Services
{
    public static class HttpService
    {
        private static string URL = "http://ih943403.myihor.ru/pubg.mod/api.php?action=";
        public static string Token;
        private static string lang = Properties.Settings.Default.language;

        private static async Task<string> GetAsync(string data)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(60);
                    var response = await httpClient.GetAsync($"{URL}{data}").ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return content;
                }                    
            }
            catch
            {
                return null;
            }
        }

        public static async Task<Tuple<bool, string>> GetRegisterUserAsync(string username, SecureString password, string hwid)
        {
            try
            {
                IntPtr passwordBSTR = default(IntPtr);
                string insecurePassword = "";
                passwordBSTR = Marshal.SecureStringToBSTR(password);
                insecurePassword = Marshal.PtrToStringBSTR(passwordBSTR);
                var result = await GetAsync($"reg&email={username}&password={insecurePassword}&hwid={hwid}").ConfigureAwait(false);
                if (result == null)
                {
                    return new Tuple<bool, string>(true, "Internal Server Error (502).");
                }
                var possibleError = result.ToLower();
                if (possibleError.Contains("user already exists") || possibleError.Contains("email is empty") || possibleError.Contains("password is empty"))
                {
                    return new Tuple<bool, string>(true, result);
                }
                return new Tuple<bool, string>(false, result);
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(true, ex.Message);
            }
        }

        public static async Task<Tuple<bool, string>> GetAuthAsync(string username, SecureString password, string hwid)
        {
            try
            {
                IntPtr passwordBSTR = default(IntPtr);
                string insecurePassword = "";
                passwordBSTR = Marshal.SecureStringToBSTR(password);
                insecurePassword = Marshal.PtrToStringBSTR(passwordBSTR);
                var result = await GetAsync($"auth&email={username}&password={insecurePassword}&hwid={hwid}").ConfigureAwait(false);
                if (result == null)
                {
                    return new Tuple<bool, string>(true, "Internal Server Error (502).");
                }
                var possibleError = result.ToLower();
                if (possibleError.Contains("wrong password") || possibleError.Contains("wrong hwid") || possibleError.Contains("user doesn't exist"))
                {
                    return new Tuple<bool, string>(true, result);
                }
                return new Tuple<bool, string>(false, result);
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(true, ex.Message);
            }
        }

        public static async Task<Tuple<bool, string>> GetRestorePasswordAsync(string email)
        {
            try
            {
                var result = await GetAsync($"restore&email={email}");
                if (result == null)
                {
                    return new Tuple<bool, string>(true, "Internal Server Error (502).");
                }
                var possibleError = result.ToLower();
                if (possibleError.Contains("error") || possibleError.Contains("user doesn't exist"))
                {
                    return new Tuple<bool, string>(true, result);
                }
                return new Tuple<bool, string>(false, result);
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(true, ex.Message);
            }
        }

        public static async Task<Tuple<bool, string>> GetAppSettingsAsync()
        {
            try
            {
                var result = await GetAsync($"appsettings");
                if (result == null)
                {
                    return new Tuple<bool, string>(true, "Internal Server Error (502).");
                }
                return new Tuple<bool, string>(false, result);
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(true, ex.Message);
            }
        }

        public static async Task<Tuple<bool, string>> GetUserDataAsync(string email, string hwid)
        {
            try
            {
                var result = await GetAsync($"getUserData&email={email}&hwid={hwid}");
                if (result == null)
                {
                    return new Tuple<bool, string>(true, "Internal Server Error (502).");
                }
                if (result.Contains("Wrong HWID"))
                    return new Tuple<bool, string>(true, result);
                if (result.Contains("User doesn't exist"))
                    return new Tuple<bool, string>(true, result);
                return new Tuple<bool, string>(false, result);
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(true, ex.Message);
            }
        }

        public static async Task<Tuple<bool, string>> CheckUserKeyAsync(string email, string userkey)
        {
            try
            {
                var result = await GetAsync($"checkUserKey&email={email}&userkey={userkey}");
                if (result == null)
                {
                    return new Tuple<bool, string>(true, "Internal Server Error (502).");
                }
                if (result.Contains("User doesn't exist"))
                    return new Tuple<bool, string>(true, null);
                return new Tuple<bool, string>(false, result);
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(true, ex.Message);
            }
        }
    }
}
