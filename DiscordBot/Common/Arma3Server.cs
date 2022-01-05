using DiscordBot.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Management;
using Serilog;
using DiscordBot.Common.SteamBridge;
using System.Threading.Tasks;
using Discord;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Common
{
    public class Arma3Server
    {
        private static Arma3Config _config { get; set; }
        public static Process Arma3Process { get; set; }

        public static Arma3Config Config { get => _config ??= BuildConfig(); }

        private static string _curProcessOwner { get; set; }
        private static string CurProcessOwner { get => _curProcessOwner ??= Utils.GetProcessOwner(Process.GetCurrentProcess().Id); }

        public static void StartServer()
        {
            UpdateMission();
            Arma3Process = new Process();
            try
            {
                Arma3Process.StartInfo.UseShellExecute = true;
                Arma3Process.StartInfo.FileName = Config.A3serverPath + "arma3server_x64.exe";
                Arma3Process.StartInfo.CreateNoWindow = false;
                Arma3Process.StartInfo.Arguments = $"" +
                    $"-profiles={Config.A3ProfilesPath} " +
                    $"-cfg={Config.A3NetworkConfigName} " +
                    $"-config={Config.A3ServerConfigName} " +
                    $"-mod={string.Join(";", GetModsList())} " +
                    $"-servermod={string.Join(";", GetServerModsList())} " +
                    $"-world=empty -autoInit -bepath=BattlEye " +
                    $"-port=" + Program.Configuration.ServerGamePort;
                Log.Information("Starting Arma 3 server {Args}", Arma3Process.StartInfo.Arguments);
                Arma3Process.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при запуске сервера");
                throw new Exception("Ошибка при запуске сервера");
            }
        }

        public static void StopServer()
        {
            try
            {
                WebSocketClient.SocketClose();
                if (Arma3Process != null) { Arma3Process.Kill(); }
                Process[] arma3process = Process.GetProcessesByName("arma3server_x64");
                foreach (Process process in arma3process)
                {
                    var pOwner = Utils.GetProcessOwner(process.Id);
                    if (pOwner != null && pOwner == CurProcessOwner)
                    {
                        process.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при остановке сервера");
                throw new Exception("Ошибка при остановке сервера");
            }
        }

        public static void RestartServer()
        {
            StopServer();
            StartServer();
        }

        public static void ClearDownloadFolder()
        {
            var files = Directory.GetFiles("Downloads", "*.pbo");
            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    try
                    {
                        File.Move(file, Program.Configuration.A3serverPath + "\\mpmissions\\" + file);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Ошибка при преносе миссии из временного каталога");
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex2)
                        {
                            Log.Error(ex2, "Ошибка при удалении мисии из временного каталога");
                        }
                    }
                }
            }
        }

        public static List<string> GetAvailableMissions()
        {
            var files = Directory.GetFiles($"{Program.Configuration.A3serverPath}\\mpmissions\\", "*.pbo");
            var list = new List<string>(files);
            return list;
        }

        public static string GetNewMission()
        {
            if (!Directory.Exists("Downloads"))
                return null;

            var files = Directory.GetFiles("Downloads", "*.pbo");
            if (files.Length > 0)
                return files[0];

            return null;
        }

        public static void MoveNewMission(string ms)
        {
            try
            {
                File.Move(ms, Program.Configuration.A3serverPath + "\\mpmissions\\" + Path.GetFileName(ms), true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при переносе новой миссии");
                throw new Exception("Ошибка при переносе новой миссии");
            }
            Log.Information("Загруженная миссия перемещна в по пути {Path}", Program.Configuration.A3serverPath + "\\mpmissions\\" + Path.GetFileName(ms));
        }

        public static void SetMS(string ms)
        {
            var path = Program.Configuration.A3serverPath + "\\" + Program.Configuration.A3ServerConfigName;
            List<string> newFile = new List<string>();
            using (StreamReader sr = new StreamReader(path))
            {
                while (sr.Peek() >= 0)
                {
                    string newLine = sr.ReadLine();
                    if (newLine.Contains("template"))
                    {
                        newLine = $"\t\ttemplate = \"{Path.GetFileNameWithoutExtension(ms)}\";";
                    }
                    newFile.Add(newLine);
                }
            }
            using (StreamWriter sw = new StreamWriter(path))
            {
                try
                {
                    foreach (string line in newFile)
                    {
                        sw.WriteLine(line);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка при записи миссии {Mission} в конфиг", ms);
                    throw new Exception($"Ошибка при записи миссии {ms} в конфиг");
                }
            }
            Log.Information("Миссия изменена на {Mission}", ms);
        }

        public static void UpdateMission()
        {
            var ms = GetNewMission();
            if (ms == null) return;

            Thread.Sleep(5000);
            MoveNewMission(ms);
            SetMS(ms);
        }

        public static async Task InstallSteamCMD(IUserMessage message)
        {
            SteamInstance.killAll();
            SteamInstaller installer = new SteamInstaller(Config.SteamCmdPath);
            if (!installer.Installed)
            {
                Log.Information("[SteamCMD] Начинаем установку...");
                installer.SteamInstalled = (sender, path) => Log.Information("[SteamCMD] Успешно загружено по пути {Path}", path);
                installer.SteamInstallationError = (sender, ex) => Log.Information(ex, "[SteamCMD] Ошибка при установке");
                await installer.installSteam();
                _ = Task.Run(async () =>
                  {
                      try
                      {
                          var st = new SteamInstance(new FileInfo(Config.SteamCmdPath + "\\steamcmd.exe"));
                          st.SteamOutput += (sender, line) => Log.Information("[SteamCMD] {Output}", line);
                          st.SteamExited += (sender, info) =>
                          {
                              var res = info == SteamExitReason.NothingSpecial ? "Успех" : "Ошибка";
                              Log.Information("[SteamCMD] результат установки: {Status}", res);
                              message.ModifyAsync(m => m.Content = $"SteamCMD результат установки: {res}");
                              Log.Information("[SteamCMD] Сеанс завершен");
                          };
                          st.Start("+quit");
                      } catch (Exception ex)
                      {
                          Log.Error(ex, "SteamCMD ошибка установки");
                          await message.ModifyAsync(m => m.Content = "SteamCMD ошибка при установке");
                      }
                      SteamInstance.killAll();
                  });
            }
        }

        public static async Task UpdateServer(IUserMessage message)
        {
            SteamInstaller installer = new SteamInstaller(Config.SteamCmdPath);
            if (installer.Installed)
            {
                _ = Task.Run(async () =>
                {
                    var messageContent = message.Content;
                    var instance = new SteamInstance(new FileInfo(Config.SteamCmdPath + "\\steamcmd.exe"));
                    instance.LoggedIn += (sender) =>
                    {
                        Log.Information("[SteamCMD] Авторизация прошла успешно, начинаем обновление");
                        messageContent = "SteamCMD Авторизация прошла успешно, начинаем обновление";
                        message.ModifyAsync(m => m.Content = messageContent);
                    };
                    instance.LoginFailed += (sender, info) =>
                    {
                        Log.Information("[SteamCMD] Ошибка при попытке авторизации из кэша");
                        message.ModifyAsync(m => m.Content = "SteamCMD Ошибка при попытке авторизации из кэша, используйте команду для авторизации");
                    };
                    instance.AppUpdateStateChanged += (sender, state) =>
                    {
                        Log.Information("[SteamCMD] Обновление сервера: {Percentage} {ReceivedBytes}/{TotalBytes} Статус {State}", state.percentage, state.receivedBytes, state.totalBytes, state.stage);
                        if (message.Content.Length > 1800)
                        {
                            messageContent = $"Обновление сервера: {state.percentage}% {state.receivedBytes}/{state.totalBytes} Статус {state.stage}";
                            message = message.Channel.SendMessageAsync(messageContent).GetAwaiter().GetResult();
                        }
                        else
                        {
                            messageContent += $"\nОбновление сервера: {state.percentage}% {state.receivedBytes}/{state.totalBytes} Статус {state.stage}";
                            message.ModifyAsync(m => m.Content = messageContent).GetAwaiter().GetResult();
                        }
                    };
                    instance.AppUpdated += (sender, state) =>
                    {
                        var res = state ? "Успех" : "Ошибка";
                        Log.Information("[SteamCMD] Результат обновления сервера: {Status}", res);
                        message.ModifyAsync(m => m.Content = $"SteamCMD результат обновления сервера: {res}");
                    };
                    instance.SteamExited += (sender, steamExit) => Log.Information("[SteamCMD] Сеанс завершен");
                    instance.Start($"+force_install_dir {Config.A3serverPath} " +
                                   $"+login {Config.SteamUserLogin} " +
                                   $"+app_update {Config.A3ServerId} -validate " +
                                   $"+quit"
                        );
                });
            }
            else
            {
                Log.Information("Ошибка: SteamCMD не установлен");
                await message.ModifyAsync(m => m.Content = "Ошибка: SteamCMD не установлен");
            }
        }

        public static async Task UpdateServerMods(IUserMessage message)
        {
            SteamInstaller installer = new SteamInstaller(Config.SteamCmdPath);
            if (installer.Installed)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var downloadedMods = new List<long>();
                        var stringBuilder = new StringBuilder();
                        var messageContent = message.Content;
                        stringBuilder.AppendLine($"force_install_dir {Config.A3serverPath}\n");
                        stringBuilder.AppendLine($"login {Config.SteamUserLogin}\n");
                        foreach (var modId in Config.Mods)
                        {
                            stringBuilder.AppendLine($"workshop_download_item {Config.A3ClientId} {modId} validate\n");
                        }
                        stringBuilder.AppendLine($"quit\n");
                        File.WriteAllText("SteamCMDTempScript.txt", stringBuilder.ToString());
                        Log.Information("[SteamCMD] Скрипт обновления готов, запускаем");

                        var instance = new SteamInstance(new FileInfo(Config.SteamCmdPath + "\\steamcmd.exe"));
                        instance.LoggedIn += (sender) =>
                        {
                            Log.Information("[SteamCMD] Авторизация прошла успешно, начинаем обновление");
                            messageContent = "SteamCMD Авторизация прошла успешно, начинаем обновление";
                            message.ModifyAsync(m => m.Content = messageContent);
                        };
                        instance.LoginFailed += (sender, info) =>
                        {
                            Log.Information("[SteamCMD] Ошибка при попытке авторизации из кэша");
                            message.ModifyAsync(m => m.Content = "SteamCMD Ошибка при попытке авторизации из кэша, используйте команду для авторизации");
                        };
                        instance.ModDownloaded += (sender, line) =>
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(line))
                                    return;
                                var modId = new DirectoryInfo(line).Name;
                                var modIdLong = long.Parse(modId);
                                var modName = GetModNameById(modIdLong);
                                downloadedMods.Add(modIdLong);
                                var newModName = "@" + modName.Replace(" ", "_").ToLower();
                                Log.Information("[SteamCMD] Mod {Name} Downloaded {Output}", modName, line);
                                if (message.Content.Length > 1800) {
                                    messageContent = $"Мод {modName}|{modId} успешно загружен";
                                    message = message.Channel.SendMessageAsync(messageContent).GetAwaiter().GetResult();
                                }
                                else {
                                    messageContent += $"\nМод {modName}|{modId} успешно загружен";
                                    message.ModifyAsync(m => m.Content = messageContent).GetAwaiter().GetResult();
                                }
                            } catch (Exception ex)
                            {
                                Log.Error(ex, "Ошибка при обновлении модов");
                            }
                        };
                        instance.SteamExited += (sender, steamExit) =>
                        {
                            var error = downloadedMods.Except(Config.Mods);
                            if (error is not null && error.Count() > 0)
                            {
                                foreach (var mod in error)
                                {
                                    Log.Error("Ошибка при загрузке мода {ModId}", mod);
                                    if (message.Content.Length > 1800)
                                        message = message.Channel.SendMessageAsync($"Ошибка при загрузке мода {mod}").GetAwaiter().GetResult();
                                    else
                                        message.ModifyAsync(m => m.Content += $"\nОшибка при загрузке мода {mod}");
                                }
                            }
                            Log.Information("[SteamCMD] Сеанс завершен");
                            messageContent += $"\nОбновление завершено";
                            message.ModifyAsync(m => m.Content = messageContent);
                        };
                        instance.Start($"+runscript {Path.Combine(Directory.GetCurrentDirectory(), "SteamCMDTempScript.txt")}");
                    } catch (Exception ex){
                        Log.Error(ex, "Ошибка при обновлении модов");
                    }
                });
            }
            else
            {
                Log.Information("Ошибка: SteamCMD не установлен");
                await message.ModifyAsync(m => m.Content = "Ошибка: SteamCMD не установлен");
            }
        }

        public static async Task SteamLogin(IUserMessage message, string login, string password, string steamGuard)
        {
            SteamInstaller installer = new SteamInstaller(Config.SteamCmdPath);
            if (installer.Installed)
            {
                _ = Task.Run(async () =>
                {
                    var instance = new SteamInstance(new FileInfo(Config.SteamCmdPath + "\\steamcmd.exe"));
                    instance.LoggedIn += (sender) =>
                    {
                        Log.Information("[SteamCMD] Авторизация прошла успешно");
                        message.ModifyAsync(m => m.Content = "SteamCMD Авторизация прошла успешно");
                        Utils.AddOrUpdateAppSetting("Arma3Config", "SteamUserLogin", login);
                        _config = BuildConfig();
                    };
                    instance.LoginFailed += (sender, info) =>
                    {
                        Log.Information("[SteamCMD] Ошибка при попытке авторизации {LoginResult}", info);
                        message.ModifyAsync(m => m.Content = $"SteamCMD Ошибка при попытке авторизации {info}");
                    };
                    instance.SteamExited += (sender, steamExit) => Log.Information("[SteamCMD] Сеанс завершен");
                    instance.Start($"+login {login} {password} {steamGuard} " +
                        $"+quit"
                        );
                });
            }
            else
            {
                Log.Information("Ошибка: SteamCMD не установлен");
                await message.ModifyAsync(m => m.Content = "Ошибка: SteamCMD не установлен");
            }
        }

        public static void DeleteMods(List<long> modIds)
        {
            var list = Config.Mods;
            foreach (var modId in modIds)
                list.Remove(modId);
            Utils.AddOrUpdateAppSetting("Arma3Config", "Mods", list);
        }

        public static void DeleteMods(long modId) => DeleteMods(new List<long> { modId });

        public static void AddMods(List<long> modIds)
        {
            var list = Config.Mods;
            list.AddRange(modIds);
            Utils.AddOrUpdateAppSetting("Arma3Config", "Mods", list);
            _config = BuildConfig();
        }

        public static void AddMods(long modId) => AddMods(new List<long> { modId });

        public static void UpdatePreset(IUserMessage message, string presetContent)
        {
            var list = new List<long>();
            var name = Regex.Matches(presetContent, @"\?id=([^<>]*)<").ToList();
            Log.Information("Список модов:");
            foreach (var item in name)
            {
                Log.Information("{Mod}", item.Groups[1].Value);
                list.Add(long.Parse(item.Groups[1].Value));
            }
            Utils.AddOrUpdateAppSetting("Arma3Config", "Mods", list);
            _config = BuildConfig();
        }

        public static async Task DeleteUnusedMods(IUserMessage message)
        {
            var unused = Directory.GetDirectories(Config.SteamContentPath).Select(x => long.Parse(new DirectoryInfo(x).Name)).Except(Config.Mods);
            var messageContent = message.Content;
            string success = "Успех";
            foreach (var item in unused)
            {
                var name = GetModNameById(item);
                success = "Успех";
                try
                {
                    Directory.Delete($"{Config.SteamContentPath}{item}");
                } catch (Exception ex)
                {
                    Log.Error(ex, $"Ошибка при удалении мода {name} | {item}");
                    success = "Ошибка";
                }
                Log.Information("Удаление мода {Name} | {ModId} : {State}", name, item, success);
                if (message.Content.Length > 1800)
                {
                    messageContent = $"Удаление мода {name} | {item} : {success}";
                    message = await message.Channel.SendMessageAsync(messageContent);
                }
                else
                {
                    messageContent += $"\nУдаление мода {name} | {item} : {success}";
                    await  message.ModifyAsync(m => m.Content = messageContent);
                }
            }
            messageContent += $"\nУдаление завершено";
            await message.ModifyAsync(m => m.Content = messageContent);
        }

        public static Dictionary<long, string> GetModsListWithNames()
        {
            var modsWithName = new Dictionary<long,string>();
            foreach (var mod in Config.Mods)
            {
                var name = "Не установлен";
                if (Directory.Exists($"{Config.SteamContentPath}{mod}"))
                {
                    name = GetModNameById(mod);
                }
                modsWithName[mod] = name;
            }
            return modsWithName;
        }

        private static List<string> GetModsList()
        {
            var dirs = Directory.GetDirectories(Config.SteamContentPath).ToList();
            var validDirs = new List<string>();
            foreach (var dir in dirs)
            {
                if (Config.Mods.Contains(long.Parse(new DirectoryInfo(dir).Name)))
                    validDirs.Add(dir);
            }
            return validDirs;
        }

        private static List<string> GetServerModsList()
        {
            return Directory.GetDirectories(Config.A3serverPath + Config.A3ServerModsPath).ToList();
        }

        public static string GetModNameById(long modId)
        {
            var metaFile = File.ReadAllLines($"{Config.SteamContentPath}{modId}\\meta.cpp");
            foreach (var item in metaFile)
            {
                if (item.Contains("name"))
                {
                    var name = Regex.Match(item, @"""(.*?)""").Groups[1];
                    return name.Value;
                }
            }
            return "";
        }

        private static Arma3Config BuildConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            return builder.GetSection("Arma3Config").Get<Arma3Config>();
        }
    }
}
